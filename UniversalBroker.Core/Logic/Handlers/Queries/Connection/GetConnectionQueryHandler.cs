using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Queries.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Connection
{
    /// <summary>
    /// Получить расширенную инофрмацию о Подключении
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="context"></param>
    public class GetConnectionQueryHandler(
        ILogger<GetConnectionQueryHandler> logger,
        IMapper mapper,
        BrockerContext context
        ) : IRequestHandler<GetConnectionQuery, ConnectionFullDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private BrockerContext _context = context;

        public async Task<ConnectionFullDto> Handle(GetConnectionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Connections
                                .Include(x => x.ConnectionAttributes).ThenInclude(x => x.Attribute)
                                .Include(x=>x.Communication)
                                .Include(x=>x.Chanels)
                                .FirstOrDefaultAsync(x => x.Id == request.Id);

                return _mapper.Map<ConnectionFullDto>(list);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении соединения");
                throw new ControllerException("Ошибка при получении соединения");
            }
        }
    }
}
