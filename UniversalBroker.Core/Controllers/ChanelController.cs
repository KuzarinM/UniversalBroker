using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Internals;
using UniversalBroker.Core.Models.Queries.Chanels;
using UniversalBroker.Core.Models.Queries.Connections;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace UniversalBroker.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChanelController(IMediator mediator) : CustomControllerBase(mediator)
    {
        [HttpGet]
        public async Task<IActionResult> GetChannels(
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0
        )
        {
            return await ControllerSimpleRequest(new GetChanelListQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
            });
        }

        [HttpGet("{id:guid}/logs")]
        public async Task<IActionResult> GetChannelLogs(
            [FromRoute]
            Guid id,
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0,
            [FromQuery]
            DateTime? StartInterval = null,
            [FromQuery]
            DateTime? StopInterval = null,
            [FromQuery]
            List<LogLevel>? Lavels = null
        )
        {
            return await ControllerSimpleRequest(new GetChanelScriptLogsQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                ChanelId = id,
                FromDate = StartInterval,
                ToDate = StopInterval,
                OnlyLavels = Lavels
            });
        }

        [HttpGet("{id:guid}/messages")]
        public async Task<IActionResult> GetChannelMessages(
           [FromRoute]
            Guid id,
           [FromQuery]
            int pageSize = 10,
           [FromQuery]
            int pageIndex = 0,
           [FromQuery]
            DateTime? StartInterval = null,
           [FromQuery]
            DateTime? StopInterval = null
       )
        {
            return await ControllerSimpleRequest(new GetChanelMessagesQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                ChanelId = id,
                FromDate = StartInterval,
                ToDate = StopInterval
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetChanel(
            [FromRoute]
            Guid id
        )
        {
            return await ControllerSimpleRequest(new GetChanelQuery()
            {
                ChanelId = id
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateChanel(
            [FromBody]
            CreateChanelDto chanel
        )
        {
            return await ControllerSimpleRequest(new AddChanelCommand()
            {
                CreateChanelDto = chanel
            });
        }

        [HttpPost("{id:guid}/sendMessage")]
        public async Task<IActionResult> SendMessageToChanel(
            [FromRoute]
            Guid id,
            [FromBody]
            InternalMessage message
            )
        {
            return await ControllerSimpleRequest(new ExecuteScriptCommand()
            {
                ChanelId= id,
                Message = message
            });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateChanel(
            [FromRoute]
            Guid id,
            [FromBody]
            CreateChanelDto chanel
        )
        {
            return await ControllerSimpleRequest(new UpdateChanelCommand()
            {
                Id = id,
                UpdateDto = chanel
            });
        }

        [HttpPut("{id:guid}/script")]
        public async Task<IActionResult> UpdateChanelScript(
            [FromRoute]
            Guid id,
            [FromBody]
            string newScript
)
        {
            return await ControllerSimpleRequest(new ChangeChanelScriptCommand()
            {
                Id = id,
                ScriptText = newScript
            });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteChanel(
            [FromRoute]
            Guid id
        )
        {
            return await ControllerSimpleRequest(new DeleteChanelCommand()
            {
                Id = id
            });
        }
    }
}
