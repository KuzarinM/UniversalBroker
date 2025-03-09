using MediatR;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class UpdateConnectionCommand: IRequest<ConnectionDto>
    {
        public Guid ConnectionId { get; set; }
        public UpdateConnectionDto UpdateDto { get; set; }

        public bool NeedNotifyAdapter { get; set; } = true;
    }
}
