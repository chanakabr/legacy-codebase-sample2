using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using APILogic.ConditionalAccess;

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
            // TODO SHIR
            // validate discountModelId!!
            //GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            //try
            //{
            //    businessModuleRuleToAdd.GroupId = groupId;
            //    DataTable dt = ApiDAL.AddBusinessModuleRule(groupId, businessModuleRuleToAdd.Name, businessModuleRuleToAdd.Description);
            //    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            //    {
            //        businessModuleRuleToAdd.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");
            //        businessModuleRuleToAdd.CreateDate = DateUtils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0], "CREATE_DATE"));
            //        businessModuleRuleToAdd.UpdateDate = businessModuleRuleToAdd.CreateDate;

            //        if (!ApiDAL.SaveBusinessModuleRuleCB(groupId, businessModuleRuleToAdd))
            //        {
            //            log.ErrorFormat("Error while saving BusinessModuleRule. groupId: {0}, BusinessModuleRuleId:{1}", groupId, businessModuleRuleToAdd.Id);
            //            return response;
            //        }

            //        SetInvalidationKeys(groupId);
            //        response.Object = businessModuleRuleToAdd;
            //        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            //    }
            //}
            //catch (Exception ex)
            //{
            //    log.ErrorFormat("Error while adding new businessModuleRule . groupId: {0}, businessModuleRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(businessModuleRuleToAdd), ex);
            //}

            //return response;
            throw new NotImplementedException();
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
                //BusinessModuleId = businessModuleId,
                //BusinessModuleType = transactionType,
                //SegmentIds = segmentIds,
                FilterByDate = true,
                //FilterBySegments = true,
                GroupId = contextData.GroupId,
                //MediaId = mediaId
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

            return true;// triggerCampaign.Evaluate(coreObject);
        }
    }
}
