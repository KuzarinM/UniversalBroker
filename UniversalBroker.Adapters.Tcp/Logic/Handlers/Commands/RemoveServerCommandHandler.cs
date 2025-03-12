using MediatR;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class RemoveServerCommandHandler(
        ILogger<RemoveClientCommandHandler> logger,
        IInitService initService,
        ITcpManager tcpManager
        ) : IRequestHandler<RemoveServerCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IInitService _initService = initService;
        private readonly ITcpManager _tcpManager = tcpManager;

        public async Task<bool> Handle(RemoveServerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (_tcpManager.GetTcpServers.TryGetValue(request.Path, out var server))
                {

                    if (
                        request.IsInput &&
                        server.InConnection != null &&
                        (string.IsNullOrEmpty(request.ConnectionId) || server.InConnection.Id == request.ConnectionId))
                    {
                        _logger.LogInformation("Удаляем существующее входное серверное подключение по пути {path}", request.Path);

                        server.InConnection = null;

                        foreach(var clients in server.Clients)
                        {
                            clients.StopListen();// Останавливаемся
                        }
                    }
                    else if (
                         !request.IsInput &&
                        server.OutConnection != null &&
                        (string.IsNullOrEmpty(request.ConnectionId) || server.OutConnection.Id == request.ConnectionId))
                    {
                        _logger.LogInformation("Удаляем существующее выходное серверное подключение по пути {path}", request.Path);

                        server.OutConnection = null;
                    }
                    else
                        _logger.LogWarning("Ничего по пути {path} не обновили. Подозрительно", request.Path);

                    if (server.InConnection == null && server.OutConnection == null)
                    {
                        _logger.LogInformation("По пути {path} не осталось ни входов ни выходов, так что убиваем всё", request.Path);

                        _tcpManager.GetTcpListeners.TryRemove(server.ReceiveClientTask, out _);

                        await _tcpManager.RestartListeners();

                        _tcpManager.GetTcpServers.TryRemove(request.Path, out _); // Удаляем

                        foreach (var clients in server.Clients)
                        {
                            clients.StopListen();// Останавливаемся на случай если успели прицепиться
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении серверного соединения по пути {path}", request.Path);

                var task = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "CANNOT DELETE"
                    }
                },
                cancellationToken);

                if (task != null)
                    await task;

                return false;
            }
        }
    }
}
