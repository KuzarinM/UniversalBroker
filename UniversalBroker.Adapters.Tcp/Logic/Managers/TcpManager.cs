using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using UniversalBroker.Adapters.Tcp.Configurations;
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
        private readonly ConcurrentDictionary<string, TcpClientModel> _tcpClients = new();

        private CancellationTokenSource _stopListeningTokenSource = new();

        public ConcurrentDictionary<Task<TcpClient>, TcpServerModel> GetTcpListeners => _tcpListeners;
        public ConcurrentDictionary<string, TcpServerModel> GetTcpServers => _tcpServers;
        public ConcurrentDictionary<string, TcpClientModel> GetTcpClients => _tcpClients;

        public async Task RestartListeners()
        {
            await _stopListeningTokenSource.CancelAsync();

            _stopListeningTokenSource = new();

            _= Task.Run(()=>StartServerListening());
        }

        public async Task<TcpClientService> StartService(TcpClient client, TcpConfiguration tcpConfiguration, string path, bool needRead = true )
        {
            var clientService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<TcpClientService>();

            await clientService.StartWork(client, tcpConfiguration, path, needRead);

            return clientService;
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

                var path = serverModel.InConnection?.Path ?? serverModel.OutConnection!.Path;

                var clientService = await StartService(tcpClientTask.Result, serverModel.TcpConfiguration, path, serverModel.InConnection != null);

                serverModel.Clients.Add(clientService);

                _tcpListeners.TryRemove(tcpClientTask, out _);

                var task = serverModel.TcpListener.AcceptTcpClientAsync();
                _tcpListeners.TryAdd(task, serverModel);
            }
        }
    }
}
