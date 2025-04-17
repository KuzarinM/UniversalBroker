using MediatR;
using UniversalBroker.Core.Models.Dtos;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetSystemRelationQuery: IRequest<List<NodeDto>>
    {
    }
}
