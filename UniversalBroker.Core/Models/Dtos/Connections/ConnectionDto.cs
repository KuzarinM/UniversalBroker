namespace UniversalBroker.Core.Models.Dtos.Connections
{
    /// <summary>
    /// Dto Подключения
    /// </summary>
    public class ConnectionDto: CreateConnectionDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }
    }
}
