
using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using Tvinci.Core.DAL;
using Core.Catalog.Request;
using Core.Catalog;
using ConfigurationManager;

namespace ElasticsearchTasksCommon
{
    public static class Utils
    {
        private static readonly HashSet<string> reservedUnifiedSearchStringFields = new HashSet<string>()
		            {
			            "name",
			            "description",
			            "epg_channel_id"
		            };

        private static readonly HashSet<string> reservedUnifiedSearchNumericFields = new HashSet<string>()
		            {
			            "like_counter",
			            "views",
			            "rating",
			            "votes"
		            };

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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

        public static string GetRecordingGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_recording", nGroupID);
        }
        public static string GetNewEpgIndexStr(int nGroupID)
        {
            return string.Format("{0}_epg_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewMediaIndexStr(int nGroupID)
        {
            return string.Format("{0}_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewRecordingIndexStr(int nGroupID)
        {
            return string.Format("{0}_recording_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
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
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ApiObjects.LanguageObj defaultLangauge = group.GetGroupDefaultLanguage();

                if (defaultLangauge == null)
                {
                    log.Error("Error - Could not get group default language from cache in GetGroupMedias");
                    return mediaTranslations;
                }

                ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");
                storedProcedure.AddParameter("@GroupID", groupId);
                storedProcedure.AddParameter("@MediaID", mediaID);

                DataSet dataSet = storedProcedure.ExecuteDataSet();
                //Task<DataSet> dataSetTask = Task<DataSet>.Factory.StartNew(() => storedProcedure.ExecuteDataSet());
                //dataSetTask.Wait();
                //DataSet dataSet = dataSetTask.Result;

                Core.Catalog.Utils.BuildMediaFromDataSet(ref mediaTranslations, ref medias, group, dataSet);

                // get media update dates
                DataTable updateDates = CatalogDAL.Get_MediaUpdateDate(new List<int>() { mediaID });

                OverrideMediaUpdateDates(ref mediaTranslations, updateDates);
            }
            catch (Exception ex)
            {
                log.Error("Media Exception", ex);
            }

            return mediaTranslations;
        }

        private static void OverrideMediaUpdateDates(ref Dictionary<int, Dictionary<int, Media>> translatedMedias, DataTable dataTable)
        {
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                int id;
                foreach (DataRow row in dataTable.Rows)
                {
                    id = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                    if (translatedMedias.ContainsKey(id) && translatedMedias[id] != null && !string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                    {
                        DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                        foreach (var media in translatedMedias[id].Values)
                        {
                            if (media != null)
                                media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                        }
                    }
                }
            }
        }

        public static MediaSearchObj BuildBaseChannelSearchObject(Channel channel, List<int> lSubGroups)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;

            if (channel.m_nMediaType != null)
            {
                searchObject.m_sMediaTypes = string.Join(";", channel.m_nMediaType.Select(type => type.ToString()));
            }

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

        public static string GetPermittedWatchRules(int nGroupId, List<int> lSubGroup = null)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId, lSubGroup);
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
                        epg.Crid = ODBCWrapper.Utils.GetSafeStr(row["crid"]);

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

        public static List<EpgCB> GetEpgPrograms(int groupId, int epgId, List<string> languages, EpgBL.BaseEpgBL epgBL = null)
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
                    if (epgBL == null)
                    {
                        epgBL = EpgBL.Utils.GetInstance(groupId);
                    }

                    ulong uEpgID = (ulong)epgId;
                    results = epgBL.GetEpgCB(uEpgID, languages);
                }
                catch (Exception ex)
                {
                    log.Error("Error (GetEpgProgram) - " + string.Format("epg:{0}, msg:{1}, st:{2}", epgId, ex.Message, ex.StackTrace), ex);
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

        #region Channels

      
        public static UnifiedSearchDefinitions BuildSearchDefinitions(Channel channel, bool useMediaTypes)
        {
            UnifiedSearchDefinitions definitions = new UnifiedSearchDefinitions();

            definitions.groupId = channel.m_nGroupID;

            if (useMediaTypes)
            {
                definitions.mediaTypes = new List<int>(channel.m_nMediaType);
            }

            if (channel.m_nMediaType != null)
            {
                // Nothing = all
                if (channel.m_nMediaType.Count == 0)
                {
                    definitions.shouldSearchEpg = true;
                    definitions.shouldSearchMedia = true;
                }
                else
                {
                    if (channel.m_nMediaType.Contains(Channel.EPG_ASSET_TYPE))
                    {
                        definitions.shouldSearchEpg = true;
                    }

                    // If there's anything besides EPG
                    if (channel.m_nMediaType.Count(type => type != Channel.EPG_ASSET_TYPE) > 0)
                    {
                        definitions.shouldSearchMedia = true;
                    }
                }
            }

            definitions.permittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
            definitions.order = new OrderObj();

            definitions.shouldUseStartDate = false;
            definitions.shouldUseFinalEndDate = false;

            if (!string.IsNullOrEmpty(channel.filterQuery))
            {
                BooleanPhraseNode filterTree = null;
                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref filterTree);

                if (parseStatus.Code != (int)eResponseStatus.OK)
                {
                    throw new KalturaException(parseStatus.Message, parseStatus.Code);
                }
                else
                {
                    definitions.filterPhrase = filterTree;
                }

                GroupManager groupManager = new GroupManager();

                Group group = groupManager.GetGroup(channel.m_nParentGroupID);

                if (group == null)
                {
                    log.Error("Error - Could not load group from cache in GetGroupMedias");
                }

                var dummyRequest = new BaseRequest()
                {
                    domainId = 0,
                    m_nGroupID = channel.m_nParentGroupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter(),
                    m_sSiteGuid = string.Empty,
                    m_sUserIP = string.Empty
                };

                CatalogLogic.UpdateNodeTreeFields(dummyRequest,
                    ref definitions.filterPhrase, definitions, group);
            }

            return definitions;
        }

        public static string GetPermittedWatchRules(int groupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(groupId, null);
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
        
        #endregion

        // This is taken from Media index builder!
        public static Dictionary<int, Dictionary<int, Media>> GetGroupMediasTotal(int nGroupID, int nMediaID)
        {
            //dictionary contains medias such that first key is media_id, which returns a dictionary with a key language_id and value Media object.
            //E.g. dMedias[123][2] --> will return media 123 of the hebrew language
            Dictionary<int, Dictionary<int, Media>> dMediaTrans = new Dictionary<int, Dictionary<int, Media>>();

            //temporary media dictionary
            Dictionary<int, Media> medias = new Dictionary<int, Media>();

            try
            {
                Group oGroup = GroupsCache.Instance().GetGroup(nGroupID);

                if (oGroup == null)
                {
                    log.Error("Could not load group from cache in GetGroupMedias");
                    return dMediaTrans;
                }

                ApiObjects.LanguageObj oDefaultLangauge = oGroup.GetGroupDefaultLanguage();

                if (oDefaultLangauge == null)
                {
                    log.Error("Could not get group default language from cache in GetGroupMedias");
                    return dMediaTrans;
                }

                ODBCWrapper.StoredProcedure groupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                groupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

                groupMedias.AddParameter("@GroupID", nGroupID);
                groupMedias.AddParameter("@MediaID", nMediaID);

                // increase timeout: default is 30. Stored procedure might take longer than that if there are too many media.
                groupMedias.SetTimeout(90);

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => groupMedias.ExecuteDataSet());
                tDS.Wait();
                DataSet dataSet = tDS.Result;

                if (dataSet != null && dataSet.Tables.Count > 0)
                {
                    if (dataSet.Tables[0].Rows.Count > 0)
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

                                // by default - media is not free
                                media.isFree = false;

                                double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                                double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                                if (dCount > 0)
                                {
                                    media.m_nVotes = (int)dCount;
                                    media.m_dRating = dSum / dCount;
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

                                media.geoBlockRule = ODBCWrapper.Utils.ExtractInteger(row, "geo_block_rule_id");

                                string epgIdentifier = ODBCWrapper.Utils.ExtractString(row, "epg_identifier");

                                if (!string.IsNullOrEmpty(epgIdentifier))
                                {
                                    media.epgIdentifier = epgIdentifier;
                                }

                                #endregion

                                #region - get all metas by groupId
                                Dictionary<string, string> dMetas;
                                //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                                if (oGroup.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out dMetas))
                                {
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                        string sMetaName;
                                        dMetas.TryGetValue(sMeta, out sMetaName);

                                        if (!string.IsNullOrEmpty(sMetaName) && !sMeta.StartsWith("date"))
                                        {
                                            string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);

                                            if (!media.m_dMeatsValues.ContainsKey(sMetaName))
                                            {
                                                media.m_dMeatsValues.Add(sMetaName, sMetaValue);
                                            }
                                            else
                                            {
                                                log.WarnFormat("Duplicate meta found. group Id = {0}, name = {1}, media_id = {2}", nGroupID, sMetaName, media.m_nMediaID);
                                            }
                                        }
                                    }
                                }
                            }
                            medias.Add(media.m_nMediaID, media);
                                #endregion
                        }

                        #region - get all the media files types for each mediaId that have been selected.
                        if (dataSet.Tables[1].Columns != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[1].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                                bool isTypeFree = ODBCWrapper.Utils.ExtractBoolean(row, "is_free");

                                Media theMedia = medias[mediaID];

                                theMedia.m_sMFTypes += string.Format("{0};", sMFT);

                                int mediaTypeId;

                                if (isTypeFree)
                                {
                                    // if at least one of the media types is free - this media is free
                                    theMedia.isFree = true;

                                    if (int.TryParse(sMFT, out mediaTypeId))
                                    {
                                        theMedia.freeFileTypes.Add(mediaTypeId);
                                    }
                                }
                            }
                        }


                        #endregion

                        #region - get regions of media

                        // Regions table should be 6h on stored procedure
                        if (dataSet.Tables.Count > 5 && dataSet.Tables[5].Columns != null && dataSet.Tables[5].Rows != null)
                        {
                            foreach (DataRow mediaRegionRow in dataSet.Tables[5].Rows)
                            {
                                int mediaId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "MEDIA_ID");
                                int regionId = ODBCWrapper.Utils.ExtractInteger(mediaRegionRow, "REGION_ID");

                                // Accumulate region ids in list
                                medias[mediaId].regions.Add(regionId);
                            }
                        }

                        // If no regions were found for this media - use 0, that indicates that the media is region-less
                        foreach (Media media in medias.Values)
                        {
                            if (media.regions.Count == 0)
                            {
                                media.regions.Add(0);
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
                                    if (oGroup.m_oGroupTags.ContainsKey(mttn))
                                    {
                                        string sTagName = oGroup.m_oGroupTags[mttn];

                                        if (!string.IsNullOrEmpty(sTagName))
                                        {
                                            if (!medias[nTagMediaID].m_dTagValues.ContainsKey(sTagName))
                                            {
                                                medias[nTagMediaID].m_dTagValues.Add(sTagName, new Dictionary<long, string>());
                                            }

                                            if (!medias[nTagMediaID].m_dTagValues[sTagName].ContainsKey(tagID))
                                            {
                                                medias[nTagMediaID].m_dTagValues[sTagName].Add(tagID, val);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    log.Error(string.Format("Caught exception when trying to add media to group tags. TagMediaId={0}; TagTypeID={1}; TagID={2}; TagValue={3}",
                                        nTagMediaID, mttn, tagID, val));
                                }
                            }
                        }
                        #endregion

                        #region - get all date meta
                        if (dataSet.Tables.Count > 6 && dataSet.Tables[6].Columns != null && dataSet.Tables[6].Rows != null && dataSet.Tables[6].Rows.Count > 0)
                        {
                            foreach (DataRow row in dataSet.Tables[6].Rows)
                            {
                                int mediaId = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                string metaName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                                DateTime val = ODBCWrapper.Utils.GetDateSafeVal(row, "value");
                                try
                                {
                                    if (!medias[mediaId].m_dMeatsValues.ContainsKey(metaName))
                                    {
                                        medias[mediaId].m_dMeatsValues.Add(metaName, val.ToString("yyyyMMddHHmmss"));
                                    }
                                }
                                catch
                                {
                                    log.Error(string.Format("Caught exception when trying to add media to group date metas. mediaId = {0}, metaName = {1}, val = {2}",
                                        mediaId, metaName, val));
                                }
                            }
                        }
                        #endregion

                        #region Clone medias to all translated languages
                        foreach (int mediaID in medias.Keys)
                        {
                            Media media = medias[mediaID];

                            Dictionary<int, Media> tempMediaTrans = new Dictionary<int, Media>();
                            foreach (ApiObjects.LanguageObj oLanguage in oGroup.GetLangauges())
                            {
                                tempMediaTrans.Add(oLanguage.ID, media.Clone());
                            }

                            dMediaTrans.Add(mediaID, tempMediaTrans);

                        }
                        #endregion

                        #region get all translated metas and media info

                        if (dataSet.Tables[3].Columns != null && dataSet.Tables[3].Rows != null && dataSet.Tables[3].Rows.Count > 0)
                        {
                            Dictionary<string, string> dMetas;

                            foreach (DataRow row in dataSet.Tables[3].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID");
                                int nLanguageID = ODBCWrapper.Utils.GetIntSafeVal(row, "LANGUAGE_ID");

                                if (dMediaTrans.ContainsKey(mediaID) && dMediaTrans[mediaID].ContainsKey(nLanguageID))
                                {
                                    Media oMedia = dMediaTrans[mediaID][nLanguageID];

                                    if (oGroup.m_oMetasValuesByGroupId.TryGetValue(oMedia.m_nGroupID, out dMetas))
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

                                if (oGroup.m_oGroupTags.ContainsKey(mttn) && !string.IsNullOrEmpty(val))
                                {
                                    Media oMedia;

                                    if (dMediaTrans.ContainsKey(nTagMediaID) && dMediaTrans[nTagMediaID].ContainsKey(nLangID))
                                    {
                                        oMedia = dMediaTrans[nTagMediaID][nLangID];
                                        string sTagTypeName = oGroup.m_oGroupTags[mttn];

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

                        #region - get countries of media

                        // Regions table should be 6h on stored procedure
                        if (dataSet.Tables.Count > 7 && dataSet.Tables[7].Columns != null && dataSet.Tables[7].Rows != null)
                        {
                            foreach (DataRow mediaCountryRow in dataSet.Tables[7].Rows)
                            {
                                int mediaId = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "MEDIA_ID");
                                int countryId = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "COUNTRY_ID");
                                bool isAllowed = ODBCWrapper.Utils.GetIntSafeVal(mediaCountryRow, "IS_ALLOWED") == 1;

                                if (isAllowed)
                                {
                                    medias[mediaId].allowedCountries.Add(countryId);
                                }
                                else
                                {
                                    medias[mediaId].blockedCountries.Add(countryId);
                                }
                            }   
                        }

                        // If no allowed countries were found for this media - use 0, that indicates that the media is allowed everywhere
                        foreach (Media media in medias.Values)
                        {
                            if (media.allowedCountries.Count == 0)
                            {
                                media.allowedCountries.Add(0);
                            }
                        }

                        #endregion

                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupMedias - {0}", ex.Message), ex);
            }

            return dMediaTrans;
        }

        public static Dictionary<int, KeyValuePair<bool, DateTime>> GetRebaseMediaInformation(int groupId)
        {
            Dictionary<int, KeyValuePair<bool, DateTime>> result = new Dictionary<int, KeyValuePair<bool, DateTime>>();

            ODBCWrapper.StoredProcedure storedProcedure = new ODBCWrapper.StoredProcedure("Get_GroupMedias_Rebase");
            storedProcedure.AddParameter("@GroupID", groupId);
            storedProcedure.SetConnectionKey("MAIN_CONNECTION_STRING");

            DataSet dataSet = null;
            try
            {
                dataSet = storedProcedure.ExecuteDataSet();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Rebase media for group {0} - failed execute data data set GetRebaseMediaInformation. ex = {1}", groupId, ex);

            }

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                var table = dataSet.Tables[0];
                
                foreach (DataRow row in table.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(row, "ID");
                    bool isActive = ODBCWrapper.Utils.ExtractBoolean(row, "IS_ACTIVE");
                    DateTime updateDate = ODBCWrapper.Utils.ExtractDateTime(row, "UPDATE_DATE");

                    result.Add(id,
                        new KeyValuePair<bool, DateTime>(isActive, updateDate));
                }
            }

            return result;
        }
    }
}
