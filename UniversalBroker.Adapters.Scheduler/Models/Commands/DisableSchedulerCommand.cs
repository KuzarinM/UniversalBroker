using MediatR;

namespace UniversalBroker.Adapters.Scheduler.Models.Commands
{
    public class DisableSchedulerCommand: IRequest<bool>
    {
        public string Path { get; set; } = string.Empty;

        public string? ConnectionId { get; set; } = null;
    }
}
