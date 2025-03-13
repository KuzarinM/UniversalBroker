using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Queries.Communication;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Communications
{
    /// <summary>
    /// Удаление Соединения по его Id
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class DeleteCommunicationCommandHandler(
        ILogger<DeleteCommunicationCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext) : IRequestHandler<DeleteCommunicationCommand, CommunicationDto?>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _brockerContext = brockerContext;

        public async Task<CommunicationDto?> Handle(DeleteCommunicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var communication = await _brockerContext.Communications
                    .Include(x => x.CommunicationAttributes).ThenInclude(x => x.Attribute)
                    .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (communication == null || communication.Status) // Запрещено удалять соединения, которые сейчас работают
                    return null;

                var dto = _mapper.Map<CommunicationDto>(communication);

                await _brockerContext.Attributes
                    .Where(x => communication.CommunicationAttributes.Select(y => y.Attribute.Id).Contains(x.Id))
                    .ExecuteDeleteAsync();

                await _brockerContext.Communications
                    .Where(x=>x.Id == request.Id)
                    .ExecuteDeleteAsync();

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении Соединения по его id");
                throw new ControllerException("Ошибка при удалении Соединения по его id");
            }
        }
    }
}
