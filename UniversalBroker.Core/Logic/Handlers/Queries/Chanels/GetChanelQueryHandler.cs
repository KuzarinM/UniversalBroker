using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    /// <summary>
    /// Получить расширенные сведения об одном канале
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class GetChanelQueryHandler(
        ILogger<GetChanelQueryHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<GetChanelQuery, ChanelFullDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<ChanelFullDto> Handle(GetChanelQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var model = await _context.Chanels
                            .Include(x => x.Script)
                            .Include(x => x.Connections).ThenInclude(x=>x.ConnectionAttributes).ThenInclude(x=>x.Attribute)
                            .Include(x=>x.FromChanels).ThenInclude(x=>x.Script)
                            .Include(x => x.FromChanels).ThenInclude(x => x.Connections)
                            .Include(x => x.FromChanels).ThenInclude(x => x.FromChanels)
                            .FirstOrDefaultAsync(x => x.Id == request.ChanelId);

                if (model == null)
                    throw new ControllerException("Не найден канас стаким Id");

                return _mapper.Map<ChanelFullDto>(model);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получени канала");
                throw new ControllerException("Ошибка при получении канала");
            }
        }
    }
}
