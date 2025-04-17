using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using UniversalBroker.Core.Models.Commands.Chanels;
using UniversalBroker.Core.Models.Dtos;
using UniversalBroker.Core.Models.Dtos.Chanels;
using UniversalBroker.Core.Models.Internals;
using UniversalBroker.Core.Models.Queries.Chanels;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;


namespace UniversalBroker.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChanelController(IMediator mediator) : CustomControllerBase(mediator)
    {
        [HttpGet]
        [SwaggerOperation(summary: "Получение списка каналов")]
        [SwaggerResponse(200, description: "Список каналов", type: typeof(PaginationModel<ChanelDto>))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetChannels(
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0,
            [FromQuery]
            string? search = null
        )
        {
            return await ControllerSimpleRequest(new GetChanelListQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                NameContatins = search
            });
        }

        [HttpGet("{id:guid}/relations")]
        [SwaggerOperation(summary: "Получение списка реасльных связей")]
        [SwaggerResponse(200, description: "Список реальных свяей", type: typeof(СhannelRelationsDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetChannelRelations([FromRoute] Guid id)
        {
            return await ControllerSimpleRequest(new GetChanelRelationsQuery()
            {
                ChanelId = id
            });
        }

        [HttpGet("relations")]
        [SwaggerOperation(summary: "Получение списка всей системы")]
        [SwaggerResponse(200, description: "Список реальных свяей", type: typeof(List<NodeDto>))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetSystemRelations()
        {
            return await ControllerSimpleRequest(new GetSystemRelationQuery()
            {
            });
        }

        [HttpGet("{id:guid}/logs")]
        [SwaggerOperation(summary: "Получение логов канала")]
        [SwaggerResponse(200, description: "Список логов", type: typeof(PaginationModel<ChanelScriptLogDto>))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetChannelLogs(
            [FromRoute]
            Guid id,
            [FromQuery]
            int pageSize = 10,
            [FromQuery]
            int pageIndex = 0,
            [FromQuery]
            DateTime? startInterval = null,
            [FromQuery]
            DateTime? stopInterval = null,
            [FromQuery]
            List<LogLevel>? lavels = null
        )
        {
            return await ControllerSimpleRequest(new GetChanelScriptLogsQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                ChanelId = id,
                FromDate = startInterval,
                ToDate = stopInterval,
                OnlyLavels = lavels
            });
        }

        [HttpGet("{id:guid}/messages")]
        [SwaggerOperation(summary: "Получение сообщений канала")]
        [SwaggerResponse(200, description: "Список логов", type: typeof(PaginationModel<MessageViewDto>))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
        public async Task<IActionResult> GetChannelMessages(
           [FromRoute]
            Guid id,
           [FromQuery]
            int pageSize = 10,
           [FromQuery]
            int pageIndex = 0,
           [FromQuery]
            DateTime? startInterval = null,
           [FromQuery]
            DateTime? stopInterval = null
       )
        {
            return await ControllerSimpleRequest(new GetChanelMessagesQuery()
            {
                PageSize = pageSize,
                PageNumber = pageIndex,
                ChanelId = id,
                FromDate = startInterval,
                ToDate = stopInterval
            });
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(summary: "Получение информации по каналу")]
        [SwaggerResponse(200, description: "Канал", type: typeof(ChanelFullDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Создание канала")]
        [SwaggerResponse(200, description: "Канал успешно создан", type: typeof(ChanelDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Отправить сообщение в канал (тестовый)")]
        [SwaggerResponse(204, description: "Сообщение отправлено")]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Изменение инфомрации о канале")]
        [SwaggerResponse(200, description: "Изменённый канал", type: typeof(ChanelDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Изменение отдельно скрипта канала")]
        [SwaggerResponse(200, description: "Изменённый канал", type: typeof(ChanelFullDto))]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
        [SwaggerOperation(summary: "Удаление канала и возврат его модели")]
        [SwaggerResponse(204, description: "Канал успешно удалён")]
        [SwaggerResponse(400, description: "Ошибка", type: typeof(string))]
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
