using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AHI.Infrastructure.Authorization;
using Broker.Application.Constants;
using Broker.Application.FileRequest.Command;
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
    public class BrokersController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BrokersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}", Name = "GetBrokerById")]
        public async Task<IActionResult> GetByIdAsync([FromRoute] Guid id, [FromQuery] bool includeDeletedRecords)
        {
            var command = new GetBrokerById(id);
            command.IncludeDeletedRecords = includeDeletedRecords;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("search")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.READ_BROKER)]
        public async Task<IActionResult> SearchAsync([FromBody] SearchBroker command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.CREATE_BROKER)]
        public async Task<IActionResult> AddBrokerAsync([FromBody] AddBroker command)
        {
            var validator = new Application.Broker.Validation.AddBrokerValidation();
            var result = validator.Validate(command);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.EDIT_BROKER)]
        public async Task<IActionResult> UpdateBrokerAsync([FromRoute] Guid id, [FromBody] UpdateBroker command)
        {
            var validator = new Application.Broker.Validation.UpdateBrokerValidation();
            var result = validator.Validate(command);
            if (!result.IsValid)
            {
                return BadRequest(result.Errors);
            }
            command.Id = id;
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.DELETE_BROKER)]
        public async Task<IActionResult> DeleteBrokersAsync(Guid id)
        {
            var command = new DeleteBrokerById(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.DELETE_BROKER)]
        public async Task<IActionResult> DeleteBrokersAsync([FromBody] IEnumerable<Guid> ids)
        {
            var command = new DeleteBroker(ids);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportBrokerAsync([FromBody] ImportFile command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("export")]
        [RightsAuthorizeFilter(Privileges.Configuration.FullRights.SHARE_CONFIGURATION)]
        public async Task<IActionResult> ExportBrokerAsync([FromBody] ExportBroker command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> CheckExistBrokerAsync(Guid id)
        {
            var command = new CheckExistBroker(new Guid[] { id });
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("exist")]
        public async Task<IActionResult> CheckExistBrokersAsync([FromBody] IEnumerable<Guid> ids)
        {
            var command = new CheckExistBroker(ids);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("{id}/fetch")]
        public async Task<IActionResult> FetchAsync(Guid id)
        {
            var command = new FetchBroker(id);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.READ_BROKER)]
        public async Task<IActionResult> ArchiveAsync([FromBody] ArchiveBroker command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("retrieve")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.CREATE_BROKER)]
        public async Task<IActionResult> RetrieveAsync([FromBody] RetrieveBroker command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("archive/verify")]
        [RightsAuthorizeFilter(Privileges.Broker.FullRights.READ_BROKER)]
        public async Task<IActionResult> VerifyAsync([FromBody] VerifyBroker command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}
