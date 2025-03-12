using MediatR;
using System.Net.Sockets;
using System.Net;
using UniversalBroker.Adapters.Tcp.Extentions;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;
using UniversalBroker.Adapters.Tcp.Models.Internal;
using UniversalBroker.Adapters.Tcp.Configurations;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class AddOrUpdateServerCommandHandler(
        ILogger<AddOrUpdateServerCommandHandler> logger, 
        IMediator mediator, 
        TcpManager tcpManager,
        IInitService initService
        ) : IRequestHandler<AddOrUpdateServerCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly TcpManager _tcpManager = tcpManager;
        private readonly IInitService _initService = initService;

        public async Task<bool> Handle(AddOrUpdateServerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if(_tcpManager.GetTcpServers.TryGetValue(request.ConnectionDto.Path, out var serverModel))
                {
                    // Обновляем конфиги
                    if (request.ConnectionDto.IsInput)
                    {
                        if (serverModel.InConnection == null)
                        {
                            serverModel.InConnection = request.ConnectionDto;
                            // Если раньше не слушали, то можем начать
                            foreach (var item in serverModel.Clients)
                            {
                                item.StartListen();
                            }
                        }
                        else
                            serverModel.InConnection = request.ConnectionDto;
                    }
                    else
                    {
                        serverModel.OutConnection = request.ConnectionDto;
                    }

                    return true;
                }

                var pathParts = request.ConnectionDto.Path.Split(":");

                var Ip = IPAddress.TryParse(pathParts.First(), out var ipAddr )? ipAddr : IPAddress.Any;
                var port = int.TryParse(pathParts.Last(), out var pt) ? pt : 80;

                var tcpConfig = request.ConnectionDto.Attributes.GetModelFromAttributes<TcpConfiguration>();

                var tcpListener = new TcpListener(Ip, port);
                tcpListener.Start();

                var model = new TcpServerModel()
                {
                    TcpListener = tcpListener,
                    ReceiveClientTask = tcpListener.AcceptTcpClientAsync(),
                    TcpConfiguration = tcpConfig
                };

                if (request.ConnectionDto.IsInput)
                    model.InConnection = request.ConnectionDto; // Тут запускать случшателей не у кого, так как сервак ещй не поднят
                else
                    model.OutConnection = request.ConnectionDto;

                _tcpManager.GetTcpServers.AddOrUpdate(request.ConnectionDto.Path, model, (_, old) =>
                {
                    foreach (var item in old.Clients)
                    {
                        item.StopWork().Wait();
                    }
                    return model;
                });

                _tcpManager.GetTcpListeners.AddOrUpdate(model.ReceiveClientTask, model, (_, _) => model);

                await _tcpManager.RestartListeners(); // Вот тут сервак поднят

                return true;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при добавлении серверного соединения");

                request.ConnectionDto.Attributes.AddOrUpdateAttribute("Error", ex.Message);

                var task = _initService.GetService?.SendMessage(new()
                {
                    Connection = request.ConnectionDto
                },
                cancellationToken);

                if (task != null)
                    await task;

                return false;
            }
        }
    }
}
