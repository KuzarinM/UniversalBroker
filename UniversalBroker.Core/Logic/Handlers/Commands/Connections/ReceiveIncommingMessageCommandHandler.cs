using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Connections
{
    /// <summary>
    /// Обработчик входящего сообщения для его отправки в каналы, связанные с подкулючением
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="mediator"></param>
    /// <param name="brockerContext"></param>
    /// <param name="dbLogingService"></param>
    public class ReceiveIncommingMessageCommandHandler(
        ILogger<ReceiveIncommingMessageCommandHandler> logger,
        IMapper mapper,
        IMediator mediator,
        BrockerContext brockerContext,
        AbstractDbLogingService dbLogingService
        ) : IRequestHandler<ReceiveIncommingMessageCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly IMediator _mediator = mediator;
        private readonly BrockerContext _context = brockerContext;
        private readonly AbstractDbLogingService _dbLogingService = dbLogingService;

        public async Task Handle(ReceiveIncommingMessageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var connection = await _context.Connections
                    .Include(x => x.Chanels)
                    .FirstOrDefaultAsync(x => x.Isinput && x.CommunicationId == request.CommunicationId && x.Path == request.Path);

                if (connection == null)
                    throw new ControllerException("На найдено подключение, в которое что-то пришло");

                var message = new InternalMessage() {
                    Data = request.Data,
                    Headers = request.Headers,
                    IsFromConnection = true,
                    SourceId = connection.Id,
                    InternalId = Guid.NewGuid(),
                };

                foreach (var item in connection.Chanels)
                {
                    try
                    {
                        var log = new MessageLog()
                        {
                            Message = message,
                            Direction = MessageDirection.ConnectionToChanel,
                            TargetId = item.Id,
                            Created = DateTime.UtcNow
                        };

                        await _mediator.Send(new ExecuteScriptCommand()
                        {
                            ChanelId = item.Id,
                            Message = message
                        });

                        await dbLogingService.LogMessage(log);
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Ошибка при попытке отправить сообщение в канал");
                    }
                }
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении сообщения в подключение");
                throw new ControllerException("Ошибка при получении сообщения в подключение");
            }
        }
    }
}
