using System;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Api.Controllers
{
    // file deepcode ignore AntiforgeryTokenDisabled: Ignore ValidateAntiForgeryToken because we have RightsAuthorizeFilter
    [Route("bkr/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "oidc")]
    public class ListenersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ListenersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("eventhubs")]
        public async Task<IActionResult> GetAllEventHub()
        {
            var command = new GetValidEventHub();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("mqtts")]
        public async Task<IActionResult> GetAllMqtts()
        {
            var command = new GetValidMqtt();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("coaps")]
        public async Task<IActionResult> GetAllCoaps()
        {
            var command = new GetValidCoap();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("waylays")]
        public async Task<IActionResult> GetAllWaylays()
        {
            var command = new GetValidWaylay();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("greenkoncept")]
        public async Task<IActionResult> GetGreenKoncepts()
        {
            var command = new GetValidGreenKoncept();
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("{id}/active")]
        public async Task<IActionResult> ActiveListenerAsync(Guid id)
        {
            var command = new ActiveListener(id);
            var result = await _mediator.Send(command);
            return Ok(new { IsSuccess = result });
        }

    }
}
