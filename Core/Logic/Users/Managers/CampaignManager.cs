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

namespace ApiLogic.Users.Managers
{
    public class CampaignManager : ICrudHandler<Campaign, long>
    {
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
            return new GenericListResponse<Campaign>();
        }

        public GenericResponse<Campaign> AddTriggerCampaign(ContextData contextData, TriggerCampaign campaignToAdd)
        {
            var response = new GenericResponse<Campaign>();
            try
            {
                if (campaignToAdd.DiscountModuleId.HasValue)
                {
                    // TODO SHIR - ASK IRA WHAT TO PASS HERE
                    //var discountModule = Core.Pricing.Module.GetDiscountCodeDataByCountryAndCurrency(contextData.GroupId, campaignToAdd.DiscountModuleId, countryCode, currencyCode);
                    //if (discountModule == null)
                    //{
                    //    response.SetStatus(eResponseStatus.DiscountCodeNotExist);
                    //    return response;
                    //}
                }

                // TODO SHIR what else need to be validate??
                campaignToAdd.GroupId = contextData.GroupId;
                campaignToAdd.IsActive = false;

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd);

                if (insertedCampaign?.Id > 0)
                {
                    campaignToAdd.Id = insertedCampaign.Id;
                    campaignToAdd.CreateDate = insertedCampaign.CreateDate;
                    campaignToAdd.UpdateDate = insertedCampaign.UpdateDate;

                    AddCampaignIdToEvent(campaignToAdd);

                    if (!PricingDAL.AddNotificationCampaignAction(contextData, campaignToAdd))
                    {
                        var message = $"Failed adding Notification Campaign Action, campaign Id: {campaignToAdd.Id}";
                        log.Error($"{message}, contextData: {contextData}");
                        response.SetStatus(eResponseStatus.Error, message);
                        return response;
                    }

                    response.Object = campaignToAdd;
                    SetInvalidationKeys(contextData);
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                    response.SetStatus(eResponseStatus.Error, $"Error saving TriggerCampaign: [{campaignToAdd.Name}] for group: {contextData.GroupId}");
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
                if (!campaign.IsOkStatusCode())
                {
                    response.SetStatus(campaign.Status);
                    return response;
                }
                var tCampaign = campaign.Object as TriggerCampaign;
                ValidateDispatch(tCampaign);
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
                PricingDAL.AddNotificationCampaignAction(contextData, tCampaign))//Update event
            {
                response.Object = tCampaign;
                SetInvalidationKeys(contextData);
                response.SetStatus(eResponseStatus.OK);
            }
            else
                response.SetStatus(eResponseStatus.Error, $"Error Updating TriggerCampaign: [{tCampaign.Id}] for group: {contextData.GroupId}");
        }

        /// <summary>
        /// check if can be dispatched
        /// </summary>
        /// <param name="object"></param>
        private void ValidateDispatch(TriggerCampaign campaign)
        {
            //Todo - Shir or Matan
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
            //TODO - Matan
            if (string.IsNullOrEmpty(campaignToUpdate.Action))
            {
                campaignToUpdate.Action = campaign.Action;
            }
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
            if (string.IsNullOrEmpty(campaignToUpdate.Service))
            {
                campaignToUpdate.Service = campaign.Service;
            }
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
        private void AddCampaignIdToEvent(TriggerCampaign triggerCampaign)
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
    }
}
