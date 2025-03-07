
using UniversalBroker.Adapters.RabbitMq.Logic.Interfaces;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Services
{
    public class InitService(
        ILogger<InitService> logger,
        IServiceProvider serviceProvider
        ) : IHostedService
    {
        protected readonly ILogger<InitService> _logger = logger;
        protected readonly IServiceProvider _serviceProvider = serviceProvider;

        protected CancellationTokenSource _cancellationTokenSource = new();
        protected IMainService? _mainService;

        public IMainService? GetService => _mainService;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run( async() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new();

                    _mainService = _serviceProvider.CreateAsyncScope().ServiceProvider.GetRequiredService<IMainService>();

                    var waiter = await _mainService.StartWork(_cancellationTokenSource);

                    await waiter.WaitAsync(cancellationToken);

                    _mainService = null;
                }

                _cancellationTokenSource?.Cancel();
            }, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            _mainService = null;
            return Task.CompletedTask;
        }
    }
}
