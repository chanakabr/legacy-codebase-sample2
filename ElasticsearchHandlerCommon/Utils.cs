using ApiObjects;
using ApiObjects.SearchObjects;
using Catalog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalog.Cache;
using GroupsCacheManager;

namespace ElasticsearchTasksCommon
{
    public static class Utils
    {
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

        public static string GetNewMediaIndexStr(int nGroupID)
        {
            return string.Format("{0}_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
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

        public static Dictionary<int, Dictionary<int, Media>> GetGroupMedias(int groupId, int mediaID)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. mediaTranslations[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> mediaTranslations = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {
                GroupManager groupManager = new GroupManager();

                Group group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    Logger.Logger.Log("Error", "Could not load group from cache in GetGroupMedias", "ESFeeder");
                    return mediaTranslations;
                }

                ApiObjects.LanguageObj defaultLangauge = group.GetGroupDefaultLanguage();

                if (defaultLangauge == null)
                {
                    Logger.Logger.Log("Error", "Could not get group default language from cache in GetGroupMedias", "ESFeeder");
                    return mediaTranslations;
                }

                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@GroupID", groupId);
                storedProcedure.AddParameter("@MediaID", mediaID);

                Task<DataSet> dataSetTask = Task<DataSet>.Factory.StartNew(() => storedProcedure.ExecuteDataSet());
                dataSetTask.Wait();
                DataSet dataSet = dataSetTask.Result;

                Catalog.Utils.BuildMediaFromDataSet(ref mediaTranslations, ref medias, group, dataSet);
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Media Exception", ex.Message, "ESFeeder");
            }

            return mediaTranslations;
        }

        public static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, List<int> lSubGroups)
        {
            MediaSearchObj searchObject = new MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = channel.m_nMediaType.ToString();
            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID, lSubGroups);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        public static void CopySearchValuesToSearchObjects(ref MediaSearchObj searchObject, CutWith cutWith, List<SearchValue> channelSearchValues)
        {
            List<SearchValue> m_dAnd = new List<SearchValue>();
            List<SearchValue> m_dOr = new List<SearchValue>();

            SearchValue search = new SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }

        public static string GetPermittedWatchRules(int nGroupId, List<int> lSubGroup)
        {
            System.Data.DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, lSubGroup);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (System.Data.DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
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

        public static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            EpgCB res = null;

            DataSet ds = Tvinci.Core.DAL.EpgDal.GetEpgProgramDetails(nGroupID, nEpgID);

            if (ds != null && ds.Tables != null)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    //Basic Details
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EpgCB epg = new EpgCB();
                        epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(row["EPG_CHANNEL_ID"]);
                        epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(row["ID"]);
                        epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(row["GROUP_ID"]);
                        epg.isActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
                        epg.Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]);
                        epg.Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]);
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["START_DATE"])))
                        {
                            epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(row["START_DATE"]);
                        }
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["END_DATE"])))
                        {
                            epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(row["END_DATE"]);
                        }

                        //Metas
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            List<string> tempList;

                            foreach (DataRow meta in ds.Tables[2].Rows)
                            {
                                string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
                                string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

                                if (epg.Metas.TryGetValue(metaName, out tempList))
                                {
                                    tempList.Add(metaValue);
                                }
                                else
                                {
                                    tempList = new List<string>() { metaValue };
                                    epg.Metas.Add(metaName, tempList);
                                }
                            }
                        }

                        //Tags
                        if (ds.Tables.Count >= 4 && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                        {
                            List<string> tempList;
                            foreach (DataRow tag in ds.Tables[3].Rows)
                            {
                                string tagName = ODBCWrapper.Utils.GetSafeStr(tag["TagTypeName"]);
                                string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["TagValueName"]);
                                if (epg.Tags.TryGetValue(tagName, out tempList))
                                {
                                    tempList.Add(tagValue);
                                }
                                else
                                {
                                    tempList = new List<string>() { tagValue };
                                    epg.Tags.Add(tagName, tempList);
                                }
                            }
                        }

                        res = epg;
                    }
                }
            }

            return res;
        }

        public static List<EpgCB> GetEpgPrograms(int groupId, int epgId, List<string> languages)
        {
            List<EpgCB> results = new List<EpgCB>();

            // If no language was received - just get epg program by old method
            if (languages == null || languages.Count == 0)
            {
                EpgCB program = GetEpgProgram(groupId, epgId);

                results.Add(program);
            }
            else
            {
                try
                {
                    EpgBL.BaseEpgBL epgBL = EpgBL.Utils.GetInstance(groupId);

                    ulong uEpgID = (ulong)epgId;
                    results = epgBL.GetEpgCB(uEpgID, languages);
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error (GetEpgProgram)", string.Format("epg:{0}, msg:{1}, st:{2}", epgId, ex.Message, ex.StackTrace), "ESUpdateHandler");
                }
            }

            return results;
        }

        public static Dictionary<ulong, EpgCB> GetEpgPrograms(int nGroupID, DateTime? dDateTime, int nEpgID)
        {
            Dictionary<ulong, EpgCB> epgs = new Dictionary<ulong, EpgCB>();

            DataSet ds = Tvinci.Core.DAL.EpgDal.Get_EpgPrograms(nGroupID, dDateTime, nEpgID);

            if (ds != null && ds.Tables != null)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    //Basic Details
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EpgCB epg = new EpgCB();
                        epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(row["EPG_CHANNEL_ID"]);
                        epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(row["ID"]);
                        epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(row["GROUP_ID"]);
                        epg.isActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
                        epg.Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]);
                        epg.Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]);
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["START_DATE"])))
                        {
                            epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(row["START_DATE"]);
                        }
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["END_DATE"])))
                        {
                            epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(row["END_DATE"]);
                        }

                        //Metas
                        if (ds.Tables.Count >= 2 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            List<string> tempList;
                            DataRow[] metas = ds.Tables[1].Select("program_id=" + epg.EpgID);
                            foreach (DataRow meta in metas)
                            {
                                string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
                                string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

                                if (epg.Metas.TryGetValue(metaName, out tempList))
                                {
                                    tempList.Add(metaValue);
                                    epg.Tags.Add(metaName, tempList);
                                }
                                else
                                {
                                    tempList = new List<string>() { metaValue };
                                    epg.Metas.Add(metaName, tempList);
                                }
                            }
                        }
                        //Tags
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            List<string> tempList;
                            DataRow[] tags = ds.Tables[2].Select("program_id=" + epg.EpgID);
                            foreach (DataRow tag in tags)
                            {
                                string tagName = ODBCWrapper.Utils.GetSafeStr(tag["name"]);
                                string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["value"]);
                                if (epg.Tags.TryGetValue(tagName, out tempList))
                                {
                                    tempList.Add(tagValue);
                                    epg.Tags.Add(tagName, tempList);
                                }
                                else
                                {
                                    tempList = new List<string>() { tagValue };
                                    epg.Tags.Add(tagName, tempList);
                                }

                            }
                        }

                        epgs.Add(epg.EpgID, epg);
                    }
                }
            }

            return epgs;
        }
    }
}
