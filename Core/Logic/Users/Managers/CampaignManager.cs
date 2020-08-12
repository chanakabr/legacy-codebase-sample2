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

        GenericResponse<Campaign> ICrudHandler<Campaign, long>.Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<Campaign> List(ContextData contextData, object filter)
        {
            return null;
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
