using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Broker.Api.Controllers
{

    [Route("bkr/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "oidc")]
    public class SchemasController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SchemasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSchemaAsync(string type)
        {
            var command = new GetSchemaByType(type);
            var response = await _mediator.Send(command);
            return Ok(response);
        }

    }
}
