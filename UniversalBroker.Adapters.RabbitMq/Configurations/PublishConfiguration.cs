using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.RabbitMq.Configurations
{
    [AutoConfiguration]
    public class PublishConfiguration
    {
        public bool UseExchenge = false;

        public bool Mandatory { get; set; } = false;
    }
}
