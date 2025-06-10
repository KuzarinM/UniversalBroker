using MediatR;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Commands.Connections
{
    public class AddConnectionCommand: IRequest<ConnectionViewDto>
    {
        public CreateConnectionDto ConnectionDto { get; set; } = new();
    }
}
