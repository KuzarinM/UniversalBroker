using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Managers;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Connections
{
    /// <summary>
    /// Удаление подключения
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class DeleteConnectionCommandHandler(
        ILogger<DeleteConnectionCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext,
        AbstractAdaptersManager abstractAdaptersManager
        ) : IRequestHandler<DeleteConnectionCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;
        private readonly AbstractAdaptersManager _abstractAdaptersManager = abstractAdaptersManager;

        public async Task Handle(DeleteConnectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var model = await _context.Connections
                        .Include(x => x.ConnectionAttributes).ThenInclude(x => x.Attribute)
                        .FirstOrDefaultAsync(x => x.Id == request.ConnectionId);
                if (model == null)
                    return;

                foreach (var item in model.ConnectionAttributes)
                {
                    _context.ConnectionAttributes.Remove(item);
                    _context.Attributes.Remove(item.Attribute);
                }

                _context.Connections.Remove(model);

                await _context.SaveChangesAsync();

                var sendTask = _abstractAdaptersManager.GetAdapterById(model.CommunicationId)?.SendMessage(new()
                {
                    DeletedConnection = _mapper.Map<Protos.ConnectionDeleteDto>(model)
                },
                   cancellationToken);

                if (sendTask != null)
                    await sendTask;
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении подключения");
                throw new ControllerException("Ошибка при удалении подключения");
            }
        }
    }
}
