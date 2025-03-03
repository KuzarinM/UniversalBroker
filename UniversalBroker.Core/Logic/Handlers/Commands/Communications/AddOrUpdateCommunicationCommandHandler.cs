using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Communications
{
    /// <summary>
    /// Добавить Подключение, если его нет, или обноить, если оно с таким именеием уже ест
    /// </summary>
    public class AddOrUpdateCommunicationCommandHandler : IRequestHandler<AddOrUpdateCommunicationCommand, CommunicationDto>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="brockerContext"></param>
        public AddOrUpdateCommunicationCommandHandler(
            ILogger<AddOrUpdateCommunicationCommandHandler> logger,
            IMapper mapper,
            BrockerContext brockerContext
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = brockerContext;
        }

        public async Task<CommunicationDto> Handle(AddOrUpdateCommunicationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var model = _mapper.Map<Communication>(request.CreateCommunicationDto);

                var existingModel = await _context.Communications
                    .Include(x=>x.CommunicationAttributes).ThenInclude(x=>x.Attribute)
                    .FirstOrDefaultAsync(x => x.Name == model.Name && x.TypeIdentifier == model.TypeIdentifier);

                if (existingModel == null)
                {
                    await _context.Communications.AddAsync(model);
                }
                else
                {
                    existingModel.Description = model.Description ?? existingModel.Description;
                    existingModel.Status = true;
                }

                await _context.SaveChangesAsync();

                return _mapper.Map<CommunicationDto>(existingModel ?? model);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при добавлении/обновлении подключения");
                throw new ControllerException("Ошибка при добавлении/обновлении подключения");
            }
        }
    }
}
