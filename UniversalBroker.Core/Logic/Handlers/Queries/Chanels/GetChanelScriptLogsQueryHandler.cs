using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Services;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    /// <summary>
    /// Получить инормацию о логах скрипта канала
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="context"></param>
    public class GetChanelScriptLogsQueryHandler(
        ILogger<DbLogingService> logger,
        IMapper mapper,
        BrockerContext context
        ) : IRequestHandler<GetChanelScriptLogsQuery, List<ChanelScriptLogDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = context;

        public async Task<List<ChanelScriptLogDto>> Handle(GetChanelScriptLogsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.ExecutionLogs
                                .Where(x =>
                                x.ScriptId == request.ChanelId &&
                                (request.FromDate == null || x.Datetime >= request.FromDate) &&
                                (request.ToDate == null || x.Datetime <= request.ToDate) &&
                                (request.OnlyLavels == null || request.OnlyLavels.Count == 0 || request.OnlyLavels.Select(x=>x.ToString()).Contains(x.Lavel)))
                                .Skip(request.PageSize*request.PageNumber).Take(request.PageSize)
                                .ToListAsync();

                return _mapper.Map<List<ChanelScriptLogDto>>(list);

            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получени логов выполнения скрипта");
                throw new ControllerException("Ошибка при получении логов выполнения скрипта");
            }
        }
    }
}
