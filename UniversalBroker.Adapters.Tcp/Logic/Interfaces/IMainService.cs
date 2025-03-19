using Protos;

namespace UniversalBroker.Adapters.Tcp.Logic.Interfaces
{
    /// <summary>
    /// Основной сервис для работы с RabbitMQ
    /// </summary>
    public interface IMainService
    {
        /// <summary>
        /// Объект того Communication, которым явялется данный адаптер
        /// </summary>
        CommunicationFullDto? Communication { get; }

        /// <summary>
        /// Метод начала работы
        /// </summary>
        /// <param name="CancellationTokenSource"></param>
        /// <returns></returns>
        Task<SemaphoreSlim> StartWork(CancellationTokenSource CancellationTokenSource);

        /// <summary>
        /// Отправка сообщения ядру
        /// </summary>
        /// <param name="coreMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendMessage(CoreMessage coreMessage, CancellationToken cancellationToken);
    }
}
