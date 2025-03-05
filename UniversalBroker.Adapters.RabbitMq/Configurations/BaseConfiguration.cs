using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.RabbitMq.Configurations
{
    [AutoConfiguration]
    public class BaseConfiguration
    {
        public string CoreBaseUrl { get; set; } = string.Empty;

        public Guid AdapterTypeId { get; set; } = Guid.Empty;

        public string AdapterName { get; set; } = string.Empty;

        public string AdapterDescription {  get; set; } = string.Empty;

        public double TimeToLiveSeconds { get; set; } = 20;

        public string ConnectionString { get; set; } = string.Empty;

        public string Login {  get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
