using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    /// <summary>
    /// Полученить лист каналов
    /// </summary>
    public class GetChanelListQueryHandler : IRequestHandler<GetChanelListQuery, List<ChanelDto>>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="brockerContext"></param>
        public GetChanelListQueryHandler(
            ILogger<GetChanelListQueryHandler> logger,
            IMapper mapper,
            BrockerContext brockerContext
    )
        {
            _logger = logger;
            _mapper = mapper;
            _context = brockerContext;
        }

        public async Task<List<ChanelDto>> Handle(GetChanelListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Chanels
                                .Include(x=>x.Script)
                                .Include(x=>x.Connections)
                                .Include(x=>x.FromChanels)
                                .Skip(request.PageNumber*request.PageSize).Take(request.PageSize)
                                .ToListAsync();

                return _mapper.Map<List<ChanelDto>>(list);

            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка каналов");
                throw new ControllerException("Ошибка при получении списка каналов");
            }
        }
    }
}
