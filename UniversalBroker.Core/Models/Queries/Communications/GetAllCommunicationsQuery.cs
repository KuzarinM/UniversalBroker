using MediatR;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Queries.Communications
{
    public class GetAllCommunicationsQuery: IRequest<List<CommunicationDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public bool? Status { get; set; }

        public string? NameSearch { get; set; }
    }
}
