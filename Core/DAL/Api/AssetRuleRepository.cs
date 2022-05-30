using ApiObjects.Rules;
using CouchbaseManager;
using Phx.Lib.Log;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DAL.Api
{
    public interface IAssetRuleRepository
    {
        bool UpdateAssetRulesLastRunDate(int groupId, List<long> assetRuleIds);
        bool RemoveCountryRulesFromMedia(int groupId, long mediaId, List<long> assetRuleIdsToRemove);
        List<long> GetGeoAssetRulesAffectingMedia(int groupId, long mediaId);
        List<AssetRule> GetAssetRules(IEnumerable<long> assetRuleIds);
        bool InsertMediaCountry(int groupId, List<int> assetIds, int countryId, bool isAllowed, long ruleId);
        long AddAssetRule(int groupId, AssetRule assetRule, int assetRuleType = 0, bool withActions = true);
        AssetRule GetAssetRule(long assetRuleId);
        bool DeleteAssetRule(int groupId, long id);
        bool UpdateAssetRule(int groupId, AssetRule assetRule, bool withActions = true);
        List<AssetRule> GetAllAssetRules();
    }

    public class AssetRuleRepository : IAssetRuleRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<AssetRuleRepository> lazy = new Lazy<AssetRuleRepository>(() => new AssetRuleRepository(), LazyThreadSafetyMode.PublicationOnly);

        public static IAssetRuleRepository Instance { get { return lazy.Value; } }

        private AssetRuleRepository()
        {
        }

        public bool UpdateAssetRulesLastRunDate(int groupId, List<long> assetRuleIds)
        {
            bool result = false;
            try
            {
                StoredProcedure sp = new StoredProcedure("UpdateAssetRulesLastRunDate");
                sp.AddParameter("@groupId", groupId);
                sp.AddIDListParameter<long>("@assetRuleIds", assetRuleIds, "ID");

                result = sp.ExecuteReturnValue<int>() > 0;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAssetRulesLastRunDate in DB, groupId: {0}, assetRuleIds: {1}, ex:{2} ", groupId, string.Join(", ", assetRuleIds), ex);
            }

            return result;
        }

        public bool RemoveCountryRulesFromMedia(int groupId, long mediaId, List<long> assetRuleIdsToRemove)
        {
            bool result = false;
            try
            {
                StoredProcedure sp = new StoredProcedure("RemoveCountryRulesFromMedia");
                sp.AddIDListParameter<long>("@assetRuleIds", assetRuleIdsToRemove, "ID");
                sp.AddParameter("@mediaId", mediaId);
                sp.AddParameter("@groupId", groupId);

                result = sp.ExecuteReturnValue<int>() > 0;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while RemoveCountryRulesFromMedia in DB, groupId: {0}, assetRuleIds: {1}, ex:{2} ", groupId, string.Join(", ", assetRuleIdsToRemove), ex);
            }

            return result;
        }

        public List<long> GetGeoAssetRulesAffectingMedia(int groupId, long mediaId)
        {
            List<long> assetRuleIds = new List<long>();
            try
            {
                StoredProcedure sp = new StoredProcedure("GetGeoAssetRulesAffectingMedia");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@mediaId", mediaId);

                var dtAssetRules = sp.Execute();
                if (dtAssetRules != null && dtAssetRules.Rows != null && dtAssetRules.Rows.Count > 0)
                {
                    foreach (DataRow assetRuleRow in dtAssetRules.Rows)
                    {
                        assetRuleIds.Add(ODBCWrapper.Utils.GetLongSafeVal(assetRuleRow, "RULE_ID"));
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetGeoAssetRulesAffectingMedia in DB, groupId: {0}, mediaId: {1}, ex:{2} ", groupId, mediaId, ex);
            }

            return assetRuleIds;
        }

        public bool InsertMediaCountry(int groupId, List<int> assetIds, int countryId, bool isAllowed, long ruleId)
        {
            try
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("InsertMediaCountry");
                sp.AddIDListParameter("@mediaIds", assetIds, "ID");
                sp.AddParameter("@countryId", countryId);
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@isAllowed", isAllowed);
                sp.AddParameter("@ruleId", ruleId);

                int rowCount = sp.ExecuteReturnValue<int>();
                return rowCount > 0;
            }
            catch (Exception ex)
            {
                log.Error("Error while adding country to medias", ex);
            }

            return false;
        }

        private static string GetAssetRuleKey(long assetRuleId)
        {
            return string.Format("asset_rule:{0}", assetRuleId);
        }

        public List<AssetRule> GetAssetRules(IEnumerable<long> assetRuleIds)
        {
            List<string> assetRuleKeys = new List<string>();
            foreach (var assetRuleId in assetRuleIds)
            {
                assetRuleKeys.Add(GetAssetRuleKey(assetRuleId));
            }

            return UtilsDal.GetObjectListFromCB<AssetRule>(eCouchbaseBucket.OTT_APPS, assetRuleKeys, true);
        }

        public long AddAssetRule(int groupId, AssetRule assetRule, int assetRuleType = 0, bool withActions = true)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Insert_AssetRule");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@name", assetRule.Name);
                sp.AddParameter("@description", assetRule.Description);
                sp.AddParameter("@assetRuleType", assetRuleType);

                var dt = sp.Execute();
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    assetRule.Id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID");

                    // Save assetRulesActions
                    if (assetRule.Id > 0 && withActions && !SaveAssetRuleCB(groupId, assetRule))
                    {
                        log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                        return 0;
                    }

                    return assetRule.Id;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddAssetRule in DB, groupId: {0}, name: {1}, description: {2} , ruleType: {3}, ex:{4} ", groupId, assetRule.Name, assetRule.Description, assetRuleType, ex);
            }

            return 0;
        }

        public AssetRule GetAssetRule(long assetRuleId)
        {
            string key = GetAssetRuleKey(assetRuleId);
            return UtilsDal.GetObjectFromCB<AssetRule>(eCouchbaseBucket.OTT_APPS, key);
        }

        private bool SaveAssetRuleCB(int groupId, AssetRule assetRule)
        {
            if (assetRule != null && assetRule.Id > 0 && assetRule.Conditions != null && assetRule.Conditions.Count > 0)
            {
                var assetRuleKey = GetAssetRuleKey(assetRule.Id);
                return UtilsDal.SaveObjectInCB<AssetRule>(eCouchbaseBucket.OTT_APPS, assetRuleKey, assetRule, true);
            }

            return false;
        }

        public bool DeleteAssetRule(int groupId, long id)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Delete_AssetRule");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", id);
                
                // delete assetRulesActions from CB
                if (sp.ExecuteReturnValue<int>() > 0 && !DeleteAssetRuleCB(groupId, id))
                {
                    log.ErrorFormat("Error while delete AssetRules CB. groupId: {0}, assetRuleId:{1}", groupId, id);
                }

                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAssetRule in DB, groupId: {0}, id: {1} , ex:{1} ", groupId, id, ex);
            }

            return false;
        }

        private bool DeleteAssetRuleCB(int groupId, long assetRuleId)
        {
            string assetRuleKey = GetAssetRuleKey(assetRuleId);
            return UtilsDal.DeleteObjectFromCB(eCouchbaseBucket.OTT_APPS, assetRuleKey);
        }

        public bool UpdateAssetRule(int groupId, AssetRule assetRule, bool withActions = true)
        {
            try
            {
                StoredProcedure sp = new StoredProcedure("Update_AssetRule");
                sp.AddParameter("@groupId", groupId);
                sp.AddParameter("@id", assetRule.Id);
                sp.AddParameter("@name", assetRule.Name);
                sp.AddParameter("@description", assetRule.Description);

                // upsert dtUpdatedAssetRulesActions            
                if (sp.ExecuteReturnValue<int>() > 0)
                {
                    if (withActions && !SaveAssetRuleCB(groupId, assetRule))
                    {
                        log.ErrorFormat("Error while saving AssetRule. groupId: {0}, assetRuleId:{1}", groupId, assetRule.Id);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAssetRule in DB, groupId: {0}, assetRuleId: {1}, ex:{2} ", groupId, assetRule.Id, ex);
            }

            return false;
        }

        public List<AssetRule> GetAllAssetRules()
        {
            StoredProcedure sp = new StoredProcedure("Get_AssetRules");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            DataTable dtAssetRules = sp.Execute();
            var assetRules = new List<AssetRule>();
            if (dtAssetRules != null && dtAssetRules.Rows != null && dtAssetRules.Rows.Count > 0)
            {
                foreach (DataRow assetRuleRow in dtAssetRules.Rows)
                {
                    long assetRuleId = ODBCWrapper.Utils.GetLongSafeVal(assetRuleRow, "ID");
                    int groupId = ODBCWrapper.Utils.GetIntSafeVal(assetRuleRow, "GROUP_ID");
                    //int assetRuleType = ODBCWrapper.Utils.GetIntSafeVal(assetRuleRow, "ASSET_RULE_TYPE");

                    AssetRule assetRule = new AssetRule()
                    {
                        Id = assetRuleId,
                        GroupId = groupId
                    };

                    assetRules.Add(assetRule);
                }
            }
            return assetRules;
        }
    }
}
