using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using Campaign = ApiObjects.Campaign;
using DAL;
using System.Linq;
using ApiObjects.EventBus;
using TVinciShared;
using System.Collections.Generic;
using CachingProvider.LayeredCache;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using Core.Catalog;
using System.Threading.Tasks;

namespace ApiLogic.Users.Managers
{
    public class CampaignManager : ICrudHandler<Campaign, long>
    {
        private const int MAX_TRIGGER_CAMPAIGNS = 100;
        private const int MAX_BATCH_CAMPAIGNS = 100;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CampaignManager> lazy = new Lazy<CampaignManager>(() => new CampaignManager());
        public static CampaignManager Instance { get { return lazy.Value; } }

        private CampaignManager() { }

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

                if (!PricingDAL.DeleteCampaign(contextData.GroupId, id))
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
                var cacheResult = LayeredCache.Instance.Get(key,
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

                    Task.Run(() => {
                        _campaign.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                        if (PricingDAL.Update_Campaign(_campaign, contextData))
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

            ManageOrderBy(filter, response);
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

            ManageOrderBy(filter, response);
            ManagePagination(pager, response);

            return response;
        }

        private static void ManagePagination<T>(CorePager pager, GenericListResponse<T> response) where T : Campaign
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

            ManageOrderBy(filter, response);

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        private static void ManageOrderBy<T>(CampaignFilter filter, GenericListResponse<T> response) where T : Campaign
        {
            if (filter.OrderBy.HasValue && filter.OrderBy.Value == CampaignOrderBy.StartDateDesc)
            {
                response.Objects = response.Objects.OrderByDescending(camp => camp.StartDate).ToList();
            }
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

            ManageOrderBy(filter, response);
            ManagePagination(pager, response);

            return response;
        }

        public GenericResponse<Campaign> AddTriggerCampaign(ContextData contextData, TriggerCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();

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

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd, contextData);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData, insertedCampaign);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, $"Error while saving TriggerCampaign");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new TriggerCampaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> AddBatchCampaign(ContextData contextData, BatchCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();

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

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd, contextData);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData, insertedCampaign);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, $"Error while saving BatchCampaign");
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new BatchCampaign. contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> UpdateTriggerCampaign(ContextData contextData, TriggerCampaign campaignToUpdate)
        {
            var response = new GenericResponse<Campaign>();
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
                    response.SetStatus(eResponseStatus.Error, $"Can't update this campaign due to current state: [{oldTriggerCampaignResponse.Object.State}]");
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

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData))
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

        public GenericResponse<Campaign> UpdateBatchCampaign(ContextData contextData, BatchCampaign campaignToUpdate)
        {
            var response = new GenericResponse<Campaign>();
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
                    response.SetStatus(eResponseStatus.Error, $"Can't update this campaign due to current state: [{oldBatchCampaignResponse.Object.State}]");
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

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData))
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

            try
            {
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
                if (PricingDAL.Update_Campaign(campaign.Object, contextData))
                {
                    SetInvalidationKeys(contextData, campaign.Object);
                    response.Set(eResponseStatus.OK);
                }
                else
                    response.Set(eResponseStatus.Error, $"Error Updating Campaign state: [{id}] for group: {contextData.GroupId}");

            }
            catch (Exception ex)
            {
                log.Error($"Failed setting campaign: {id} state to: {newState}, ex: {ex}", ex);
                response.Set(eResponseStatus.Error, $"Campaign: {id} wasn't updated");
            }

            return response;
        }

        private Status ValidateStateChange(ContextData contextData, Campaign campaign, CampaignState newState)
        {
            var response = new Status(eResponseStatus.OK);

            if (campaign.State == newState)
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} already in state: {newState}");
                return response;
            }
            else if ((campaign.State == CampaignState.INACTIVE && newState == CampaignState.ACTIVE)
                || campaign.State == CampaignState.ACTIVE && newState == CampaignState.ARCHIVE)
            {
                log.Info($"Updating campaign: {campaign.Id} to state: {newState}");
            }
            else
            {
                response.Set(eResponseStatus.Error, $"Set state error, from: {campaign.State} to {newState} is not allowed");
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
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} was ended");
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
                var discounts = Core.Pricing.Module.GetValidDiscounts(contextData.GroupId);
                if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == campaign.Promotion.DiscountModuleId))
                {
                    status.Set(eResponseStatus.DiscountCodeNotExist);
                    return status;
                }
            }

            if (campaign.CollectionIds?.Count > 0)
            {
                var channels = new List<GroupsCacheManager.Channel>();

                if (Core.Catalog.CatalogManagement.CatalogManager.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    var result = ChannelManager.GetChannelsListResponseByChannelIds(contextData.GroupId, campaign.CollectionIds.Select(x => (int)x).ToList(), true, null);

                    if (!result.Status.IsOkStatusCode() || result.Objects.Count != campaign.CollectionIds.Count)
                    {
                        status.Set(eResponseStatus.NotExist, "One or more collection Ids are invalid or not found");
                        return status;
                    }
                }
                else
                {
                    Group group = null;
                    GroupManager groupManager = new GroupsCacheManager.GroupManager();
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
                LayeredCache.Instance.SetInvalidationKey(invalidationKey);
            }
        }

        private bool SetCampaignInvalidationKey(ContextData contextData, long campaignId)
        {
            var invalidationKey = LayeredCacheKeys.GetCampaignInvalidationKey(contextData.GroupId, campaignId);
            return LayeredCache.Instance.SetInvalidationKey(invalidationKey);
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

                    var publisher = EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
                    publisher.Publish(serviceEvent);
                }
            }
            catch (Exception ex)
            {

            }            
        }

        private List<T> ListCampaignsByType<T>(ContextData contextData, CampaignSearchFilter filter, eCampaignType campaignType) where T : Campaign, new()
        {
            List<T> list = null;

            try
            {
                IEnumerable<CampaignDB> campaignsDB = null;
                var key = LayeredCacheKeys.GetGroupCampaignKey(contextData.GroupId, (int)campaignType);
                var cacheResult = LayeredCache.Instance.Get(key,
                                                            ref campaignsDB,
                                                            ListCampaignsByGroupIdDB,
                                                            new Dictionary<string, object>() {
                                                                { "groupId", contextData.GroupId },
                                                                { "campaignType", campaignType }
                                                            },
                                                            contextData.GroupId,
                                                            LayeredCacheConfigNames.LIST_CAMPAIGNS_BY_GROUP_ID,
                                                            new List<string>() { LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, (int)campaignType) });

                if (campaignsDB != null)
                {
                    var utcNow = DateUtils.GetUtcUnixTimestampNow();
                    if (filter.StateEqual.HasValue)
                    {
                        if (filter.StateEqual == CampaignState.ACTIVE)
                        {
                            campaignsDB = campaignsDB.Where(x => x.EndDate >= utcNow && x.State == CampaignState.ACTIVE);
                            if (filter.IsActiveNow)
                            {
                                campaignsDB = campaignsDB.Where(x => x.StartDate <= utcNow);
                            }
                        }
                        else if (filter.StateEqual == CampaignState.ARCHIVE)
                        {
                            campaignsDB = campaignsDB.Where(x => (x.State == filter.StateEqual.Value) || (x.EndDate < utcNow && x.State == CampaignState.ACTIVE));
                        }
                        else if (filter.StateEqual == CampaignState.INACTIVE)
                        {
                            campaignsDB = campaignsDB.Where(x => x.State == filter.StateEqual.Value);
                        }
                    }

                    if (filter.StartDateGreaterThanOrEqual.HasValue)
                    {
                        campaignsDB = campaignsDB.Where(x => x.StartDate >= filter.StartDateGreaterThanOrEqual.Value);
                    }

                    if (filter.EndDateLessThanOrEqual.HasValue)
                    {
                        campaignsDB = campaignsDB.Where(x => x.EndDate <= filter.EndDateLessThanOrEqual.Value);
                    }

                    if (filter.HasPromotion.HasValue)
                    {
                        if (filter.HasPromotion.Value)
                        {
                            campaignsDB = campaignsDB.Where(x => x.HasPromotion);
                        }
                        else
                        {
                            campaignsDB = campaignsDB.Where(x => !x.HasPromotion);
                        }
                    }

                    if (campaignsDB.Count() > 0)
                    {
                        list = campaignsDB.Select(x => Get(contextData, x.Id, true))
                            .Where(campaignResponse => campaignResponse.HasObject() && campaignResponse.Object.CampaignType == campaignType)
                            .Select(camp => (T)camp.Object).ToList();
                    }
                    else
                    {
                        list = new List<T>();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to ListCampaignsByType contextData:{contextData}, ex: {ex}", ex);
            }

            return list;
        }

        private static Tuple<IEnumerable<CampaignDB>, bool> ListCampaignsByGroupIdDB(Dictionary<string, object> arg)
        {
            IEnumerable<CampaignDB> list = null;

            try
            {
                var groupId = (int)arg["groupId"];
                var campaignType = (eCampaignType)arg["campaignType"];
                list = PricingDAL.GetCampaignsByGroupId(groupId, campaignType);
            }
            catch (Exception ex)
            {
                log.Error($"Failed ListCampaignsByGroupId group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<IEnumerable<CampaignDB>, bool>(list, list != null);
        }

        private static Tuple<Campaign, bool> Get_CampaignsByIdDB(Dictionary<string, object> arg)
        {
            Campaign campaign = null;

            try
            {
                var groupId = (int)arg["groupId"];
                var id = (long)arg["campaignId"];
                campaign = PricingDAL.GetCampaignById(groupId, id);
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetCampaignByIds group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<Campaign, bool>(campaign, campaign != null);
        }
    }
}