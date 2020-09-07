using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using APILogic.ConditionalAccess;
using Campaign = ApiObjects.Campaign;
using DAL;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using ApiObjects.EventBus;
using TVinciShared;
using System.Collections.Generic;
using CachingProvider.LayeredCache;

namespace ApiLogic.Users.Managers
{
    public class CampaignManager : ICrudHandler<Campaign, long>
    {
        private const int MAX_TRIGGER_CAMPAIGNS = 500;
        private const int MAX_BATCH_CAMPAIGNS = 500;
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<CampaignManager> lazy = new Lazy<CampaignManager>(() => new CampaignManager());
        public static CampaignManager Instance { get { return lazy.Value; } }

        private CampaignManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<Campaign> Get(ContextData contextData, long id)
        {
            // TODO SHIR - THIS "GET" WILL CALL TO LIST WITH ID_IN_FILTER AND THIS FILTER IS FROM CAHCE OF ALL CAMPAIGNS OF GROUP
            var response = new GenericResponse<Campaign>();
            var campaign = List(contextData, null, null)?.Objects?.Where(camp => camp.Id == id).FirstOrDefault();
            if (campaign == null || campaign.Id == 0)
            {
                response.SetStatus(eResponseStatus.Error, $"Campaign not found, id: {id}");
                return response;
            }

            response.Object = campaign;
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<Campaign> List(ContextData contextData, CampaignFilter filter, CorePager pager)
        {
            // TODO SHIR
            //Get and List should be separated by inheritance type?

            var response = new GenericListResponse<Campaign>();
            return response;
        }

        public GenericListResponse<TriggerCampaign> ListTriggerCampaigns(ContextData contextData, CampaignFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<TriggerCampaign>();

            if (pager == null)
            {
                pager = new CorePager();
            }

            if (filter == null)
            {
                filter = new CampaignIdInFilter();
            }

            var campaigns = GetList<TriggerCampaign>(contextData);

            if (!campaigns.IsOkStatusCode())
            {
                response.SetStatus(campaigns.Status);
                return response;
            }

            if (filter is CampaignIdInFilter)
            {
                var _filter = filter as CampaignIdInFilter;
                if (_filter.IdIn?.Count > 0)
                {
                    campaigns.Objects = campaigns.Objects.Where(camp => _filter.IdIn.Contains(camp.Id)).ToList();
                }
            }

            response.Objects = campaigns.Objects.Select(cmp => JsonConvert.DeserializeObject<TriggerCampaign>(cmp.CampaignJson)).ToList();
            response.SetStatus(eResponseStatus.OK);

            return response;

            // TODO SHIR / MATAN - ListTriggerCampaigns WHEN ODED WILL FINISH WITH SPEC
            //TODO SHIR FILTER by WITH INSERT ACTION AND DOMAIN DEVICE OBJECT

        }

        public GenericResponse<Campaign> AddTriggerCampaign(ContextData contextData, TriggerCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                // TODO SHIR what else need to be validate??
                campaignToAdd.GroupId = contextData.GroupId;
                //campaignToAdd.IsActive = false;
                campaignToAdd.Status = 0;

                //TODO SHIR  set filter 
                var triggerCampaignFilter = new TriggerCampaignFilter();
                var triggerCampaigns = ListTriggerCampaigns(contextData, triggerCampaignFilter);
                if (triggerCampaigns.HasObjects() && triggerCampaigns.Objects.Count >= MAX_TRIGGER_CAMPAIGNS)
                {
                    // TODO SHIR ERROR FOR limit to 500 per group
                }

                if (campaignToAdd.DiscountModuleId.HasValue)
                {
                    var discounts = Core.Pricing.Module.GetValidDiscounts(contextData.GroupId);
                    if (!discounts.HasObjects() || !discounts.Objects.Any(x => x.Id == campaignToAdd.DiscountModuleId.Value))
                    {
                        response.SetStatus(eResponseStatus.DiscountCodeNotExist);
                        return response;
                    }
                }

                //TODO - MATAN, TBD: Add init ?
                campaignToAdd.CreateDate = DateUtils.ToUtcUnixTimestampSeconds(DateTime.UtcNow);
                campaignToAdd.UpdateDate = campaignToAdd.CreateDate;
                campaignToAdd.UpdaterId = contextData.UserId.Value;
                campaignToAdd.Status = 0;
                campaignToAdd.State = ObjectState.INACTIVE;

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd);
                if (insertedCampaign?.Id > 0)
                {
                    response.Object = insertedCampaign;
                    SetInvalidationKeys(contextData);
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
            // TODO SHIR
            throw new NotImplementedException();
        }

        public GenericResponse<Campaign> UpdateTriggerCampaign(ContextData contextData, TriggerCampaign campaignToUpdate)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                var _response = Get(contextData, campaignToUpdate.Id);
                if (!_response.IsOkStatusCode() || !(_response.Object is TriggerCampaign))
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaignToUpdate.Id}");
                    return response;
                }

                var campaign = response.Object as TriggerCampaign;

                if (campaign == null)
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaign.Id}, campaign not found");
                    return response;
                }

                ValidateParametersForUpdate(campaignToUpdate);
                FillCampaignTriggerObject(campaign, campaignToUpdate);

                campaignToUpdate.UpdateDate = DateUtils.ToUtcUnixTimestampSeconds(DateTime.UtcNow);
                campaignToUpdate.GroupId = contextData.GroupId;

                if (PricingDAL.Update_Campaign(campaignToUpdate, contextData.UserId.Value))
                {
                    response.Object = campaignToUpdate;
                    SetInvalidationKeys(contextData);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{campaignToUpdate.Id}] for group: {contextData.GroupId}");
            }
            catch (Exception ex)
            {
                log.Error($"Error while updating new TriggerCampaign({campaignToUpdate.Id}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        public GenericResponse<Campaign> SetState(ContextData contextData, long id, ObjectState newState)
        {
            var response = new GenericResponse<Campaign>();

            try
            {
                var campaign = Get(contextData, id);

                if (!campaign.IsOkStatusCode())
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }

                ValidateStateChange(campaign.Object, newState);

                var _campaign = campaign.Object as TriggerCampaign;
                var validated = new Status(eResponseStatus.OK);

                if (newState == ObjectState.ACTIVE)
                {
                    validated = ValidateActivation(_campaign);
                }
                else if (newState == ObjectState.INACTIVE)
                {
                    validated = ValidateDeactivation(_campaign);
                }

                if (!validated.IsOkStatusCode())
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }

                if (PricingDAL.Update_Campaign(_campaign, contextData.UserId.Value))
                {
                    SetInvalidationKeys(contextData);
                    response.Object = Get(contextData, _campaign.Id)?.Object;
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{id}] for group: {contextData.GroupId}");

            }
            catch (Exception ex)
            {
                log.Error($"Failed setting campaign: {id} state to: {newState}, ex: {ex}", ex);
                response.SetStatus(eResponseStatus.Error, $"Campaign: {id} wasn't updated");
            }

            return response;
        }

        private Status ValidateStateChange(Campaign campaign, ObjectState newState)
        {
            var response = new Status(eResponseStatus.OK);

            if (campaign.State == ObjectState.ARCHIVE)
            {
                response.Set(eResponseStatus.Error, "Can't update archived campaign");
                return response;
            }

            if (campaign.State == newState)
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} already in state: {newState}");
                return response;
            }

            if (newState == ObjectState.ACTIVE
                && campaign.EndDate <= DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow))//Check set active
            {
                response.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} already ended");
                return response;
            }

            campaign.State = newState;

            return response;
        }

        /// <summary>
        /// check if can be activate
        /// </summary>
        /// <param name="object"></param>
        private Status ValidateActivation(TriggerCampaign campaign)
        {
            var status = Status.Ok;
            //Todo - Shir or Matan
            //if (campaign.IsActive)
            if (campaign.Status == 1 || campaign.State == ObjectState.ACTIVE)
            {
                log.Error($"Campaign: {campaign.Id} is already active, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} is already active");
            }
            if (campaign.EndDate <= TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow))
            {
                log.Error($"Campaign: {campaign.Id} was ended at ({campaign.EndDate}), campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} was ended");
            }
            if (campaign.TriggerConditions == null || campaign.TriggerConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single trigger condition, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single trigger condition");
            }
            if (campaign.DiscountConditions == null || campaign.DiscountConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single discount condition, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single discount condition");
            }

            campaign.Status = status.IsOkStatusCode() ? 1 : 0;
            return status;
        }

        /// <summary>
        /// check if can be deactivate
        /// </summary>
        /// <param name="object"></param>
        private Status ValidateDeactivation(TriggerCampaign campaign)
        {
            var status = Status.Ok;
            //if (!campaign.IsActive)
            if (campaign.Status != 1)
            {
                log.Error($"Campaign: {campaign.Id} is not active, campaign event: {campaign.CampaignJson}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} is not active");
            }

            campaign.Status = status.IsOkStatusCode() ? 1 : 0;
            return status;
        }

        private void ValidateParametersForUpdate(TriggerCampaign campaignToUpdate)
        {
            //TODO - Shir
        }

        /// <summary>
        /// Fill missing parameters by old object
        /// </summary>
        /// <param name="campaign"></param>
        /// <param name="campaignToUpdate"></param>
        private void FillCampaignTriggerObject(TriggerCampaign campaign, TriggerCampaign campaignToUpdate)
        {
            // TODO MATAN
            //if (string.IsNullOrEmpty(campaignToUpdate.Action))
            //{
            //    campaignToUpdate.Action = campaign.Action;
            //}
            
            if (campaignToUpdate.DaynamicData == null)
            {
                campaignToUpdate.DaynamicData = campaign.DaynamicData;
            }
            if (string.IsNullOrEmpty(campaignToUpdate.Description))
            {
                campaignToUpdate.Description = campaign.Description;
            }
            if (campaignToUpdate.DiscountConditions == null)
            {
                campaignToUpdate.DiscountConditions = campaign.DiscountConditions;
            }
            if (campaignToUpdate.DiscountModuleId == null)
            {
                campaignToUpdate.DiscountModuleId = campaign.DiscountModuleId;
            }
            if (campaignToUpdate.EndDate == default)
            {
                campaignToUpdate.EndDate = campaign.EndDate;
            }
            //if (campaignToUpdate.IsActive == default)
            //{
            //    campaignToUpdate.IsActive = campaign.IsActive;
            //}
            if (string.IsNullOrEmpty(campaignToUpdate.Message))
            {
                campaignToUpdate.Message = campaign.Message;
            }
            if (string.IsNullOrEmpty(campaignToUpdate.Name))
            {
                campaignToUpdate.Name = campaign.Name;
            }
            if (string.IsNullOrEmpty(campaignToUpdate.SystemName))
            {
                campaignToUpdate.SystemName = campaign.SystemName;
            }
            if (campaignToUpdate.StartDate == default)
            {
                campaignToUpdate.StartDate = campaign.StartDate;
            }
            //if (string.IsNullOrEmpty(campaignToUpdate.Service))
            //{
            //    campaignToUpdate.Service = campaign.Service;
            //}
            if (campaignToUpdate.TriggerConditions == null)
            {
                campaignToUpdate.TriggerConditions = campaign.TriggerConditions;
            }
            if (campaignToUpdate.UpdateDate == default)
            {
                campaignToUpdate.UpdateDate = TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow);
            }
        }

        public GenericResponse<Campaign> UpdateBatchCampaign(ContextData contextData, BatchCampaign campaignToUpdate)
        {
            // TODO SHIR
            throw new NotImplementedException();
        }

        private void SetInvalidationKeys(ContextData contextData)
        {
            // TODO SHIR - SetInvalidationKeys
        }

        public void PublishTriggerCampaign(int groupId, int domainId, ICampaignObject eventObject, ApiService apiService, ApiAction apiAction)
        {
            //TODO - Shir: Check if campaign of this type is allowed for group
            if (1 != 1)
            {
                return;
            }

            var serviceEvent = new CampaignTriggerEvent()
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

        private GenericListResponse<T> GetList<T>(ContextData contextData) where T : Campaign, new()
        {
            var response = new GenericListResponse<T>();
            try
            {
                var type = typeof(T).Name;
                IEnumerable<T> campaigns = null;
                var cacheResult = LayeredCache.Instance.Get(
                    LayeredCacheKeys.GetGroupCampaignKey(contextData.GroupId, type),
                    ref campaigns,
                    GetCampaignsByGroupId<T>,
                    new Dictionary<string, object>() { { "groupId", contextData.GroupId } },
                    contextData.GroupId,
                    LayeredCacheConfigNames.GET_GROUP_CAMPAIGNS,
                    new List<string>() { LayeredCacheKeys.GetGroupCampaignInvalidationKey(contextData.GroupId, type) });

                if (campaigns != null && campaigns.Count() > 0)
                {
                    response.Objects.AddRange(campaigns);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"ex: {ex}", ex);
                response.SetStatus(eResponseStatus.Error, $"Couldn't get list of campaigns for group: {contextData.GroupId}");
            }

            return response;
        }

        private static Tuple<IEnumerable<T>, bool> GetCampaignsByGroupId<T>(Dictionary<string, object> arg) where T : Campaign, new()
        {
            try
            {
                var groupId = (int)arg["groupId"];
                var contextData = new ContextData(groupId);
                var list = PricingDAL.List_Campaign<T>(contextData);
                return new Tuple<IEnumerable<T>, bool>(list, true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get Campaign list from DB group:[{arg["groupId"]}], ex: {ex}", ex);
                return new Tuple<IEnumerable<T>, bool>(Enumerable.Empty<T>(), false);
            }
        }

        // TODO SHIR
        // 1. list by group id -> return ALL CAMPAIGNS
    }
}
