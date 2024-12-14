using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using UniversalBroker.Core.Database.Models;
using UniversalBroker.Core.Models.Commands.Connections;
using UniversalBroker.Core.Models.Dtos.Connections;
using UniversalBroker.Core.Models.Queries.Chanels;
using UniversalBroker.Core.Models.Queries.Communications;
using UniversalBroker.Core.Models.Queries.Connections;

namespace UniversalBroker.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectionController(IMediator mediator) : CustomControllerBase(mediator)
    {

        [HttpGet]
        public async Task<IActionResult> GetConnections(
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0,
            [FromQuery]
            Guid? CommunicationId = null,
            [FromQuery]
            bool? InputOnly = null

            )
        {
            return await ControllerSimpleRequest(new GetConnectionListQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                CommunicationId = CommunicationId,
                InputOnly = InputOnly
            });
        }

        [HttpGet("{id:guid}/messages")]
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
             DateTime? StopInterval = null
        )
        {
            return await ControllerSimpleRequest(new GetConnectionMessagesQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                ConnectionId = id,
                FromDate = StartInterval,
                ToDate = StopInterval
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetConnection(
            [FromRoute] 
            Guid id
            )
        {
            return await ControllerSimpleRequest(new GetConnectionQuery()
            {
                Id = id
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateConnection(
            [FromBody]
            CreateConnectionDto connection
        )
        {
            return await ControllerSimpleRequest(new AddConnectionCommand()
            {
                ConnectionDto = connection
            });
        }

        [HttpPut("{connectionId:guid}")]
        public async Task<IActionResult> UpdateConnection(
            [FromRoute]
            Guid connectionId,
            [FromBody]
            UpdateConnectionDto connection
        )
        {
            return await ControllerSimpleRequest(new UpdateConnectionCommand()
            {
                ConnectionId = connectionId,
                UpdateDto = connection
            });
        }

        [HttpDelete("{connectionId:guid}")]
        public async Task<IActionResult> DeleteConnection(
            [FromRoute]
            Guid connectionId
        )
        {
            return await ControllerSimpleRequest(new DeleteConnectionCommand()
            {
                ConnectionId = connectionId
            });
        }

        [HttpPost("{connectionId:guid}/receive")]
        public async Task<IActionResult> ReceiveMessage(
            [FromRoute]
            Guid connectionId,
            [FromQuery]
            string Path,
            [FromQuery]
            Dictionary<string,string> Headers,
            [FromBody]
            List<byte> Data
            )
        {
            return await ControllerSimpleRequest(new ReceiveIncommingMessageCommand()
            {
                Path = Path,
                Headers = Headers,
                Data = Data,
                CommunicationId = connectionId
            });
        }
        [HttpPost("{connectionId:guid}/receiveStr")]
        public async Task<IActionResult> ReceiveMessageString(
            [FromRoute]
            Guid connectionId,
            [FromQuery]
            string Path,
            [FromQuery]
            Dictionary<string,string> Headers,
            [FromBody]
            string text
            )
        {
            return await ControllerSimpleRequest(new ReceiveIncommingMessageCommand()
            {
                Path = Path,
                Headers = Headers,
                Data = Encoding.UTF8.GetBytes(text).ToList(),
                CommunicationId = connectionId
            });
        }
    }
}
