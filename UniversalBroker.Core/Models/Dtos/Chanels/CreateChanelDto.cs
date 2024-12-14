
namespace UniversalBroker.Core.Models.Dtos.Chanels
{
    /// <summary>
    /// Dto для изменения канала
    /// </summary>
    public class CreateChanelDto
    {
        /// <summary>
        /// Имя канала
        /// </summary>
        public string Name {  get; set; } = string.Empty;

        /// <summary>
        /// Текст скрипта
        /// </summary>
        public string Script { get; set; } = string.Empty;

        /// <summary>
        /// Id входных подключений
        /// </summary>
        public List<Guid> InputConnections { get; set; } = new();

        /// <summary>
        /// Id выходных подключений
        /// </summary>
        public List<Guid> OutputConnections { get; set; } = new();

        /// <summary>
        /// Id выходных каналов
        /// </summary>
        public List<Guid> OutputChanels { get; set; } = new();
    }
}
