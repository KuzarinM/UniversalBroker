namespace UniversalBroker.Adapters.Tcp.Logic.Interfaces
{
    public interface IInitService : IHostedService
    {
        public IMainService? GetService { get; }
    }
}
