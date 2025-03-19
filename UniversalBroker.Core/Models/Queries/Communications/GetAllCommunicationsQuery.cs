using MediatR;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Queries.Communications
{
    public class GetAllCommunicationsQuery: IRequest<PaginationModel<CommunicationDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public bool? Status { get; set; }

        public string? NameSearch { get; set; }
    }
}
