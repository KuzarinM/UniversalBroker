using MediatR;
using Microsoft.Extensions.Options;
using System.Text;
using UniversalBroker.Adapters.Scheduler.Configurations;
using UniversalBroker.Adapters.Scheduler.Extentions;
using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;
using UniversalBroker.Adapters.Scheduler.Logic.Managers;
using UniversalBroker.Adapters.Scheduler.Models.Commands;

namespace UniversalBroker.Adapters.Scheduler.Logic.Handlers.Commands
{
    public class RestartSchedulerCommandHandler(
        ILogger<DisableSchedulerCommandHandler> logger,
        IMediator mediator,
        ISchedulerManager schedulerManager,
        IInitService initService,
        IOptions<SchedulerConfiguration> options
        ) : IRequestHandler<RestartSchedulerCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly ISchedulerManager _schedulerManager = schedulerManager;
        private readonly IInitService _initService = initService;
        private readonly SchedulerConfiguration _schedulerConfig = options.Value;

        public async Task<bool> Handle(RestartSchedulerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var path = Encoding.UTF8.GetString(request.Message.Data.ToArray());

                if (_schedulerManager.GetActiveSchedulers.TryGetValue(path, out var scheduler)) 
                {
                    scheduler.SchedulerConfiguration.SetValueFromAttributes(request.Message.Headers);

                    if (!string.IsNullOrEmpty(_schedulerConfig.MessageText))
                        scheduler.MessageBody = Encoding.UTF8.GetBytes(_schedulerConfig.MessageText);

                    scheduler.MyTimer!.Interval = _schedulerConfig.IntervalMs;
                    scheduler.MyTimer!.AutoReset = _schedulerConfig.AutoReset;

                    scheduler.MyTimer!.Stop();
                    scheduler.MyTimer!.Start();

                    return true;
                }
                return false;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при попытке перезапустить таймер сообщением {message}", request.Message);

                var res = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = true,
                        Data = "CANNOT RESTART TIMER"
                    }
                },
                cancellationToken);

                if (res != null)
                    await res;

                return false;
            }
        }
    }
}
