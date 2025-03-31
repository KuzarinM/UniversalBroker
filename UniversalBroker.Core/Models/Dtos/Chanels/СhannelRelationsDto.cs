namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Ответ по реальным связям
    /// </summary>
    public class СhannelRelationsDto
    {
        /// <summary>
        /// Список связей
        /// </summary>
        public List<СhanelRelationDto> Relations { get; set; } = new();

        /// <summary>
        /// Идентификатор канала
        /// </summary>
        public Guid ChanelId { get; set; }

        /// <summary>
        /// Имя канала
        /// </summary>
        public string ChanelName { get; set; } = string.Empty;
    }
}
