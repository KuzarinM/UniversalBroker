using AutoMapper;
using Google.Protobuf.Collections;
using MediatR;
using Protos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using UniversalBroker.Adapters.RabbitMq.Extentions;
using UniversalBroker.Adapters.RabbitMq.Models.Commands;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Services
{
    public class RabbitMqService(
        ILogger<RabbitMqService> logger, 
        IMediator mediator, 
        IMapper mapper)
    {
        protected readonly ILogger _logger = logger;
        protected readonly IMediator _mediator = mediator;
        protected readonly IMapper _mapper = mapper;

        protected ConnectionFactory _connectionConfig = new ConnectionFactory()
        {
            HostName = "hostname",
            UserName = "username",
            Password = "password",
        };
        protected IConnection? _connection;

        public IConnection? GetConnection => _connection;
        public ConnectionFactory GetConnectionConfig => _connectionConfig;

        public readonly ConcurrentDictionary<string, ConnectionDto> InputConnections = new();
        public readonly ConcurrentDictionary<string, CancellationTokenSource> Consumers = new();

        public readonly ConcurrentDictionary<string, ConnectionDto> OutputConnections = new();

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            bool needResubscribe = false;

            if(_connection != null)
            {
                _connection.Dispose();

                foreach (var item in Consumers.Values)
                {
                    item.Cancel();
                }

                needResubscribe = true;
            }

            _connection = await _connectionConfig.CreateConnectionAsync(cancellationToken: cancellationToken);

            if (needResubscribe) 
            {
                foreach (var item in InputConnections.Values)
                {
                    await _mediator.Send(new SubscribeOnTopicCommand()
                    {
                        Connection = item
                    });
                }
            }
        }
    }
}
