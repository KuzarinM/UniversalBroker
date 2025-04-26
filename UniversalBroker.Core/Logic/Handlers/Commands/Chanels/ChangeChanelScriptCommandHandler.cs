using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Chanels
{
    /// <summary>
    /// Изменить текст скрипта в канале
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class ChangeChanelScriptCommandHandler(
        ILogger<ChangeChanelScriptCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<ChangeChanelScriptCommand, ChanelFullDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<ChanelFullDto> Handle(ChangeChanelScriptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var model = await _context.Chanels
                            .Include(x => x.Script)
                            .Include(x => x.Connections).ThenInclude(x => x.ConnectionAttributes).ThenInclude(x => x.Attribute)
                            .Include(x => x.FromChanels).ThenInclude(x => x.Script)
                            .Include(x => x.FromChanels).ThenInclude(x => x.Connections)
                            .Include(x => x.FromChanels).ThenInclude(x => x.FromChanels)
                            .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (model == null)
                    throw new ControllerException("Не найден канас стаким Id");

                model.Script.Path = request.ScriptText;

                await _context.SaveChangesAsync();

                return _mapper.Map<ChanelFullDto>(model);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении скрипта канала");
                throw new ControllerException("Ошибка при изменении скрипта канала");
            }
        }
    }
}
