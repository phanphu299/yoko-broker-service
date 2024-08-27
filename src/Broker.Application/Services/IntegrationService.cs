using System.Threading;
using System.Threading.Tasks;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Service.Abstraction;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using Broker.Application.Repository.Abstraction;
using System;
using AHI.Infrastructure.Service;
using Microsoft.EntityFrameworkCore;
using AHI.Infrastructure.SharedKernel.Model;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Broker.Application.Constant;
using Broker.Application.Event;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Exception;
using Device.Domain.Entity;
using Broker.Application.Repository;
using AHI.Infrastructure.Exception.Helper;
using FluentValidation.Results;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Audit.Constant;
using Newtonsoft.Json;
using FluentValidation;
using Configuration.Application.Constant;
using AHI.Infrastructure.UserContext.Abstraction;
using AHI.Infrastructure.Security.Extension;
using Broker.Application.Constants;
using Broker.Application.Helper;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AHI.Infrastructure.Service.Tag.Extension;

namespace Broker.Application.Service
{
    public class IntegrationService : BaseSearchService<Domain.Entity.Integration, Guid, SearchIntegration, IntegrationDto>, IIntegrationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly IDictionary<string, IIntegrationHandler> _handler;
        private readonly IIntegrationValidator _validator;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogService _auditLogService;
        private readonly ILookupService _lookupService;
        private readonly IUserContext _userContext;
        private readonly IValidator<ArchiveIntegrationDto> _integrationVerifyValidator;
        private readonly IDictionary<string, IContentVerificationHandler> _verifyHandler;
        private readonly IDomainEventDispatcher _dispatcher;
        private readonly ITagService _tagService;

        public IntegrationService(
            IUnitOfWork unitOfWork,
            ITenantContext tenantContext,
            IServiceProvider serviceProvider,
            IDictionary<string, IIntegrationHandler> handler,
            IIntegrationValidator validator,
            IConfiguration configuration,
            IAuditLogService auditLogService,
            IUserContext userContext,
            IValidator<ArchiveIntegrationDto> integrationVerifyValidator,
            IDictionary<string, IContentVerificationHandler> verifyHandler,
            IDomainEventDispatcher dispatcher,
            ITagService tagService,
            ILookupService lookupService)
            : base(IntegrationDto.Create, serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _handler = handler;
            _validator = validator;
            _configuration = configuration;
            _auditLogService = auditLogService;
            _userContext = userContext;
            _integrationVerifyValidator = integrationVerifyValidator;
            _verifyHandler = verifyHandler;
            _dispatcher = dispatcher;
            _tagService = tagService;
            _lookupService = lookupService;
        }

        public async Task<IntegrationDto> AddAsync(AddIntegration command, CancellationToken token)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(command.Type, token);

                // persist to database and implement extra logic
                var errors = new List<ValidationFailure>();
                errors.AddRange(await _validator.ValidateAsync(command, token));

                //validate name dulicated
                if (await IsDuplicationNameAsync(command.Name, new Guid()))
                    errors.Add(new ValidationFailure(nameof(AddIntegration.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED));

                if (errors.Any())
                    throw EntityValidationExceptionHelper.GenerateException(errors);

                // implement the adding features
                var entity = AddIntegration.Create(command);
                await _unitOfWork.Integrations.AddAsync(entity);

                var tagIds = Array.Empty<long>();
                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(_userContext.ApplicationId ?? ApplicationInformation.APPLICATION_ID);
                if (command.Tags != null && command.Tags.Any())
                {
                    tagIds = await _tagService.UpsertTagsAsync(command);
                }
                var entityId = EntityTagHelper.GetEntityId(entity.Id);
                entity.EntityTags = EntityTagHelper.GetEntityTags(EntityTypeConstants.INTEGRATION, tagIds, entityId);
                await _unitOfWork.CommitAsync();
                // integration insert success -> dispatch to service if needed.
                await _dispatcher.SendAsync(new IntegrationChangedEvent(entity.Id, entity.Name, entity.Type, _tenantContext, true, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Created));
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Add, ActionStatus.Success, entity.Id, entity.Name, command);
                return await _tagService.FetchTagsAsync(IntegrationDto.Create(entity));
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Add, ActionStatus.Fail, payload: command);
                throw;
            }
        }
        public async Task<IntegrationDto> UpdateAsync(UpdateIntegration command, CancellationToken token)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(command.Type, token);

                var integrationDB = await _unitOfWork.Integrations.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
                if (integrationDB == null)
                    throw new EntityNotFoundException(command.Id.ToString());

                var errors = new List<ValidationFailure>();
                errors.AddRange(await _validator.ValidateAsync(command, token));

                //validate name dulicated
                if (await IsDuplicationNameAsync(command.Name, command.Id))
                    errors.Add(new ValidationFailure(nameof(AddIntegration.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED));

                if (errors.Any())
                    throw EntityValidationExceptionHelper.GenerateException(errors);

                // implement the adding features
                var entity = UpdateIntegration.Create(command);
                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(_userContext.ApplicationId ?? ApplicationInformation.APPLICATION_ID);

                var isSameTag = command.IsSameTags(integrationDB.EntityTags);
                if (!isSameTag)
                {
                    await _unitOfWork.EntityTags.RemoveByEntityIdAsync(EntityTypeConstants.INTEGRATION, integrationDB.Id, true);

                    var tagIds = await _tagService.UpsertTagsAsync(command);
                    if (tagIds.Any())
                    {
                        var entityId = EntityTagHelper.GetEntityId(command.Id);
                        var entitiesTags = EntityTagHelper.GetEntityTags(EntityTypeConstants.INTEGRATION, tagIds, entityId).ToArray();
                        await _unitOfWork.EntityTags.AddRangeWithSaveChangeAsync(entitiesTags);
                        entity.EntityTags = entitiesTags;
                    }
                }

                await _unitOfWork.Integrations.UpdateAsync(entity.Id, entity);
                await _unitOfWork.CommitAsync();
                // integration insert success -> dispatch to service if needed.

                await _dispatcher.SendAsync(new IntegrationChangedEvent(command.Id, integrationDB.Name, entity.Type, _tenantContext, true, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Updated));
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Update, ActionStatus.Success, entity.Id, entity.Name, command);
                return await _tagService.FetchTagsAsync(IntegrationDto.Create(entity));
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Update, ActionStatus.Fail, command.Id, command.Name, payload: command);
                throw;
            }
        }

        public async Task<BaseSearchResponse<FetchDataDto>> FetchDataAsync(FetchIntegrationData command, CancellationToken token)
        {
            var integration = await _unitOfWork.Integrations.AsQueryable().Where(x => x.Id == command.Id).FirstAsync();
            if (_handler.TryGetValue(integration.Type, out var handler))
            {
                return await handler.FetchAsync(integration, command);
            }
            return null;
        }

        public async Task<IntegrationDto> FindByIdAsync(GetIntegrationById command, CancellationToken token)
        {
            var entity = await _unitOfWork.Integrations.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
            if (entity == null)
                throw new EntityNotFoundException(command.Id.ToString());
            return await _tagService.FetchTagsAsync(IntegrationDto.Create(entity));
        }

        public override async Task<BaseSearchResponse<IntegrationDto>> SearchAsync(SearchIntegration criteria)
        {
            criteria.MappingSearchTags();
            var response = await base.SearchAsync(criteria);
            return await _tagService.FetchTagsAsync(response);
        }

        protected override Type GetDbType()
        {
            return typeof(IIntegrationRepository);
        }

        public async Task<BaseResponse> RemoveAsync(DeleteIntegration command, CancellationToken token)
        {
            var integrationIds = command.Integrations.Select(ig => ig.Id);
            var intergations = await _unitOfWork.Integrations.AsQueryable().Where(x => integrationIds.Contains(x.Id)).ToListAsync();
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                bool success = await _unitOfWork.Integrations.RemoveListEntityWithRelationAsync(command.Integrations.Select(e => DeleteIntegration.Create(e)).ToList());
                foreach (var id in integrationIds)
                {
                    await _unitOfWork.EntityTags.RemoveByEntityIdAsync(EntityTypeConstants.INTEGRATION, id, true);
                }
                await _unitOfWork.CommitAsync();
                foreach (var integration in command.Integrations)
                {
                    await _dispatcher.SendAsync(new IntegrationChangedEvent(integration.Id, null, null, _tenantContext, false, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Deleted));
                }
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Delete, success ? ActionStatus.Success : ActionStatus.Fail, command.Integrations.Select(x => x.Id), intergations.Select(x => x.Name), command);
                return BaseResponse.Success;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityActionContants.INTEGRATION_ENTITY, ActionType.Delete, ActionStatus.Fail, command.Integrations.Select(x => x.Id), intergations.Select(x => x.Name), command);
                throw;
            }
        }

        private Task<bool> IsDuplicationNameAsync(string name, Guid id)
        {
            return _unitOfWork.Integrations.AsQueryable().Where(x => x.Name.ToLower() == name.ToLower() && x.Id != id).AnyAsync();
        }

        public async Task<IEnumerable<TimeSeriesDto>> QueryTimeSeriesDataAsync(QueryIntegrationData command, CancellationToken cancellationToken)
        {
            var integration = await _unitOfWork.Integrations.AsQueryable().Include(x => x.Detail).AsNoTracking().Where(x => x.Id == command.Id).FirstOrDefaultAsync();
            if (_handler.TryGetValue(integration.Type, out var handler))
            {
                return await handler.QueryAsync(integration, command);
            }
            return Array.Empty<TimeSeriesDto>();
        }

        public async Task<BaseResponse> CheckExistIntegrationsAsync(CheckExistIntegration command, CancellationToken token)
        {
            //var exist = await _unitOfWork.Integrations.AsQueryable().AsNoTracking().Where(x => integrations.Ids.Contains(x.Id)).AnyAsync();
            var requestIds = command.Ids.Distinct();
            var count = await _unitOfWork.Integrations.AsQueryable().AsNoTracking().Where(x => requestIds.Contains(x.Id)).CountAsync();
            if (count < requestIds.Count())
            {
                throw new EntityNotFoundException(MessageConstants.INTEGRATION_NOT_FOUND);
            }
            return BaseResponse.Success;
        }

        public async Task<IEnumerable<ArchiveIntegrationDto>> ArchiveAsync(ArchiveIntegration command, CancellationToken token)
        {
            var searchInput = new SearchIntegration
            {
                PageSize = int.MaxValue,
                Filter = JsonConvert.SerializeObject(new
                {
                    And = new[] {
                        new {
                            queryKey = "updatedUtc",
                            queryType = "datetime",
                            operation = "lte",
                            queryValue = command.ArchiveTime.ToString(AHI.Infrastructure.SharedKernel.Extension.Constant.DefaultDateTimeFormat)
                        }
                    }
                }),
                Fields = new[] { "id,name,type,detail,updatedUtc,createdUtc" }
            };

            var integrations = await this.SearchAsync(searchInput);
            return integrations.Data.Select(integration =>
            {
                var result = ArchiveIntegrationDto.Create(integration);
                RemoveConfidentialInfo(result);
                return result;
            });
        }

        public async Task<BaseResponse> VerifyArchiveDataAsync(VerifyIntegration command, CancellationToken token)
        {
            var integrations = JsonConvert.DeserializeObject<IEnumerable<ArchiveIntegrationDto>>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            foreach (var integration in integrations)
            {
                var validation = await _integrationVerifyValidator.ValidateAsync(integration);
                if (!validation.IsValid)
                {
                    throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
                }

                var content = JsonConvert.DeserializeObject<IDictionary<string, object>>(integration.Content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
                var isValid = _verifyHandler[integration.Type].Handle(content);
                if (!isValid)
                    throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
            }

            return BaseResponse.Success;
        }

        public async Task<IDictionary<string, object>> RetrieveAsync(RetrieveIntegration command, CancellationToken token)
        {
            var integrations = JsonConvert.DeserializeObject<IEnumerable<ArchiveIntegrationDto>>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            var addedIds = new List<BrokerIdDto>();
            _userContext.SetUpn(command.Upn);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                integrations = integrations.OrderBy(x => x.UpdatedUtc);
                foreach (var integration in integrations)
                {
                    var input = new AddIntegration
                    {
                        Name = integration.Name,
                        Type = integration.Type,
                        Details = JsonConvert.DeserializeObject<Dictionary<string, object>>(integration.Content),
                    };

                    var newId = await ProcessRetrieveIntegrationsAsync(input, integration, token);
                    addedIds.Add(new BrokerIdDto(integration.Id, newId));
                }
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var dispatchTasks = integrations.Select(integration => _dispatcher.SendAsync(new IntegrationChangedEvent(addedIds.Find(x => x.OldId == integration.Id).NewId, integration.Name, integration.Type, _tenantContext, true, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Created)));
            await Task.WhenAll(dispatchTasks);
            return addedIds.ToDictionary(x => x.OldId.ToString(), x => (object)x.NewId);
        }

        private async Task<Guid> ProcessRetrieveIntegrationsAsync(
            AddIntegration integration,
            ArchiveIntegrationDto archiveIntegrationDto,
            CancellationToken token)
        {
            _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(integration.Type, token);
            string saltKey = _configuration[ConfigurationKeys.SALT_KEY];

            switch (integration.Type)
            {
                case IntegrationTypeConstants.INTEGRATION_EVENT_HUB:
                    integration.Details[IntegrationContentKeys.EVENT_HUB_NAME] = string.Empty;
                    integration.Details[IntegrationContentKeys.CONNECTION_STRING] = string.Empty;
                    break;
                case IntegrationTypeConstants.WAY_LAY:
                    integration.Details[IntegrationContentKeys.API_KEY] = string.Empty;
                    integration.Details[IntegrationContentKeys.API_SECRET] = string.Empty;
                    break;
                case IntegrationTypeConstants.GREEN_KONCEPT:
                    integration.Details[IntegrationContentKeys.CLIENT_ID] = integration.Details[IntegrationContentKeys.CLIENT_ID].ToString().Base64Decode(saltKey);
                    integration.Details[IntegrationContentKeys.CLIENT_SECRET] = integration.Details[IntegrationContentKeys.CLIENT_SECRET].ToString().Base64Decode(saltKey);
                    integration.Details[IntegrationContentKeys.ENDPOINT] = integration.Details[IntegrationContentKeys.ENDPOINT].ToString().Base64Decode(saltKey);
                    break;
                default:
                    break;
            }

            var errors = new List<ValidationFailure>();
            errors.AddRange(await _validator.ValidateAsync(integration, token));

            if (string.IsNullOrEmpty(integration.Name))
                errors.Add(new ValidationFailure(nameof(AddIntegration.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_REQUIRED));

            if (await IsDuplicationNameAsync(integration.Name, new Guid()))
                errors.Add(new ValidationFailure(nameof(AddIntegration.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED));

            if (errors.Any())
                throw EntityValidationExceptionHelper.GenerateException(errors);

            var entity = AddIntegration.Create(integration);
            entity.CreatedUtc = DateTime.UtcNow;
            entity.UpdatedUtc = DateTime.UtcNow;
            await _unitOfWork.Integrations.AddAsync(entity);
            return entity.Id;
        }

        private void RemoveConfidentialInfo(ArchiveIntegrationDto integration)
        {
            string saltKey = _configuration[ConfigurationKeys.SALT_KEY];
            var content = JsonConvert.DeserializeObject<Dictionary<string, object>>(integration.Content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            switch (integration.Type)
            {
                case IntegrationTypeConstants.INTEGRATION_EVENT_HUB:
                    content = content.Where(x => IntegrationContentKeys.EVENT_HUB_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                    if (content[IntegrationContentKeys.CONNECTION_STRING] != null)
                        content[IntegrationContentKeys.CONNECTION_STRING] = content[IntegrationContentKeys.CONNECTION_STRING].ToString().Base64Encode(saltKey);
                    break;
                case IntegrationTypeConstants.WAY_LAY:
                    content = content.Where(x => IntegrationContentKeys.WAYLAY_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                    if (content[IntegrationContentKeys.API_KEY] != null)
                        content[IntegrationContentKeys.API_KEY] = content[IntegrationContentKeys.API_KEY].ToString().Base64Encode(saltKey);
                    if (content[IntegrationContentKeys.API_SECRET] != null)
                        content[IntegrationContentKeys.API_SECRET] = content[IntegrationContentKeys.API_SECRET].ToString().Base64Encode(saltKey);
                    break;
                case IntegrationTypeConstants.GREEN_KONCEPT:
                    content = content.Where(x => IntegrationContentKeys.GREEN_KONCEPT_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

                    if (content[IntegrationContentKeys.CLIENT_ID] != null)
                        content[IntegrationContentKeys.CLIENT_ID] = content[IntegrationContentKeys.CLIENT_ID].ToString().Base64Encode(saltKey);
                    if (content[IntegrationContentKeys.CLIENT_SECRET] != null)
                        content[IntegrationContentKeys.CLIENT_SECRET] = content[IntegrationContentKeys.CLIENT_SECRET].ToString().Base64Encode(saltKey);
                    if (content[IntegrationContentKeys.ENDPOINT] != null)
                        content[IntegrationContentKeys.ENDPOINT] = content[IntegrationContentKeys.ENDPOINT].ToString().Base64Encode(saltKey);
                    break;
                default:
                    break;
            }
            integration.Content = JsonConvert.SerializeObject(content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
        }
    }
}
