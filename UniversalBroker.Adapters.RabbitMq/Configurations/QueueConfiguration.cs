using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.RabbitMq.Configurations
{
    /// <summary>
    /// Конфигурация для очереди
    /// </summary>
    [AutoConfiguration]
    public class QueueConfiguration
    {
        public bool NeedDeclare { get; set; } = true;

        public bool Durable { get; set; }

        public bool Exclusive { get; set; }

        public bool AutoDelete { get; set; }

        public bool Passive { get; set; } = false; 

        public bool NoWait { get; set; } = false;
    }
}
