using ApiObjects;
using ApiObjects.AssetLifeCycleRules;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using QueueWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TVinciShared;

namespace APILogic.Api.Managers
{
    public class AssetLifeCycleRuleManager
    {

        private const string ACTION_RULE_TASK = "distributed_tasks.process_action_rule";

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

        public List<AssetLifeCycleRule> GetAllLifeCycleRules(int groupId)
        {
            List<AssetLifeCycleRule> rules = null;
            try
            {
                DataSet ds = DAL.ApiDAL.GetAllLifeCycleRules(groupId);
                if (ds != null && ds.Tables != null && ds.Tables.Count == 4)
                {
                    Dictionary<long, AssetLifeCycleRule> mappedRules = new Dictionary<long, AssetLifeCycleRule>();
                    DataTable rulesDt = ds.Tables[0];
                    if (rulesDt != null && rulesDt.Rows != null && rulesDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in rulesDt.Rows)
                        {
                            long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                            if (id > 0 && !mappedRules.ContainsKey(id))
                            {
                                string name = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                                string description = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                                string filter = ODBCWrapper.Utils.GetSafeStr(dr, "KSQL_FILTER");
                                int transitionIntervalInDays = ODBCWrapper.Utils.GetIntSafeVal(dr, "TRANSITION_INTERVAL");
                                AssetLifeCycleRule alcr = new AssetLifeCycleRule(id, name, description, filter, transitionIntervalInDays);
                                mappedRules.Add(id, alcr);
                            }
                        }

                        DataTable dtRulesTags = ds.Tables[1];
                        DataTable dtRulesFileTypesAndPpvs = ds.Tables[2];
                        DataTable dtRulesGeoBlock = ds.Tables[3];
                        Dictionary<long, List<int>> ruleIdToTagIdsToAddMap = new Dictionary<long, List<int>>();
                        Dictionary<long, List<int>> ruleIdToTagIdsToRemoveMap = new Dictionary<long, List<int>>();
                        Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> fileTypesAndPpvsToAdd = new Dictionary<long,LifeCycleFileTypesAndPpvsTransitions>();
                        Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> fileTypesAndPpvsToRemove = new Dictionary<long, LifeCycleFileTypesAndPpvsTransitions>();
                        Dictionary<long, int?> ruleIdToGeoBlockMap = new Dictionary<long, int?>();
                        if (FillRulesToTags(dtRulesTags, mappedRules, ref ruleIdToTagIdsToAddMap, ref ruleIdToTagIdsToRemoveMap) &&
                            FillRulesToFileTypesAndPpvs(dtRulesFileTypesAndPpvs, mappedRules, ref fileTypesAndPpvsToAdd, ref fileTypesAndPpvsToRemove) &&
                            FillRulesToGeoBlock(dtRulesGeoBlock, mappedRules, ref ruleIdToGeoBlockMap))
                        {
                            foreach (long alcrId in mappedRules.Keys)
                            {
                                mappedRules[alcrId].Actions = new LifeCycleTransitions(ruleIdToTagIdsToAddMap.ContainsKey(alcrId) ? ruleIdToTagIdsToAddMap[alcrId] : new List<int>(),
                                                                                       ruleIdToTagIdsToRemoveMap.ContainsKey(alcrId) ? ruleIdToTagIdsToRemoveMap[alcrId] : new List<int>(),
                                                                                       fileTypesAndPpvsToAdd.ContainsKey(alcrId) ? fileTypesAndPpvsToAdd[alcrId] : new LifeCycleFileTypesAndPpvsTransitions(),
                                                                                       fileTypesAndPpvsToRemove.ContainsKey(alcrId) ? fileTypesAndPpvsToRemove[alcrId] : new LifeCycleFileTypesAndPpvsTransitions(),
                                                                                       ruleIdToGeoBlockMap.ContainsKey(alcrId) ? ruleIdToGeoBlockMap[alcrId] : null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllLifeCycleRules failed, groupId : {0}", groupId), ex);
            }

            return rules;
        }

        public bool ApplyLifeCycleRuleActionsOnAssets(List<int> assetIds, AssetLifeCycleRule ruleToApply) // currency assetIds=mediaIds
        {
            bool res = false;
            try
            {
                if (assetIds != null && assetIds.Count > 0 && ruleToApply != null && ruleToApply.Actions != null)
                {
                    res = ApplyLifeCycleRuleTagTransitionsOnAssets(assetIds, ruleToApply.Actions.TagIdsToAdd, ruleToApply.Actions.TagIdsToRemove) &&
                          ApplyLifeCycleRuleFileTypeAndPpvTransitionsOnAssets(assetIds, ruleToApply.Actions.FileTypesAndPpvsToAdd, ruleToApply.Actions.FileTypesAndPpvsToRemove) &&
                          (!ruleToApply.Actions.GeoBlockRuleToSet.HasValue || ApplyLifeCycleRuleGeoBlockTransitionOnAssets(assetIds, ruleToApply.Actions.GeoBlockRuleToSet.Value));
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

        public bool DoActionRules(int groupId, List<long> ruleIds)
        {
            bool result = true;

            // Get all rules of this group
            List<AssetLifeCycleRule> allRules = GetAllLifeCycleRules(groupId);
            List<AssetLifeCycleRule> rules = new List<AssetLifeCycleRule>();

            // If parameter is null - we want to run all rules
            if (ruleIds == null)
            {
                rules = allRules;
            }
            else
            {
                // If parameter is not null, we want to run only specific rules
                rules = allRules.Where(rule => ruleIds.Contains(rule.Id)).ToList();
            }

            Task<bool>[] tasks = new Task<bool>[rules.Count];

            for (int i = 0; i < rules.Count; i++)
            {
                tasks[i] = Task.Factory.StartNew<bool>(
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

                            if (response != null && response.searchResults != null)
                            {
                                List<int> assetIds = response.searchResults.Select(asset => Convert.ToInt32(asset.AssetId)).ToList();

                                // Apply rule on assets that returned from search
                                this.ApplyLifeCycleRuleActionsOnAssets(assetIds, rule);
                            }

                            return true;
                        }
                        catch (Exception ex)
                        {
                            log.ErrorFormat("Failed doing actions of rule: groupId = {0}, ruleId = {1}, ex = {2}", groupId, ruleId, ex);
                            return false;
                        }
                    }, i);
            }

            Task.WaitAll(tasks);

            // Return false if one of the tasks returned false
            foreach (var task in tasks)
            {
                if (task != null)
                {
                    if (!task.Result)
                    {
                        result = false;
                    }

                    task.Dispose();
                }
            }

            // If proper interval was set, enqueue follow-up message so that task is run indefinitely
            int actionRuleTaskIntervalHours = TVinciShared.WS_Utils.GetTcmIntValue("action_rule_task_interval");

            if (actionRuleTaskIntervalHours > 0)
            {
                GenericCeleryQueue queue = new GenericCeleryQueue();

                string dataId = Guid.NewGuid().ToString();
                BaseCeleryData data = new BaseCeleryData(dataId, ACTION_RULE_TASK, groupId, ruleIds);

                bool enqueueResult = queue.Enqueue(data, string.Format("PROCESS_ACTION_RULE\\{0}", groupId));

                // Success of this method is dependent on enqueing the follow-up message
                result &= enqueueResult;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool FillRulesToTags(DataTable dt, Dictionary<long, AssetLifeCycleRule> mappedRules, ref Dictionary<long, List<int>> ruleIdToTagIdsToAddMap,
                                            ref Dictionary<long, List<int>> ruleIdToTagIdsToRemoveMap)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && mappedRules.ContainsKey(id))
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

        private bool FillRulesToFileTypesAndPpvs(DataTable dt, Dictionary<long, AssetLifeCycleRule> mappedRules, ref Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> ruleIdToFileTypesAndPpvsToAdd,
                                                        ref Dictionary<long, LifeCycleFileTypesAndPpvsTransitions> ruleIdToFileTypesAndPpvsToRemove)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && mappedRules.ContainsKey(id))
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

        private bool FillRulesToGeoBlock(DataTable dt, Dictionary<long, AssetLifeCycleRule> mappedRules, ref Dictionary<long, int?> ruleIdToGeoBlockMap)
        {
            bool res = false;
            try
            {
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && mappedRules.ContainsKey(id))
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
                    removeResult = ApiDAL.RemoveFileTypesAndPpvsFromAssets(assetIds, fileTypesAndPpvsToRemove);
                }
                else
                {
                    removeResult = true;
                }

                if (fileTypesAndPpvsToAdd != null && (fileTypesAndPpvsToAdd.FileTypeIds.Count > 0 || fileTypesAndPpvsToAdd.PpvIds.Count > 0))
                {
                    addResult = ApiDAL.AddFileTypesAndPpvsToAssets(assetIds, fileTypesAndPpvsToAdd);
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

        #endregion

    }
}
