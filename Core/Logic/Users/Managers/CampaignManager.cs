using ApiLogic.Api.Managers;
using APILogic.ConditionalAccess;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using DAL;
using EventBus.Abstraction;
using GroupsCacheManager;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TVinciShared;
using Campaign = ApiObjects.Campaign;

namespace ApiLogic.Users.Managers
{
    public class CampaignManager
    {
        private const int MAX_TRIGGER_CAMPAIGNS = 100;
        private const int MAX_BATCH_CAMPAIGNS = 100;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CampaignManager> lazy = new Lazy<CampaignManager>(() =>
            new CampaignManager(LayeredCache.Instance,
                                PricingDAL.Instance,
                                ChannelManager.Instance,
                                EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration(),
                                CatalogManager.Instance,
                                GroupsCache.Instance(),
                                ConditionValidator.Instance,
                                PromotionValidator.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ILayeredCache _layeredCache;
        private readonly ICampaignRepository _repository;
        private readonly IChannelManager _channelManager;
        private readonly IEventBusPublisher _eventBusPublisher;
        private readonly ICatalogManager _catalogManager;
        private readonly IGroupsCache _groupsCache;
        private readonly IConditionValidator _conditionValidator;
        private readonly IPromotionValidator _promotionValidator;

        public static CampaignManager Instance { get { return lazy.Value; } }

        public CampaignManager(ILayeredCache layeredCache,
                               ICampaignRepository repository,
                               IChannelManager channelManager,
                               IEventBusPublisher eventBusPublisher,
                               ICatalogManager catalogManager,
                               IGroupsCache groupsCache,
                               IConditionValidator conditionValidator,
                               IPromotionValidator promotionValidator)
        {
            _layeredCache = layeredCache;
            _repository = repository;
            _channelManager = channelManager;
            _eventBusPublisher = eventBusPublisher;
            _catalogManager = catalogManager;
            _groupsCache = groupsCache;
            _conditionValidator = conditionValidator;
            _promotionValidator = promotionValidator;
        }

        public Status Delete(ContextData contextData, long id)
        {
            var response = Status.Error;

            try
            {
                var campaignToDeleteResponse = Get(contextData, id, true);
                if (!campaignToDeleteResponse.HasObject())
                {
                    response.Set(campaignToDeleteResponse.Status);
                    return response;
                }

                if (campaignToDeleteResponse.Object.State != CampaignState.INACTIVE)
                {
                    response.Set(eResponseStatus.CanDeleteOnlyInactiveCampaign, "Can delete only inactive campaign");
                    return response;
                }

                if (!_repository.DeleteCampaign(contextData.GroupId, id))
                {
                    response.Set(eResponseStatus.Error, "Error while deleting Campaign");
                    return response;
                }

                SetInvalidationKeys(contextData, campaignToDeleteResponse.Object);
                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Error while delete Campaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> Get(ContextData contextData, long id)
        {
            return Get(contextData, id, true);
        }

        private GenericResponse<Campaign> Get(ContextData contextData, long id, bool lazySetState)
        {
            var response = new GenericResponse<Campaign>();

            Campaign _campaign = null;

            try
            {
                var key = LayeredCacheKeys.GetCampaignKey(contextData.GroupId, id);
                var cacheResult = _layeredCache.Get(key,
                                                    ref _campaign,
                                                    Get_CampaignsByIdDB,
                                                    new Dictionary<string, object>()
                                                    {
                                                            { "groupId", contextData.GroupId },
                                                            { "campaignId", id }
                                                    },
                                                    contextData.GroupId,
                                                    LayeredCacheConfigNames.GET_CAMPAIGN_BY_ID,
                                                    new List<string>() { LayeredCacheKeys.GetCampaignInvalidationKey(contextData.GroupId, id) },
                                                    true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to Get Campaign contextData: {contextData}, id: {id} ex: {ex}", ex);
            }

            if (_campaign != null)
            {
                // lazy update for state
                var now = DateUtils.GetUtcUnixTimestampNow();
                if (lazySetState && _campaign.State == CampaignState.ACTIVE && _campaign.EndDate < now)
                {
                    _campaign.State = CampaignState.ARCHIVE;

                    Task.Run(() =>
                    {
                        _campaign.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                        if (_repository.Update_Campaign(_campaign, contextData))
                        {
                            SetInvalidationKeys(contextData, _campaign);
                            log.Debug($"success to set campaign:[${id}] state to archive.");
                        }
                        else
                        {
                            log.Error($"error while set campaign:[${id}] state to archive.");
                        }
                    });
                }

                response.Object = _campaign;
                response.SetStatus(eResponseStatus.OK);
            }
            else
            {
                response.SetStatus(eResponseStatus.CampaignDoesNotExist, $"Campaign: {id} not found");
            }

            return response;
        }

        public void NotifyTriggerCampaignEvent(TriggerCampaign campaign, ContextData contextData)
        {
            try
            {
                var eventObject = new TriggerCampaignEvent
                {
                    GroupId = contextData.GroupId,
                    DomainId = contextData.DomainId ?? 0,
                    UserId = contextData.UserId ?? 0,
                    Udid = contextData.Udid,
                    CampaignId = campaign.Id
                };

                if (eventObject.Notify())
                {
                    log.Debug($"Event for campaign: {campaign.Id}, contextData: {contextData} was added successfully");
                }
                else
                {
                    log.Warn($"Event for campaign: {campaign.Id}, contextData: {contextData} was ended with a failure");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new Campaign: {campaign.Id} Event. contextData: {contextData}, ex: {ex}", ex);
            }
        }

        public GenericListResponse<TriggerCampaign> ListTriggerCampaigns(ContextData contextData, TriggerCampaignFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<TriggerCampaign>();

            response.Objects = ListCampaignsByType<TriggerCampaign>(contextData, filter, eCampaignType.Trigger);

            if (response.Objects == null)
            {
                response.SetStatus(eResponseStatus.Error, "error while searching trigger campaigns");
                return response;
            }

            response.SetStatus(eResponseStatus.OK);

            if (filter.Service.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Service == filter.Service.Value).ToList();
            }

            if (filter.Action.HasValue)
            {
                response.Objects = response.Objects.Where(x => x.Action == filter.Action.Value).ToList();
            }

            response.Objects = filter.ApplyOrderBy(response.Objects);
            ManagePagination(pager, response);

            return response;
        }

        public GenericListResponse<BatchCampaign> ListBatchCampaigns(ContextData contextData, BatchCampaignFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<BatchCampaign>();

            response.Objects = ListCampaignsByType<BatchCampaign>(contextData, filter, eCampaignType.Batch);

            if (response.Objects == null)
            {
                response.SetStatus(eResponseStatus.Error, "error while searching batch campaigns");
                return response;
            }

            response.SetStatus(eResponseStatus.OK);

            response.Objects = filter.ApplyOrderBy(response.Objects);
            ManagePagination(pager, response);

            return response;
        }

        public GenericListResponse<Campaign> ListCampaignsBySegment(ContextData contextData, CampaignSegmentFilter filter)
        {
            var response = new GenericListResponse<Campaign>();
            response.SetStatus(Status.Ok);

            var filteredCampaigns = SearchCampaigns(contextData, filter);

            if (filteredCampaigns.HasObjects())
            {
                foreach (var campaign in filteredCampaigns.Objects)
                {
                    var conditions = campaign.GetConditions();
                    if (ConditionsContainsValidator.ValidateSegmentExist(conditions, filter.SegmentIdEqual))
                    {
                        response.Objects.Add(campaign);
                    }
                }
            }
            response.TotalItems = response.Objects.Count;

            return response;
        }

        private void ManagePagination<T>(CorePager pager, GenericListResponse<T> response) where T : Campaign
        {
            if (pager?.PageSize > 0)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            }
        }

        public GenericListResponse<Campaign> ListCampaingsByIds(ContextData contextData, CampaignIdInFilter filter)
        {
            var response = new GenericListResponse<Campaign>();
            if (filter.IdIn?.Count > 0)
            {
                response.Objects = filter.IdIn.Select(id => Get(contextData, id, true)).Where(campaignResponse => campaignResponse.HasObject()).Select(x => x.Object).ToList();
                if (!filter.IsAllowedToViewInactiveCampaigns)
                {
                    response.Objects = response.Objects.Where(x => x.State != CampaignState.INACTIVE).ToList();
                }
            }

            response.Objects = filter.ApplyOrderBy(response.Objects);

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<Campaign> SearchCampaigns(ContextData contextData, CampaignSearchFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<Campaign>();

            var triggerCampaigns = ListCampaignsByType<TriggerCampaign>(contextData, filter, eCampaignType.Trigger);
            if (triggerCampaigns?.Count > 0)
            {
                response.Objects.AddRange(triggerCampaigns);
            }

            var batchCampaigns = ListCampaignsByType<BatchCampaign>(contextData, filter, eCampaignType.Batch);
            if (batchCampaigns?.Count > 0)
            {
                response.Objects.AddRange(batchCampaigns);
            }

            response.SetStatus(eResponseStatus.OK);

            if (response.Objects?.Count > 0)
            {
                response.Objects = filter.ApplyOrderBy(response.Objects);
                ManagePagination(pager, response);
            }

            return response;
        }

        public GenericResponse<T> AddCampaign<T>(ContextData contextData, T campaignToAdd) where T : Campaign, new()
        {
            var response = new GenericResponse<T>();

            try
            {
                var validateStatus = ValidateCampaign(contextData, campaignToAdd);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToAdd.State = CampaignState.INACTIVE;
                campaignToAdd.CreateDate = DateUtils.GetUtcUnixTimestampNow();
                campaignToAdd.UpdateDate = campaignToAdd.CreateDate;

                var insertedCampaign = _repository.AddCampaign(campaignToAdd, contextData);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData, insertedCampaign);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, $"Error while saving {campaignToAdd.type} Campaign");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new {campaignToAdd.type} Campaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<TriggerCampaign> UpdateTriggerCampaign(ContextData contextData, TriggerCampaign campaignToUpdate)
        {
            var response = new GenericResponse<TriggerCampaign>();
            try
            {
                var oldTriggerCampaignResponse = Get(contextData, campaignToUpdate.Id, true);
                if (!oldTriggerCampaignResponse.HasObject())
                {
                    response.SetStatus(oldTriggerCampaignResponse.Status);
                    return response;
                }

                if (oldTriggerCampaignResponse.Object.CampaignType != eCampaignType.Trigger)
                {
                    response.SetStatus(eResponseStatus.Error, $"invalid campaign type for update");
                    return response;
                }

                if (oldTriggerCampaignResponse.Object.State != CampaignState.INACTIVE)
                {
                    response.SetStatus(eResponseStatus.CampaignUpdateNotAllowed, $"Can't update this campaign due to current state: [{oldTriggerCampaignResponse.Object.State}]");
                    return response;
                }

                var oldTriggercampaign = oldTriggerCampaignResponse.Object as TriggerCampaign;
                if (oldTriggercampaign == null)
                {
                    response.SetStatus(eResponseStatus.CampaignDoesNotExist, $"TriggerCampaign: {campaignToUpdate.Id} not found");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToUpdate);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToUpdate.FillEmpty(oldTriggercampaign);
                campaignToUpdate.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                if (_repository.Update_Campaign(campaignToUpdate, contextData))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData, campaignToUpdate);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{campaignToUpdate.Id}]");
            }
            catch (Exception ex)
            {
                log.Error($"Error while updating TriggerCampaign({campaignToUpdate.Id}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<BatchCampaign> UpdateBatchCampaign(ContextData contextData, BatchCampaign campaignToUpdate)
        {
            var response = new GenericResponse<BatchCampaign>();
            try
            {
                var oldBatchCampaignResponse = Get(contextData, campaignToUpdate.Id, true);
                if (!oldBatchCampaignResponse.HasObject())
                {
                    response.SetStatus(oldBatchCampaignResponse.Status);
                    return response;
                }

                if (oldBatchCampaignResponse.Object.CampaignType != eCampaignType.Batch)
                {
                    response.SetStatus(eResponseStatus.Error, $"invalid campaign type for update");
                    return response;
                }

                if (oldBatchCampaignResponse.Object.State != CampaignState.INACTIVE)
                {
                    response.SetStatus(eResponseStatus.CampaignUpdateNotAllowed, $"Can't update this campaign due to current state: [{oldBatchCampaignResponse.Object.State}]");
                    return response;
                }

                var oldBatchcampaign = oldBatchCampaignResponse.Object as BatchCampaign;
                if (oldBatchcampaign == null)
                {
                    response.SetStatus(eResponseStatus.CampaignDoesNotExist, $"BatchCampaign: {campaignToUpdate.Id} not found");
                    return response;
                }

                var validateStatus = ValidateCampaign(contextData, campaignToUpdate);
                if (!validateStatus.IsOkStatusCode())
                {
                    response.SetStatus(validateStatus);
                    return response;
                }

                campaignToUpdate.FillEmpty(oldBatchcampaign);
                campaignToUpdate.UpdateDate = DateUtils.GetUtcUnixTimestampNow();

                if (_repository.Update_Campaign(campaignToUpdate, contextData))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData, campaignToUpdate);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating BatchCampaign: [{campaignToUpdate.Id}]");
            }
            catch (Exception ex)
            {
                log.Error($"Error while updating BatchCampaign({campaignToUpdate.Id}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public Status SetState(ContextData contextData, long id, CampaignState newState)
        {
            var response = Status.Error;

            var campaign = Get(contextData, id, false);

            if (!campaign.IsOkStatusCode())
            {
                response.Set(campaign.Status);
                return response;
            }

            var validationStatus = ValidateStateChange(contextData, campaign.Object, newState);
            if (!validationStatus.IsOkStatusCode())
            {
                response.Set(validationStatus);
                return response;
            }

            campaign.Object.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
            if (_repository.Update_Campaign(campaign.Object, contextData))
            {
                SetInvalidationKeys(contextData, campaign.Object);
                response.Set(eResponseStatus.OK);
            }
            else
            {
                response.Set(eResponseStatus.Error, $"Error Updating Campaign state: [{id}] for group: {contextData.GroupId}");
            }

            return response;
        }

        private Status ValidateStateChange(ContextData contextData, Campaign campaign, CampaignState newState)
        {
            var response = new Status(eResponseStatus.OK);

            if (campaign.State == newState)
            {
                response.Set(eResponseStatus.InvalidCampaignState, $"Campaign: {campaign.Id} already in state: {newState}");
                return response;
            }
            else if ((campaign.State == CampaignState.INACTIVE && newState == CampaignState.ACTIVE)
                || campaign.State == CampaignState.ACTIVE && newState == CampaignState.ARCHIVE)
            {
                log.Info($"Updating campaign: {campaign.Id} to state: {newState}");
            }
            else
            {
                response.Set(eResponseStatus.CampaignStateUpdateNotAllowed, $"Set state error, from: {campaign.State} to {newState} is not allowed");
                return response;
            }

            if (newState == CampaignState.ACTIVE)
            {
                if (campaign.CampaignType == eCampaignType.Trigger)
                {
                    var campaignFilter = new TriggerCampaignFilter() { StateEqual = CampaignState.ACTIVE };
                    var campaigns = ListTriggerCampaigns(contextData, campaignFilter);

                    if (campaigns.HasObjects() && campaigns.Objects.Count >= MAX_TRIGGER_CAMPAIGNS)
                    {
                        response.Set(eResponseStatus.ExceededMaxCapacity, "Active trigger campaigns Exceeded Max Size");
                        return response;
                    }
                }
                else if (campaign.CampaignType == eCampaignType.Batch)
                {
                    var campaignFilter = new BatchCampaignFilter() { StateEqual = CampaignState.ACTIVE };
                    var campaigns = ListBatchCampaigns(contextData, campaignFilter);

                    if (campaigns.HasObjects() && campaigns.Objects.Count >= MAX_BATCH_CAMPAIGNS)
                    {
                        response.Set(eResponseStatus.ExceededMaxCapacity, "Active batch campaigns Exceeded Max Size");
                        return response;
                    }
                }
            }

            if (newState == CampaignState.ACTIVE && campaign.EndDate <= DateUtils.GetUtcUnixTimestampNow())
            {
                response.Set(eResponseStatus.InvalidCampaignEndDate, $"Campaign: {campaign.Id} was ended");
                return response;
            }

            campaign.State = newState;

            return response;
        }

        private Status ValidateCampaign(ContextData contextData, Campaign campaign)
        {
            var status = Status.Error;
            if (campaign.Promotion != null)
            {
                status = _promotionValidator.Validate(contextData.GroupId, campaign.Promotion);
                if (!status.IsOkStatusCode())
                {
                    return status;
                }

                if (campaign.Promotion.Conditions?.Count > 0)
                {
                    foreach (RuleCondition condition in campaign.Promotion.Conditions)
                    {
                        status = _conditionValidator.Validate(contextData.GroupId, condition);
                        if (!status.IsOkStatusCode())
                        {
                            return status;
                        }
                    }
                }
            }

            if (campaign.CollectionIds?.Count > 0)
            {
                var channels = new List<GroupsCacheManager.Channel>();

                if (_catalogManager.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    var result = _channelManager.GetChannelsListResponseByChannelIds(contextData.GroupId, campaign.CollectionIds.Select(x => (int)x).ToList(), true, null);

                    if (!result.Status.IsOkStatusCode() || result.Objects.Count != campaign.CollectionIds.Count)
                    {
                        status.Set(eResponseStatus.NotExist, "One or more collection Ids are invalid or not found");
                        return status;
                    }
                }
                else
                {
                    Group group = null;
                    GroupManager groupManager = new GroupsCacheManager.GroupManager(_groupsCache);
                    GroupsCacheManager.Channel channel = null;

                    foreach (var channelId in campaign.CollectionIds)
                    {
                        groupManager.GetGroupAndChannel((int)channelId, contextData.GroupId, ref group, ref channel);

                        if (channel == null)
                        {
                            status.Set(eResponseStatus.NotExist, "One or more collection Ids are invalid or not found");
                            return status;
                        }

                    }
                }
            }

            status.Set(eResponseStatus.OK);
            return status;
        }

        private void SetInvalidationKeys(ContextData contextData, Campaign campaign)
        {
            if (SetCampaignInvalidationKey(contextData, campaign.Id))
            {
                var invalidationKey = LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, (int)campaign.CampaignType);
                _layeredCache.SetInvalidationKey(invalidationKey);
            }
        }

        private bool SetCampaignInvalidationKey(ContextData contextData, long campaignId)
        {
            var invalidationKey = LayeredCacheKeys.GetCampaignInvalidationKey(contextData.GroupId, campaignId);
            return _layeredCache.SetInvalidationKey(invalidationKey);
        }

        public void PublishTriggerCampaign(int groupId, int domainId, Core.Users.DomainDevice eventObject, ApiService apiService, ApiAction apiAction)
        {
            try
            {
                var filter = new TriggerCampaignFilter()
                {
                    Action = apiAction,
                    IsActiveNow = true,
                    Service = apiService,
                    StateEqual = CampaignState.ACTIVE
                };
                var contextData = new ContextData(groupId) { DomainId = domainId };

                var campaings = ListTriggerCampaigns(contextData, filter);
                if (campaings.HasObjects())
                {
                    var serviceEvent = new Core.Api.Modules.CampaignTriggerEvent()
                    {
                        RequestId = KLogger.GetRequestId(),
                        GroupId = groupId,
                        EventObject = eventObject,
                        DomainId = domainId,
                        ApiAction = (int)apiAction,
                        ApiService = (int)apiService
                    };

                    _eventBusPublisher.Publish(serviceEvent);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private List<T> ListCampaignsByType<T>(ContextData contextData, CampaignSearchFilter filter, eCampaignType campaignType) where T : Campaign, new()
        {
            List<T> list = null;

            IEnumerable<CampaignDB> campaignsDB = null;
            var key = LayeredCacheKeys.GetGroupCampaignKey(contextData.GroupId, (int)campaignType);
            var cacheResult = _layeredCache.Get(key,
                                                ref campaignsDB,
                                                ListCampaignsByGroupIdDB,
                                                new Dictionary<string, object>() {
                                                        { "groupId", contextData.GroupId },
                                                        { "campaignType", campaignType }
                                                },
                                                contextData.GroupId,
                                                LayeredCacheConfigNames.LIST_CAMPAIGNS_BY_GROUP_ID,
                                                new List<string>() { LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, (int)campaignType) });

            campaignsDB = filter.Apply(campaignsDB);

            if (campaignsDB?.Count() > 0)
            {
                bool lazySetState = !filter.IsActiveNow; // BEO-13101 when searching campaigns which are active now we dont want to run lazy update for their state
                list = campaignsDB.Select(x => Get(contextData, x.Id, lazySetState))
                    .Where(campaignResponse => campaignResponse.HasObject() && campaignResponse.Object.CampaignType == campaignType)
                    .Select(camp => (T)camp.Object).ToList();

                list = filter.Apply(list);
            }
            else
            {
                list = new List<T>();
            }

            return list;
        }

        private Tuple<IEnumerable<CampaignDB>, bool> ListCampaignsByGroupIdDB(Dictionary<string, object> arg)
        {
            IEnumerable<CampaignDB> list = null;

            try
            {
                var groupId = (int)arg["groupId"];
                var campaignType = (eCampaignType)arg["campaignType"];
                list = _repository.GetCampaignsByGroupId(groupId, campaignType);
            }
            catch (Exception ex)
            {
                log.Error($"Failed ListCampaignsByGroupId group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<IEnumerable<CampaignDB>, bool>(list, list != null);
        }

        private Tuple<Campaign, bool> Get_CampaignsByIdDB(Dictionary<string, object> arg)
        {
            Campaign campaign = null;

            try
            {
                var groupId = (int)arg["groupId"];
                var id = (long)arg["campaignId"];
                campaign = _repository.GetCampaignById(groupId, id);
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetCampaignByIds group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<Campaign, bool>(campaign, campaign != null);
        }


    }
}