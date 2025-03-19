using System.Collections.Concurrent;
using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;
using UniversalBroker.Adapters.Scheduler.Models.Internal;

namespace UniversalBroker.Adapters.Scheduler.Logic.Managers
{
    public class SchedulerManager(
        ILogger<SchedulerManager> logger
        ): ISchedulerManager
    {
        protected readonly ILogger _logger = logger;

        private readonly ConcurrentDictionary<string, SchedulerInstanceModel> _activeSchedulers = new();

        public ConcurrentDictionary<string, SchedulerInstanceModel> GetActiveSchedulers => _activeSchedulers;
    }
}
