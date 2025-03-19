using MediatR;
using Protos;

namespace UniversalBroker.Adapters.Tcp.Models.Commands
{
    public class AddOrUpdateServerCommand: IRequest<bool>
    {
        public ConnectionDto ConnectionDto { get; set; }
    }
}
