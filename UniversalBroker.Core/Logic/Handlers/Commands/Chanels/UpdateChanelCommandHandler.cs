using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Commands.Chanels
{
    /// <summary>
    /// Изменить все данные канала на новые.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class UpdateChanelCommandHandler(
        ILogger<UpdateChanelCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
    ) : IRequestHandler<UpdateChanelCommand, ChanelDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<ChanelDto> Handle(UpdateChanelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingModel = await _context.Chanels
                                            .Include(x => x.Script)
                                            .Include(x => x.Connections)
                                            .Include(x => x.FromChanels)
                                            .FirstOrDefaultAsync(x => x.Id == request.Id);

                if (existingModel == null)
                    throw new ControllerException("Канал с этим Id не найден");

                await TestRelations(request.UpdateDto);

                existingModel.Script.Path = request.UpdateDto.Script;
                existingModel.Name = request.UpdateDto.Name;

                existingModel.FromChanels = existingModel.FromChanels.Where(x => request.UpdateDto.OutputChanels.Contains(x.Id)).ToList();

                var addingChannels = request.UpdateDto.OutputChanels.Where(x => !existingModel.FromChanels.Select(x => x.Id).Contains(x));

                await _context.Chanels.Where(x => addingChannels.Contains(x.Id)).ForEachAsync(x => existingModel.FromChanels.Add(x));

                existingModel.Connections = existingModel.Connections
                        .Where(x => 
                            request.UpdateDto.OutputConnections.Contains(x.Id) || 
                            request.UpdateDto.InputConnections.Contains(x.Id))
                        .ToList();
                var addingConnections = request.UpdateDto.InputConnections
                                                .Union(request.UpdateDto.OutputConnections)
                                                .Where(x => !existingModel.Connections.Select(x => x.Id).Contains(x))
                                                .ToList();

                await _context.Connections.Where(x=>addingConnections.Contains(x.Id)).ForEachAsync(x=>existingModel.Connections.Add(x));

                await _context.SaveChangesAsync();

                return _mapper.Map<ChanelDto>(existingModel);
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении канала");
                throw new ControllerException("Ошибка при изменении канала");
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
