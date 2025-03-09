using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.Scheduler.Configurations
{
    [AutoConfiguration]
    public class SchedulerConfiguration
    {
        public int IntervalMs { get; set; } = 25000;

        public string? MessageText = null;

        public bool AutoReset = true;
    }
}
