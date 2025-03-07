using Google.Protobuf;
using Google.Protobuf.Collections;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Protos;
using RabbitMQ.Client;
using UniversalBroker.Adapters.RabbitMq.Configurations;
using UniversalBroker.Adapters.RabbitMq.Extentions;
using UniversalBroker.Adapters.RabbitMq.Logic.Interfaces;
using UniversalBroker.Adapters.RabbitMq.Logic.Services;
using UniversalBroker.Adapters.RabbitMq.Models.Commands;
using static Google.Rpc.Context.AttributeContext.Types;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Handlers.Commands
{
    /// <summary>
    /// Отправка сообщения в топик или в эксченжер
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="mediator"></param>
    /// <param name="rabbitMqService"></param>
    /// <param name="mainService"></param>
    public class PublishToTopicCommandHandler(
        ILogger<PublishToTopicCommandHandler> logger, 
        IMediator mediator, 
        IRabbitMqService rabbitMqService, 
        IMainService mainService
        ) : IRequestHandler<PublishToTopicCommand>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly IRabbitMqService _rabbitMqService = rabbitMqService;
        private readonly IMainService _mainService = mainService;

        public async Task Handle(PublishToTopicCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Создаём канал
                var connection = _rabbitMqService.GetConnection!;

                using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

                // Получаем настройки Подключения, если таковые были
                _rabbitMqService.OutputConnections.TryGetValue(request.Message.Path, out var ConnectionDto);

                var publishConfig = CreateConfig<PublishConfiguration>(ConnectionDto, request.Message.Headers);

                if (publishConfig.UseExchenge)
                {

                    var exchangeConfig = CreateConfig<ExchangeConfig>(ConnectionDto, request.Message.Headers);

                    if (exchangeConfig.NeedDeclare)
                    {
                        await channel.ExchangeDeclareAsync(
                            request.Message.Path,
                            exchangeConfig.Type,
                            exchangeConfig.Durable,
                            exchangeConfig.AutoDelete,
                            passive: exchangeConfig.Passive,
                            noWait: exchangeConfig.NoWait,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                else
                {
                    var queueConfig = CreateConfig<QueueConfiguration>(ConnectionDto, request.Message.Headers);

                    if (queueConfig.NeedDeclare)
                    {
                        await channel.QueueDeclareAsync(
                            request.Message.Path,
                            queueConfig.Durable,
                            queueConfig.Exclusive,
                            queueConfig.AutoDelete,
                            passive: queueConfig.Passive,
                            noWait: queueConfig.NoWait,
                            cancellationToken: cancellationToken
                        );
                    }
                }

                await channel.BasicPublishAsync(
                    exchange: publishConfig.UseExchenge ? request.Message.Path : string.Empty, 
                    routingKey: publishConfig.UseExchenge ? string.Empty : request.Message.Path,
                    mandatory: publishConfig.Mandatory,
                    body:request.Message.Data.ToArray(),
                    cancellationToken: cancellationToken
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке отправить сообщение в топик");

                await _mainService.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "MESSAGE SEND ERROR",
                    }
                },
                cancellationToken);

                // Идём на 2-ой заход
                _ = Task.Run(async() => await _mediator.Send(request));
            }
        }

        private T CreateConfig<T>(ConnectionDto? connection, RepeatedField<AttributeDto> headers) where T : class
        {
            var config = connection?.Attributes.GetModelFromAttributes<T>();
            if (config == null)
                config = headers.GetModelFromAttributes<T>();
            else
                config.SetValueFromAttributes(headers);

            return config;
        }
    }
}
