using MediatR;
using UniversalBroker.Adapters.Scheduler.Extentions;
using UniversalBroker.Adapters.Scheduler.Logic.Interfaces;
using UniversalBroker.Adapters.Scheduler.Logic.Managers;
using UniversalBroker.Adapters.Scheduler.Models.Commands;
using UniversalBroker.Adapters.Scheduler.Models.Internal;
using static Google.Rpc.Context.AttributeContext.Types;

namespace UniversalBroker.Adapters.Scheduler.Logic.Handlers.Commands
{
    public class DisableSchedulerCommandHandler(
        ILogger<DisableSchedulerCommandHandler> logger, 
        IMediator mediator, 
        ISchedulerManager schedulerManager,
        IInitService initService
        ) : IRequestHandler<DisableSchedulerCommand, bool>
    {
        private readonly ILogger _logger = logger;
        private readonly IMediator _mediator = mediator;
        private readonly ISchedulerManager _schedulerManager = schedulerManager;
        private readonly IInitService _initService = initService;

        public async Task<bool> Handle(DisableSchedulerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if(request.ConnectionId == null)
                {
                    if(_schedulerManager.GetActiveSchedulers.TryRemove(request.Path, out var modelSchedulers))
                    {
                        _logger.LogInformation("Успешно удалён планировщик по пути: {path}", request.Path);

                        return await DisposeScheduler(modelSchedulers, cancellationToken);
                    }
                    return false;
                }

                var model = _schedulerManager.GetActiveSchedulers.TryGetValue(request.Path, out var scheduler) ? scheduler : null;

                if (model == null)
                    return false;

                if (model.Connection.Id == request.ConnectionId)
                {
                    if(_schedulerManager.GetActiveSchedulers.TryRemove(request.Path, out _))
                    {
                        _logger.LogInformation("Успешно удалён планировщик по пути: {path}", request.Path);

                        return await DisposeScheduler(model, cancellationToken);
                    }
                    return false;
                }
                else
                    _logger.LogWarning(
                        "По пути {path} есть планировщик, но его Id не совпадает: {resId}!={needId}",
                        request.Path,
                        model.Connection.Id,
                        request.ConnectionId
                    );

                return false;

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при попытке удалить планировщик по пути {path}", request.Path);

                var task = _initService.GetService?.SendMessage(new()
                {
                    StatusDto = new()
                    {
                        Status = false,
                        Data = ex.Message,
                    }
                },
                cancellationToken);

                if(task != null)    
                    await task;

                return false;
            }
        }

        private async Task<bool> DisposeScheduler(SchedulerInstanceModel model, CancellationToken cancellationToken)
        {
            try
            {
                model.CancellationTokenSource.Cancel();

                model.MyTimer?.Dispose();

                return true;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Ошибка при попытке удалить планировщик по пути {path}", model.Connection.Path);

                model?.Connection.Attributes.AddOrUpdateAttribute("Error", ex.Message);

                var task = _initService.GetService?.SendMessage(new()
                {
                    Connection = model?.Connection,
                },
                cancellationToken);

                if (task != null)
                    await task;

                return true;
            }
        }
    }
}
