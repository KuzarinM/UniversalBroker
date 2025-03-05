using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Services;

namespace UniversalBroker.Core.Logic.Abstracts
{
    public abstract class AbstractAdaptersManager : BackgroundService
    {
        /// <summary>
        /// Время которое Адаптер может не отвечать и это нормально
        /// </summary>
        public virtual int TimeToLiveS { get; }

        /// <summary>
        /// Создание нового экземпляра сервиса
        /// </summary>
        public virtual IAdapterCoreService CreateService { get; }

        /// <summary>
        /// Регистрируемся как новые адаптер
        /// </summary>
        /// <param name="communicationId"></param>
        /// <param name="coreService"></param>
        /// <returns></returns>
        public abstract Task RegisterNewAdapter(Guid communicationId, IAdapterCoreService coreService);

        /// <summary>
        /// Дерегистрируемся как адаптер 
        /// </summary>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        public abstract Task DisregisterAdapter(Guid communicationId);

        /// <summary>
        /// Получаем адаптер по Id Соединения
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract IAdapterCoreService? GetAdapterById(Guid id);
    }
}
