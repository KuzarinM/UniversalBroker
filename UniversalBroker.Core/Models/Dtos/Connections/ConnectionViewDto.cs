namespace UniversalBroker.Core.Models.Dtos.Connections
{
    /// <summary>
    /// Dto Подключения
    /// </summary>
    public class ConnectionViewDto: CreateConnectionDto
    {
        /// <summary>
        /// Id
        /// </summary>
        public Guid Id { get; set; }
    }
}
