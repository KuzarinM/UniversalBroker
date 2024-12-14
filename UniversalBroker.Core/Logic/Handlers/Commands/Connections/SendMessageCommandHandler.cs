using AutoMapper;
using MediatR;
using Newtonsoft.Json;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Connections
{
    /// <summary>
    /// Отправка сообзения в выходное Подключение. Пока что заглушка
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class SendMessageCommandHandler(
        ILogger<AddConnectionCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
        ) : IRequestHandler<SendMessageCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task Handle(SendMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //TODO Тут будем вызывать по gRPC всё что надо;

                _logger.LogWarning("Сообщение не отправлено: {message}", JsonConvert.SerializeObject(request.Message,Formatting.Indented));
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отправке сообщения в подключение");
                throw new ControllerException("Ошибка при отправке сообщения в подключение");
            }
        }
    }
}
