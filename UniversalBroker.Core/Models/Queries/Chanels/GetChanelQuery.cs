using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetChanelQuery: IRequest<ChanelFullDto>
    {
        public Guid ChanelId { get; set; }
    }
}
