using MediatR;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Models.Dtos.Connections;

namespace UniversalBroker.Core.Models.Queries.Connections
{
    public class GetConnectionListQuery: IRequest<List<ConnectionDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public Guid? CommunicationId { get; set; }

        public bool? InputOnly { get; set; } = null;
    }
}
