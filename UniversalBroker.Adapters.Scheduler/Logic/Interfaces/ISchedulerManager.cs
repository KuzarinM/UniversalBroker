using System.Collections.Concurrent;
using UniversalBroker.Adapters.Scheduler.Models.Internal;

namespace UniversalBroker.Adapters.Scheduler.Logic.Interfaces
{
    public interface ISchedulerManager
    {
        public ConcurrentDictionary<string, SchedulerInstanceModel> GetActiveSchedulers { get; }
    }
}
