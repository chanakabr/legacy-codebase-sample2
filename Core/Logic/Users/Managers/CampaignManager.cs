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
using System.Linq;

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
            throw new NotImplementedException();
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

                if (!PricingDAL.AddNotificationCampaignAction(contextData, campaignToAdd))
                {
                    var message = $"Failed adding Notification Campaign Action, campaign Id: {campaignToAdd.Id}";
                    log.Error($"{message}, contextData: {contextData}");
                    response.SetStatus(eResponseStatus.Error, message);
                    return response;
                }

                var insertedCampaign = PricingDAL.AddCampaign(campaignToAdd);

                if (insertedCampaign?.Id > 0)
                {
                    campaignToAdd.Id = insertedCampaign.Id;
                    campaignToAdd.CreateDate = insertedCampaign.CreateDate;
                    campaignToAdd.UpdateDate = insertedCampaign.UpdateDate;
                    response.Object = campaignToAdd;
                }

                if (response.Object != null)
                {
                    SetInvalidationKeys(contextData);
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while adding new TriggerCampaign. contextData: {contextData}, ex: {ex};", ex);
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
            // TODO SHIR
            throw new NotImplementedException();
        }

        public GenericResponse<Campaign> UpdateBatchCampaign(ContextData contextData, BatchCampaign campaignToUpdate)
        {
            // TODO SHIR
            throw new NotImplementedException();
        }

        // TODO MATAN
        /// <summary>
        /// Validate if user matches to CampaignConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateCampaignConditionsToUser(ContextData contextData, Campaign campaign)
        {
            ConditionScope filter = new ConditionScope()
            {
                FilterByDate = true,
                GroupId = contextData.GroupId,
                UserId = contextData.UserId.ToString(),

                //TODO - Matan
                BrandId = campaign.CampaignConditions?.Where(c => c.Type == RuleConditionType.Campaign).Select(c => 4).FirstOrDefault(),
                ManufacturerId = campaign.CampaignConditions?.Where(c => c.Type == RuleConditionType.Campaign).Select(c => 4).FirstOrDefault(),
                Model = campaign.CampaignConditions?.Where(c => c.Type == RuleConditionType.Campaign).Select(c => "").FirstOrDefault(),
            };

            return campaign.Evaluate(filter);
        }

        // TODO MATAN
        /// <summary>
        /// Validate coreobject matches to TriggerConditions
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="triggerCampaign"></param>
        /// <param name="coreObject"></param>
        /// <returns></returns>
        public bool ValidateTriggerCampaign(TriggerCampaign triggerCampaign, CoreObject coreObject)
        {
            ConditionScope filter = new ConditionScope()
            {
                //BusinessModuleId = businessModuleId,
                //BusinessModuleType = transactionType,
                //SegmentIds = segmentIds,
                FilterByDate = true,
                //FilterBySegments = true,
                GroupId = coreObject.GroupId,
                //MediaId = mediaId
            };

            return triggerCampaign.Evaluate(coreObject);
        }

        private void SetInvalidationKeys(ContextData contextData)
        {
            // TODO SHIR - SetInvalidationKeys
        }
    }
}
