using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Interfaces
{
    /// <summary>
    /// Сервис для записей логов в БД
    /// </summary>
    public interface IDbLogingService
    {
        /// <summary>
        /// Залогировать асинхронно сообщение
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task LogMessage(MessageLog log);

        /// <summary>
        /// Залогировать асинхронно лог работы скрипта
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task LogScriptExecution(ScriptExecutionLog log);

        /// <summary>
        /// Начать работу сервиса логирования, как Task - тоесть в отдельном потоке
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task StartLogging(CancellationToken cancellationToken);
    }
}
