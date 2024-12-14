using MediatR;
using UniversalBroker.Core.Models.Dtos.Chanels;

namespace UniversalBroker.Core.Models.Commands.Chanels
{
    public class AddChanelCommand: IRequest<ChanelDto>
    {
        public CreateChanelDto CreateChanelDto { get; set; } = new();
    }
}
