using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Queries.Communications;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Communication
{
    /// <summary>
    /// Получить список всех Соединений
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class GetAllCommunicationsQueryHandler(
        ILogger<GetAllCommunicationsQueryHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
        ) : IRequestHandler<GetAllCommunicationsQuery, PaginationModel<CommunicationDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _brockerContext = brockerContext;

        public async Task<PaginationModel<CommunicationDto>> Handle(GetAllCommunicationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var rawList = await _brockerContext.Communications
                    .Include(x => x.CommunicationAttributes).ThenInclude(x => x.Attribute)
                    .Where(x=>
                            (!request.Status.HasValue || x.Status == request.Status) &&
                            (string.IsNullOrEmpty(request.NameSearch) || x.Name.Contains(request.NameSearch))
                    )
                    .Skip(request.PageNumber * request.PageSize).Take(request.PageSize)
                    .ToListAsync();

                var totalPages = (await _brockerContext.Communications
                    .Where(x =>
                            (!request.Status.HasValue || x.Status == request.Status) &&
                            (string.IsNullOrEmpty(request.NameSearch) || x.Name.Contains(request.NameSearch))
                    )
                    .CountAsync()) * 1f / request.PageSize;

                return new()
                {
                    Page = _mapper.Map<List<CommunicationDto>>(rawList),
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalPages)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список соединений");
                throw new ControllerException("Не удалось получить список соединений");
            }
        }
    }
}
