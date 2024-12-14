namespace UniversalBroker.Core.Models.Dtos.Connections
{
    /// <summary>
    /// Dto для создения Подключения
    /// </summary>
    public class CreateConnectionDto : UpdateConnectionDto
    {
        /// <summary>
        /// Id Соединения
        /// </summary>
        public Guid CommunicationId { get; set; }

        /// <summary>
        /// Выходное подключения или входное
        /// </summary>
        public bool IsInput { get; set; }
    }
}
