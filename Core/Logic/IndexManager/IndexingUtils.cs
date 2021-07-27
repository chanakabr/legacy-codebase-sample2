
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
using ApiObjects.Catalog;
using ApiLogic.Catalog.IndexManager.GroupBy;

namespace Core.Catalog
{
    public static class IndexingUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly int DEFAULT_CURRENT_REQUEST_DAYS_OFFSET = 7;

        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        public static string GetEpgIndexAlias(int nGroupID)
        {
            return string.Format("{0}_epg", nGroupID);
        }

        public static string GetDailyEpgIndexName(int groupId, DateTime indexDate)
        {
            string dateString = indexDate.Date.ToString(ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT);
            return $"{groupId}_epg_v2_{dateString}";
        }

        public static string GetMediaIndexAlias(int nGroupID)
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

        public static string GetNewIPv6IndexString()
        {
            return string.Format("ipv6_{0}", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
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

        public static string GetRecordingGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_recording", nGroupID);
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

        public static bool GroupBySearchIsSupportedForOrder(OrderBy orderBy) => GetStrategy(orderBy) != null;


        internal static IGroupBySearch GetStrategy(OrderBy orderBy)
        {
            switch (orderBy)
            {
                case OrderBy.NONE:
                case OrderBy.ID:
                case OrderBy.CREATE_DATE:
                case OrderBy.START_DATE: return new GroupByWithOrderByNumericField();
                case OrderBy.NAME:
                case OrderBy.META: return new GroupByWithOrderByNonNumericField();
                default: return null;
            }
        }

        public enum eESFeederType
        {
            MEDIA,
            EPG
        }
    }
}
