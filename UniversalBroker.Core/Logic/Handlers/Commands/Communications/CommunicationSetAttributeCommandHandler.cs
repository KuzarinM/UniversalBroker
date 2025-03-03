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
    /// Установить значения атрибутов доя Подключения
    /// </summary>
    public class CommunicationSetAttributeCommandHandler : IRequestHandler<CommunicationSetAttributeCommand, CommunicationDto>
    {
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly BrockerContext _context;

        /// <param name="logger"></param>
        /// <param name="mapper"></param>
        /// <param name="brockerContext"></param>
        public CommunicationSetAttributeCommandHandler(
            ILogger<CommunicationSetAttributeCommandHandler> logger,
            IMapper mapper,
            BrockerContext brockerContext
        )
        {
            _logger = logger;
            _mapper = mapper;
            _context = brockerContext;
        }

        public async Task<CommunicationDto> Handle(CommunicationSetAttributeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var communoication = await _context.Communications
                                                .Include(x => x.CommunicationAttributes).ThenInclude(x => x.Attribute)
                                                .FirstOrDefaultAsync(x => x.Id == request.CommunicationId);
                if (communoication == null)
                    throw new ControllerException("Не удалось найти соединение с такоим Id");

                foreach (var item in request.Attributes)
                {
                    try
                    {
                        var current = communoication.CommunicationAttributes.FirstOrDefault(x => x.Attribute.Key == item.Key);
                        if (current != null)
                        {
                            if (item.Value == null)
                            {
                                _context.CommunicationAttributes.Remove(current);
                                _context.Attributes.Remove(current.Attribute);
                            }
                                

                            else if (current.Attribute.Value != item.Value)
                                current.Attribute.Value = item.Value;
                        }
                        else if(item.Value != null)
                        {
                            current = _mapper.Map<CommunicationAttribute>(item);
                            current.ConnectionId = request.CommunicationId;

                            await _context.Attributes.AddAsync(current.Attribute);
                            await _context.CommunicationAttributes.AddAsync(current);
                        }
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Не удалось добавить атрибут.");
                    }
                }
                await _context.SaveChangesAsync();

                return _mapper.Map<CommunicationDto>(communoication!);
            }
            catch(ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при обновлении аттрибутов соединения");
                throw new ControllerException("Ошибка при обновлении аттрибутов соединения");
            }
        }
    }
}
