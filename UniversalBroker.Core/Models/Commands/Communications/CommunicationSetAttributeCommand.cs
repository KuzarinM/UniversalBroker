using MediatR;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Commands.Communications
{
    public class CommunicationSetAttributeCommand: IRequest<CommunicationDto>
    {
        public Guid CommunicationId { get; set; }
        public Dictionary<string, string?> Attributes { get; set; } = new();
    }
}
