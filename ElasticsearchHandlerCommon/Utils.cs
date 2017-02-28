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
        private static int maxNGram = 0;

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

                var dummyRequest = new Catalog.Request.BaseRequest()
                {
                    domainId = 0,
                    m_nGroupID = channel.m_nParentGroupID,
                    m_nPageIndex = 0,
                    m_nPageSize = 0,
                    m_oFilter = new Filter(),
                    m_sSiteGuid = string.Empty,
                    m_sUserIP = string.Empty
                };

                Catalog.Catalog.UpdateNodeTreeFields(dummyRequest,
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

        /// <summary>
        /// Update filter tree fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        public static void UpdateNodeTreeFields(ref BooleanPhraseNode filterTree, UnifiedSearchDefinitions definitions, Group group)
        {
            if (group != null && filterTree != null)
            {
                Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping = new Dictionary<BooleanPhraseNode, BooleanPhrase>();

                Queue<BooleanPhraseNode> nodes = new Queue<BooleanPhraseNode>();
                nodes.Enqueue(filterTree);

                // BFS
                while (nodes.Count > 0)
                {
                    BooleanPhraseNode node = nodes.Dequeue();

                    // If it is a leaf, just replace the field name
                    if (node.type == BooleanNodeType.Leaf)
                    {
                        TreatLeaf(ref filterTree, definitions, group, node, parentMapping);
                    }
                    else if (node.type == BooleanNodeType.Parent)
                    {
                        BooleanPhrase phrase = node as BooleanPhrase;

                        // Run on tree - enqueue all child nodes to continue going deeper
                        foreach (var childNode in phrase.nodes)
                        {
                            nodes.Enqueue(childNode);
                            parentMapping.Add(childNode, phrase);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update filter tree node fields for specific fields/values.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filterTree"></param>
        /// <param name="definitions"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        private static void TreatLeaf(ref BooleanPhraseNode filterTree, UnifiedSearchDefinitions definitions,
            Group group, BooleanPhraseNode node, Dictionary<BooleanPhraseNode, BooleanPhrase> parentMapping)
        {
            // initialize maximum nGram member only once - when this is negative it is still not set
            if (maxNGram < 0)
            {
                maxNGram = TVinciShared.WS_Utils.GetTcmIntValue("max_ngram");
            }
            
            BooleanLeaf leaf = node as BooleanLeaf;
            bool isTagOrMeta;

            // Add prefix (meta/tag) e.g. metas.{key}

            HashSet<string> searchKeys = GetUnifiedSearchKey(leaf.field, group, out isTagOrMeta);

            if (searchKeys.Count > 1)
            {
                if (isTagOrMeta)
                {
                    List<BooleanPhraseNode> newList = new List<BooleanPhraseNode>();

                    // Split the single leaf into several brothers connected with an "or" operand
                    foreach (var searchKey in searchKeys)
                    {
                        newList.Add(new BooleanLeaf(searchKey, leaf.value, leaf.valueType, leaf.operand));
                    }

                    BooleanPhrase newPhrase = new BooleanPhrase(newList, eCutType.Or);

                    BooleanPhraseNode.ReplaceLeafWithPhrase(ref filterTree, parentMapping, leaf, newPhrase);
                }
            }
            else if (searchKeys.Count == 1)
            {
                string searchKeyLowered = searchKeys.FirstOrDefault().ToLower();
                string originalKey = leaf.field;

                // Default - string, until proved otherwise
                leaf.valueType = typeof(string);

                // If this is a tag or a meta, we can continue happily.
                // If not, we check if it is one of the "core" fields.
                // If it is not one of them, an exception will be thrown
                if (!isTagOrMeta)
                {
                    // If the filter uses non-default start/end dates, we tell the definitions no to use default start/end date
                    if (searchKeyLowered == "start_date")
                    {
                        definitions.defaultStartDate = false;
                        leaf.valueType = typeof(DateTime);

                        long epoch = Convert.ToInt64(leaf.value);

                        leaf.value = TVinciShared.DateUtils.UnixTimeStampToDateTime(epoch);
                    }
                    else if (searchKeyLowered == "end_date")
                    {
                        definitions.defaultEndDate = false;
                        leaf.valueType = typeof(DateTime);

                        long epoch = Convert.ToInt64(leaf.value);

                        leaf.value = TVinciShared.DateUtils.UnixTimeStampToDateTime(epoch);
                    }
                    else if (searchKeyLowered == "update_date")
                    {
                        leaf.valueType = typeof(DateTime);

                        long epoch = Convert.ToInt64(leaf.value);

                        leaf.value = TVinciShared.DateUtils.UnixTimeStampToDateTime(epoch);
                    }
                    else if (searchKeyLowered == "geo_block")
                    {
                        // geo_block is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            // I mock operator+value so that it will not have any meaning, it will be ignored
                            leaf.field = "_id";
                            leaf.operand = ComparisonOperator.NotEquals;
                            leaf.value = "-1";                          
                        }
                        else
                        {
                            throw new KalturaException("Invalid search value or operator was sent for geo_block", (int)eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == "parental_rules")
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand == ComparisonOperator.Equals && leaf.value.ToString().ToLower() == "true")
                        {
                            // I mock operator+value so that it will not have any meaning, it will be ignored
                            leaf.field = "_id";
                            leaf.operand = ComparisonOperator.NotEquals;
                            leaf.value = "-1";                         
                        }
                        else
                        {
                            throw new KalturaException("Invalid search value or operator was sent for parental_rules", (int)eResponseStatus.BadSearchRequest);
                        }
                    }
                    else if (searchKeyLowered == "entitled_assets")
                    {
                        // Same as geo_block: it is a personal filter that currently will work only with "true".
                        if (leaf.operand != ComparisonOperator.Equals || leaf.value.ToString().ToLower() != "true")
                        {
                            throw new KalturaException("Invalid search value or operator was sent for entitled_assets", (int)eResponseStatus.BadSearchRequest);
                        }

                        // I mock operator+value so that it will not have any meaning, it will be ignored
                        leaf.field = "_id";
                        leaf.operand = ComparisonOperator.NotEquals;
                        leaf.value = "-1";
                    }
                    else if (reservedUnifiedSearchNumericFields.Contains(searchKeyLowered))
                    {
                        leaf.valueType = typeof(long);
                    }
                    else if (!reservedUnifiedSearchStringFields.Contains(searchKeyLowered))
                    {
                        throw new KalturaException(string.Format("Invalid search key was sent: {0}", originalKey), (int)eResponseStatus.InvalidSearchField);
                    }
                }

                leaf.field = searchKeys.FirstOrDefault();

                #region IN operator

                // Handle IN operator - validate the value, convert it into a proper list that the ES-QueryBuilder can use
                if (leaf.operand == ComparisonOperator.In || leaf.operand == ComparisonOperator.NotIn &&
                    leaf.valueType != typeof(List<string>))
                {
                    leaf.valueType = typeof(List<string>);
                    string value = leaf.value.ToString().ToLower();

                    string[] values = value.Split(',');

                    // If there are 
                    if (values.Length == 0)
                    {
                        throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey), (int)eResponseStatus.SyntaxError);
                    }

                    foreach (var single in values)
                    {
                        int temporaryInteger;

                        if (!int.TryParse(single, out temporaryInteger))
                        {
                            throw new KalturaException(string.Format("Invalid IN clause of: {0}", originalKey),
                                (int)eResponseStatus.SyntaxError);
                        }
                    }

                    // Put new list of strings in boolean leaf
                    leaf.value = values.ToList();
                }

                #endregion

            }

            #region Trim search value

            // If the search is contains or not contains, trim the search value to the size of the maximum NGram.
            // Otherwise the search will not work completely 
            if (maxNGram > 0 &&
                (leaf.operand == ComparisonOperator.Contains || leaf.operand == ComparisonOperator.NotContains
                || leaf.operand == ComparisonOperator.WordStartsWith))
            {
                leaf.value = TVinciShared.StringUtils.Truncate(leaf.value.ToString(), maxNGram);
            }

            #endregion
        }

        /// <summary>
        /// Verifies that the search key is a tag or a meta of either EPG or media
        /// </summary>
        /// <param name="originalKey"></param>
        /// <param name="group"></param>
        /// <param name="isTagOrMeta"></param>
        /// <returns></returns>
        private static HashSet<string> GetUnifiedSearchKey(string originalKey, Group group, out bool isTagOrMeta)
        {
            isTagOrMeta = false;

            HashSet<string> searchKeys = new HashSet<string>();

            if (originalKey.StartsWith("tags."))
                originalKey = originalKey.Substring(5);

            if (originalKey.StartsWith("metas."))
                originalKey = originalKey.Substring(6);

            foreach (string tag in group.m_oGroupTags.Values)
            {
                if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    isTagOrMeta = true;

                    searchKeys.Add(string.Format("tags.{0}", tag.ToLower()));
                    break;
                }
            }

            var metas = group.m_oMetasValuesByGroupId.Select(i => i.Value).Cast<Dictionary<string, string>>().SelectMany(d => d.Values).ToList();

            foreach (var meta in metas)
            {
                if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    isTagOrMeta = true;
                    searchKeys.Add(string.Format("metas.{0}", meta.ToLower()));
                    break;
                }
            }

            foreach (var tag in group.m_oEpgGroupSettings.m_lTagsName)
            {
                if (tag.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    isTagOrMeta = true;
                    searchKeys.Add(string.Format("tags.{0}", tag.ToLower()));
                    break;
                }
            }

            foreach (var meta in group.m_oEpgGroupSettings.m_lMetasName)
            {
                if (meta.Equals(originalKey, StringComparison.OrdinalIgnoreCase))
                {
                    isTagOrMeta = true;
                    searchKeys.Add(string.Format("metas.{0}", meta.ToLower()));
                    break;
                }
            }

            if (!isTagOrMeta)
            {
                searchKeys.Add(originalKey.ToLower());
            }

            return searchKeys;
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

                ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias_ml");
                GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

                GroupMedias.AddParameter("@GroupID", nGroupID);
                GroupMedias.AddParameter("@MediaID", nMediaID);

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => GroupMedias.ExecuteDataSet());
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

                                //string epgIdentifier = ODBCWrapper.Utils.ExtractString(row, "epg_identifier");

                                //if (!string.IsNullOrEmpty(epgIdentifier))
                                //{
                                //    media.epgIdentifier = epgIdentifier;
                                //}

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

                                        if (!string.IsNullOrEmpty(sMetaName))
                                        {
                                            string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);
                                            media.m_dMeatsValues.Add(sMetaName, sMetaValue);
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
