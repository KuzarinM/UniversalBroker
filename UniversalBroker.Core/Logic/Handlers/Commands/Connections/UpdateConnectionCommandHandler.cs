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
    /// Обновление инорфмации о Подключении
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mapper"></param>
    /// <param name="brockerContext"></param>
    public class UpdateConnectionCommandHandler(
        ILogger<UpdateConnectionCommandHandler> logger,
        IMapper mapper,
        BrockerContext brockerContext
        ) : IRequestHandler<UpdateConnectionCommand, ConnectionDto>
    {
        private readonly ILogger _logger = logger;
        private readonly IMapper _mapper = mapper;
        private readonly BrockerContext _context = brockerContext;

        public async Task<ConnectionDto> Handle(UpdateConnectionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingModel = await _context.Connections
                                        .Include(x => x.ConnectionAttributes).ThenInclude(x => x.Attribute)
                                        .FirstOrDefaultAsync(x => x.Id == request.ConnectionId);
                if (existingModel == null)
                    throw new ControllerException("Не найдено подключения с такмим Id");

                existingModel.Name = request.UpdateDto.Name;
                existingModel.Path = request.UpdateDto.Path;

                foreach (var connectionAttribute in existingModel.ConnectionAttributes)
                {
                    if (!request.UpdateDto.Attribues.ContainsKey(connectionAttribute.Attribute.Key))
                    {
                        _context.ConnectionAttributes.Remove(connectionAttribute);
                        _context.Attributes.Remove(connectionAttribute.Attribute);
                    }
                    else if(request.UpdateDto.Attribues[connectionAttribute.Attribute.Key] != connectionAttribute.Attribute.Value)
                    {
                        connectionAttribute.Attribute.Value = request.UpdateDto.Attribues[connectionAttribute.Attribute.Key];
                        request.UpdateDto.Attribues.Remove(connectionAttribute.Attribute.Key);
                    }
                }

                foreach (var newAttribute in request.UpdateDto.Attribues)
                {
                    var newAttrtinuteObj = _mapper.Map<ConnectionAttribute>(newAttribute);
                    newAttrtinuteObj.ConnectionId = request.ConnectionId;

                    await _context.ConnectionAttributes.AddAsync(newAttrtinuteObj);
                    await _context.Attributes.AddAsync(newAttrtinuteObj.Attribute);
                }

                await _context.SaveChangesAsync();

                return _mapper.Map<ConnectionDto>(existingModel);

            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при изменении параметров подключения");
                throw new ControllerException("Ошибка при добавлении подключения");
            }
        }
    }
}
