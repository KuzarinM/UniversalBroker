using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UniversalBroker.Core.Models.Commands.Communications;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Dtos.Communications;
using UniversalBroker.Core.Models.Queries.Communications;

namespace UniversalBroker.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommunicationController(IMediator mediator) : CustomControllerBase(mediator)
    {

        [HttpGet]
        [SwaggerOperation(summary: "Получение Соединений с фильтрами")]
        [SwaggerResponse(200, description: "Список Соединений", type: typeof(PaginationModel<CommunicationDto>))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetCommunications(
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0,
            [FromQuery]
            bool? Status = null,
            [FromQuery]
            string? Search = null
            )
        {
            return await ControllerSimpleRequest(new GetAllCommunicationsQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                Status = Status,
                NameSearch = Search
            });
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(summary: "Получение Соединения")]
        [SwaggerResponse(200, description: "Соединениt", type: typeof(CommunicationDto))]
        [SwaggerResponse(204, description: "Не удалось найти Соединение")]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Обновление Соединения, если есть, если нет - добавление")]
        [SwaggerResponse(200, description: "Модель Соединения", type: typeof(CommunicationDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> AddOrUpdateCommunication(
            [FromBody]
            CreateCommunicationDto createDto)
        {
            return await ControllerSimpleRequest(new AddOrUpdateCommunicationCommand()
            {
                CreateCommunicationDto = createDto
            });
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(summary: "Удаление Соединения")]
        [SwaggerResponse(200, description: "Модель Соединения, если оно было удалено", type: typeof(CommunicationDto))]
        [SwaggerResponse(204, description: "Не найдено Соединения на удаление")]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> DeleteCommunication(
            [FromRoute]
            Guid id)
        {
            return await ControllerSimpleRequest(new DeleteCommunicationCommand()
            {
                Id = id
            });
        }

        [HttpPatch("{id:guid}")]
        [SwaggerOperation(summary: "Обновление атрибутов Соединения")]
        [SwaggerResponse(200, description: "Модель Соединения, если оно было изменено", type: typeof(CommunicationDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
