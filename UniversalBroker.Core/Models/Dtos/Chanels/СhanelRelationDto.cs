using UniversalBroker.Core.Models.Enums;

namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Модель одной связи
    /// </summary>
    public class СhanelRelationDto
    {
        /// <summary>
        /// Id объекта, с которым есть связь
        /// </summary>
        public Guid RelationId { get; set; }

        /// <summary>
        /// Имя объекта с которым есть связь
        /// </summary>
        public string RelationName { get; set; }

        /// <summary>
        /// Направление передачи данных 
        /// </summary>
        public MessageDirection Direction { get; set; }

        /// <summary>
        /// Статус использования данной связи
        /// </summary>
        public RelationUsageStatus Status { get; set; }

    }
}
