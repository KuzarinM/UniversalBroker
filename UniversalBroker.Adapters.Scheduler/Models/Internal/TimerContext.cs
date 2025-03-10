using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;

namespace UniversalBroker.Adapters.Scheduler.Models.Internal
{
    public class TimerContext
    {
        public IInitService InitService { get; set; }

        public SchedulerInstanceModel SchedulerInstanceModel { get; set; }
    }
}
