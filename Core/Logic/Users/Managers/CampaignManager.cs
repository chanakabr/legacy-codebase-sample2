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
            var response = new GenericResponse<Campaign>();
            var campaign = PricingDAL.Get_Campaign(contextData, id);//Cache?
            if (campaign == null || campaign.Id == 0)
            {
                response.SetStatus(eResponseStatus.Error, $"Campaign not found, id: {id}");
                return response;
            }

            response.Object = JsonConvert.DeserializeObject<TriggerCampaign>(campaign.CoreObject);
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<Campaign> List(ContextData contextData, CampaignFilter filter, CorePager pager)
        {
            // TODO SHIR
            //Get and List should be separated by inheritance type?

            var response = new GenericListResponse<Campaign>();
            var campaigns = PricingDAL.List_Campaign(contextData);//Cache?
            if (campaigns == null || campaigns.Count == 0)
            {
                response.SetStatus(eResponseStatus.Error, $"Campaigns not found, ContextData: {contextData}");
                return response;
            }

            response.Objects = campaigns.Select(cmp => JsonConvert.DeserializeObject<Campaign>(cmp.CoreObject)).ToList();
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<TriggerCampaign> ListTriggerCampaigns(ContextData contextData, TriggerCampaignFilter filter, CorePager pager = null)
        {
            if (pager == null)
            {
                pager = new CorePager();
            }

            // TODO SHIR / MATAN - ListTriggerCampaigns WHEN ODED WILL FINISH WITH SPEC
            //TODO SHIR FILTER by WITH INSERT ACTION AND DOMAIN DEVICE OBJECT

            return new GenericListResponse<TriggerCampaign>();
        }

        public GenericResponse<Campaign> AddTriggerCampaign(ContextData contextData, TriggerCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                // TODO SHIR what else need to be validate??
                campaignToAdd.GroupId = contextData.GroupId;
                campaignToAdd.IsActive = false;

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

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd);
                if (insertedCampaign?.Id > 0)
                {
                    campaignToAdd = insertedCampaign;
                    var campaignEvent = PricingDAL.GetCampaignEventNotification(contextData, campaignToAdd);
                    if (string.IsNullOrEmpty(campaignEvent))
                    {
                        SetCampaignIdToEvent(campaignToAdd);
                        if (!PricingDAL.SaveNotificationCampaignAction(contextData, campaignToAdd))
                        {
                            var message = $"Failed adding Notification Campaign Action, campaign Id: {campaignToAdd.Id}";
                            log.Error($"{message}, contextData: {contextData}");
                            response.SetStatus(eResponseStatus.Error, message);
                            return response;
                        }
                    }
                    else
                    {

                    }

                    response.Object = campaignToAdd;
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
                var campaign = Get(contextData, campaignToUpdate.Id)?.Object as TriggerCampaign;

                //Check if trigger campaign exist
                if (campaign == null)
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaign.Id}, campaign not found");
                    return response;
                }
                //Check if trigger campaign can be update (not dispatched yet)
                if (campaign.IsActive)
                {
                    response.SetStatus(eResponseStatus.Error, $"Couldn't update TriggerCampaign: {campaign.Id}, campaign was dispatched");
                    return response;
                }

                ValidateParametersForUpdate(campaignToUpdate);
                FillCampaignTriggerObject(campaign, campaignToUpdate);

                if (PricingDAL.Update_Campaign(campaignToUpdate))
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

        public GenericResponse<Campaign> DispatchTriggerCampaign(ContextData contextData, long campaignId)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                var campaign = Get(contextData, campaignId);
                if (!campaign.IsOkStatusCode() || !(campaign.Object is TriggerCampaign))
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }

                var tCampaign = campaign.Object as TriggerCampaign;
                var validated = ValidateDispatch(tCampaign);

                if (!validated.IsOkStatusCode())
                {
                    response.SetStatus(validated);
                    return response;
                }

                Dispatch(contextData, tCampaign, response);
            }
            catch (Exception ex)
            {
                log.Error($"Error while dispatching TriggerCampaign({campaignId}). contextData: {contextData}, ex: {ex}", ex);
            }

            return response;
        }

        private void Dispatch(ContextData contextData, TriggerCampaign tCampaign, GenericResponse<Campaign> response)
        {
            tCampaign.IsActive = true;

            if (SetEventStatus(tCampaign) && //Update internal json
                PricingDAL.Update_Campaign(tCampaign) && //Update campaign object in db
                PricingDAL.SaveNotificationCampaignAction(contextData, tCampaign))//Update event in CB
            {
                SetInvalidationKeys(contextData);
                response.Object = Get(contextData, tCampaign.Id)?.Object;
                response.SetStatus(eResponseStatus.OK);
            }
            else
                response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{tCampaign.Id}] for group: {contextData.GroupId}");
        }

        /// <summary>
        /// check if can be dispatched
        /// </summary>
        /// <param name="object"></param>
        private Status ValidateDispatch(TriggerCampaign campaign)
        {
            var status = Status.Ok;
            //Todo - Shir or Matan
            if (campaign.IsActive)
            {
                log.Error($"Campaign: {campaign.Id} is already active, campaign event: {campaign.EventNotification}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} is already active");
            }
            if (campaign.EndDate <= TVinciShared.DateUtils.DateTimeToUtcUnixTimestampSeconds(DateTime.UtcNow))
            {
                log.Error($"Campaign: {campaign.Id} was ended at ({campaign.EndDate}), campaign event: {campaign.EventNotification}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} was ended");
            }
            if (campaign.TriggerConditions == null || campaign.TriggerConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single trigger condition, campaign event: {campaign.EventNotification}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single trigger condition");
            }
            if (campaign.DiscountConditions == null || campaign.DiscountConditions.Count == 0)
            {
                log.Error($"Campaign: {campaign.Id} must have a t least a single discount condition, campaign event: {campaign.EventNotification}");
                status.Set(eResponseStatus.Error, $"Campaign: {campaign.Id} must have a t least a single discount condition");
            }

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
            if (string.IsNullOrEmpty(campaignToUpdate.CoreAction))
            {
                campaignToUpdate.CoreAction = campaign.CoreAction;
            }
            if (string.IsNullOrEmpty(campaignToUpdate.CoreObject))
            {
                campaignToUpdate.CoreObject = campaign.CoreObject;
            }
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
            if (string.IsNullOrEmpty(campaignToUpdate.EventNotification))
            {
                campaignToUpdate.EventNotification = campaign.EventNotification;
            }
            if (campaignToUpdate.IsActive == default)
            {
                campaignToUpdate.IsActive = campaign.IsActive;
            }
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

        // TODO Shir
        /// <summary>
        /// Validate if user matches to CampaignConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateCampaignConditionsToUser(ContextData contextData, Campaign campaign)
        {
            //TODO - Shir or Matan, build filter
            ConditionScope filter = new ConditionScope()
            {
                FilterByDate = true,
                GroupId = contextData.GroupId,
                UserId = contextData.UserId.ToString()
            };

            return campaign.Evaluate(filter);
        }

        // TODO Shir
        /// <summary>
        /// Validate coreobject matches to TriggerConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateTriggerCampaign(TriggerCampaign triggerCampaign, CoreObject coreObject)
        {
            return triggerCampaign.Evaluate(coreObject);
        }

        private void SetInvalidationKeys(ContextData contextData)
        {
            // TODO SHIR - SetInvalidationKeys
        }

        /// <summary>
        /// Replace json event campaignId
        /// </summary>
        /// <param name="triggerCampaign"></param>
        private void SetCampaignIdToEvent(TriggerCampaign triggerCampaign)
        {
            var _object = JObject.Parse(triggerCampaign.EventNotification);
            var val = _object["Actions"][0];
            val["CampaignId"] = triggerCampaign.Id;
            triggerCampaign.EventNotification = _object.ToString();
        }


        /// <summary>
        /// Replace json event status
        /// </summary>
        /// <param name="triggerCampaign"></param>
        private bool SetEventStatus(TriggerCampaign triggerCampaign)
        {
            try
            {
                var _object = JObject.Parse(triggerCampaign.EventNotification);
                var val = _object["Actions"][0];
                val["Status"] = triggerCampaign.IsActive ? 1 : 0;
                triggerCampaign.EventNotification = _object.ToString();
                return true;
            }
            catch (Exception ex)
            {
                log.Error($"Failed updating trigger campaign: {triggerCampaign.Id} status to: {triggerCampaign.IsActive}", ex);
            }
            return false;
        }

        public void PublishTriggerCampaign(ContextData contextData, CoreObject eventObject, ApiService apiService, ApiAction apiAction)
        {
            var filter = new TriggerCampaignFilter()
            {
                Service = apiService,
                Action = apiAction
            };

            var triggerCampaigns = this.ListTriggerCampaigns(contextData, filter);
            if (triggerCampaigns.HasObjects())
            {
                foreach (var triggerCampaign in triggerCampaigns.Objects)
                {
                    var serviceEvent = new CampaignTriggerEvent()
                    {
                        RequestId = KLogger.GetRequestId(),
                        GroupId = contextData.GroupId,
                        CampaignId = triggerCampaign.Id,
                        EventObject = eventObject,
                        DomainId = contextData.DomainId.Value
                    };

                    var publisher = EventBus.RabbitMQ.EventBusPublisherRabbitMQ.GetInstanceUsingTCMConfiguration();
                    publisher.Publish(serviceEvent);
                }
            }
        }
    }
}
