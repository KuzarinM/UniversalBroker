using Google.Protobuf;
using Google.Protobuf.Collections;
using MediatR;
using Protos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UniversalBroker.Adapters.RabbitMq.Configurations;
using UniversalBroker.Adapters.RabbitMq.Extentions;
using UniversalBroker.Adapters.RabbitMq.Logic.Interfaces;
using UniversalBroker.Adapters.RabbitMq.Logic.Services;
using UniversalBroker.Adapters.RabbitMq.Models.Commands;
using static Google.Rpc.Context.AttributeContext.Types;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Handlers.Commands
{
    /// <summary>
    /// Подписываемся/переподписываемся на топик
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mediator"></param>
    /// <param name="rabbitMqService"></param>
    /// <param name="mainService"></param>
    public class SubscribeOnTopicCommandHandler(
        ILogger<SubscribeOnTopicCommandHandler> logger,
        IMediator mediator,
        IRabbitMqService rabbitMqService,
        IInitService initService) : IRequestHandler<SubscribeOnTopicCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
        private readonly IInitService _initService = initService;

        public async Task Handle(SubscribeOnTopicCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _rabbitMqService.InputConnections.AddOrUpdate(request.Connection.Path, request.Connection, (key, old)=>request.Connection);

                if(_rabbitMqService.Consumers.TryGetValue(request.Connection.Path, out var removedConcumer))
                {
                    removedConcumer.Item1.Cancel();
                    await removedConcumer.Item2.CloseAsync();
                }
                        

                // Создаём канал
                var connection = _rabbitMqService.GetConnection!;

                var tokenSource = new CancellationTokenSource();

                

                var channel = await connection.CreateChannelAsync(cancellationToken: tokenSource.Token);

                _rabbitMqService.Consumers.AddOrUpdate(request.Connection.Path, (tokenSource,channel), (key, oldT) => {
                    oldT.Item1.Cancel();
                    oldT.Item2.CloseAsync().Wait();
                    return (tokenSource, channel);
                });

                var queueConfig = request.Connection.Attributes.GetModelFromAttributes<QueueConfiguration>();

                if (queueConfig.NeedDeclare)
                {
                    await channel.QueueDeclareAsync(
                        request.Connection.Path,
                        queueConfig.Durable,
                        queueConfig.Exclusive,
                        queueConfig.AutoDelete,
                        passive: queueConfig.Passive,
                        noWait: queueConfig.NoWait,
                        cancellationToken: tokenSource.Token
                    );
                }

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    RepeatedField<AttributeDto> headers =
                    [
                        // Докидываем заголовков которые считаем верными
                        new AttributeDto()
                        {
                            Name = "Custom.ReceiveDateTimeUtc",
                            Value = DateTime.UtcNow.ToString()
                        },
                        new AttributeDto()
                        {
                            Name = "Custom.DataLenth",
                            Value = ea.Body.Length.ToString()
                        },
                    ];

                    try
                    {
                        if (_initService.GetService != null)
                            await _initService.GetService.SendMessage(new()
                            {
                                Message = new()
                                {
                                    Data = ByteString.CopyFrom(ea.Body.ToArray()),
                                    Headers = {headers},
                                    Path = request.Connection.Path
                                }
                            },
                        tokenSource.Token);

                        // Подтверждаем что мы сообщение отдали
                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                    catch (Exception ex) 
                    {
                        _logger.LogError(ex, "Ошибка при отправке");
                        request.Connection.Attributes.AddOrUpdateAttribute("Error", "Ошибка при попытке отправить сообщение из топика");
                        await SendInformationToCore(request.Connection, tokenSource.Token);
                    }
                };

                await channel.BasicConsumeAsync(
                    request.Connection.Path, 
                    autoAck: false, //Модель общения асинхронная, но нам нужно слать подтверждения, чтобы избежать проблем
                    consumer: consumer,
                    cancellationToken:cancellationToken);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при попытке добавить добавить подписку на топик");
                request.Connection.Attributes.AddOrUpdateAttribute("Error", "Ошибка при попытке добавить добавить подписку на топик");

                await SendInformationToCore(request.Connection, cancellationToken);
            }
        }

        private async Task SendInformationToCore(ConnectionDto connectionDto, CancellationToken cancellationToken)
        {
            try
            {
                await _initService.GetService!.SendMessage(new()
                {
                    Connection = connectionDto
                }, cancellationToken);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Вот тут тупик");
            }
        }
    }
}
