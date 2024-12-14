using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Queries.Connections
{
    public class GetConnectionMessagesQuery: IRequest<List<MessageViewDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public Guid ConnectionId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
