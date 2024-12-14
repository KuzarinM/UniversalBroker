using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Services;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    /// <summary>
    /// Получить сообщения из истории канала
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="context"></param>
    public class GetChanelMessagesQueryHandler(
        ILogger<GetChanelMessagesQueryHandler> logger,
        IMapper mapper,
        BrockerContext context
        ) : IRequestHandler<GetChanelMessagesQuery, List<MessageViewDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = context;

        public async Task<List<MessageViewDto>> Handle(GetChanelMessagesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Messages
                                    .Include(x=>x.Connection)
                                    .Include(x=>x.Headers)
                                    .Where(x =>
                                        (x.TargetChannelId == request.ChanelId || x.SourceChannelId == request.ChanelId) &&
                                        (request.FromDate == null || x.Datetime >= request.FromDate) &&
                                        (request.ToDate == null || x.Datetime <= request.ToDate)
                                    )
                                    .OrderBy(x => x.Datetime)
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
                _logger.LogError(ex, "Ошибка при получени списка сообщений, прошедсших через канал");
                throw new ControllerException("Ошибка при получени списка сообщений, прошедсших через канал");
            }
        }
    }
}
