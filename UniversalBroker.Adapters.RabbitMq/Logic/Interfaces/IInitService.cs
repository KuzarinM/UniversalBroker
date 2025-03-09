namespace UniversalBroker.Adapters.RabbitMq.Logic.Interfaces
{
    public interface IInitService: IHostedService
    {
        public IMainService? GetService {  get; }
    }
}
