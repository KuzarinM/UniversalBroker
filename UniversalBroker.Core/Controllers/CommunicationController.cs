using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Queries.Communications;

namespace UniversalBroker.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunicationController(IMediator mediator) : CustomControllerBase(mediator)
    {

        [HttpGet]
        public async Task<IActionResult> GetCommunications(
            [FromQuery]
            int pageSize = 10, 
            [FromQuery]
            int pageIndex = 0
            )
        {
            return await ControllerSimpleRequest(new GetAllCommunicationsQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
            });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetCommunication(
            [FromRoute]
            Guid id
            )
        {
            return await ControllerSimpleRequest(new GetCommunicationQuery()
            {
                Id = id
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCommunication(
            [FromBody]
            CreateCommunicationDto createDto)
        {
            return await ControllerSimpleRequest(new AddOrUpdateCommunicationCommand()
            {
                CreateCommunicationDto = createDto
            });
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateCommunicationAttribute(
            [FromRoute]
            Guid id,
            [FromBody]
            Dictionary<string,string?> attributeList)
        {
            return await ControllerSimpleRequest(new CommunicationSetAttributeCommand()
            {
                CommunicationId = id,
                Attributes = attributeList
            });
        }
    }
}
