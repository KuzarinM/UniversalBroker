using MediatR;
using Microsoft.AspNetCore.Mvc;
using UniversalBroker.Core.Exceptions;
using UniversalBroker.Core.Models.Queries.Communications;

namespace UniversalBroker.Core.Controllers
{
    /// <summary>
    /// Небольшая абстракиця для упрощения контрллера
    /// </summary>
    /// <param name="mediator"></param>
    public class CustomControllerBase(IMediator mediator) : ControllerBase
    {
        protected readonly IMediator _mediator = mediator;

        /// <summary>
        /// Отправяет запрос в медиатор и если вылетит кастомный эксепшен - запишет ответ как StatusCode()
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="unexpectedErrorText"></param>
        /// <returns></returns>
        protected async Task<IActionResult> ControllerSimpleRequest<T>(IRequest<T> request, string unexpectedErrorText = "Неодиданная ошибка во врем выполения")
        {
            try
            {
                return Ok(await _mediator.Send(request));
            }
            catch (ControllerException ex)
            {
                return StatusCode(ex.StatusCodeInt, ex.Message);
            }
            catch
            {
                return BadRequest(unexpectedErrorText);
            }
        }

        /// <summary>
        /// Отправяет запрос в медиатор и если вылетит кастомный эксепшен - запишет ответ как StatusCode(), но тут IRequest не типизированный
        /// </summary>
        /// <param name="request"></param>
        /// <param name="unexpectedErrorText"></param>
        /// <returns></returns>
        protected async Task<IActionResult> ControllerSimpleRequest(IRequest request, string unexpectedErrorText = "Неодиданная ошибка во врем выполения")
        {
            try
            {
                await _mediator.Send(request);
                return Ok();
            }
            catch (ControllerException ex)
            {
                return StatusCode(ex.StatusCodeInt, ex.Message);
            }
            catch
            {
                return BadRequest(unexpectedErrorText);
            }
        }

    }
}
