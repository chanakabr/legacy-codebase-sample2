using ApiObjects.Response;
using ApiObjects.Rules;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace APILogic.Api.Managers
{
    public class BusinessModuleRuleManager
    {
        private const string BUSINESS_MODULE_RULE_NOT_EXIST = "Business module rule doesn't exist";
        private const string BUSINESS_MODULE_RULE_FAILED_DELETE = "failed to delete business module rule";
        private const string BUSINESS_MODULE_RULE_FAILED_UPDATE = "failed to update business module rule";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        internal static Status DeleteBusinessModuleRule(int groupId, long businessModuleRuleId)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                //check if rule exists
                BusinessModuleRule businessModuleRule = ApiDAL.GetBusinessModuleRuleCB(businessModuleRuleId);
                if (businessModuleRule == null || businessModuleRule.Id == 0 || groupId != businessModuleRule.GroupId)
                {
                    response.Set((int)eResponseStatus.RuleNotExists, BUSINESS_MODULE_RULE_NOT_EXIST);
                    return response;
                }

                if (!ApiDAL.DeleteBusinessModuleRule(groupId, businessModuleRuleId))
                {
                    response.Set((int)eResponseStatus.Error, BUSINESS_MODULE_RULE_FAILED_DELETE);
                    return response;
                }

                // delete rule from CB
                if (!ApiDAL.DeleteBusinessModuleRuleCB(groupId, businessModuleRuleId))
                {
                    log.ErrorFormat("Error while deleting BusinessModuleRule from CB. groupId: {0}, businessModuleRuleId:{1}", groupId, businessModuleRuleId);
                }

                //SetInvalidationKeys(groupId, businessModuleRuleId);
                response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRuleId);
            }

            return response;
        }

        internal static GenericResponse<BusinessModuleRule> UpdateBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRule)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                businessModuleRule.GroupId = groupId;

                if (!ApiDAL.UpdateBusinessModuleRule(groupId, businessModuleRule.Id, businessModuleRule.Name, businessModuleRule.Description))
                {
                    response.SetStatus(eResponseStatus.Error, BUSINESS_MODULE_RULE_FAILED_UPDATE);
                    return response;
                }

                if (!ApiDAL.SaveBusinessModuleRuleCB(groupId, businessModuleRule))
                {
                    log.ErrorFormat("Error while saving BusinessModuleRule. groupId: {0}, businessModuleRuleId:{1}", groupId, businessModuleRule.Id);
                }
                else
                {
                    //SetInvalidationKeys(groupId, businessModuleRule.Id);
                    response.Object = businessModuleRule;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRule.Id);
            }

            return response;
        }
    }
}
