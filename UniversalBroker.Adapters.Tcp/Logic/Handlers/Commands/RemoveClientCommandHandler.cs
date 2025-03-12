using MediatR;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class RemoveClientCommandHandler(
        ILogger<RemoveClientCommandHandler> logger, 
        IInitService initService,
        TcpManager tcpManager
        ) : IRequestHandler<RemoveClientCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IInitService _initService = initService;
        private readonly TcpManager _tcpManager = tcpManager;

        public async Task<bool> Handle(RemoveClientCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (_tcpManager.GetTcpClients.TryGetValue(request.Path, out var client))
                {

                    if (
                        request.IsInput &&
                        client.InConnection != null &&
                        (string.IsNullOrEmpty(request.ConnectionId) || client.InConnection.Id == request.ConnectionId))
                    {
                        _logger.LogInformation("Удаляем существующее входное клиенсткое подключение по пути {path}", request.Path);

                        client.InConnection = null;

                        client.Client.StopListen();// Останавливаемся
                    }
                    else if (
                         !request.IsInput &&
                        client.OutConnection != null &&
                        (string.IsNullOrEmpty(request.ConnectionId) || client.OutConnection.Id == request.ConnectionId))
                    {
                        _logger.LogInformation("Удаляем существующее выходное клиенсткое подключение по пути {path}", request.Path);

                        client.OutConnection = null;
                    }
                    else
                        _logger.LogWarning("Ничего по пути {path} не обновили. Подозрительно", request.Path);

                    if (client.InConnection == null && client.OutConnection == null) 
                    {
                        _logger.LogInformation("По пути {path} не осталось ни входов ни выходов, так что убиваем всё", request.Path);

                        _tcpManager.GetTcpClients.TryRemove(request.Path, out _); // Удаляем
                    }
                }

                return true;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при удалении клиентского соединения по пути {path}", request.Path);

                var task = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "CANNOT DELETE"
                    }
                },
                cancellationToken);

                if(task != null) 
                    await task;

                return false;
            }
        }
    }
}
