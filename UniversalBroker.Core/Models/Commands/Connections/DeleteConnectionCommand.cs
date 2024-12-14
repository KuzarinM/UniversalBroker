using MediatR;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class DeleteConnectionCommand: IRequest
    {
        public Guid ConnectionId { get; set; }
    }
}
