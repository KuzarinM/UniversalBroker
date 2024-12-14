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
    /// Получить список о Подключений
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="context"></param>
    public class GetConnectionListQueryHandler(
        ILogger<GetConnectionListQueryHandler> logger,
        IMapper mapper,
        BrockerContext context
        ) : IRequestHandler<GetConnectionListQuery, List<ConnectionDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private BrockerContext _context = context;

        public async Task<List<ConnectionDto>> Handle(GetConnectionListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Connections
                                .Include(x=>x.ConnectionAttributes).ThenInclude(x=>x.Attribute)
                                .Skip(request.PageSize*request.PageNumber).Take(request.PageSize)
                                .Where(x=>
                                        (request.CommunicationId == null || request.CommunicationId == x.CommunicationId) &&
                                        (request.InputOnly == null || x.Isinput == request.InputOnly)
                                )
                                .ToListAsync();

                return _mapper.Map<List<ConnectionDto>>(list);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка соединений");
                throw new ControllerException("Ошибка при получении списка соединений");
            }
        }
    }
}
