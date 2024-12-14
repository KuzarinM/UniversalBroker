namespace UniversalBroker.Core.Models.Dtos.Communications
{
    /// <summary>
    /// Модель Соединения
    /// </summary>
    public class CommunicationDto : CreateCommunicationDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Статус Соединения
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Список аттрибутов
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new();
    }
}
