using MediatR;
using Microsoft.EntityFrameworkCore;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    public class GetSustemRelationQueryHandler(
        ILogger<GetSustemRelationQueryHandler> logger,
        BrockerContext context) : IRequestHandler<GetSystemRelationQuery, List<NodeDto>>
    {
        private readonly ILogger _logger = logger;
        private readonly BrockerContext _context = context;

        public async Task<List<NodeDto>> Handle(GetSystemRelationQuery request, CancellationToken cancellationToken)
        {
            var relations = new Dictionary<Guid, NodeDto>();

            // Получаем реальные связи (то есть те, что по сообщениям)
            var realConnections = await _context.Messages
                   .Include(x => x.TargetChannel)
                   .Include(x => x.SourceChannel)
                   .Include(x => x.Connection)
                   .GroupBy(x => new
                   {
                       x.SourceChannelId,
                       x.TargetChannelId,
                       x.ConnectionId,
                   })
                   .Select(x => x.Key)
                   .ToListAsync();

            // Создаём первую версию связей
            foreach (var connection in realConnections) 
            {
                Guid? from = connection.SourceChannelId ?? connection.ConnectionId;
                Guid? to = connection.TargetChannelId ?? connection.ConnectionId;

                if (from == null || to == null)
                    continue;

                // Добавляем узлы
                var fromNode = GetOrAddNodeDto(relations, from.Value, new()
                {
                    ObjectId = from.Value,
                    IsChanel = connection.SourceChannelId != null
                });

                var toNode = GetOrAddNodeDto(relations, to.Value, new()
                {
                    ObjectId = to.Value,
                    IsChanel = connection.TargetChannelId != null
                });

                // Добавляем связь, считаем что её не обозначили
                GetOrAddRelationDto(fromNode.OutputIds, to.Value).Status = RelationUsageStatus.NotMarked;
            }

            // Получаем списко каналов, чтобы их все указать
            var allChanels = await _context.Chanels
                                    .Include(x => x.FromChanels)
                                    .Include(x => x.Connections)
                                    .ToListAsync();

            foreach (var item in allChanels)
            {
                var node = GetOrAddNodeDto(relations, item.Id, new()
                {
                    ObjectId = item.Id,
                    IsChanel = true
                });

                node.ObjectName = item.Name;

                foreach (var chanelRelation in item.FromChanels.Select(x => GetOrAddRelationDto(node.OutputIds, x.Id)))
                {
                    chanelRelation.Status = chanelRelation.Status == RelationUsageStatus.NotMarked ? RelationUsageStatus.InUse : chanelRelation.Status;
                }

                foreach (var chanelRelation in item.Connections.Where(x=>!x.Isinput).Select(x => GetOrAddRelationDto(node.OutputIds, x.Id)))
                {
                    chanelRelation.Status = chanelRelation.Status == RelationUsageStatus.NotMarked ? RelationUsageStatus.InUse : chanelRelation.Status;
                }
            }

            // Получаем список подключений, чтобы их добавить
            var allConnection = await _context.Connections
                                    .Include(x=>x.Chanels)
                                    .ToListAsync();

            foreach (var item in allConnection)
            {
                var node = GetOrAddNodeDto(relations, item.Id, new()
                {
                    ObjectId = item.Id,
                    IsChanel = false
                });

                node.ObjectName = item.Name;

                if (item.Isinput)
                {
                    foreach (var chanelRelation in item.Chanels.Select(x => GetOrAddRelationDto(node.OutputIds, x.Id)))
                    {
                        chanelRelation.Status = chanelRelation.Status == RelationUsageStatus.NotMarked ? RelationUsageStatus.InUse : chanelRelation.Status;
                    }
                }
            }

            return relations.Values.ToList();
        }

        private NodeDto GetOrAddNodeDto(Dictionary<Guid, NodeDto> dict, Guid id, NodeDto dto)
        {
            if(dict.ContainsKey(id))
                return dict[id];

            dict.Add(id, dto);

            return dto;
        }

        private RelationDto GetOrAddRelationDto(List<RelationDto> list, Guid id) 
        {
            var dto = list.FirstOrDefault(x=>x.TargetId == id);

            if (dto == null)
            {
                dto = new RelationDto()
                {
                    TargetId = id,
                    Status = RelationUsageStatus.NotUsed
                };
                list.Add(dto);
            }

            return dto;
        }
    }
}
