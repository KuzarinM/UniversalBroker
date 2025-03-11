using MediatR;
using System.Linq;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Logic.Managers;
using UniversalBroker.Adapters.Tcp.Models.Commands;

namespace UniversalBroker.Adapters.Tcp.Logic.Handlers.Commands
{
    public class SendMessageCommandHandler(
        ILogger<SendMessageCommandHandler> logger,
        IInitService initService,
        TcpManager tcpManager)
        : IRequestHandler<SendMessageCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IInitService _initService = initService;
        private readonly TcpManager _tcpManager = tcpManager;

        public async Task<bool> Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (_tcpManager.GetTcpServers.TryGetValue(request.Message.Path, out var tcpServer)) 
                {
                    // todo обработка заголовка Custom.ToAddres

                    await Task.WhenAll(tcpServer.Clients.Select(x => x.SendMessage(request.Message.Data.ToList())).ToArray());
                }
                else
                    _logger.LogInformation("Серверов по пути {path} не найдено", request.Message.Path);

                if (_tcpManager.GetTcpClients.TryGetValue(request.Message.Path, out var tcpClient))
                {
                    await tcpClient.Client.SendMessage(request.Message.Data.ToList());
                }
                else
                    _logger.LogInformation("Клиентов по пути {path} не найдено", request.Message.Path);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке отправить сообщение по пути {path}", request.Message.Path);

                var task = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "MESSAGE NOT SEND"
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
