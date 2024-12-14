using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Chanels
{
    /// <summary>
    /// Выполнить скрипт в канале = отправить сообщение в этот канал
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    /// <param name="interpretatorService"></param>
    public class ExecuteScriptCommandHandler(
        ILogger<ExecuteScriptCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext,
        IChanelJsInterpretatorService interpretatorService
    ) : IRequestHandler<ExecuteScriptCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;
        private readonly IChanelJsInterpretatorService _interpretatorService = interpretatorService;

        public async Task Handle(ExecuteScriptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var chanel = await _context.Chanels
                                .Include(x=>x.Script)
                                .Include(x=>x.Connections)
                                .Include(x=>x.FromChanels)
                                .FirstOrDefaultAsync(x => x.Id == request.ChanelId);

                if (chanel == null)
                    throw new ControllerException("Не найден канал с таким ID");

                await _interpretatorService.ExecuteScript(chanel, request.Message);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при выполнении скрипта");
                throw new ControllerException("Ошибка при выполнении скрипта");
            }
        }
    }
}
