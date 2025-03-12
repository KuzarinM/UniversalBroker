using MediatR;
using System.Net;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Extentions;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;
using UniversalBroker.Adapters.Tcp.Models.Internal;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class AddOrUpdateClientCommandHandler(
        ILogger<AddOrUpdateServerCommandHandler> logger,
        IMediator mediator, 
        TcpManager tcpManager,
        IInitService initService
        ) : IRequestHandler<AddOrUpdateClientCommand, bool>
    { 
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly TcpManager _tcpManager = tcpManager;
        private readonly IInitService _initService = initService;

        public async Task<bool> Handle(AddOrUpdateClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (_tcpManager.GetTcpClients.TryGetValue(request.ConnectionDto.Path, out var clientModel))
                {
                    // Обновляем конфиги
                    if (request.ConnectionDto.IsInput)
                    {
                        if(clientModel.InConnection == null)
                        {
                            clientModel.InConnection = request.ConnectionDto;
                            clientModel.Client.StartListen(); // Если раньше не слушали, то можем начать
                        }
                        else
                            clientModel.InConnection = request.ConnectionDto;
                    }
                    else
                    {
                       clientModel.OutConnection = request.ConnectionDto;
                    }

                    clientModel.TcpConfiguration.SetValueFromAttributes(request.ConnectionDto.Attributes);

                    return true;
                }

                var pathParts = request.ConnectionDto.Path.Split(":");

                var Ip = pathParts.First();
                var port = int.TryParse(pathParts.Last(), out var pt) ? pt : 80;

                var listener = new TcpClient(Ip, port);

                var tcpConfig = request.ConnectionDto.Attributes.GetModelFromAttributes<TcpConfiguration>();

                var service = await _tcpManager.StartService(listener, tcpConfig, request.ConnectionDto.Path, request.ConnectionDto.IsInput);

                var model = new TcpClientModel()
                {
                    TcpConfiguration = tcpConfig,
                    Client = service!
                };

                if (request.ConnectionDto.IsInput)
                    model.InConnection = request.ConnectionDto;
                else
                    model.OutConnection = request.ConnectionDto;

                _tcpManager.GetTcpClients.AddOrUpdate(request.ConnectionDto.Path, model, (_, old) =>
                {
                    old.Client.StopWork().Wait();
                    return model;
                });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении клиентского соединения");

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
