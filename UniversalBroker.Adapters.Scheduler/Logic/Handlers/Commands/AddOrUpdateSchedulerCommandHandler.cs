using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;
using UniversalBroker.Adapters.Scheduler.Logic.Managers;
using UniversalBroker.Adapters.Scheduler.Models.Commands;
using System.Timers;
using UniversalBroker.Adapters.Scheduler.Models.Internal;
using Protos;
using Google.Protobuf;
using UniversalBroker.Adapters.Scheduler.Extentions;
using Microsoft.Extensions.Options;
using UniversalBroker.Adapters.Scheduler.Configurations;
using PIHelperSh.Core.Extensions;
using System.Text;
using Timer = System.Timers.Timer;

namespace UniversalBroker.Adapters.Scheduler.Logic.Handlers.Commands
{
    public class AddOrUpdateSchedulerCommandHandler(
        ILogger<DisableSchedulerCommandHandler> logger,
        IMediator mediator,
        ISchedulerManager schedulerManager,
        IInitService initService,
        IOptions<SchedulerConfiguration> options
        ) : IRequestHandler<AddOrUpdateSchedulerCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly ISchedulerManager _schedulerManager = schedulerManager;
        private readonly IInitService _initService = initService;
        private readonly SchedulerConfiguration _schedulerConfig = options.Value;

        public async Task<bool> Handle(AddOrUpdateSchedulerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new DisableSchedulerCommand()
                {
                    Path = request.Connection.Path,
                });

                _schedulerConfig.SetValueFromAttributes(request.Connection.Attributes);

                var model = new SchedulerInstanceModel()
                {
                    SchedulerConfiguration = _schedulerConfig,
                    Connection = request.Connection
                };

                if(!string.IsNullOrEmpty(_schedulerConfig.MessageText))
                    model.MessageBody = Encoding.UTF8.GetBytes(_schedulerConfig.MessageText);

                model.MyTimer = new Timer();

                model.MyTimer.Interval = _schedulerConfig.IntervalMs;
                model.MyTimer.AutoReset = _schedulerConfig.AutoReset;
                model.MyTimer.Elapsed += async (sender, e) => await TimerCallback(model);

                if(_schedulerManager.GetActiveSchedulers.TryAdd(request.Connection.Path, model))
                {
                    model.MyTimer.Start();

                    return true;
                }
                _logger.LogWarning("Не удалось запустить таймер по пути {path}", request.Connection.Path);

                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при попытке добавить планировщик по пути {path}", request.Connection.Path);

                request.Connection.Attributes.AddOrUpdateAttribute("Error", ex.Message);

                var task = _initService.GetService?.SendMessage(new()
                {
                    Connection = request.Connection
                },
                cancellationToken);

                if (task != null)
                    await task;

                return false;
            }
        }

        private async Task TimerCallback(SchedulerInstanceModel model)
        {
            try
            {
                if (model.CancellationTokenSource.IsCancellationRequested)
                {
                    model.MyTimer?.Stop();
                    return;
                }

                var message = new MessageDto()
                {
                    Data = ByteString.CopyFrom(model.MessageBody),
                    Path = model.Connection.Path,
                    Headers = { new List<AttributeDto>()
                {
                        new AttributeDto()
                        {
                            Name = "Custom.ReceiveDateTimeUtc",
                            Value = DateTime.UtcNow.ToString()
                        },
                        new AttributeDto()
                        {
                            Name = "Custom.DataLenth",
                            Value = model.MessageBody.Length.ToString()
                        },
                        new AttributeDto()
                        {
                            Name = "SchedulerInstanceModel.IntervalMs",
                            Value = model.SchedulerConfiguration.IntervalMs.ToString()
                        }
                } }
                };

                var task = _initService.GetService?.SendMessage(new()
                {
                    Message = message
                },
                model.CancellationTokenSource.Token);

                if (task != null)
                    await task;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при отпавке сообщения по таймеру");

                var task = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = "MESSAGE CANT BE SEND"
                    }
                },
                model.CancellationTokenSource.Token);

                if (task != null)
                    await task;
            }
        }
    }
}
