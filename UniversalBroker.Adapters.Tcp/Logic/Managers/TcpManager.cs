using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Logic.Services;
using UniversalBroker.Adapters.Tcp.Models.Internal;

namespace UniversalBroker.Adapters.Tcp.Logic.Managers
{
    public class TcpManager(
        ILogger<TcpManager> logger,
        IServiceProvider serviceProvider
        )
    {
        private readonly ILogger _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        private readonly ConcurrentDictionary<Task<TcpClient>, TcpServerModel> _tcpListeners = new();
        private readonly ConcurrentDictionary<string, TcpServerModel> _tcpServers = new();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeServers;

        private CancellationTokenSource _stopListeningTokenSource = new();

        public ConcurrentDictionary<Task<TcpClient>, TcpServerModel> GetTcpListeners => _tcpListeners;
        public ConcurrentDictionary<string, TcpServerModel> GetTcpServers => _tcpServers;

        public async Task RestartListeners()
        {
            await _stopListeningTokenSource.CancelAsync();

            _stopListeningTokenSource = new();

            await StartServerListening();
        }

        private async Task StartServerListening()
        {
            var cancellationTask = Task.Run(() => {
                _stopListeningTokenSource.Token.WaitHandle.WaitOne();
                return (TcpClient?)null;
            });

            while (await Task.WhenAny(_tcpListeners.Keys.Append(cancellationTask!)) is { } tcpClientTask)
            {
                if (_stopListeningTokenSource.IsCancellationRequested)
                {
                    _logger.LogInformation("Была запрошена остановка прослушивания клиентов");
                    return;
                }

                var serverModel = _tcpListeners[tcpClientTask];

                var clientService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<TcpClientService>();

                serverModel.Clients.Add(clientService);

                await clientService.StartWork(tcpClientTask.Result, serverModel.TcpConfiguration, serverModel.Connection.Path);

                _tcpListeners.TryRemove(tcpClientTask, out _);

                var task = serverModel.TcpListener.AcceptTcpClientAsync();
                _tcpListeners.TryAdd(task, serverModel);
            }
        }
    }
}
