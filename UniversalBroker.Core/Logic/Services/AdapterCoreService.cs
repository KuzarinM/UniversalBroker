using AutoMapper;
using Google.Protobuf;
using Google.Rpc;
using Grpc.Core;
using MediatR;
using Protos;
using System.IO;
using System.Reflection.PortableExecutable;
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
        protected IAsyncStreamReader<CoreMessage> _requestStream;
        protected IServerStreamWriter<CoreMessage> _responseStream;
        protected Models.Dtos.Communications.CommunicationDto? _myCommunication;
        private DateTime _lastSendMessage = DateTime.UtcNow;
        private DateTime _lastReceivedMessage = DateTime.UtcNow;
       

        public TimeSpan SiliensInterval => DateTime.UtcNow.Subtract(_lastSendMessage > _lastReceivedMessage ? _lastReceivedMessage: _lastSendMessage ); 

        public async Task Stop()
        {
            _logger.LogInformation("Разрываем подключение к Адаптеру с Id {id}", _myCommunication?.Id.ToString() ?? "UNKNOWN");

            // Сообщаем, что всё, общение окончено
            await SendMessage(new()
            {
                Status = new()
                {
                    Status_ = false,
                    Data = "DISCONNECT"
                }
            },_cancellationTokenSource.Token);

            _cancellationTokenSource.Cancel();

            if (_myCommunication != null)
                await _manager.DisregisterAdapter(_myCommunication.Id);
        }

        public Task StartWork(IAsyncStreamReader<CoreMessage> requestStream, IServerStreamWriter<CoreMessage> responseStream)
        {
            _requestStream = requestStream;
            _responseStream = responseStream;

            // Запускаем слушателя
            _ = Task.Run(() => StartMessageListener(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            // Запускам проверку жизнеспособности
            _ = Task.Run(() => StartStatusCheker(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            return Task.CompletedTask;
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

        private async Task StartMessageListener(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested) 
            {
                await foreach (var message in _requestStream.ReadAllAsync())
                {
                    _logger.LogDebug("Пришло сообщение от Адаптера {id}", _myCommunication?.Id.ToString() ?? "UNKNOWN");

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
                await Task.Delay(int.Max(_manager.TimeToLiveS - withoutMessageS, 0) * 1000 + 1);
            }
        }

        protected async Task HandleStatusMessage(Protos.Status statusMessage, CancellationToken cancellationToken)
        {
            // TODO тут будет обработчик ошибок от адаптера
        }

        protected async Task HandleDataMessage(Protos.MessageDto dataMessage, CancellationToken cancellationToken)
        {
            if(_myCommunication == null)
            {
                _logger.LogWarning("Сообщение с данными не может прийти раньше конфига");

                await SendMessage(new()
                {
                    Status = new()
                    {
                        Status_ = false,
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
                    Status = new()
                    {
                        Status_ = false,
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
                //TODO обновление description

                var communicationSetAttribute = _mapper.Map<CommunicationSetAttributeCommand>(communicationFullDto);

                var _myCommunication = await _mediator.Send(communicationSetAttribute);
            }

            var res = _mapper.Map<CommunicationFullDto>(_myCommunication);

            // Отвечаем полной версией конфига
            await SendMessage(new()
            {
                Config = res,
            }, cancellationToken);

            // Регистрируемся как полноправный участник
            await _manager.RegisterNewAdapter(_myCommunication.Id, this);
        }

        /// <summary>
        ///  Отправка адаптеру сообщения по установленному каналу связи
        /// </summary>
        /// <param name="coreMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task SendMessage(Protos.CoreMessage coreMessage, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogDebug("Отправлено сообщение Адаптеру {id}", _myCommunication?.Id.ToString() ?? "UNKNOWN");
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
