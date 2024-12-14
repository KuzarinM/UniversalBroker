using MediatR;

namespace UniversalBroker.Core.Models.Commands.Chanels
{
    public class DeleteChanelCommand: IRequest
    {
        public Guid Id { get; set; }
    }
}
