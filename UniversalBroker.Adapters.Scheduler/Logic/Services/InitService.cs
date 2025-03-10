
using Microsoft.Extensions.Options;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UniversalBroker.Adapters.Scheduler.Configurations;
using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;

namespace UniversalBroker.Adapters.Scheduler.Logic.Services
{
    public class InitService(
        ILogger<InitService> logger,
        IServiceProvider serviceProvider
        ) : IInitService
    {
        protected readonly ILogger _logger = logger;
        protected readonly IServiceProvider _serviceProvider = serviceProvider;

        protected CancellationTokenSource _cancellationTokenSource = new();
        protected IMainService? _mainService;

        public IMainService? GetService => _mainService;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () =>
            {
                var serverHost = _serviceProvider.GetService<IOptions<BaseConfiguration>>();

                while (!cancellationToken.IsCancellationRequested)
                {
                    if (PingHost(serverHost.Value.CoreBaseUrl))
                        break;
                    await Task.Delay(200);
                }

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

        private bool PingHost(string nameOrAddress)
        {
            try
            {
                var rawAddres = nameOrAddress.Split("://").Last().Split("/").First();

                var parts = rawAddres.Split(":");

                var host = parts[0];

                var portStr = parts.Length == 1 ? nameOrAddress.StartsWith("https") ? "443" : "80" : parts[1];

                var port = int.TryParse(portStr, out var value) ? value : 80;

                using (var client = new TcpClient(host, port))
                    return true;
            }
            catch (SocketException ex)
            {
                return false;
            }
        }
    }
}
