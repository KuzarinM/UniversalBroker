using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetChanelRelationsQuery: IRequest<СhannelRelationsDto?>
    {
        public Guid ChanelId { get; set; }
    }
}
