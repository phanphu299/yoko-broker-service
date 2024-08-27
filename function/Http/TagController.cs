using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AHI.Broker.Function.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.Service.Tag.Model;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using Function.Helper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;

namespace Function.Http
{
    public class TagController
    {
        private readonly ITenantContext _tenantContext;
        private readonly IConfiguration _configuration;
        private readonly ITagService _tagService;
        private readonly IEntityTagService _entityTagService;
        private readonly ILoggerAdapter<TagController> _logger;
        private readonly IMasterService _masterService;

        public TagController(
                IConfiguration configuration,
                ITenantContext tenantContext,
                ITagService tagService,
                IEntityTagService entityTagService,
                IMasterService masterService,
                ILoggerAdapter<TagController> logger)
        {
            _configuration = configuration;
            _tenantContext = tenantContext;
            _tagService = tagService;
            _entityTagService = entityTagService;
            _masterService = masterService;
            _logger = logger;
        }
        [FunctionName("DeleteTagBinding")]
        public async Task<IActionResult> DeleteTagBindingAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "fnc/bkr/tags")] HttpRequestMessage req)
        {
            var principal = await SecurityHelper.ValidateTokenAsync(req.Headers.Authorization);
            if (principal == null)
            {
                return new UnauthorizedResult();
            }

            _tenantContext.RetrieveFromHeader(req.Headers);
            var existProject = await IsProjectExistingAsync(_tenantContext.ProjectId);
            if (!existProject)
                return new OkResult();

            var content = await req.Content.ReadAsByteArrayAsync();
            var deleteTagMessage = content.Deserialize<DeleteTagMessage>();
            var brokerIds = await _entityTagService.GetEntityIdsByTagIdsAsync(deleteTagMessage.TagIds);
            await _tagService.DeleteTagsAsync(deleteTagMessage.TagIds);
            // only broker has entity tag, so only process for broker
            await _entityTagService.RemoveBrokerDetailCacheAsync(brokerIds);
            return new OkResult();
        }

        private async Task<bool> IsProjectExistingAsync(string id)
        {
            var projects = await _masterService.GetAllProjectsAsync();
            return projects.Any(p => p.Id == id &&
                                     p.ProjectType == "asset");
        }
    }
}

