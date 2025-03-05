using MediatR;
using Microsoft.EntityFrameworkCore;
using Protos;
using System.Collections.Concurrent;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Abstracts;
using UniversalBroker.Core.Logic.Interfaces;
using UniversalBroker.Core.Logic.Services;

namespace UniversalBroker.Core.Logic.Managers
{
    public class AdaptersManager(
        ILogger<AdaptersManager> logger, 
        Func<IMediator> mediatorFunc,
        Func<BrockerContext> contextFunc,
        IServiceProvider serviceProvider): AbstractAdaptersManager
    {
        protected ILogger _logger = logger;
        protected Func<IMediator> _mediatorFunc = mediatorFunc;
        protected Func<BrockerContext> _contextFunc = contextFunc;
        protected IServiceProvider _serviceProvider = serviceProvider;

        protected readonly ConcurrentDictionary<Guid, IAdapterCoreService> _activeServices = new();
        private int _timeToLiveS = 20;

        public override int TimeToLiveS => _timeToLiveS;

        public override IAdapterCoreService CreateService => _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IAdapterCoreService>();

        public override async Task RegisterNewAdapter(Guid communicationId, IAdapterCoreService coreService)
        {
            _activeServices.AddOrUpdate(communicationId, coreService, (key, oldService) =>
            {
                oldService.Stop().Wait();
                return coreService;
            });

            await SetCommunicationStatus(communicationId, true);
        }

        private async Task LifesignCheck()
        {
            var disregisterList = new List<IAdapterCoreService>();

            foreach (var item in _activeServices.Values)
            {
                // Даём чуть больше времени
                if(item.SiliensInterval.TotalSeconds > (_timeToLiveS*1.25))
                {
                    disregisterList.Add(item);
                }
            }

            foreach (var item in disregisterList)
            {
                await item.Stop();
            }
        }

        public override async Task DisregisterAdapter(Guid communicationId)
        {
            _activeServices.TryRemove(communicationId, out _ );

            await SetCommunicationStatus(communicationId, false);
        }

        public override IAdapterCoreService? GetAdapterById(Guid id) => _activeServices.TryGetValue(id, out var adapter)? adapter : null;

        private async Task SetCommunicationStatus(Guid communicationId, bool staus)
        {
            await _contextFunc().Communications
                .Where(x => x.Id == communicationId)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, staus));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = Task.Run(async () =>
            {
                while (stoppingToken.IsCancellationRequested)
                {
                    await LifesignCheck();
                    await Task.Delay(_timeToLiveS * 800); // *0.8*1000 = * 800
                }
            });

            return Task.CompletedTask;
        }
    }
}
