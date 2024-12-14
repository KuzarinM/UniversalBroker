using MediatR;
using UniversalBroker.Core.Models.Dtos.Communications;

namespace UniversalBroker.Core.Models.Queries.Communications
{
    public class GetCommunicationQuery: IRequest<CommunicationDto?>
    {
        public Guid Id { get; set; }
    }
}
