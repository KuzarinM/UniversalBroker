

using PIHelperSh.Configuration.Attributes;

namespace UniversalBroker.Core.Configurations
{
    /// <summary>
    /// Конфигурация адаптера
    /// </summary>
    [AutoConfiguration]
    public class AdapterConfiguration
    {
        /// <summary>
        /// Время жизни в секундах
        /// </summary>
        public double TimeToLiveSeconds { get; set; } = 20;
    }
}
