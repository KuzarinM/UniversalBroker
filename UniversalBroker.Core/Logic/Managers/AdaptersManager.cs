using MediatR;
using Microsoft.EntityFrameworkCore;
using Protos;
using System.Collections.Concurrent;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Logic.Services;

namespace UniversalBroker.Core.Logic.Managers
{
    public class AdaptersManager(
        ILogger<AdaptersManager> logger, 
        IMediator mediator,
        BrockerContext context)
    {
        protected ILogger _logger = logger;
        protected IMediator _mediator = mediator;
        protected BrockerContext _context = context;

        protected readonly ConcurrentDictionary<Guid, AdapterCoreService> _activeServices = new();
        private int _timeToLiveS = 20;

        public int TimeToLiveS => _timeToLiveS;

        public async Task RegisterNewAdapter(Guid communicationId, AdapterCoreService coreService)
        {
            _activeServices.AddOrUpdate(communicationId, coreService, (key, oldService) =>
            {
                oldService.Stop().Wait();
                return coreService;
            });

            await SetCommunicationStatus(communicationId, true);
        }

        public async Task LifesignCheck()
        {
            var disregisterList = new List<AdapterCoreService>();

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

        public async Task DisregisterAdapter(Guid communicationId)
        {
            _activeServices.TryRemove(communicationId, out _ );

            await SetCommunicationStatus(communicationId, false);
        }

        public AdapterCoreService? GetAdapterById(Guid id) => _activeServices.TryGetValue(id, out var adapter)? adapter : null;

        private async Task SetCommunicationStatus(Guid communicationId, bool staus)
        {
            await _context.Communications
                .Where(x => x.Id == communicationId)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.Status, staus));
        }
    }
}
