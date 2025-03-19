namespace UniversalBroker.Core.Models.Dtos.Connections
{
    /// <summary>
    /// Dto для обновления Подключения
    /// </summary>
    public class UpdateConnectionDto
    {
        /// <summary>
        ///  Имя подключения
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Путь
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Аттрибуты
        /// </summary>
        public Dictionary<string, string> Attribues { get; set; } = new();

        public List<Guid> ChannelsIds { get; set; } = new();
    }
}
