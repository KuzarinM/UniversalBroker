using MediatR;

namespace UniversalBroker.Adapters.Tcp.Models.Commands
{
    public class RemoveServerCommand: IRequest<bool>
    {
        public string Path { get; set; } = string.Empty;

        public string? ConnectionId { get; set; }

        public bool IsInput { get; set; } = false;
    }
}
