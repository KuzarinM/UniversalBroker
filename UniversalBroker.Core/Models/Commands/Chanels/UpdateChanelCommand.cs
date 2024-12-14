using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Commands.Chanels
{
    public class UpdateChanelCommand: IRequest<ChanelDto>
    {
        public Guid Id { get; set; }
        public CreateChanelDto UpdateDto { get; set; } = new();
    }
}
