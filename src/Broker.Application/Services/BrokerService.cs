using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using AHI.Infrastructure.Audit.Constant;
using AHI.Infrastructure.Audit.Service.Abstraction;
using AHI.Infrastructure.Bus.ServiceBus.Abstraction;
using AHI.Infrastructure.Cache.Abstraction;
using AHI.Infrastructure.Exception;
using AHI.Infrastructure.Exception.Helper;
using AHI.Infrastructure.MultiTenancy.Abstraction;
using AHI.Infrastructure.MultiTenancy.Extension;
using AHI.Infrastructure.Service;
using AHI.Infrastructure.Service.Tag.Extension;
using AHI.Infrastructure.Service.Tag.Service.Abstraction;
using AHI.Infrastructure.SharedKernel.Extension;
using AHI.Infrastructure.SharedKernel.Model;
using AHI.Infrastructure.UserContext.Abstraction;
using Broker.Application.Constant;
using Broker.Application.Constants;
using Broker.Application.Event;
using Broker.Application.Handler.Command;
using Broker.Application.Handler.Command.Model;
using Broker.Application.Helper;
using Broker.Application.Models;
using Broker.Application.Repository;
using Broker.Application.Repository.Abstraction;
using Broker.Application.Service.Abstraction;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Broker.Application.Service
{
    public class BrokerService : BaseSearchService<Domain.Entity.Broker, Guid, SearchBroker, BrokerDto>, IBrokerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly IAuditLogService _auditLogService;
        private readonly IBrokerValidator _validator;
        private readonly ILookupService _lookupService;
        private readonly IFileEventService _fileEventService;
        private readonly ICache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IUserContext _userContext;
        private readonly IValidator<ArchiveBrokerDto> _brokerVerifyValidator;
        private readonly IDictionary<string, IContentVerificationHandler> _verifyHandler;
        private readonly IDomainEventDispatcher _dispatcher;
        private readonly ITagService _tagService;

        public BrokerService(IUnitOfWork unitOfWork,
                ITenantContext tenantContext,
                IServiceProvider serviceProvider,
                IAuditLogService auditLogService,
                IBrokerValidator validator,
                ILookupService lookupService,
                IFileEventService fileEventService,
                ICache cache,
                IHttpClientFactory httpClientFactory,
                IConfiguration configuration,
                IValidator<ArchiveBrokerDto> brokerVerifyValidator,
                IDictionary<string, IContentVerificationHandler> verifyHandler,
                IDomainEventDispatcher dispatcher,
                IUserContext userContext,
                ITagService tagService) : base(BrokerDto.Create, serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _auditLogService = auditLogService;
            _validator = validator;
            _lookupService = lookupService;
            _fileEventService = fileEventService;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _userContext = userContext;
            _verifyHandler = verifyHandler;
            _brokerVerifyValidator = brokerVerifyValidator;
            _dispatcher = dispatcher;
            _tagService = tagService;
        }

        public async Task<BrokerDto> AddBrokerAsync(AddBroker command, CancellationToken token)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {   //process  lookup
                _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(command.Type, token);

                if (await IsDuplicationBrokerNameAsync(command.Name, Guid.Empty))
                    throw EntityValidationExceptionHelper.GenerateException(nameof(AddBroker.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED);
                await _validator.ValidateAsync(command, token);
                ValidateDetails(command.Details);

                var tagIds = Array.Empty<long>();
                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(_userContext.ApplicationId ?? ApplicationInformation.APPLICATION_ID);
                if (command.Tags != null && command.Tags.Any())
                {
                    tagIds = await _tagService.UpsertTagsAsync(command);
                }

                bool isEmqxBroker = BrokerTypeConstants.EMQX_BROKERS.Contains(command.Type);
                if (isEmqxBroker)
                {
                    ProcessEmqxBroker(command);
                }
                var existingEmqxBroker = await _unitOfWork.Brokers.AsQueryable()
                                                                  .AsNoTracking()
                                                                  .AnyAsync(x => BrokerTypeConstants.EMQX_BROKERS.Contains(x.Type));

                var entity = AddBroker.Create(command);
                entity.CreatedBy = _userContext.Upn;
                entity.ResourcePath = string.Format(ObjectBaseConstants.RESOURCE_PATH, entity.Id);
                entity.Status = isEmqxBroker ? BrokerStatusConstants.ACTIVE : BrokerStatusConstants.INACTIVE;
                if (command.Details.ContainsKey(BrokerContentKeys.ENABLE_SHARING))
                {
                    entity.IsShared = Convert.ToBoolean(command.Details[BrokerContentKeys.ENABLE_SHARING]);
                }
                await _unitOfWork.Brokers.AddAsync(entity, command.Details);
                var entityId = EntityTagHelper.GetEntityId(entity.Id);
                entity.EntityTags = EntityTagHelper.GetEntityTags(FileEntityConstants.BROKER, tagIds, entityId);
                await _unitOfWork.CommitAsync();

                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Add, ActionStatus.Success, entity.Id, entity.Name, command);
                bool requestDeploy = isEmqxBroker && !existingEmqxBroker;
                await _dispatcher.SendAsync(new BrokerChangedEvent(entity.Id, entity.Name, entity.Type, _tenantContext, requestDeploy, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Created));
                return BrokerDto.Create(entity);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Add, ActionStatus.Fail, payload: command);
                throw;
            }
        }

        private void ProcessEmqxBroker(AddBroker command)
        {
            string mqttHost = _configuration[BrokerConfig.EMQX_MQTT_CONNECTION_STRING];
            string coapHost = _configuration[BrokerConfig.EMQX_COAP_CONNECTION_STRING];
            switch (command.Type)
            {
                case BrokerTypeConstants.EMQX_MQTT:
                    command.Details.TryAdd(BrokerContentKeys.HOST, mqttHost);
                    break;
                case BrokerTypeConstants.EMQX_COAP:
                    command.Details.TryAdd(BrokerContentKeys.HOST, $"coap://{coapHost}:5683/ps/coap");
                    break;
                default:
                    throw new SystemNotSupportedException();
            }
        }

        private BrokerDto ProcessEmqxBroker(BrokerDto command)
        {
            var brokerContent = JsonConvert.DeserializeObject<JObject>(command.Content);
            if (brokerContent == null)
                return command;

            var hostUri = brokerContent[BrokerContentKeys.HOST];

            if (hostUri == null)
            {
                string mqttHost = _configuration[BrokerConfig.EMQX_MQTT_CONNECTION_STRING];
                string coapHost = _configuration[BrokerConfig.EMQX_COAP_CONNECTION_STRING];
                switch (command.Type)
                {
                    case BrokerTypeConstants.EMQX_MQTT:
                        brokerContent.TryAdd(BrokerContentKeys.HOST, mqttHost);
                        break;
                    case BrokerTypeConstants.EMQX_COAP:
                        brokerContent.TryAdd(BrokerContentKeys.HOST, $"coap://{coapHost}:5683/ps/coap");
                        break;
                    default:
                        throw new SystemNotSupportedException();
                }
            }
            command.Content = JsonConvert.SerializeObject(brokerContent);
            return command;
        }

        public async Task<BrokerDto> UpdateBrokerAsync(UpdateBroker command, CancellationToken token)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(command.Type, token);
                var brokerDB = await _unitOfWork.Brokers.AsQueryable().Where(x => x.Id == command.Id).FirstOrDefaultAsync();
                if (brokerDB == null)
                {
                    throw new EntityNotFoundException(command.Id.ToString());
                }
                //Validate type valid broker, can't change type Event-Hub to IoT-Hub
                if (brokerDB.Type != command.Type)
                {
                    throw new EntityInvalidException(command.Id.ToString());
                }
                if (await IsDuplicationBrokerNameAsync(command.Name, command.Id))
                    throw EntityValidationExceptionHelper.GenerateException(nameof(UpdateBroker.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED);
                await _validator.ValidateAsync(command, token);
                ValidateDetails(command.Details);

                var oldDetail = JObject.Parse(brokerDB.Detail.Content);
                //US 8322: regenerate Sas token when new duration
                if (command.Type == BrokerTypeConstants.EVENT_HUB
                    && (!oldDetail.ContainsKey(BrokerContentKeys.SAS_TOKEN_DURATION) || oldDetail[BrokerContentKeys.SAS_TOKEN_DURATION].ToString() != command.Details[BrokerContentKeys.SAS_TOKEN_DURATION].ToString()))
                {
                    var ruleUri = $"{oldDetail[BrokerContentKeys.EVENT_HUB_ID].ToString()}/authorizationRules/Send/listKeys?api-version={AzureContants.AZURE_API_VERSION}";
                    var azureClient = _httpClientFactory.CreateClient("azure-service");
                    var response = await azureClient.PostAsync(ruleUri, new StringContent("{}", System.Text.Encoding.UTF8, "appplication/json"));
                    if (response.IsSuccessStatusCode)
                    {
                        var payload = await response.Content.ReadAsByteArrayAsync();
                        var keyResponse = payload.Deserialize<AzureKeyResponse>();
                        string resourceUri = $"{command.Details[BrokerContentKeys.CONNECTION_STRING].ToString().Split(';')[0].Split("//")[1]}{command.Details[BrokerContentKeys.EVENT_HUB_NAME]}";
                        command.Details[BrokerContentKeys.SAS_TOKEN] = GenerateSasToken(resourceUri, "Send", keyResponse.PrimaryKey, Int32.Parse(command.Details[BrokerContentKeys.SAS_TOKEN_DURATION].ToString()));
                    }
                }

                var entity = UpdateBroker.Create(command);
                command.Upn = _userContext.Upn;
                command.ApplicationId = Guid.Parse(_userContext.ApplicationId ?? ApplicationInformation.APPLICATION_ID);

                if (string.IsNullOrEmpty(command.Status))
                    entity.Status = brokerDB.Status;
                if (command.Details.ContainsKey(BrokerContentKeys.ENABLE_SHARING))
                {
                    entity.IsShared = Convert.ToBoolean(command.Details[BrokerContentKeys.ENABLE_SHARING]);
                }
                var isSameTag = command.IsSameTags(brokerDB.EntityTags);
                if (!isSameTag)
                {
                    await _unitOfWork.EntityTags.RemoveByEntityIdAsync(FileEntityConstants.BROKER, brokerDB.Id, true);

                    var tagIds = await _tagService.UpsertTagsAsync(command);
                    if (tagIds.Any())
                    {
                        var entityTagId = EntityTagHelper.GetEntityId(command.Id);
                        var entitiesTags = EntityTagHelper.GetEntityTags(FileEntityConstants.BROKER, tagIds, entityTagId).ToArray();
                        await _unitOfWork.EntityTags.AddRangeWithSaveChangeAsync(entitiesTags);
                    }
                }

                await _unitOfWork.Brokers.UpdateAsync(entity.Id, entity);
                await _unitOfWork.CommitAsync();
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Update, ActionStatus.Success, entity.Id, entity.Name, command);
                var requestDeploy = command.Details.ContainsKey(BrokerContentKeys.CONNECTION_STRING) && !string.IsNullOrEmpty(command.Details[BrokerContentKeys.CONNECTION_STRING].ToString());

                await _dispatcher.SendAsync(new BrokerChangedEvent(entity.Id, entity.Name, entity.Type, _tenantContext, requestDeploy, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Updated));

                var key = GetCacheKey(command.Id);
                await _cache.DeleteAsync(key);

                return BrokerDto.Create(entity);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Update, ActionStatus.Fail, command.Id, command.Name, payload: command);
                throw;
            }
        }

        public async Task<BaseResponse> DeleteBrokerAsync(DeleteBrokerById command, CancellationToken token)
        {
            var broker = await _unitOfWork.Brokers.AsQueryable().AsNoTracking().FirstOrDefaultAsync(x => x.Id == command.Id);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (broker == null)
                {
                    throw new EntityNotFoundException(command.Id.ToString());
                }
                BaseResponse response = null;
                var success = await _unitOfWork.Brokers.RemoveAsync(command.Id);
                await _unitOfWork.EntityTags.RemoveByEntityIdAsync(FileEntityConstants.BROKER, command.Id, true);
                await _unitOfWork.CommitAsync();
                response = success ? BaseResponse.Success : BaseResponse.Failed;

                var key = GetCacheKey(command.Id);
                await _cache.DeleteAsync(key);

                await RemoveEmqxBrokersAsync(new List<Guid> { command.Id });
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Delete, success ? ActionStatus.Success : ActionStatus.Fail, broker.Id, broker.Name, command);
                await _dispatcher.SendAsync(new BrokerChangedEvent(command.Id, name: broker.Name, type: broker.Type, _tenantContext, false, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Deleted));

                return response;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Delete, ActionStatus.Fail, command.Id, broker?.Name, command);
                throw;
            }
        }

        public async Task<BaseResponse> DeleteBrokersAsync(DeleteBroker command, CancellationToken token)
        {
            var distincIds = command.Ids.Distinct().ToList();
            var brokers = await _unitOfWork.Brokers.AsQueryable().AsNoTracking().Where(x => distincIds.Contains(x.Id) && !x.Deleted).ToListAsync();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var foundIds = brokers.Select(x => x.Id).ToList();
                if (distincIds.Count > foundIds.Count)
                {
                    throw new EntityNotFoundException(detailCode: ExceptionErrorCode.DetailCode.ERROR_VALIDATION_SOME_ITEMS_DELETED);
                }
                var success = await _unitOfWork.Brokers.RemoveBrokersAsync(distincIds);
                foreach (var id in distincIds)
                {
                    await _unitOfWork.EntityTags.RemoveByEntityIdAsync(FileEntityConstants.BROKER, id, true);
                }

                await _unitOfWork.CommitAsync();
                BaseResponse response = success ? BaseResponse.Success : BaseResponse.Failed;
                var keys = distincIds.Select(id => GetCacheKey(id));
                var tasks = keys.Select(key => _cache.DeleteAsync(key));
                await Task.WhenAll(tasks);

                await RemoveEmqxBrokersAsync(distincIds);
                foreach (var id in distincIds)
                {
                    var broker = brokers.FirstOrDefault(x => x.Id == id);
                    await _dispatcher.SendAsync(new BrokerChangedEvent(id, name: broker.Name, type: broker.Type, _tenantContext, false, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Deleted));
                }
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Delete, success ? ActionStatus.Success : ActionStatus.Fail, distincIds, brokers.Select(x => x.Name), command);
                return response;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Delete, ActionStatus.Fail, distincIds, brokers.Select(x => x.Name), command);
                throw;
            }
        }

        private async Task RemoveEmqxBrokersAsync(IEnumerable<Guid> brokerIds)
        {
            if (!brokerIds.Any())
                return;

            var brokerFunction = _httpClientFactory.CreateClient(HttpClientNames.BROKER_FUNCTION, _tenantContext);
            var requestBody = new Dictionary<string, object>
            {
                {"brokerIds", brokerIds}
            };
            var response = await brokerFunction.PostAsync("fnc/bkr/emqx/remove/brokers", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
        }

        public async Task<BrokerDto> FindByIdAsync(GetBrokerById command, CancellationToken token)
        {
            var key = GetCacheKey(command.Id);
            var brokerDto = await _cache.GetAsync<BrokerDto>(key);
            if (brokerDto == null)
            {
                var brokers = _unitOfWork.Brokers.AsQueryable();
                brokers = brokers.Include(x => x.Detail).Include(x => x.EntityTags).AsNoTracking();

                if (command.IncludeDeletedRecords)
                    brokers = brokers.IgnoreQueryFilters();
                else
                    brokers = brokers.Where(x => !x.Deleted);

                var entity = await brokers.FirstOrDefaultAsync(x => x.Id == command.Id);
                if (entity == null)
                    throw new EntityNotFoundException(command.Id.ToString());

                brokerDto = await _tagService.FetchTagsAsync(BrokerDto.Create(entity));

                if (BrokerTypeConstants.EMQX_BROKERS.Contains(brokerDto.Type))
                {
                    brokerDto = ProcessEmqxBroker(brokerDto);
                }

                if (!entity.Deleted)
                {
                    // only store the broker whick not deleted
                    await _cache.StoreAsync(key, brokerDto);
                }
            }
            return await _tagService.FetchTagsAsync(brokerDto);
        }

        public async Task<ActivityResponse> ExportAsync(ExportBroker request, CancellationToken cancellationToken)
        {
            try
            {
                var entities = new CheckExistBroker(request.Ids.Select(x => Guid.Parse(x)));
                await ValidateExistBrokersAsync(entities, cancellationToken);
                await _fileEventService.SendExportEventAsync(request.ActivityId, request.ObjectType, request.Ids);
                return new ActivityResponse(request.ActivityId);
            }
            catch
            {
                await _auditLogService.SendLogAsync(ActivityLogEntityContants.BROKER_ENTITY, ActionType.Export, ActionStatus.Fail, payload: request);
                throw;
            }
        }

        public override async Task<BaseSearchResponse<BrokerDto>> SearchAsync(SearchBroker criteria)
        {
            criteria.MappingSearchTags();
            var response = await base.SearchAsync(criteria);
            return await _tagService.FetchTagsAsync(response);
        }

        protected override Type GetDbType()
        {
            return typeof(IBrokerRepository);
        }

        private string GetCacheKey(Guid brokerId)
        {
            return $"{_tenantContext.TenantId}_{_tenantContext.SubscriptionId}_{_tenantContext.ProjectId}_broker_{brokerId}".ToLowerInvariant();
        }

        private Task<bool> IsDuplicationBrokerNameAsync(string name, Guid id)
        {
            if (!id.Equals(Guid.Empty))
                return _unitOfWork.Brokers.AsQueryable().Where(x => x.Name.ToLower() == name.ToLower()).Where(x => x.Id != id).AnyAsync();
            else
                return _unitOfWork.Brokers.AsQueryable().Where(x => x.Name.ToLower() == name.ToLower()).AnyAsync();
        }

        public Task<BaseResponse> CheckExistBrokersAsync(CheckExistBroker brokers, CancellationToken cancellationToken)
        {
            return ValidateExistBrokersAsync(brokers, cancellationToken);
        }

        public async Task<IEnumerable<ArchiveBrokerDto>> ArchiveAsync(ArchiveBroker command, CancellationToken token)
        {
            var searchInput = new SearchBroker
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

            var brokers = await this.SearchAsync(searchInput);
            return brokers.Data.Select(broker =>
            {
                var result = ArchiveBrokerDto.Create(broker);
                RemoveConfidentialInfo(result);

                return result;
            });
        }

        public async Task<IDictionary<string, object>> RetrieveAsync(RetrieveBroker command, CancellationToken token)
        {
            var addedIds = new List<BrokerIdDto>();
            _userContext.SetUpn(command.Upn);
            var brokers = JsonConvert.DeserializeObject<IEnumerable<ArchiveBrokerDto>>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                brokers = brokers.OrderBy(x => x.UpdatedUtc);
                var duplicatedBrokers = brokers.GroupBy(x => x.Name)
                                               .Select(x => new { x.Key, Items = x.Skip(1).Select((b, i) => new { broker = b, index = i + 1 }) })
                                               .Where(x => x.Items.Any())
                                               .ToList();
                foreach (var broker in brokers)
                {
                    var duplicatedGroup = duplicatedBrokers.FirstOrDefault(x => x.Key == broker.Name);
                    if (duplicatedGroup != null)
                    {
                        var currentBroker = duplicatedGroup.Items.FirstOrDefault(x => x.broker.Id == broker.Id);
                        if (currentBroker != null)
                        {
                            broker.Name = $"{broker.Name} {currentBroker.index}";
                        }
                    }

                    var input = new AddBroker
                    {
                        Name = broker.Name,
                        Type = broker.Type,
                        Details = JsonConvert.DeserializeObject<Dictionary<string, object>>(broker.Content),
                    };

                    var newId = await ProcessRetrieveBrokersAsync(input, token);
                    addedIds.Add(new BrokerIdDto(broker.Id, newId));
                }
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            var dispatchTasks = brokers.Select(broker => _dispatcher.SendAsync(new BrokerChangedEvent(addedIds.Find(x => x.OldId == broker.Id).NewId, broker.Name, broker.Type, _tenantContext, false, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Created)));

            // Trigger listeners
            await _dispatcher.SendAsync(new BrokerChangedEvent(Guid.Empty, string.Empty, string.Empty, _tenantContext, true, AHI.Infrastructure.Bus.ServiceBus.Enum.ActionTypeEnum.Created));
            await Task.WhenAll(dispatchTasks);
            return addedIds.ToDictionary(x => x.OldId.ToString(), x => (object)x.NewId);
        }

        private async Task<Guid> ProcessRetrieveBrokersAsync(
            AddBroker broker,
            CancellationToken token)
        {
            _ = await _lookupService.ProcessLookUpFromConfigurationServiceAsync(broker.Type, token);

            if (await IsDuplicationBrokerNameAsync(broker.Name, Guid.Empty))
                throw EntityValidationExceptionHelper.GenerateException(nameof(AddBroker.Name), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_DUPLICATED);
            await _validator.ValidateAsync(broker, token);
            ValidateMaximumThroughput(broker.Details);

            bool isEmqxBroker = BrokerTypeConstants.EMQX_BROKERS.Contains(broker.Type);
            if (isEmqxBroker)
            {
                ProcessEmqxBroker(broker);
            }

            var entity = AddBroker.Create(broker);
            entity.CreatedBy = _userContext.Upn;
            entity.CreatedUtc = DateTime.UtcNow;
            entity.UpdatedUtc = DateTime.UtcNow;
            entity.ResourcePath = string.Format(ObjectBaseConstants.RESOURCE_PATH, entity.Id);
            entity.Status = isEmqxBroker ? BrokerStatusConstants.ACTIVE : BrokerStatusConstants.INACTIVE;
            if (broker.Details.ContainsKey(BrokerContentKeys.ENABLE_SHARING))
            {
                entity.IsShared = Convert.ToBoolean(broker.Details[BrokerContentKeys.ENABLE_SHARING]);
            }
            await _unitOfWork.Brokers.AddAsync(entity, broker.Details);
            return entity.Id;
        }

        public async Task<BaseResponse> VerifyArchiveDataAsync(VerifyBroker command, CancellationToken token)
        {
            var brokers = JsonConvert.DeserializeObject<IEnumerable<ArchiveBrokerDto>>(command.Data, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            foreach (var broker in brokers)
            {
                var validation = await _brokerVerifyValidator.ValidateAsync(broker);
                if (!validation.IsValid)
                {
                    throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
                }

                var content = JsonConvert.DeserializeObject<IDictionary<string, object>>(broker.Content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
                var isValid = _verifyHandler[broker.Type].Handle(content);
                if (!isValid)
                    throw EntityValidationExceptionHelper.GenerateException(nameof(command.Data), ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
            }

            return BaseResponse.Success;
        }

        private void RemoveConfidentialInfo(ArchiveBrokerDto broker)
        {
            var content = JsonConvert.DeserializeObject<Dictionary<string, object>>(broker.Content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
            switch (broker.Type)
            {
                case BrokerTypeConstants.EMQX_COAP:
                    content = content.Where(x => BrokerContentKeys.COAP_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case BrokerTypeConstants.EMQX_MQTT:
                    content = content.Where(x => BrokerContentKeys.MQTT_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case BrokerTypeConstants.EVENT_HUB:
                    content = content.Where(x => BrokerContentKeys.EVENT_HUB_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case BrokerTypeConstants.IOT_HUB:
                    content = content.Where(x => BrokerContentKeys.IOT_HUB_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    break;
                case BrokerTypeConstants.REST_API:
                    content = content.Where(x => BrokerContentKeys.REST_API_KEYS.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    break;
                default:
                    break;
            }
            broker.Content = JsonConvert.SerializeObject(content, AHI.Infrastructure.SharedKernel.Extension.Constant.JsonSerializerSetting);
        }

        private async Task<BaseResponse> ValidateExistBrokersAsync(CheckExistBroker brokers, CancellationToken cancellationToken)
        {
            var requestIds = brokers.Ids.Distinct();
            var count = await _unitOfWork.Brokers.AsQueryable().AsNoTracking().Where(x => requestIds.Contains(x.Id)).CountAsync();
            if (count < requestIds.Count())
            {
                throw new EntityNotFoundException(MessageConstants.BROKER_NOT_FOUND);
            }
            return BaseResponse.Success;
        }

        private string GenerateSasToken(string resourceUri, string keyName, string key, int duration)
        {
            TimeSpan sinceEpoch = DateTime.UtcNow - new DateTime(1970, 1, 1);
            var timeExpired = TimeSpan.FromDays(duration);
            var expiry = Convert.ToString((int)sinceEpoch.TotalSeconds + timeExpired.TotalSeconds);
            string stringToSign = HttpUtility.UrlEncode(resourceUri) + "\n" + expiry;
            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var signature = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
            var sasToken = String.Format(CultureInfo.InvariantCulture, "SharedAccessSignature sr={0}&sig={1}&se={2}&skn={3}", HttpUtility.UrlEncode(resourceUri), HttpUtility.UrlEncode(signature), expiry, keyName);
            return sasToken;
        }

        private void ValidateDetails(IDictionary<string, object> details)
        {
            ValidateMaximumThroughput(details);
            ValidatePasswordLength(details);
        }

        private void ValidateMaximumThroughput(IDictionary<string, object> details)
        {
            if (details.ContainsKey(EventHubKeyConstants.AutoInflate) && details[EventHubKeyConstants.Tier].ToString() == AzureEventHubTier.Standard)
            {
                bool isAutoInflate = bool.Parse(details[EventHubKeyConstants.AutoInflate].ToString());
                int maximumThroughputUnits = int.Parse(details[EventHubKeyConstants.MaxThroughputUnit].ToString());
                if (!isAutoInflate && maximumThroughputUnits > 0)
                    throw EntityValidationExceptionHelper.GenerateException(EventHubKeyConstants.AutoInflate, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
            }
        }

        private void ValidatePasswordLength(IDictionary<string, object> details)
        {
            if (details.ContainsKey(BrokerContentKeys.PASSWORD_LENGTH))
            {
                if (int.TryParse(details[BrokerContentKeys.PASSWORD_LENGTH].ToString(), out int result))
                {
                    if (result < BrokerConfig.MINIMUM_PASSWORD_LENGTH || result > BrokerConfig.MAXIMUM_PASSWORD_LENGTH)
                    {
                        throw EntityValidationExceptionHelper.GenerateException(BrokerContentKeys.PASSWORD_LENGTH, ExceptionErrorCode.DetailCode.ERROR_VALIDATION_INVALID);
                    }
                }
            }
        }
    }
}
