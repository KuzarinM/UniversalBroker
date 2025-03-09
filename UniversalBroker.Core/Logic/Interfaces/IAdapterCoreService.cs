using Grpc.Core;
using Protos;
using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Interfaces
{
    /// <summary>
    /// Сервис отвечает за конкретный адаптер
    /// </summary>
    public interface IAdapterCoreService
    {
        /// <summary>
        /// Интервал тишены, после истечения которого Адаптер считается не зоровым
        /// </summary>
        TimeSpan SiliensInterval { get; }

        /// <summary>
        /// Остановка адаптера и его дерегистрация
        /// </summary>
        /// <returns></returns>
        Task Stop();

        /// <summary>
        /// Начало работы адаптера
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        Task StartWork(Models.Dtos.Communications.CommunicationDto communication);

        /// <summary>
        /// Передать обратный вызов
        /// </summary>
        /// <param name="responseStream"></param>
        /// <returns></returns>
        Task<SemaphoreSlim> ConnectAdapter(IServerStreamWriter<CoreMessage> responseStream);

        /// <summary>
        /// Отправка Адаптеру сообщения в Канал
        /// </summary>
        /// <param name="message"></param>
        /// <param name="Path"></param>
        /// <returns></returns>
        Task SendMessageToPath(InternalMessage message, string Path);

        /// <summary>
        /// Принять и обработать сообщение от адаптера
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<Protos.StatusDto> ReceiveMessage(CoreMessage message, CancellationToken cancellationToken);

        /// <summary>
        ///  Отправка адаптеру сообщения по установленному каналу связи
        /// </summary>
        /// <param name="coreMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendMessage(Protos.CoreMessage coreMessage, CancellationToken cancellationToken);
    }
}
