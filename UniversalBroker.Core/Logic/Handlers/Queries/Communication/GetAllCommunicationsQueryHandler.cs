using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
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
        ) : IRequestHandler<GetAllCommunicationsQuery, List<CommunicationDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _brockerContext = brockerContext;

        public async Task<List<CommunicationDto>> Handle(GetAllCommunicationsQuery request, CancellationToken cancellationToken)
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

                return _mapper.Map<List<CommunicationDto>>(rawList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось получить список соединений");
                throw new ControllerException("Не удалось получить список соединений");
            }
        }
    }
}
