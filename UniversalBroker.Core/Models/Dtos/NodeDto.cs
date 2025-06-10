namespace UniversalBroker.Core.Models.Dtos
{
    /// <summary>
    /// Модель узла, то есть канала или подключения
    /// </summary>
    public class NodeDto
    {
        /// <summary>
        /// Id объекта
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Имя объекта
        /// </summary>
        public string ObjectName { get; set; }

        /// <summary>
        /// Список выходов данного узла
        /// </summary>
        public List<RelationDto> OutputIds { get; set; } = [];

        /// <summary>
        /// Является ли данный узел каналом
        /// </summary>
        public bool IsChanel { get; set; }
    }
}
