using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetChanelMessagesQuery: IRequest<List<MessageViewDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public Guid ChanelId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }
    }
}
