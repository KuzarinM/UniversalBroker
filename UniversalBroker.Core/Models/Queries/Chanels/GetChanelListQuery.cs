using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetChanelListQuery: IRequest<List<ChanelDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; } 

        public string? NameContatins { get; set; }
    }
}
