using UniversalBroker.Core.Models.Internals;

namespace UniversalBroker.Core.Logic.Abstracts
{
    /// <summary>
    /// Сервис для записей логов в БД
    /// </summary>
    public abstract class AbstractDbLogingService : BackgroundService
    {
        /// <summary>
        /// Залогировать асинхронно сообщение
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public abstract Task LogMessage(MessageLog log);

        /// <summary>
        /// Залогировать асинхронно лог работы скрипта
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public abstract Task LogScriptExecution(ScriptExecutionLog log);
    }
}
