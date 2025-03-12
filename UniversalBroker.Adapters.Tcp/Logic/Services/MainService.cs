using Grpc.Core;
using MediatR;
using Microsoft.Extensions.Options;
using Protos;
using UniversalBroker.Adapters.Tcp.Configurations;
using UniversalBroker.Adapters.Tcp.Extentions;
using UniversalBroker.Adapters.Tcp.Logic.Interfaces;
using UniversalBroker.Adapters.Tcp.Models.Commands;
using static Protos.CoreService;

namespace UniversalBroker.Adapters.Tcp.Logic.Services
{
    public class MainService(
        ILogger<MainService> logger,
        IMediator mediator,
        CoreServiceClient coreService,
        IOptions<BaseConfiguration> options,
        IOptions<AdapterConfiguration> adapterConfig)
        : IMainService
    {
        protected readonly ILogger _logger = logger;
        protected readonly IMediator _mediator = mediator;
        protected readonly CoreServiceClient _coreService = coreService;
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

            await HandleConfigMessage(_myCommunication, CancellationTokenSource.Token);

            _ = Task.Run(() => ListenMessages(CancellationTokenSource.Token), CancellationTokenSource.Token);
            _ = Task.Run(() => StartStatusCheker(CancellationTokenSource.Token), CancellationTokenSource.Token);
            _ = Task.Run(() => StartLifesignChecker(CancellationTokenSource), CancellationTokenSource.Token);

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
                if (SiliensInterval.TotalSeconds > _adapterConfig.TimeToLiveSeconds * 1.25)
                {
                    _logger.LogWarning("Тест жизнеспособности провален, отключаемся");

                    if (_myCommunication != null)
                    {
                        try
                        {
                            await _coreService.DisconnectAsync(new()
                            {
                                Id = _myCommunication.Id
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Ошибка при дисконнекте");
                        }
                    }

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
                            case CoreMessage.BodyOneofCase.DeletedConnection:
                                await HandleDeleteConnectionMessage(message.DeletedConnection, cancellationToken);
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

        protected async Task HandleStatusMessage(StatusDto statusMessage, CancellationToken cancellationToken)
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
            var res = await _mediator.Send(new SendMessageCommand()
            {
                Message = dataMessage,
            });
        }

        protected async Task HandleConnectionMessage(ConnectionDto connectionDto, CancellationToken cancellationToken)
        {
            var tcpConfig = connectionDto.Attributes.GetModelFromAttributes<TcpConfiguration>();

            if (tcpConfig.IsClient)
            {
                var res = await _mediator.Send(new AddOrUpdateClientCommand()
                {
                    ConnectionDto = connectionDto,
                });
            }
            else
            {
                var res = await _mediator.Send(new AddOrUpdateServerCommand()
                {
                    ConnectionDto = connectionDto,
                });
            }
        }

        protected async Task HandleConfigMessage(CommunicationFullDto communicationFullDto, CancellationToken cancellationToken)
        {
            _myCommunication = communicationFullDto;

            if (_baseConfig.AdapterName != _myCommunication.Name)
                _baseConfig.AdapterName = _myCommunication.Name;

            if (_baseConfig.AdapterDescription != _myCommunication.Description)
                _baseConfig.AdapterDescription = _myCommunication.Description;

            bool needUpdate = false;

            var updatedCount = _adapterConfig.SetValueFromAttributes(_myCommunication.Attributes);

            // todo тут можно понять, обновилось ли у нас что-то

            needUpdate = _adapterConfig.GetAttributesFromModel(_myCommunication.Attributes, false) > 0;

            if (needUpdate)
                await SendMessage(new()
                {
                    Config = _myCommunication
                },
                cancellationToken);
        }

        protected async Task HandleDeleteConnectionMessage(ConnectionDeleteDto connectionDeleteDto, CancellationToken cancellationToken)
        {
            var res = _mediator.Send(new RemoveClientCommand()
            {
                ConnectionId = connectionDeleteDto.Id,
                Path = connectionDeleteDto.Path,
                IsInput = connectionDeleteDto.IsInput,
            });

            res = _mediator.Send(new RemoveServerCommand()
            {
                ConnectionId = connectionDeleteDto.Id,
                Path = connectionDeleteDto.Path,
                IsInput = connectionDeleteDto.IsInput,
            });
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

                StatusDto? res = await _coreService.SendAdapterMessageAsync(new()
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
