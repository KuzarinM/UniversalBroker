using MediatR;
using Protos;

namespace UniversalBroker.Adapters.Tcp.Models.Commands
{
    public class AddOrUpdateClientCommand : IRequest<bool>
    {
        public ConnectionDto ConnectionDto { get; set; }
    }
}
