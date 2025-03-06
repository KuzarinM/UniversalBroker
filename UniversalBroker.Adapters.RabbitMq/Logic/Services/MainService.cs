using Google.Rpc;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Options;
using Protos;
using UniversalBroker.Adapters.RabbitMq.Configurations;
using UniversalBroker.Adapters.RabbitMq.Extentions;
using UniversalBroker.Adapters.RabbitMq.Models.Commands;
using static Protos.CoreService;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Services
{
    public class MainService(
        ILogger<MainService> logger,
        IMediator mediator,
        CoreServiceClient coreService,
        RabbitMqService rabbitMqService,
        IOptions<BaseConfiguration> options,
        IOptions<AdapterConfiguration> adapterConfig)
    {
        protected readonly ILogger _logger = logger;
        protected readonly IMediator _mediator = mediator;
        protected readonly CoreServiceClient _coreService = coreService;
        protected readonly RabbitMqService _rabbitMqService = rabbitMqService;
        protected readonly BaseConfiguration _baseConfig = options.Value;
        protected readonly AdapterConfiguration _adapterConfig = adapterConfig.Value;

        protected IClientStreamWriter<CoreMessage> _requestStream;
        protected IAsyncStreamReader<CoreMessage> _responseStream;

        private readonly SemaphoreSlim _processSemaphore = new SemaphoreSlim(0, 1);
        private CommunicationFullDto? _myCommunication;
        private DateTime _lastSendMessage = DateTime.UtcNow;
        private DateTime _lastReceivedMessage = DateTime.UtcNow;

        public TimeSpan SiliensInterval => DateTime.UtcNow.Subtract(_lastSendMessage > _lastReceivedMessage ? _lastReceivedMessage : _lastSendMessage);

        public CommunicationFullDto? Communication => _myCommunication;

        public async Task<SemaphoreSlim> StartWork(CancellationTokenSource CancellationTokenSource)
        {
            var streams = _coreService.Connect();

            _requestStream = streams.RequestStream;
            _responseStream = streams.ResponseStream;

            _ = Task.Run(() => ListenMessages(CancellationTokenSource.Token), CancellationTokenSource.Token);
            _ = Task.Run(() => StartStatusCheker(CancellationTokenSource.Token), CancellationTokenSource.Token);
            
            await SendInit(CancellationTokenSource.Token);

            _ = Task.Run(() => StartLifesignChecker(CancellationTokenSource), CancellationTokenSource.Token);

            return _processSemaphore;
        }

        private async Task StartStatusCheker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var withoutMessageS = DateTime.UtcNow.Subtract(_lastSendMessage).TotalSeconds;
                if (withoutMessageS > _adapterConfig.TimeToLiveSeconds)
                {
                    // Шлём сообщение
                    await SendMessage(new()
                    {
                        Status = new()
                        {
                            Status_ = true,
                            Data = "LIFESIGN_REQ"
                        }
                    },
                    cancellationToken);

                    withoutMessageS = 0;
                }
                // Если ещё не время, довайте подождём до момента, когда будет время
                await Task.Delay((int)double.Max(_adapterConfig.TimeToLiveSeconds - withoutMessageS, 0) * 1000 + 1);
            }
        }

        private async Task StartLifesignChecker(CancellationTokenSource CancellationTokenSource)
        {
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                if(SiliensInterval.TotalSeconds > (_adapterConfig.TimeToLiveSeconds * 1.25))
                {
                    await CancellationTokenSource.CancelAsync();
                    _processSemaphore.Release();
                    break;
                }

                await Task.Delay((int)_adapterConfig.TimeToLiveSeconds * 800, CancellationTokenSource.Token); // *0.8*1000 = * 800
            }
        }

        private async Task SendInit(CancellationToken cancellationToken)
        {
            await SendMessage(new CoreMessage()
            {
                Config = new()
                {
                    TypeIdentifier = _baseConfig.AdapterTypeId.ToString(),
                    Name = _baseConfig.AdapterName,
                    Description = _baseConfig.AdapterDescription
                }
            },
            cancellationToken);
        }

        private async Task ListenMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await foreach (var message in _responseStream.ReadAllAsync())
                {
                    _logger.LogDebug("Пришло сообщение от Ядра");

                    _lastReceivedMessage = DateTime.UtcNow;

                    try
                    {
                        switch (message.BodyCase)
                        {
                            case CoreMessage.BodyOneofCase.Status:
                                await HandleStatusMessage(message.Status, cancellationToken);
                                break;
                            case CoreMessage.BodyOneofCase.Config:
                                await HandleConfigMessage(message.Config, cancellationToken);
                                break;
                            case CoreMessage.BodyOneofCase.Connection:
                                await HandleConnectionMessage(message.Connection, cancellationToken);
                                break;
                            case CoreMessage.BodyOneofCase.Message:
                                await HandleDataMessage(message.Message, cancellationToken);
                                break;
                            default:
                                _logger.LogWarning("Сообщение прибыло вообще без данных. Это подозрительно");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка в процессе обработки сообщения.");

                        await SendMessage(new()
                        {
                            Status = new()
                            {
                                Status_ = false,
                                Data = "MESSAGE NOT HANDLED"
                            }
                        },
                        cancellationToken);
                    }
                }
            }
        }

        protected async Task HandleStatusMessage(Protos.Status statusMessage, CancellationToken cancellationToken)
        {

        }

        protected async Task HandleDataMessage(MessageDto dataMessage, CancellationToken cancellationToken)
        {
            await _mediator.Send(new PublishToTopicCommand()
            {
                Message = dataMessage,
            });
        }

        protected async Task HandleConnectionMessage(ConnectionDto connectionDto, CancellationToken cancellationToken)
        {
            if (connectionDto.IsInput)
            {
                await _mediator.Send(new SubscribeOnTopicCommand()
                {
                    Connection = connectionDto,
                });
            }
            else
            {
                _rabbitMqService.OutputConnections.AddOrUpdate(connectionDto.Path, connectionDto, (_, _) => connectionDto);
            }
        }

        protected async Task HandleConfigMessage(CommunicationFullDto communicationFullDto, CancellationToken cancellationToken)
        {
            _myCommunication = communicationFullDto;

            if(_baseConfig.AdapterName != _myCommunication.Name)
                _baseConfig.AdapterName = _myCommunication.Name;

            if(_baseConfig.AdapterDescription != _myCommunication.Description)
                _baseConfig.AdapterDescription = _myCommunication.Description;

            bool needUpdate = false;

            var updatedCount = _adapterConfig.SetValueFromAttributes(_myCommunication.Attributes);

            // todo тут можно понять, обновилось ли у нас что-то

            needUpdate = _adapterConfig.GetAttributesFromModel(_myCommunication.Attributes) > 0;

            // Пытаемся сконфигурировать RabbitMQ
            updatedCount = rabbitMqService.GetConnectionConfig.SetValueFromAttributes(_myCommunication.Attributes);

            if(updatedCount > 0)
            {
                // Если какие-то параметры уже есть, давайте коннектится
                await _rabbitMqService.ConnectAsync(cancellationToken);
            }

            // В любом случае, вдруг что-то там такое интересное забыли
            needUpdate |= _rabbitMqService.GetConnectionConfig.GetAttributesFromModel(_myCommunication.Attributes) > 0;

            if (needUpdate)
                await SendMessage(new()
                {
                    Config = _myCommunication
                },
                cancellationToken);
        }

        protected async Task<bool> GetOrUpdateAttribute(string attributeName, Func<BaseConfiguration,Task<string>> getter, Func<BaseConfiguration, string, Task> setter)
        {
            if (_myCommunication == null)
                return false;

            var Header = _myCommunication.Attributes.FirstOrDefault(x => x.Name == attributeName);
            if (Header == null)
            {
                _myCommunication.Attributes.Add(new AttributeDto()
                {
                    Name = attributeName,
                    Value = await getter(_baseConfig)
                });

                return true;
            }
            else if (Header.Value != await getter(_baseConfig))
            {
                await setter(_baseConfig, Header.Value);
            }

            return false;
        }
        
        /// <summary>
        ///  Отправка ядру сообщения по установленному каналу связи
        /// </summary>
        /// <param name="coreMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMessage(CoreMessage coreMessage, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Отправлено сообщение Ядру");
                await _requestStream.WriteAsync(coreMessage, cancellationToken);

                _lastSendMessage = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отправить сообщение");
            }
        }
    }
}
