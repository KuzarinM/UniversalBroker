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
                    // Перезапускать смысла нет, достаточно просто обновить конфиги и всё
                    serverModel.TcpConfiguration.SetValueFromAttributes(request.ConnectionDto.Attributes);

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
                    Connection = request.ConnectionDto,
                    TcpListener = tcpListener,
                    ReceiveClientTask = tcpListener.AcceptTcpClientAsync(),
                    TcpConfiguration = tcpConfig
                };

                _tcpManager.GetTcpServers.AddOrUpdate(request.ConnectionDto.Path, model, (_, old) =>
                {
                    foreach (var item in old.Clients)
                    {
                        item.StopWork().Wait();
                    }
                    return model;
                });

                _tcpManager.GetTcpListeners.AddOrUpdate(model.ReceiveClientTask, model, (_, _) => model);

                await _tcpManager.RestartListeners();

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
