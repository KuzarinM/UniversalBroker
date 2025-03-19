using MediatR;
using Protos;

namespace UniversalBroker.Adapters.Scheduler.Models.Commands
{
    public class RestartSchedulerCommand : IRequest<bool>
    {
        public MessageDto Message { get; set; }
    }
}
