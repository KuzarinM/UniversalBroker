using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Enums;
using UniversalBroker.Core.Models.Queries.Chanels;

namespace UniversalBroker.Core.Logic.Handlers.Queries.Chanels
{
    public class GetChanelRelationsQueryHandler(
        ILogger<GetChanelRelationsQueryHandler> logger,
        BrockerContext context
        ) : IRequestHandler<GetChanelRelationsQuery, СhannelRelationsDto?>
    {
        private readonly ILogger _logger = logger;
        private readonly BrockerContext _context = context;

        public async Task<СhannelRelationsDto?> Handle(GetChanelRelationsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var relations = await _context.Messages
                    .Include(x => x.TargetChannel)
                    .Include(x => x.SourceChannel)
                    .Include(x => x.Connection)
                    .Where(x => x.SourceChannelId == request.ChanelId || x.TargetChannelId == request.ChanelId)
                    .GroupBy(x => new
                    {
                        x.SourceChannelId,
                        x.TargetChannelId,
                        x.ConnectionId,
                    })
                    .Select(x => new СhanelRelationDto()
                    {
                        RelationId = (x.Key.SourceChannelId != null && x.Key.SourceChannelId != request.ChanelId) 
                                            ? x.Key.SourceChannelId.Value
                                            : (x.Key.TargetChannelId != null && x.Key.TargetChannelId != request.ChanelId 
                                                    ? x.Key.TargetChannelId.Value
                                                    : x.Key.ConnectionId!.Value) ,
                        RelationName = (x.Key.SourceChannelId != null && x.Key.SourceChannelId != request.ChanelId)
                                            ? x.First().SourceChannel.Name
                                            : (x.Key.TargetChannelId != null && x.Key.TargetChannelId != request.ChanelId
                                                    ? x.First().TargetChannel.Name
                                                    : x.First().Connection.Name),
                        Status = RelationUsageStatus.NotMarked,
                        Direction = x.First().Connection == null 
                                                ? MessageDirection.ChanelToChanel 
                                                : (x.First().Connection.Isinput 
                                                            ? MessageDirection.ConnectionToChanel 
                                                            : MessageDirection.ChanelToConnection
                                                    )
                    })
                    .ToListAsync();

                var chanel = await _context.Chanels
                                            .Include(x => x.FromChanels)
                                            .Include(x => x.ToChanels)
                                            .Include(x => x.Connections)
                                            .FirstOrDefaultAsync(x => x.Id == request.ChanelId);

                List<СhanelRelationDto> declaredRelations = new();

                if (chanel == null)
                    return null;
                
                declaredRelations.AddRange(chanel.Connections.Select(x => new СhanelRelationDto()
                {
                    RelationId = x.Id,
                    RelationName = x.Name,
                    Status = RelationUsageStatus.NotUsed,
                    Direction = x.Isinput ? MessageDirection.ConnectionToChanel : MessageDirection.ChanelToConnection
                }));

                declaredRelations.AddRange(chanel.FromChanels.Select(x => new СhanelRelationDto()
                {
                    RelationId = x.Id,
                    RelationName = x.Name,
                    Status = RelationUsageStatus.NotUsed,
                    Direction = MessageDirection.ChanelToChanel
                }));

                declaredRelations.AddRange(chanel.ToChanels.Select(x => new СhanelRelationDto()
                {
                    RelationId = x.Id,
                    RelationName = x.Name,
                    Status = RelationUsageStatus.NotUsed,
                    Direction = MessageDirection.ChanelToChanel
                }));
                
                foreach (var item in relations)
                {
                    var relation = declaredRelations.FirstOrDefault(x => x.RelationId == item.RelationId);

                    if(relation != null)
                    {
                        declaredRelations.Remove(relation);

                        item.Status = RelationUsageStatus.InUse;
                    }
                }

                relations.AddRange(declaredRelations);

                return new()
                {
                    Relations = relations,
                    ChanelId = request.ChanelId,
                    ChanelName = chanel.Name
                };
            }
            catch (ControllerException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка реальных связей Канала");
                throw new ControllerException("Ошибка при получении списка реальных связей Канала");
            }
        }
    }
}
