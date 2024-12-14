using MediatR;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class SendMessageCommand: IRequest
    {
        public Guid ConnectionId { get; set; }

        public InternalMessage Message { get; set; } = new();
    }
}
