using AutoMapper;
using Google.Protobuf;
using Google.Rpc;
using Grpc.Core;
using MediatR;
using Protos;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Managers;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Internals;
using static Google.Rpc.Context.AttributeContext.Types;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace UniversalBroker.Core.Logic.Services
{
    public class AdapterCoreService(
        ILogger<AdapterCoreService> logger, 
        IMediator mediator, 
        IMapper mapper, 
        AbstractAdaptersManager manager): IAdapterCoreService
    {
        protected readonly ILogger _logger = logger;
        protected readonly IMediator _mediator = mediator;
        protected readonly IMapper _mapper = mapper;
        protected readonly AbstractAdaptersManager _manager = manager;
        
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        protected IServerStreamWriter<CoreMessage> _responseStream;
        protected Models.Dtos.Communications.CommunicationDto? _myCommunication;
        private DateTime _lastSendMessage = DateTime.UtcNow;
        private DateTime _lastReceivedMessage = DateTime.UtcNow;
        private SemaphoreSlim _workingSemaphore = new SemaphoreSlim(0, 1);

        public TimeSpan SiliensInterval => DateTime.UtcNow.Subtract(_lastSendMessage > _lastReceivedMessage ? _lastReceivedMessage: _lastSendMessage ); 

        public async Task Stop()
        {
            _logger.LogInformation("Разрываем подключение к Адаптеру с Id {id}", _myCommunication?.Id.ToString() ?? "UNKNOWN");

            // Сообщаем, что всё, общение окончено
            await SendMessage(new()
            {
                StatusDto = new()
                {
                    Status = false,
                    Data = "DISCONNECT"
                }
            },_cancellationTokenSource.Token);

            _cancellationTokenSource.Cancel();
            _workingSemaphore.Release();

            if (_myCommunication != null)
                await _manager.DisregisterAdapter(_myCommunication.Id);
        }

        public Task StartWork(Models.Dtos.Communications.CommunicationDto communication)
        {
            _myCommunication = communication;

            // Запускам проверку жизнеспособности
            _ = Task.Run(() => StartStatusCheker(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            return Task.CompletedTask;
        }

        public Task<SemaphoreSlim> ConnectAdapter(IServerStreamWriter<CoreMessage> responseStream)
        {
            _responseStream = responseStream;

            return Task.FromResult(_workingSemaphore);
        }

        public async Task SendMessageToPath(InternalMessage message, string Path)
        {
            await SendMessage(new()
            {
                Message = new()
                {
                    Data = ByteString.CopyFrom(message.Data.ToArray()),
                    Headers = {message.Headers.Select(x => new AttributeDto()
                    {
                        Name = x.Key,
                        Value = x.Value,
                    }) },
                    Path = Path
                }
            },
            _cancellationTokenSource.Token);
        }

        public async Task<Protos.StatusDto> ReceiveMessage(CoreMessage message, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Пришло сообщение от Адаптера {id}", _myCommunication.Id);

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

                return new()
                {
                    Status = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в процессе обработки сообщения.");

                return new()
                {
                    Status = false,
                    Data = "MESSAGE NOT HANDLED"
                };
            }
        }

        private async Task StartStatusCheker(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var withoutMessageS = (int)DateTime.UtcNow.Subtract(_lastSendMessage).TotalSeconds;
                if (withoutMessageS > _manager.TimeToLiveS)
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
                await Task.Delay(int.Max(_manager.TimeToLiveS - withoutMessageS, 0) * 1000 + 1);
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

        protected async Task HandleDataMessage(Protos.MessageDto dataMessage, CancellationToken cancellationToken)
        {
            if(_myCommunication == null)
            {
                _logger.LogWarning("Сообщение с данными не может прийти раньше конфига");

                await SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "UNKNOWN COMMUNICATION"
                    }
                }, cancellationToken);
                return;
            }

            var model = new ReceiveIncommingMessageCommand()
            {
                Path = dataMessage.Path,
                Headers = dataMessage.Headers.ToDictionary(x => x.Name, x => x.Value),
                Data = dataMessage.Data.ToList(),
                CommunicationId = _myCommunication.Id
            };

            // Отправялем сообщение на обработку в полностью асинхронном режиме
            _ = _mediator.Send(model);
        }

        protected async Task HandleConnectionMessage(Protos.ConnectionDto connectionDto, CancellationToken cancellationToken)
        {
            if (_myCommunication == null)
            {
                _logger.LogWarning("Сообщение с подключениями не может прийти раньше конфига");

                await SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "UNKNOWN COMMUNICATION"
                    }
                }, cancellationToken);
                return;
            }

            // Создавать подключения может ТОЛЬКО пользователь. Следовательно, тут только обновление

            var upateConnectionDto = _mapper.Map<UpdateConnectionDto>(connectionDto);

            var res = await _mediator.Send(new UpdateConnectionCommand()
            {
                ConnectionId = _myCommunication.Id,
                UpdateDto = upateConnectionDto,
            });

            var resDto = _mapper.Map<Protos.ConnectionDto>(res);

            await SendMessage(new()
            {
                Connection = resDto,
            }, cancellationToken);
        }

        protected async Task HandleConfigMessage(Protos.CommunicationFullDto communicationFullDto, CancellationToken cancellationToken)
        {
            if (_myCommunication == null) 
            {
                var communicationCreateDto = _mapper.Map<CreateCommunicationDto>(communicationFullDto);

                var _myCommunication = await _mediator.Send(new AddOrUpdateCommunicationCommand()
                {
                    CreateCommunicationDto = communicationCreateDto,
                });
            }
            else
            {
                var communicationSetAttribute = _mapper.Map<CommunicationSetAttributeCommand>(communicationFullDto);

                _myCommunication = await _mediator.Send(communicationSetAttribute);
            }
        }

        public async Task SendMessage(Protos.CoreMessage coreMessage, CancellationToken cancellationToken)
        {
            if(_responseStream == null)
            {
                _cancellationTokenSource.Cancel();

                if (_myCommunication != null)
                    await _manager.DisregisterAdapter(_myCommunication.Id);

                _workingSemaphore.Release();

                return;
            }

            try
            {
                logger.LogInformation("Отправлено сообщение Адаптеру {id}:{message}", _myCommunication?.Id.ToString() ?? "UNKNOWN", coreMessage);
                await _responseStream.WriteAsync(coreMessage, cancellationToken);

                _lastSendMessage = DateTime.UtcNow;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Не удалось отправить сообщение");
            }
        }
    }
}
