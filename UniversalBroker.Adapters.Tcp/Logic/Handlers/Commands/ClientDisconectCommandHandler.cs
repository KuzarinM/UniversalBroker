using MediatR;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class ClientDisconectCommandHandler(
        ILogger<ClientDisconectCommandHandler> logger,
        TcpManager tcpManager
        ) : IRequestHandler<ClientDisconectCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly TcpManager _tcpManager = tcpManager;

        public async Task Handle(ClientDisconectCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if(_tcpManager.GetTcpClients.TryGetValue(request.Path, out var tcpClient))
                {
                    // Стопаем, на всякий
                    tcpClient.Client.StopListen();

                    // Клиенту нужен реконнект
                    var pathParts = request.Path.Split(":");

                    var Ip = pathParts.First();
                    var port = int.TryParse(pathParts.Last(), out var pt) ? pt : 80;

                    var listener = new TcpClient(Ip, port);

                    tcpClient.Client = await _tcpManager.StartService(listener, tcpClient.TcpConfiguration, request.Path, tcpClient.InConnection != null);
                }
                else if(_tcpManager.GetTcpServers.TryGetValue(request.Path, out var tcpServer))
                {
                    // На всякий стопаем
                    request.Client.StopListen();

                    // Серверу достаточно просто отключиться
                    tcpServer.Clients.Remove(request.Client);
                }
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при попытке клиента удалить самого себя после отключения");
            }
        }
    }
}
