using MediatR;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Commands.Communications
{
    public class DeleteCommunicationCommand: IRequest<CommunicationDto?>
    {
        public Guid Id { get; set; }
    }
}
