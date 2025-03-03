using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Connections
{
    /// <summary>
    /// Добавление нового Подключения
    /// </summary>
    public class AddConnectionCommandHandler : IRequestHandler<AddConnectionCommand, ConnectionDto>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="brockerContext"></param>
        public AddConnectionCommandHandler(
            ILogger<AddConnectionCommandHandler> logger,
            IMapper mapper,
            BrockerContext brockerContext
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = brockerContext;
        }

        public async Task<ConnectionDto> Handle(AddConnectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!(await _context.Communications.AnyAsync(x => x.Id == request.ConnectionDto.CommunicationId)))
                    throw new ControllerException("Не найдено соединение с таким Id");

                if (await _context.Connections.AnyAsync(x => x.Name == request.ConnectionDto.Name))
                    throw new ControllerException("Подключение с таким именем уже есть");

                var model = _mapper.Map<Connection>(request.ConnectionDto);

                foreach (var item in model.ConnectionAttributes)
                {
                    await _context.Attributes.AddAsync(item.Attribute);
                    await _context.ConnectionAttributes.AddAsync(item);
                }
                await _context.AddAsync(model);

                await _context.SaveChangesAsync();

                return _mapper.Map<ConnectionDto>(model);

            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении подключения");
                throw new ControllerException("Ошибка при добавлении подключения");
            }
        }
    }
}
