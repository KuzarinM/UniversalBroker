namespace UniversalBroker.Core.Models.Dtos.Communications
{
    /// <summary>
    /// Dto для создения Соединения
    /// </summary>
    public class CreateCommunicationDto
    {
        /// <summary>
        /// Имя Соединения
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Идентификатор типа
        /// </summary>
        public Guid TypeIdentifier { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string? Description { get; set; }
    }
}
