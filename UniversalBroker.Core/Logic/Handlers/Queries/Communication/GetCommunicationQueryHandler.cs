using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Queries.Communications;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Communication
{
    /// <summary>
    /// Получить информацию о Соединении
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class GetCommunicationQueryHandler(
        ILogger<GetCommunicationQueryHandler> logger, 
        IMapper mapper, 
        BrockerContext brockerContext) : IRequestHandler<GetCommunicationQuery, CommunicationDto?>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _brockerContext = brockerContext;

        public async Task<CommunicationDto?> Handle(GetCommunicationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var communication = await _brockerContext.Communications
                    .Include(x=>x.CommunicationAttributes).ThenInclude(x=>x.Attribute)
                    .FirstOrDefaultAsync(x=>x.Id == request.Id);

                if (communication == null)
                    return null;

                return _mapper.Map<CommunicationDto>(communication);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при получении Соединения по его id");
                throw new ControllerException("Ошибка при получении Соединения по его id");
            }
        }
    }
}
