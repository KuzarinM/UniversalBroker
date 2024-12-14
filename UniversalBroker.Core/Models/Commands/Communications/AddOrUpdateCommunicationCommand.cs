using MediatR;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Commands.Communications
{
    public class AddOrUpdateCommunicationCommand: IRequest<CommunicationDto>
    {
        public CreateCommunicationDto CreateCommunicationDto { get; set; } = new();
    }
}
