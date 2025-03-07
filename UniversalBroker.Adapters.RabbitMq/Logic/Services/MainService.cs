using Google.Rpc;
using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Options;
using Protos;
using System.Threading;
using UniversalBroker.Adapters.RabbitMq.Configurations;
using UniversalBroker.Adapters.RabbitMq.Extentions;
using UniversalBroker.Adapters.RabbitMq.Logic.Interfaces;
using UniversalBroker.Adapters.RabbitMq.Models.Commands;
using static Protos.CoreService;

namespace UniversalBroker.Adapters.RabbitMq.Logic.Services
{
    public class MainService(
        ILogger<MainService> logger,
        IMediator mediator,
        CoreServiceClient coreService,
        IRabbitMqService rabbitMqService,
        IOptions<BaseConfiguration> options,
        IOptions<AdapterConfiguration> adapterConfig) : IMainService
    {
        protected readonly ILogger _logger = logger;
        protected readonly IMediator _mediator = mediator;
        protected readonly CoreServiceClient _coreService = coreService;
        protected readonly IRabbitMqService _rabbitMqService = rabbitMqService;
        protected readonly BaseConfiguration _baseConfig = options.Value;
        protected readonly AdapterConfiguration _adapterConfig = adapterConfig.Value;

        protected IAsyncStreamReader<CoreMessage> _responseStream;

        private readonly SemaphoreSlim _processSemaphore = new SemaphoreSlim(0, 1);
        private CommunicationFullDto? _myCommunication;
        private DateTime _lastSendMessage = DateTime.UtcNow;
        private DateTime _lastReceivedMessage = DateTime.UtcNow;

        public CommunicationFullDto? Communication => _myCommunication;

        private TimeSpan SiliensInterval => DateTime.UtcNow.Subtract(_lastSendMessage > _lastReceivedMessage ? _lastReceivedMessage : _lastSendMessage);

        public async Task<SemaphoreSlim> StartWork(CancellationTokenSource CancellationTokenSource)
        {

            await SendInit(CancellationTokenSource.Token);

            var streams = _coreService.Connect(
                new()
                {
                    Id = _myCommunication!.Id
                }
            );

            _responseStream = streams.ResponseStream;

            _ = Task.Run(() => ListenMessages(CancellationTokenSource.Token), CancellationTokenSource.Token);
            _ = Task.Run(() => StartStatusCheker(CancellationTokenSource.Token), CancellationTokenSource.Token);
            _ = Task.Run(() => StartLifesignChecker(CancellationTokenSource), CancellationTokenSource.Token);

            await HandleConfigMessage(_myCommunication, CancellationTokenSource.Token);

            await LoadConnections(CancellationTokenSource.Token);

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
                        StatusDto = new()
                        {
                            Status = true,
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
                    _logger.LogWarning("Тест жизнеспособности провален, отключаемся");
                    await CancellationTokenSource.CancelAsync();
                    _processSemaphore.Release();
                    break;
                }

                await Task.Delay((int)_adapterConfig.TimeToLiveSeconds * 800, CancellationTokenSource.Token); // *0.8*1000 = * 800
            }
        }

        private async Task SendInit(CancellationToken cancellationToken)
        {
            _myCommunication = await _coreService.InitAsync(new CommunicationDto
            {
                TypeIdentifier = _baseConfig.AdapterTypeId.ToString(),
                Name = _baseConfig.AdapterName,
                Description = _baseConfig.AdapterDescription
            });
        }

        private async Task ListenMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (await _responseStream.MoveNext())
                {
                    var message = _responseStream.Current;

                    _logger.LogInformation("Пришло сообщение от Ядра");

                    _lastReceivedMessage = DateTime.UtcNow;

                    try
                    {
                        switch (message.BodyCase)
                        {
                            case CoreMessage.BodyOneofCase.StatusDto:
                                await HandleStatusMessage(message.StatusDto, cancellationToken);
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
                            StatusDto = new()
                            {
                                Status = false,
                                Data = "MESSAGE NOT HANDLED"
                            }
                        },
                        cancellationToken);
                    }
                }
            }
        }

        protected async Task HandleStatusMessage(Protos.StatusDto statusMessage, CancellationToken cancellationToken)
        {
            if (statusMessage.Status && statusMessage.Data.Contains("LIFESIGN_REQ"))
            {
                await SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = true,
                        Data = "LIFESIGN_RES"
                    }
                },
                cancellationToken);
            }
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
            updatedCount = _rabbitMqService.GetConnectionConfig.SetValueFromAttributes(_myCommunication.Attributes);

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

        protected async Task LoadConnections(CancellationToken cancellationToken)
        {
            var inputConnections = await _coreService.LoadInConnectionsAsync(new()
            {
                Id = _myCommunication!.Id
            });

            foreach (var connection in inputConnections.Connections)
            {
                await HandleConnectionMessage(connection, cancellationToken);
            }

            await SendMessage(new()
            {
                StatusDto = new()
                {
                    Status = true,
                    Data = "IN CONNECTIONS LOADED"
                }
            },
            cancellationToken);

            var outputConnections = await _coreService.LoadOutConnectionsAsync(new()
            {
                Id = _myCommunication!.Id
            });

            foreach (var connection in outputConnections.Connections)
            {
                await HandleConnectionMessage(connection, cancellationToken);
            }

            await SendMessage(new()
            {
                StatusDto = new()
                {
                    Status = true,
                    Data = "OUT CONNECTIONS LOADED"
                }
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
                logger.LogInformation("Отправлено сообщение Ядру: {message}", coreMessage);

                Protos.StatusDto? res = await _coreService.SendAdapterMessageAsync(new()
                {
                    AdapterId = _myCommunication?.Id.ToString() ?? string.Empty,
                    Message = coreMessage,
                });
                _lastSendMessage = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Не удалось отправить сообщение");
            }
        }
    }
}
