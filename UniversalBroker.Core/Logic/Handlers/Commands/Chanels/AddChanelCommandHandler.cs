using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Logic.Handlers.Commands.Connections;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Chanels
{
    /// <summary>
    /// Создание нового канала
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class AddChanelCommandHandler(
        ILogger<AddChanelCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<AddChanelCommand, ChanelDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<ChanelDto> Handle(AddChanelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                //Проверям, можно ли в целом создавать модель
                await TestRelations(request.CreateChanelDto);

                var model = _mapper.Map<Chanel>(request.CreateChanelDto);

                await _context.Scripts.AddAsync(model.Script);

                model.Connections = await _context.Connections
                        .Where(x => 
                            request.CreateChanelDto.InputConnections.Contains(x.Id) || 
                            request.CreateChanelDto.OutputConnections.Contains(x.Id))
                        .ToListAsync();
                model.FromChanels = await _context.Chanels.Where(x=>request.CreateChanelDto.OutputChanels.Contains(x.Id)).ToListAsync();

                await _context.Chanels.AddAsync(model);

                await _context.SaveChangesAsync();

                return _mapper.Map<ChanelDto>(model);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении канала");
                throw new ControllerException("Ошибка при добавлении канала");
            }
        }

        public async Task TestRelations(CreateChanelDto createChanelDto)
        {
            if ((await _context.Connections
                .CountAsync(x => createChanelDto.InputConnections.Contains(x.Id) && x.Isinput)) != createChanelDto.InputConnections.Count)
                throw new ControllerException("Передаваемое как входное подключение на деле - выход, или не существует");

            if ((await _context.Connections
                .CountAsync(x => createChanelDto.OutputConnections.Contains(x.Id) && !x.Isinput)) != createChanelDto.OutputConnections.Count)
                throw new ControllerException("Передаваемое как выходное подключение на деле - вход, или не существует");

            if ((await _context.Chanels.CountAsync(x => createChanelDto.OutputChanels.Contains(x.Id))) != createChanelDto.OutputChanels.Count)
                throw new ControllerException("Один или несколько выходных каналов не существуют");
        }
    }
}
