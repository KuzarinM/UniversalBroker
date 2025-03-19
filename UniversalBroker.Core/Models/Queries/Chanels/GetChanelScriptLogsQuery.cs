using MediatR;
using NLog;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Chanels;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace UniversalBroker.Core.Models.Queries.Chanels
{
    public class GetChanelScriptLogsQuery: IRequest<PaginationModel<ChanelScriptLogDto>>
    {
        public int PageSize { get; set; }

        public int PageNumber { get; set; }

        public Guid ChanelId { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public List<LogLevel>? OnlyLavels { get; set; }
    }
}
