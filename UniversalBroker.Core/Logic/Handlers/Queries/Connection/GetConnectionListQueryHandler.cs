using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos;
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
        ) : IRequestHandler<GetConnectionListQuery, PaginationModel<ConnectionDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private BrockerContext _context = context;

        public async Task<PaginationModel<ConnectionDto>> Handle(GetConnectionListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Connections
                                .Include(x=>x.ConnectionAttributes).ThenInclude(x=>x.Attribute)
                                .Where(x=>
                                        (request.CommunicationId == null || request.CommunicationId == x.CommunicationId) &&
                                        (request.InputOnly == null || x.Isinput == request.InputOnly) &&
                                        (string.IsNullOrEmpty(request.NameContains) || x.Name.Contains(request.NameContains))
                                )
                                .Skip(request.PageSize * request.PageNumber).Take(request.PageSize)
                                .ToListAsync();

                var totalPages = (await _context.Connections
                            .Where(x =>
                                        (request.CommunicationId == null || request.CommunicationId == x.CommunicationId) &&
                                        (request.InputOnly == null || x.Isinput == request.InputOnly) &&
                                        (string.IsNullOrEmpty(request.NameContains) || x.Name.Contains(request.NameContains))
                            ).CountAsync()) * 1f / request.PageSize;

                return new()
                {
                    PageSize = request.PageSize,
                    Page = _mapper.Map<List<ConnectionDto>>(list),
                    CurrentPage = request.PageNumber,
                    TotalPages = (int)Math.Ceiling(totalPages)
                };
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
