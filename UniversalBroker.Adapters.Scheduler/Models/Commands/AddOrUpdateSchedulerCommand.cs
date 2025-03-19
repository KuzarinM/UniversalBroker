using MediatR;
using Protos;

namespace UniversalBroker.Adapters.Scheduler.Models.Commands
{
    public class AddOrUpdateSchedulerCommand: IRequest<bool>
    {
        public ConnectionDto Connection { get; set; }
    }
}
