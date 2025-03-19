using Protos;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Interfaces
{
    public interface IRabbitMqService
    {
        IConnection? GetConnection {  get; }

        ConnectionFactory GetConnectionConfig { get; }

        ConcurrentDictionary<string, ConnectionDto> InputConnections { get; }

        ConcurrentDictionary<string, (CancellationTokenSource, IChannel)> Consumers { get; }

        ConcurrentDictionary<string, ConnectionDto> OutputConnections { get; }

        Task ConnectAsync(CancellationToken cancellationToken);
    }
}
