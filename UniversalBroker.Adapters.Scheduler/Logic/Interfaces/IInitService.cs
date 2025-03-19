namespace UniversalBroker.Adapters.Scheduler.Logic.Interfaces
{
    public interface IInitService : IHostedService
    {
        public IMainService? GetService { get; }
    }
}
