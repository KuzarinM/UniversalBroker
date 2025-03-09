using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.Scheduler.Configurations
{
    [AutoConfiguration]
    public class AdapterConfiguration
    {
        public double TimeToLiveSeconds { get; set; } = 20;
    }
}
