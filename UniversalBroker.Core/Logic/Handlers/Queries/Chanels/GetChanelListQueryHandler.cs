using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    /// <summary>
    /// Полученить лист каналов
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class GetChanelListQueryHandler(
        ILogger<GetChanelListQueryHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<GetChanelListQuery, PaginationModel<ChanelDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<PaginationModel<ChanelDto>> Handle(GetChanelListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var list = await _context.Chanels
                                .Include(x=>x.Script)
                                .Include(x=>x.Connections)
                                .Include(x=>x.FromChanels)
                                .Where(x=>
                                    (string.IsNullOrEmpty(request.NameContatins) || x.Name.Contains(request.NameContatins))
                                )
                                .Skip(request.PageNumber*request.PageSize).Take(request.PageSize)
                                .ToListAsync();

                var totalPages = (await _context.Chanels.Where(x =>
                                    (string.IsNullOrEmpty(request.NameContatins) || x.Name.Contains(request.NameContatins))
                                ).CountAsync()) * 1f / request.PageSize;

                return new()
                {
                    Page = _mapper.Map<List<ChanelDto>>(list),
                    PageSize = request.PageSize,
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
                _logger.LogError(ex, "Ошибка при получении списка каналов");
                throw new ControllerException("Ошибка при получении списка каналов");
            }
        }
    }
}
