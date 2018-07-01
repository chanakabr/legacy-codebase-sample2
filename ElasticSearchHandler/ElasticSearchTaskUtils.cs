using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;
using KLogMonitor;
using System.Reflection;
using GroupsCacheManager;
using ElasticSearch.Searcher;
using ApiObjects.Response;
using ElasticSearch.Common;
using Core.Catalog;
using Core.Catalog.Cache;
using Core.Catalog.CatalogManagement;
using ConfigurationManager;

namespace ElasticSearchHandler
{
    public static class ElasticSearchTaskUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly int DAYS = 7;
       
        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        public static string GetEpgGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_epg", nGroupID);
        }

        public static string GetMediaGroupAliasStr(int nGroupID)
        {
            return nGroupID.ToString();
        }

        public static string GetNewEpgIndexStr(int nGroupID)
        {
            return string.Format("{0}_epg_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewRecordingIndexStr(int nGroupID)
        {
            return string.Format("{0}_recording_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewMediaIndexStr(int nGroupID)
        {
            return string.Format("{0}_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewUtilsIndexString()
        {
            return string.Format("utils_{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));        
        }

        internal static string GetMetadataGroupAliasStr(int groupId)
        {
            return string.Format("{0}_metadata", groupId);
        }

        internal static string GetNewMetadataIndexName(int groupId)
        {
            return string.Format("{0}_metadata_{1}", groupId, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetNewChannelMetadataIndexName(int groupId)
        {
            return string.Format("{0}_channel_{1}", groupId, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        internal static string GetChannelMetadataIndexName(int groupId)
        {
            return string.Format("{0}_channel", groupId);
        }

        public static string GetTanslationType(string sType, LanguageObj oLanguage)
        {
            if (oLanguage.IsDefault)
            {
                return sType;
            }
            else
            {
                return string.Concat(sType, "_", oLanguage.Code);
            }
        }

        public static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            EpgCB epg = null;

            EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
            try
            {
                ulong uEpgID = ulong.Parse(nEpgID.ToString());
                epg = oEpgBL.GetEpgCB(uEpgID);
                return epg;
            }
            catch (Exception ex)
            {
                log.Error("Error (GetEpgProgram) " + string.Format("epg:{0}, msg:{1}, st:{2}", nEpgID, ex.Message, ex.StackTrace), ex);
                return null;
            }
        }
        public static List<EpgCB> GetEpgProgram(int nGroupID, int nEpgID, List<string> languages)
        {
            List<EpgCB> epgs = null;

            EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
            try
            {
                ulong uEpgID = ulong.Parse(nEpgID.ToString());
                epgs = oEpgBL.GetEpgCB(uEpgID, languages);
                return epgs;
            }
            catch (Exception ex)
            {
                log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", nEpgID, ex.Message, ex.StackTrace));
                return new List<EpgCB>();
            }
        }

        public static string GetPermittedWatchRules(int nGroupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, null);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }

        public static List<LanguageObj> GetLanguages(int nGroupID)
        {
            List<LanguageObj> lLang = new List<LanguageObj>();
            try
            {
                lLang = CatalogDAL.GetGroupLanguages(nGroupID);
                return lLang;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error GettingLanguages of group {0}. Exception: {1}", nGroupID, ex);

                return new List<LanguageObj>();
            }
        }

        // Get linear channel settings from catalog cache 
        public static void GetLinearChannelValues(List<EpgCB> lEpg, int groupID)
        {
            try
            {
                int days = ApplicationConfiguration.CatalogLogicConfiguration.CurrentRequestDaysOffset.IntValue;

                if (days == 0)
                {
                    days = DAYS;
                }

                List<string> epgChannelIds = lEpg.Distinct().Select(item => item.ChannelID.ToString()).ToList<string>();
                Dictionary<string, LinearChannelSettings> linearChannelSettings = CatalogCache.Instance().GetLinearChannelSettings(groupID, epgChannelIds);

                Parallel.ForEach(lEpg.Cast<EpgCB>(), currentElement =>
                {
                    if (!linearChannelSettings.ContainsKey(currentElement.ChannelID.ToString()))
                    {
                        currentElement.SearchEndDate = currentElement.EndDate.AddDays(days);
                    }
                    else if (linearChannelSettings[currentElement.ChannelID.ToString()].EnableCatchUp)
                    {
                        currentElement.SearchEndDate = 
                            currentElement.EndDate.AddMinutes(linearChannelSettings[currentElement.ChannelID.ToString()].CatchUpBuffer);
                    }
                    else
                    {
                        currentElement.SearchEndDate = currentElement.EndDate;
                    }
                });
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. (in GetLinearChannelValues). Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }
        }

        public static bool GetMetasAndTagsForMapping(int groupId, bool? doesGroupUsesTemplates, ref Dictionary<string, KeyValuePair<eESFieldType, string>> metas, ref List<string> tags,
                                                    BaseESSeralizer serializer, Group group = null, CatalogGroupCache catalogGroupCache = null, bool isEpg = false)
        {
            bool result = true;
            tags = new List<string>();
            metas = new Dictionary<string, KeyValuePair<eESFieldType, string>>();
            if (!doesGroupUsesTemplates.HasValue)
            {
                doesGroupUsesTemplates = CatalogManager.DoesGroupUsesTemplates(groupId);
            }

            if (doesGroupUsesTemplates.Value && catalogGroupCache != null)
            {
                try
                {
                    HashSet<string> topicsToIgnore = Core.Catalog.CatalogLogic.GetTopicsToIgnoreOnBuildIndex();
                    HashSet<long> epgAssetStructMetaIds = catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(AssetManager.EPG_ASSET_STRUCT_SYSTEM_NAME) ? 
                                                            new HashSet<long>(catalogGroupCache.AssetStructsMapBySystemName[AssetManager.EPG_ASSET_STRUCT_SYSTEM_NAME].MetaIds) : new HashSet<long>();
                    tags = catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type == ApiObjects.MetaType.Tag && !topicsToIgnore.Contains(x.Key)
                                                                            && (!isEpg || epgAssetStructMetaIds.Contains(x.Value.Id))).Select(x => x.Key).ToList();
                    foreach (Topic topic in catalogGroupCache.TopicsMapBySystemName.Where(x => x.Value.Type != ApiObjects.MetaType.Tag && !topicsToIgnore.Contains(x.Key)
                                && (!isEpg || epgAssetStructMetaIds.Contains(x.Value.Id))).Select(x => x.Value))
                    {
                        string nullValue;
                        eESFieldType metaType;
                        serializer.GetMetaType(topic.Type, out metaType, out nullValue);
                        metas.Add(topic.SystemName, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                    }
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Failed BuildIndex for groupId: {0} because CatalogGroupCache", groupId), ex);
                    return false;
                }
            }
            else if (group != null)
            {
                if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lTagsName != null)
                {
                    foreach (var item in group.m_oEpgGroupSettings.m_lTagsName)
                    {
                        if (!tags.Contains(item))
                        {
                            tags.Add(item);
                        }
                    }
                }

                if (group.m_oGroupTags != null)
                {
                    foreach (var item in group.m_oGroupTags.Values)
                    {
                        if (!tags.Contains(item))
                        {
                            tags.Add(item);
                        }
                    }
                }

                if (group.m_oEpgGroupSettings != null && group.m_oEpgGroupSettings.m_lMetasName != null)
                {
                    foreach (string epgMeta in group.m_oEpgGroupSettings.m_lMetasName)
                    {
                        string nullValue;
                        eESFieldType metaType;
                        serializer.GetMetaType(epgMeta, out metaType, out nullValue);
                        metas.Add(epgMeta, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                    }
                }

                if (group.m_oMetasValuesByGroupId != null)
                {
                    foreach (Dictionary<string, string> metaMap in group.m_oMetasValuesByGroupId.Values)
                    {
                        foreach (KeyValuePair<string, string> meta in metaMap)
                        {
                            string nullValue;
                            eESFieldType metaType;
                            serializer.GetMetaType(meta.Key, out metaType, out nullValue);
                            metas.Add(meta.Value, new KeyValuePair<eESFieldType, string>(metaType, nullValue));
                        }
                    }
                }
            }

            return result;
        }

        public enum eESFeederType
        {
            MEDIA,
            EPG
        }


    }
}
