using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Adapters.Tcp.Configurations
{
    [AutoConfiguration]
    public class BaseConfiguration
    {
        public string CoreBaseUrl { get; set; } = string.Empty;

        public Guid AdapterTypeId { get; set; } = Guid.Empty;

        public string AdapterName { get; set; } = string.Empty;

        public string AdapterDescription { get; set; } = string.Empty;
    }
}
