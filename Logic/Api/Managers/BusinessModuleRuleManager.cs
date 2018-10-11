using ApiObjects.Response;
using ApiObjects.Rules;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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

                SetInvalidationKeys(groupId, businessModuleRuleId);
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
                    SetInvalidationKeys(groupId, businessModuleRule.Id);
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

        internal static GenericResponse<BusinessModuleRule> GetBusinessModuleRule(int groupId, long businessModuleRuleId)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                BusinessModuleRule businessModuleRule = ApiDAL.GetBusinessModuleRuleCB(businessModuleRuleId);

                if (businessModuleRule == null || businessModuleRule.Id == 0 || groupId != businessModuleRule.GroupId)
                {
                    response.SetStatus(eResponseStatus.RuleNotExists, BUSINESS_MODULE_RULE_NOT_EXIST);
                    return response;
                }

                response.Object = businessModuleRule;
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetBusinessModuleRule failed ex={0}, groupId={1}, businessModuleRuleId={2}", ex, groupId, businessModuleRuleId);
            }

            return response;
        }

        internal static GenericResponse<BusinessModuleRule> AddBusinessModuleRule(int groupId, BusinessModuleRule businessModuleRuleToAdd)
        {
            GenericResponse<BusinessModuleRule> response = new GenericResponse<BusinessModuleRule>();
            try
            {
                businessModuleRuleToAdd.GroupId = groupId;
                DataTable dt = ApiDAL.AddBusinessModuleRule(groupId, businessModuleRuleToAdd.Name, businessModuleRuleToAdd.Description);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    businessModuleRuleToAdd.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    if (!ApiDAL.SaveBusinessModuleRuleCB(groupId, businessModuleRuleToAdd))
                    {
                        log.ErrorFormat("Error while saving BusinessModuleRule. groupId: {0}, BusinessModuleRuleId:{1}", groupId, businessModuleRuleToAdd.Id);
                        return response;
                    }

                    SetInvalidationKeys(groupId);
                    response.Object = businessModuleRuleToAdd;
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while adding new businessModuleRule . groupId: {0}, businessModuleRule: {1}, ex: {2}", groupId, JsonConvert.SerializeObject(businessModuleRuleToAdd), ex);
            }

            return response;
        }

        internal static GenericListResponse<BusinessModuleRule> GetBusinessModuleRules(int groupId)
        {
            GenericListResponse<BusinessModuleRule> response = new GenericListResponse<BusinessModuleRule>();

            try
            {
                List<BusinessModuleRule> allBusinessModuleRules = new List<BusinessModuleRule>();
                string allBusinessModuleRulesKey = LayeredCacheKeys.GetAllBusinessModuleRulesKey(groupId);

                if (!LayeredCache.Instance.Get<List<BusinessModuleRule>>(allBusinessModuleRulesKey,
                                                                ref allBusinessModuleRules,
                                                                GetAllBusinessModuleRules,
                                                                new Dictionary<string, object>()
                                                                {
                                                                    { "groupId", groupId }
                                                                },
                                                                groupId,
                                                                LayeredCacheConfigNames.GET_ALL_BUSINESS_MODULE_RULES,
                                                                new List<string>() { LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId) }))
                {
                    log.ErrorFormat("GetBusinessModuleRules - GetBusinessModuleRules - Failed get data from cache. groupId: {0}", groupId);
                    return response;
                }

                response.Objects = allBusinessModuleRules;

                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.ErrorFormat("Failed GetBusinessModuleRules groupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }

        private static Tuple<List<BusinessModuleRule>, bool> GetAllBusinessModuleRules(Dictionary<string, object> funcParams)
        {
            List<BusinessModuleRule> allBusinessModuleRules = new List<BusinessModuleRule>();

            try
            {
                if (funcParams != null && funcParams.Count == 1)
                {
                    if (funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;

                        if (groupId.HasValue)
                        {
                            List<BusinessModuleRule> allBusinessModuleRulesDB = new List<BusinessModuleRule>();
                            string allBusinessModuleRulesFromDBKey = LayeredCacheKeys.GetAllBusinessModuleRulesFromDBKey();

                            if (!LayeredCache.Instance.Get<List<BusinessModuleRule>>(allBusinessModuleRulesFromDBKey,
                                                                            ref allBusinessModuleRulesDB,
                                                                            GetAllBusinessModuleRulesDB,
                                                                            null,
                                                                            groupId.Value,
                                                                            LayeredCacheConfigNames.GET_ALL_BUSINESS_MODULE_RULES_FROM_DB,
                                                                            new List<string>() { LayeredCacheKeys.GetAllBusinessModuleRulesInvalidationKey() }))
                            {
                                allBusinessModuleRules = null;
                                log.ErrorFormat("GetAllBusinessModuleRules - GetAllBusinessModuleRulesDB - Failed get data from cache. groupId: {0}", groupId);
                            }
                            if (allBusinessModuleRulesDB.Count > 0)
                            {
                                List<long> ruleIds = allBusinessModuleRulesDB.Select(x => x.Id).ToList();

                                var businessModuleRulesCB = ApiDAL.GetBusinessModuleRulesCB(ruleIds);

                                if (businessModuleRulesCB != null && businessModuleRulesCB.Count > 0)
                                {
                                    allBusinessModuleRules = businessModuleRulesCB;
                                }
                            }

                            log.Debug("GetAllBusinessModuleRules - success");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                allBusinessModuleRules = null;
                log.Error(string.Format("GetAllBusinessModuleRules failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BusinessModuleRule>, bool>(allBusinessModuleRules, allBusinessModuleRules != null);
        }

        private static Tuple<List<BusinessModuleRule>, bool> GetAllBusinessModuleRulesDB(Dictionary<string, object> funcParams)
        {
            List<BusinessModuleRule> businessModuleRules = null;

            try
            {
                DataTable dtBusinessModuleRules = ApiDAL.GetBusinessModuleRulesDB();
                businessModuleRules = new List<BusinessModuleRule>();

                if (dtBusinessModuleRules != null && dtBusinessModuleRules.Rows != null && dtBusinessModuleRules.Rows.Count > 0)
                {
                    foreach (DataRow businessModuleRuleRow in dtBusinessModuleRules.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(businessModuleRuleRow, "ID");
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(businessModuleRuleRow, "GROUP_ID");

                        BusinessModuleRule businessModuleRule = new BusinessModuleRule()
                        {
                            Id = id,
                            GroupId = groupId
                        };

                        businessModuleRules.Add(businessModuleRule);
                    }
                }
                log.Debug("businessModuleRules - success");
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllBusinessModuleRulesDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<BusinessModuleRule>, bool>(businessModuleRules, businessModuleRules != null);
        }

        private static void SetInvalidationKeys(int groupId, long? ruleId = null)
        {
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllBusinessModuleRulesInvalidationKey());
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetAllBusinessModuleRulesGroupInvalidationKey(groupId));

            if (ruleId.HasValue)
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetBusinessModuleRuleInvalidationKey(ruleId.Value));
            }
        }
    }
}
