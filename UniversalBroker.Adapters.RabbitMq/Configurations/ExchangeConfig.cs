using PIHelperSh.Configuration.Attributes;
using RabbitMQ.Client;

namespace UniversalBroker.Adapters.RabbitMq.Configurations
{
    [AutoConfiguration]
    public class ExchangeConfig
    {
        public bool NeedDeclare { get; set; } = false;

        public string Type { get; set; } = ExchangeType.Fanout;

        public bool Durable { get; set; }

        public bool Exclusive { get; set; }

        public bool AutoDelete { get; set; }

        public bool Passive { get; set; } = false;

        public bool NoWait { get; set; } = false;
    }
}
