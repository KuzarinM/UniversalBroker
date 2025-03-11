using MediatR;
using Protos;

namespace UniversalBroker.Adapters.Tcp.Models.Commands
{
    public class SendMessageCommand: IRequest<bool>
    {
        public MessageDto Message { get; set; }
    }
}
