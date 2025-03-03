using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Queries.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;
using UniversalBroker.Core.Models.Queries.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Connection
{
    /// <summary>
    /// Получить информацию о сообщениях в Подключении
    /// </summary>
    public class GetConnectionMessagesQueryHandler : IRequestHandler<GetConnectionMessagesQuery, List<MessageViewDto>>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="context"></param>
        public GetConnectionMessagesQueryHandler(
   ILogger<GetConnectionMessagesQueryHandler> logger,
            IMapper mapper,
            BrockerContext context
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<MessageViewDto>> Handle(GetConnectionMessagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Messages
                                    .Include(x => x.Connection)
                                    .Include(x => x.Headers)
                                    .Where(x =>
                                        x.ConnectionId == request.ConnectionId &&
                                        (request.FromDate == null || x.Datetime >= request.FromDate) &&
                                        (request.ToDate == null || x.Datetime <= request.ToDate)
                                    )
                                    .OrderBy(x=>x.Datetime)
                                    .Skip(request.PageSize * request.PageNumber).Take(request.PageSize)
                                    .ToListAsync();

                return _mapper.Map<List<MessageViewDto>>(list);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получени списка сообщений, прошедсших через подключение");
                throw new ControllerException("Ошибка при получени списка сообщений, прошедсших через подключение");
            }
        }
    }
}
