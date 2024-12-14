using MediatR;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Queries.Connections
{
    public class GetConnectionQuery: IRequest<ConnectionFullDto>
    {
        public Guid Id { get; set; }
    }
}
