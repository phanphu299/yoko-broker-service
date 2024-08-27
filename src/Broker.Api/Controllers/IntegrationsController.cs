using System;
using System.Threading.Tasks;
using Broker.Application.Constants;
using Broker.Application.Handler.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AHI.Infrastructure.Authorization;
using System.Collections.Generic;

namespace Broker.Api.Controllers
{
    // file deepcode ignore AntiforgeryTokenDisabled: Ignore ValidateAntiForgeryToken because we have RightsAuthorizeFilter
    [Route("bkr/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "oidc")]
    public class IntegrationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public IntegrationsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{id}", Name = "GetIntegrationById")]
        public async Task<IActionResult> GetByIdAsync(System.Guid id)
        {
            var command = new GetIntegrationById(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        [HttpPost("search")]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.VIEW_INTEGRATION)]
        public async Task<IActionResult> SearchAsync([FromBody] SearchIntegration command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("{id}/fetch")]
        public async Task<IActionResult> FetchDataAsync(Guid id, string type, string data)
        {
            var command = new FetchIntegrationData(id, type, data);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.CREATE_INTEGRATION)]
        public async Task<IActionResult> AddIntegrationAsync([FromBody] AddIntegration command)
        {
            var validator = new Application.Intergration.Validation.AddIntegrationValidation();
            var result = validator.Validate(command);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        [HttpPut("{id}")]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.EDIT_INTEGRATION)]
        public async Task<IActionResult> UpdateIntegrationAsync(Guid id, [FromBody] UpdateIntegration command)
        {
            var validator = new Application.Intergration.Validation.UpdateIntegrationValidation();
            var result = validator.Validate(command);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            command.Id = id;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.DELETE_INTEGRATION)]
        public async Task<IActionResult> RemoveIntegrationAsync([FromBody] DeleteIntegration command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        [HttpGet("{id}/series")]
        public async Task<IActionResult> QuerySeriesDataAsync(Guid id, string entityId, string metricKey, long? start, long? end, string aggregate, string grouping)
        {
            if (string.IsNullOrEmpty(entityId) || string.IsNullOrEmpty(metricKey))
            {
                return new BadRequestObjectResult(new { IsSuccess = false, Message = "EntityId or Metric is empty" });
            }
            var command = new QueryIntegrationData(id, entityId, metricKey, start, end, aggregate, grouping);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> CheckExistIntegrationAsync(Guid id)
        {
            var command = new CheckExistIntegration(new Guid[] { id });
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("exist")]
        public async Task<IActionResult> CheckExistIntegrationsAsync([FromBody] IEnumerable<Guid> ids)
        {
            var command = new CheckExistIntegration(ids);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("{id}/fetch/common")]
        public async Task<IActionResult> FetchAsync(Guid id)
        {
            var command = new FetchIntegration(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive")]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.VIEW_INTEGRATION)]
        public async Task<IActionResult> ArchiveAsync([FromBody] ArchiveIntegration command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("retrieve")]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.CREATE_INTEGRATION)]
        public async Task<IActionResult> RetrieveAsync([FromBody] RetrieveIntegration command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive/verify")]
        [RightsAuthorizeFilter(Privileges.Integrations.FullRights.VIEW_INTEGRATION)]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyIntegration command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}
