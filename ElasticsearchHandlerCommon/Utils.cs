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

                if (dataSet != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow row in dataSet.Tables[0].Rows)
                    {
                        Media media = new Media();

                        if (dataSet.Tables[0].Columns != null && dataSet.Tables[0].Rows != null)
                        {
                            #region media info
                            media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                            media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "watch_permission_type_id");
                            media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_type_id");
                            media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id");
                            media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "is_active");
                            media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row, "device_rule_id");
                            media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row, "like_counter");
                            media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row, "views");
                            media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                            double votesSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                            double votesCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                            if (votesCount > 0)
                            {
                                media.m_nVotes = (int)votesCount;
                                media.m_dRating = votesSum / votesCount;
                            }

                            media.m_sName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                            media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row, "description");

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "create_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date");
                                media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                                media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "start_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "start_date");
                                media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "end_date");
                                media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");

                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "final_end_date")))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "final_end_date");
                                media.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");

                            }
                            #endregion

                            #region - get all metas by groupId

                            Dictionary<string, string> metas;

                            //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                            if (group.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out metas))
                            {
                                foreach (string meta in metas.Keys)
                                {
                                    //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                    string metaName;
                                    metas.TryGetValue(meta, out metaName);

                                    if (!string.IsNullOrEmpty(metaName))
                                    {
                                        string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[meta]);
                                        media.m_dMeatsValues.Add(metaName, sMetaValue);
                                    }
                                }
                            }

                            #endregion

                        }

                        medias.Add(media.m_nMediaID, media);

                    }

                    #region - get all the media files types for each mediaId that have been selected.

                    if (dataSet.Tables[1].Columns != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[1].Rows)
                        {
                            int mediaFileMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            string mediaFileTypeId = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                            medias[mediaFileMediaID].m_sMFTypes += string.Format("{0};", mediaFileTypeId);
                        }
                    }
                    #endregion

                    #region - get all media tags
                    if (dataSet.Tables[2].Columns != null && dataSet.Tables[2].Rows != null && dataSet.Tables[2].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[2].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "value");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            try
                            {
                                if (group.m_oGroupTags.ContainsKey(mttn))
                                {
                                    string tagName = group.m_oGroupTags[mttn];

                                    if (!string.IsNullOrEmpty(tagName))
                                    {
                                        if (!medias[nTagMediaID].m_dTagValues.ContainsKey(tagName))
                                        {
                                            medias[nTagMediaID].m_dTagValues.Add(tagName, new Dictionary<long, string>());
                                        }

                                        medias[nTagMediaID].m_dTagValues[tagName].Add(tagID, val);
                                    }
                                }
                            }
                            catch
                            {
                                Logger.Logger.Log("Error", string.Format("Caught exception when trying to add media to group tags. TagMediaId={0}; TagTypeID={1}; TagID={2}; TagValue={3}", nTagMediaID, mttn, tagID, val), "ESFeeder");
                            }
                        }
                    }
                    #endregion

                    #region Clone medias to all translated languages

                    foreach (int currentMediaId in medias.Keys)
                    {
                        Media media = medias[currentMediaId];

                        Dictionary<int, Media> tempMediaTrans = new Dictionary<int, Media>();
                        foreach (ApiObjects.LanguageObj oLanguage in group.GetLangauges())
                        {
                            tempMediaTrans.Add(oLanguage.ID, media.Clone());
                        }

                        mediaTranslations.Add(currentMediaId, tempMediaTrans);

                    }
                    #endregion

                    #region get all translated metas and media info

                    if (dataSet.Tables[3].Columns != null && dataSet.Tables[3].Rows != null && dataSet.Tables[3].Rows.Count > 0)
                    {
                        Dictionary<string, string> dMetas;

                        foreach (DataRow row in dataSet.Tables[3].Rows)
                        {
                            int currentMediaId = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                            int nLanguageID = ODBCWrapper.Utils.GetIntSafeVal(row, "LANGUAGE_ID");

                            if (mediaTranslations.ContainsKey(currentMediaId) && mediaTranslations[currentMediaId].ContainsKey(nLanguageID))
                            {
                                Media oMedia = mediaTranslations[currentMediaId][nLanguageID];

                                if (group.m_oMetasValuesByGroupId.TryGetValue(oMedia.m_nGroupID, out dMetas))
                                {
                                    #region get media translated name
                                    string sTransName = ODBCWrapper.Utils.GetSafeStr(row, "NAME");

                                    if (!string.IsNullOrEmpty(sTransName))
                                        oMedia.m_sName = sTransName;
                                    #endregion

                                    #region get media translated description
                                    string sTransDesc = ODBCWrapper.Utils.GetSafeStr(row, "DESCRIPTION");

                                    if (!string.IsNullOrEmpty(sTransDesc))
                                        oMedia.m_sDescription = sTransDesc;
                                    #endregion

                                    #region get media translated metas
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //if meta is a string, then get translated value from DB, for all other metas, we keep the same values as there's no translation
                                        if (sMeta.EndsWith("_STR"))
                                        {
                                            string sMetaName;
                                            dMetas.TryGetValue(sMeta, out sMetaName);

                                            if (!string.IsNullOrEmpty(sMetaName))
                                            {
                                                string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row, sMeta);

                                                if (!string.IsNullOrEmpty(sMetaValue))
                                                {
                                                    oMedia.m_dMeatsValues[sMetaName] = sMetaValue;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion

                    #region - get all translated media tags
                    if (dataSet.Tables[4].Columns != null && dataSet.Tables[4].Rows != null && dataSet.Tables[4].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[4].Rows)
                        {
                            int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                            int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                            string val = ODBCWrapper.Utils.GetSafeStr(row, "translated_value");
                            int nLangID = ODBCWrapper.Utils.GetIntSafeVal(row, "language_id");
                            long tagID = ODBCWrapper.Utils.GetLongSafeVal(row, "tag_id");

                            if (group.m_oGroupTags.ContainsKey(mttn) && !string.IsNullOrEmpty(val))
                            {
                                Media oMedia;

                                if (mediaTranslations.ContainsKey(nTagMediaID) && mediaTranslations[nTagMediaID].ContainsKey(nLangID))
                                {
                                    oMedia = mediaTranslations[nTagMediaID][nLangID];
                                    string sTagTypeName = group.m_oGroupTags[mttn];

                                    if (oMedia.m_dTagValues.ContainsKey(sTagTypeName))
                                    {
                                        oMedia.m_dTagValues[sTagTypeName][tagID] = val;
                                    }
                                    else
                                    {
                                        Dictionary<long, string> dTemp = new Dictionary<long, string>();
                                        dTemp[tagID] = val;
                                        oMedia.m_dTagValues[sTagTypeName] = dTemp;
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }
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
