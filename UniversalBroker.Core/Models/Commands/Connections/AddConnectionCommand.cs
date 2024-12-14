using MediatR;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class AddConnectionCommand: IRequest<ConnectionDto>
    {
        public CreateConnectionDto ConnectionDto { get; set; } = new();
    }
}
