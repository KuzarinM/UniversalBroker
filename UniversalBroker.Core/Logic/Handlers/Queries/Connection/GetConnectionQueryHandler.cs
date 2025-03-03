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
    public class GetConnectionQueryHandler : IRequestHandler<GetConnectionQuery, ConnectionFullDto>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="context"></param>
        public GetConnectionQueryHandler(
            ILogger<GetConnectionListQueryHandler> logger,
            IMapper mapper,
            BrockerContext context
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        public async Task<ConnectionFullDto> Handle(GetConnectionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Connections
                                .Include(x => x.ConnectionAttributes).ThenInclude(x => x.Attribute)
                                .Include(x=>x.Communication)
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
