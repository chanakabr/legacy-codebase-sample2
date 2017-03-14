using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using QueueWrapper;
using ScheduledTasks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace Core.Api.Managers
{
    public class AssetLifeCycleRuleManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();
        private static AssetLifeCycleRuleManager instance = null;

        private AssetLifeCycleRuleManager() { }

        public static AssetLifeCycleRuleManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new AssetLifeCycleRuleManager();
                        }
                    }
                }

                return instance;
            }
        }

        #region Public Methods

        public Dictionary<int, List<AssetLifeCycleRule>> GetLifeCycleRules(int groupId = 0, List<long> rulesIds = null)
        {
            Dictionary<int, List<AssetLifeCycleRule>> groupIdToRulesMap = new Dictionary<int,List<AssetLifeCycleRule>>();
            try
            {
                DataSet ds = DAL.ApiDAL.GetLifeCycleRules(groupId, rulesIds);
                if (ds != null && ds.Tables != null && ds.Tables.Count == 4)
                {
                    groupIdToRulesMap = BuildAssetLifeCycleRuleFromDataSet(ds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAllLifeCycleRules failed", ex);
            }

            return groupIdToRulesMap;
        }

        public bool ApplyLifeCycleRuleActionsOnAssets(int groupId, List<int> assetIds, AssetLifeCycleRule ruleToApply) // currency assetIds=mediaIds
        {
            bool res = false;
            try
            {
                if (assetIds != null && assetIds.Count > 0 && ruleToApply != null && ruleToApply.Actions != null)
                {
                    res = ApplyLifeCycleRuleTagTransitionsOnAssets(assetIds, ruleToApply.Actions.TagIdsToAdd, ruleToApply.Actions.TagIdsToRemove) &&
                          ApplyLifeCycleRuleFileTypeAndPpvTransitionsOnAssets(assetIds, ruleToApply.Actions.FileTypesAndPpvsToAdd, ruleToApply.Actions.FileTypesAndPpvsToRemove) &&
                          (!ruleToApply.Actions.GeoBlockRuleToSet.HasValue || ApplyLifeCycleRuleGeoBlockTransitionOnAssets(assetIds, ruleToApply.Actions.GeoBlockRuleToSet.Value));
                    if (!ApiDAL.UpdateAssetLifeCycleLastRunDate(ruleToApply.Id))
                    {
                        log.WarnFormat("failed to update asset life cycle last run date for groupId: {0}, rule: {1}", groupId, ruleToApply);
                    }
                    if (!Catalog.Module.UpdateIndex(assetIds, groupId, eAction.Update))
                    {
                        log.WarnFormat("failed to update index of assetIds: {0} after applying rule: {1} for groupId: {2}",string.Join(",", assetIds), ruleToApply, groupId);
                    }
                }

                return res;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ApplyLifeCycleRulesOnAssets failed, assetIds: {0}, ruleToApply: {1}",
                                            assetIds != null && assetIds.Count > 0 ? string.Join(",", assetIds, ruleToApply.ToString()) : string.Empty), ex);
            }

            return res;
        }

        public int DoActionRules(int groupId = 0, List<long> rulesIds = null)
        {
            int result = 0;

            try
            {
                // Get all rules of this group
                Dictionary<int, List<AssetLifeCycleRule>> allRules = GetLifeCycleRules(groupId, rulesIds);                
                foreach (KeyValuePair<int, List<AssetLifeCycleRule>> pair in allRules)
                {
                    groupId = pair.Key;
                    List<AssetLifeCycleRule> rules = pair.Value;
                    Task<int>[] tasks = new Task<int>[rules.Count];

                    for (int i = 0; i < rules.Count; i++)
                    {
                        tasks[i] = Task.Factory.StartNew<int>(
                            (index) =>
                            {
                                long ruleId = -1;

                                try
                                {
                                    var rule = rules[(int)index];
                                    ruleId = rule.Id;

                                    #region UnifiedSearchRequest

                                    // Initialize unified search request:
                                    // SignString/Signature (basic catalog parameters)
                                    string sSignString = Guid.NewGuid().ToString();
                                    string sSignatureString = WS_Utils.GetTcmConfigValue("CatalogSignatureKey");
                                    string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

                                    // page size should be max_results so it will return everything
                                    int pageSize = WS_Utils.GetTcmIntValue("MAX_RESULTS");

                                    UnifiedSearchRequest unifiedSearchRequest = new UnifiedSearchRequest()
                                    {
                                        m_sSignature = sSignature,
                                        m_sSignString = sSignString,
                                        m_nGroupID = groupId,
                                        m_oFilter = new Core.Catalog.Filter()
                                        {
                                            m_bOnlyActiveMedia = true
                                        },
                                        m_nPageIndex = 0,
                                        m_nPageSize = pageSize,
                                        shouldIgnoreDeviceRuleID = true,
                                        shouldDateSearchesApplyToAllTypes = true,
                                        order = new ApiObjects.SearchObjects.OrderObj()
                                        {
                                            m_eOrderBy = ApiObjects.SearchObjects.OrderBy.ID,
                                            m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC
                                        },
                                        filterQuery = rule.KsqlFilter
                                    };

                                    #endregion

                                    // Call catalog
                                    var response = unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;
                                    List<int> assetIds = new List<int>();
                                    if (response != null && response.searchResults != null && response.searchResults.Count > 0)
                                    {
                                        // Remember count, first and last result - to verify that action was successful
                                        int count = response.m_nTotalItems;
                                        string firstAssetId = response.searchResults.FirstOrDefault().AssetId;
                                        string lastAssetId = response.searchResults.LastOrDefault().AssetId;

                                        assetIds = response.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                        // Apply rule on assets that returned from search
                                        this.ApplyLifeCycleRuleActionsOnAssets(groupId, assetIds, rule);

                                        var verificationResponse = unifiedSearchRequest.GetResponse(unifiedSearchRequest) as UnifiedSearchResponse;

                                        if (verificationResponse != null && verificationResponse.searchResults != null && verificationResponse.searchResults.Count > 0)
                                        {
                                            int verificationCount = verificationResponse.m_nTotalItems;
                                            string verificationFirstAssetId = response.searchResults.FirstOrDefault().AssetId;
                                            string verificationLastAssetId = response.searchResults.LastOrDefault().AssetId;

                                            // If search result is identical, it means that action is invalid - either the KSQL is not good or the action itself
                                            if (count == verificationCount && firstAssetId == verificationFirstAssetId && lastAssetId == verificationLastAssetId)
                                            {
                                                this.DisableRule(groupId, rule);
                                            }
                                        }
                                    }

                                    return assetIds.Count;
                                }
                                catch (Exception ex)
                                {
                                    log.ErrorFormat("Failed doing actions of rule: groupId = {0}, ruleId = {1}, ex = {2}", groupId, ruleId, ex);
                                    return 0;
                                }
                            }, i);
                    }

                    #region Finish tasks

                    Task.WaitAll(tasks);                    
                    
                    foreach (var task in tasks)
                    {
                        if (task != null)
                        {
                            result += task.Result;
                            task.Dispose();
                        }
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error in DoActionRules", ex);                
            }

            return result;
        }

        public bool BuildActionRuleDataFromKsql(int groupId, long ruleId, out string tagType, out string tagValue, out string dateMeta, out int dateValue)
        {
            bool result = false;
            tagType = string.Empty;
            tagValue = string.Empty;
            dateMeta = string.Empty;
            dateValue = 0;

            // Get the rule's data from Database
            var rules = GetLifeCycleRules(groupId, new List<long>() { ruleId });

            if (rules != null && rules.Count > 0)
            {
                // There should be only one rule
                var rule = rules[groupId].First();

                BooleanPhraseNode phrase = null;

                // Parse the rule's KSQL
                var status = BooleanPhraseNode.ParseSearchExpression(rule.KsqlFilter, ref phrase);

                // Validate parse result
                if (status != null && status.Code == (int)ResponseStatus.OK && phrase != null)
                {
                    // It should be a phrase, because it is (and ...)
                    if (phrase is BooleanPhrase)
                    {
                        var nodes = (phrase as BooleanPhrase).nodes;

                        // Validate there is at least one node
                        // First node should be a LEAF, looking like: cycletag='A'
                        if (nodes.Count > 0)
                        {
                            var firstNode = nodes[0] as BooleanLeaf;

                            if (firstNode != null)
                            {
                                tagType = firstNode.field;
                                tagValue = Convert.ToString(firstNode.value);
                            }
                        }

                        // Validate that date nodes actually exist - there should be two
                        // Second and third nodes should be LEAFs as well, looking like: cycledate>='-3days'  cycledate<'-2days'
                        if (nodes.Count > 2)
                        {
                            var secondNode = nodes[1] as BooleanLeaf;
                            var thirdNode = nodes[2] as BooleanLeaf;

                            if (secondNode != null && thirdNode != null)
                            {
                                dateMeta = thirdNode.field;

                                long thirdNodeValue = Convert.ToInt64(thirdNode.value);

                                // the value is the days-back represented in seconds
                                dateValue = -1 * (int)(thirdNodeValue / 60 / 60 / 24);

                                result = true;
                            }
                        }
                    }
                }
            }

            return result;
        }

        public bool BuildActionRuleKsqlFromData(int groupId, string tagType, string tagValue, string dateMeta, int dateValue, out string ksql)
        {
            bool result = false;
            ksql = string.Empty;

            if (!string.IsNullOrEmpty(tagType) && !string.IsNullOrEmpty(tagValue) && !string.IsNullOrEmpty(dateMeta) && dateValue > 0)
            {
                long firstDate = -1 * (dateValue + 1) * 24 * 60 * 60;
                long secondDate = -1 * (dateValue) * 24 * 60 * 60;

                ksql = string.Format("(and {0}='{1}' {2}>='{3}' {2}<'{4}')",
                    tagType, tagValue, dateMeta, firstDate, secondDate);

                result = true;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool FillRulesToTags(DataTable dt, List<AssetLifeCycleRule> rules, ref Dictionary<long, List<int>> ruleIdToTagIdsToAddMap,
                                            ref Dictionary<long, List<int>> ruleIdToTagIdsToRemoveMap)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ALCR_ID", 0);
                        if (id > 0)
                        {
                            int tagId = ODBCWrapper.Utils.GetIntSafeVal(dr, "TAG_ID", 0);
                            string actionId = ODBCWrapper.Utils.GetSafeStr(dr, "ACTION_ID");
                            AssetLifeCycleRuleAction action;
                            if (tagId > 0 && Enum.TryParse<AssetLifeCycleRuleAction>(actionId, out action))
                            {
                                switch (action)
                                {
                                    case AssetLifeCycleRuleAction.Add:
                                        if (ruleIdToTagIdsToAddMap.ContainsKey(id))
                                        {
                                            ruleIdToTagIdsToAddMap[id].Add(tagId);
                                        }
                                        else
                                        {
                                            ruleIdToTagIdsToAddMap.Add(id, new List<int>() { tagId });
                                        }
                                        break;
                                    case AssetLifeCycleRuleAction.Remove:
                                        if (ruleIdToTagIdsToRemoveMap.ContainsKey(id))
                                        {
                                            ruleIdToTagIdsToRemoveMap[id].Add(tagId);
                                        }
                                        else
                                        {
                                            ruleIdToTagIdsToRemoveMap.Add(id, new List<int>() { tagId });
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    res = true;
                }

                return res;
            }
            catch (Exception ex)
            {
                log.Error("FillRulesToTags failed", ex);
            }

            return res;
        }

        private bool FillRulesToFileTypesAndPpvs(DataTable dt, List<AssetLifeCycleRule> rules, ref Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> ruleIdToFileTypesAndPpvsToAdd,
                                                        ref Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> ruleIdToFileTypesAndPpvsToRemove)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ALCR_ID", 0);
                        if (id > 0)
                        {
                            int ppvId = ODBCWrapper.Utils.GetIntSafeVal(dr, "PPV_ID", 0);
                            int fileTypeId = ODBCWrapper.Utils.GetIntSafeVal(dr, "FILE_TYPE_ID", 0);
                            string actionId = ODBCWrapper.Utils.GetSafeStr(dr, "ACTION_ID");
                            AssetLifeCycleRuleAction action;
                            if (ppvId > 0 && fileTypeId > 0 && Enum.TryParse<AssetLifeCycleRuleAction>(actionId, out action))
                            {
                                switch (action)
                                {
                                    case AssetLifeCycleRuleAction.Add:
                                        if (!ruleIdToFileTypesAndPpvsToAdd.ContainsKey(id))
                                        {
                                            ruleIdToFileTypesAndPpvsToAdd.Add(id, new LifeCycleFileTypesAndPpvsTransitions());
                                        }

                                        if (!ruleIdToFileTypesAndPpvsToAdd[id].FileTypeIds.Contains(fileTypeId))
                                        {
                                            ruleIdToFileTypesAndPpvsToAdd[id].FileTypeIds.Add(fileTypeId);
                                        }

                                        if (!ruleIdToFileTypesAndPpvsToAdd[id].PpvIds.Contains(ppvId))
                                        {
                                            ruleIdToFileTypesAndPpvsToAdd[id].PpvIds.Add(ppvId);
                                        }
                                        break;
                                    case AssetLifeCycleRuleAction.Remove:
                                        if (!ruleIdToFileTypesAndPpvsToRemove.ContainsKey(id))
                                        {
                                            ruleIdToFileTypesAndPpvsToRemove.Add(id, new LifeCycleFileTypesAndPpvsTransitions());
                                        }

                                        if (!ruleIdToFileTypesAndPpvsToRemove[id].FileTypeIds.Contains(fileTypeId))
                                        {
                                            ruleIdToFileTypesAndPpvsToRemove[id].FileTypeIds.Add(fileTypeId);
                                        }

                                        if (!ruleIdToFileTypesAndPpvsToRemove[id].PpvIds.Contains(ppvId))
                                        {
                                            ruleIdToFileTypesAndPpvsToRemove[id].PpvIds.Add(ppvId);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                    res = true;
                }

                return res;
            }
            catch (Exception ex)
            {
                log.Error("FillRulesToFileTypesAndPpvs failed", ex);
            }

            return res;
        }

        private bool FillRulesToGeoBlock(DataTable dt, List<AssetLifeCycleRule> rules, ref Dictionary<long, int?> ruleIdToGeoBlockMap)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ALCR_ID", 0);
                        if (id > 0)
                        {
                            int? geoBlockRuleId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GEO_BLOCK_RULE_ID", -1);
                            geoBlockRuleId = geoBlockRuleId.HasValue && geoBlockRuleId.Value == -1 ? null : geoBlockRuleId;
                            if (ruleIdToGeoBlockMap.ContainsKey(id))
                            {
                                ruleIdToGeoBlockMap[id] = geoBlockRuleId;
                            }
                            else
                            {
                                ruleIdToGeoBlockMap.Add(id, geoBlockRuleId);
                            }
                        }
                    }

                    res = true;
                }

                return res;
            }
            catch (Exception ex)
            {
                log.Error("FillRulesToGeoBlock failed", ex);
            }

            return res;
        }

        private bool ApplyLifeCycleRuleTagTransitionsOnAssets(List<int> assetIds, List<int> tagIdsToAdd, List<int> tagIdsToRemove)
        {
            bool removeResult = false;
            bool addResult = false;
            try
            {
                if (tagIdsToRemove != null && tagIdsToRemove.Count > 0)
                {
                    removeResult = ApiDAL.RemoveTagsFromAssets(assetIds, tagIdsToRemove);
                }
                else
                {
                    removeResult = true;
                }

                if (tagIdsToAdd != null && tagIdsToAdd.Count > 0)
                {
                    addResult = ApiDAL.AddTagsToAssets(assetIds, tagIdsToAdd);
                }
                else
                {
                    addResult = true;
                }                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ApplyLifeCycleRuleTagTransitionsOnAssets failed, assetIds: {0}, tagIdsToAdd: {1}, tagIdsToRemove: {2}",
                                            assetIds != null && assetIds.Count > 0 ? string.Join(",", assetIds) : string.Empty,
                                            tagIdsToAdd != null && tagIdsToAdd.Count > 0 ? string.Join(",", tagIdsToAdd) : string.Empty,
                                            tagIdsToRemove != null && tagIdsToRemove.Count > 0 ? string.Join(",", tagIdsToRemove) : string.Empty), ex);
            }

            return removeResult && addResult;
        }

        private bool ApplyLifeCycleRuleFileTypeAndPpvTransitionsOnAssets(List<int> assetIds, LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToAdd,
                                                                                LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvsToRemove)
        {
            bool removeResult = false;
            bool addResult = false;
            try
            {
                if (fileTypesAndPpvsToAdd != null && (fileTypesAndPpvsToRemove.FileTypeIds.Count > 0 || fileTypesAndPpvsToRemove.PpvIds.Count > 0))
                {
                    removeResult = PricingDAL.RemoveFileTypesAndPpvsFromAssets(assetIds, fileTypesAndPpvsToRemove);
                }
                else
                {
                    removeResult = true;
                }

                if (fileTypesAndPpvsToAdd != null && (fileTypesAndPpvsToAdd.FileTypeIds.Count > 0 || fileTypesAndPpvsToAdd.PpvIds.Count > 0))
                {
                    addResult = PricingDAL.AddFileTypesAndPpvsToAssets(assetIds, fileTypesAndPpvsToAdd);
                }
                else
                {
                    addResult = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ApplyLifeCycleRuleFileTypeAndPpvTransitionsOnAssets failed, assetIds: {0}, fileTypesToPpvsMapToAdd: {1}, fileTypesToPpvsMapToRemove: {2}",
                                            assetIds != null && assetIds.Count > 0 ? string.Join(",", assetIds) : string.Empty, GetFileTypesToPpvsStringForLog(fileTypesAndPpvsToAdd),
                                            GetFileTypesToPpvsStringForLog(fileTypesAndPpvsToRemove)), ex);
            }

            return removeResult && addResult;
        }        

        private bool ApplyLifeCycleRuleGeoBlockTransitionOnAssets(List<int> assetIds, int geoBlockRuleId)
        {
            bool res = false;            
            try
            {
                res = ApiDAL.SetGeoBlockRuleIdOnAssets(assetIds, geoBlockRuleId);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("ApplyLifeCycleRuleGeoBlockTransitionOnAssets failed, assetIds: {0}, geoBlockRuleId: {1}",
                                        assetIds != null && assetIds.Count > 0 ? string.Join(",", assetIds) : string.Empty, geoBlockRuleId), ex);
            }

            return res;
        }

        private string GetFileTypesToPpvsStringForLog(LifeCycleFileTypesAndPpvsTransitions fileTypesAndPpvs)
        {
            if (fileTypesAndPpvs != null && (fileTypesAndPpvs.FileTypeIds.Count > 0 || fileTypesAndPpvs.PpvIds.Count > 0))
            {
                return string.Format("fileTypeId - {0}, ppvIds - {1}", string.Join(",", fileTypesAndPpvs.FileTypeIds), string.Join(",", fileTypesAndPpvs.PpvIds));                                
            }
            else
            {
                return string.Empty;
            }
        }

        private void DisableRule(int groupId, AssetLifeCycleRule rule)
        {
            if (rule != null)
            {
                try
                {
                    if (!ApiDAL.DisableRule(groupId, rule.Id))
                    {
                        log.ErrorFormat("Error when disabling rule {0} in group {1}", rule.Id, groupId);
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("Error when disabling rule {0} in group {1}, ex = {2}", rule.Id, groupId, ex);
                }
            }
        }


        private Dictionary<int, List<AssetLifeCycleRule>> BuildAssetLifeCycleRuleFromDataSet(DataSet ds)
        {
            Dictionary<int, List<AssetLifeCycleRule>> groupIdToRulesMap = new Dictionary<int, List<AssetLifeCycleRule>>();

            List<AssetLifeCycleRule> rules = new List<AssetLifeCycleRule>();
            DataTable rulesDt = ds.Tables[0];
            if (rulesDt != null && rulesDt.Rows != null && rulesDt.Rows.Count > 0)
            {
                foreach (DataRow dr in rulesDt.Rows)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                    int groupId = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID", 0);
                    if (id > 0 && groupId > 0)
                    {
                        string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                        string description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                        string filter = ODBCWrapper.Utils.GetSafeStr(dr, "KSQL_FILTER");
                        string metaDateName = ODBCWrapper.Utils.GetSafeStr(dr, "META_DATE_NAME");
                        AssetLifeCycleRule alcr = new AssetLifeCycleRule(id, groupId, name, description, filter, metaDateName);
                        rules.Add(alcr);
                    }
                }

                DataTable dtRulesTags = ds.Tables[1];
                DataTable dtRulesFileTypesAndPpvs = ds.Tables[2];
                DataTable dtRulesGeoBlock = ds.Tables[3];
                Dictionary<long, List<int>> ruleIdToTagIdsToAddMap = new Dictionary<long, List<int>>();
                Dictionary<long, List<int>> ruleIdToTagIdsToRemoveMap = new Dictionary<long, List<int>>();
                Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> fileTypesAndPpvsToAdd = new Dictionary<long, LifeCycleFileTypesAndPpvsTransitions>();
                Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> fileTypesAndPpvsToRemove = new Dictionary<long, LifeCycleFileTypesAndPpvsTransitions>();
                Dictionary<long, int?> ruleIdToGeoBlockMap = new Dictionary<long, int?>();
                if (FillRulesToTags(dtRulesTags, rules, ref ruleIdToTagIdsToAddMap, ref ruleIdToTagIdsToRemoveMap) &&
                    FillRulesToFileTypesAndPpvs(dtRulesFileTypesAndPpvs, rules, ref fileTypesAndPpvsToAdd, ref fileTypesAndPpvsToRemove) &&
                    FillRulesToGeoBlock(dtRulesGeoBlock, rules, ref ruleIdToGeoBlockMap))
                {
                    foreach (AssetLifeCycleRule alcr in rules)
                    {
                        alcr.Actions = new LifeCycleTransitions(ruleIdToTagIdsToAddMap.ContainsKey(alcr.Id) ? ruleIdToTagIdsToAddMap[alcr.Id] : new List<int>(),
                                                                               ruleIdToTagIdsToRemoveMap.ContainsKey(alcr.Id) ? ruleIdToTagIdsToRemoveMap[alcr.Id] : new List<int>(),
                                                                               fileTypesAndPpvsToAdd.ContainsKey(alcr.Id) ? fileTypesAndPpvsToAdd[alcr.Id] : new LifeCycleFileTypesAndPpvsTransitions(),
                                                                               fileTypesAndPpvsToRemove.ContainsKey(alcr.Id) ? fileTypesAndPpvsToRemove[alcr.Id] : new LifeCycleFileTypesAndPpvsTransitions(),
                                                                               ruleIdToGeoBlockMap.ContainsKey(alcr.Id) ? ruleIdToGeoBlockMap[alcr.Id] : null);
                        if (!groupIdToRulesMap.ContainsKey(alcr.GroupId))
                        {
                            groupIdToRulesMap.Add(alcr.GroupId, new List<AssetLifeCycleRule>());
                        }

                        groupIdToRulesMap[alcr.GroupId].Add(alcr);
                    }
                }
            }

            return groupIdToRulesMap;
        }

        #endregion

    }
}
