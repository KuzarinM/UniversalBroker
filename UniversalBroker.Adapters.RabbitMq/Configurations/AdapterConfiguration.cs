using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.RabbitMq.Configurations
{
    [AutoConfiguration]
    public class AdapterConfiguration
    {
        public double TimeToLiveSeconds { get; set; } = 20;
    }
}
