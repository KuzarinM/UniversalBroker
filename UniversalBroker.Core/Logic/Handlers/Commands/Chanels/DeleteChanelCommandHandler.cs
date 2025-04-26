using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Queries.Chanels;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Chanels
{
    /// <summary>
    /// Удалить канал
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class DeleteChanelCommandHandler(
        ILogger<DeleteChanelCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<DeleteChanelCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task Handle(DeleteChanelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var model = await _context.Chanels.Include(x=>x.Script).FirstOrDefaultAsync(x=>x.Id == request.Id);

                if (model == null)
                    return;
               
                if(model.Script != null)
                    _context.Scripts.Remove(model.Script);

                _context.Chanels.Remove(model);

                await _context.SaveChangesAsync();
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении канала");
                throw new ControllerException("Ошибка при удалении канала");
            }
        }
    }
}
