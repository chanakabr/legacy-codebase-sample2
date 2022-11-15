using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using GroupsCacheManager.Mappers;
using Tvinci.Core.DAL;

namespace GroupsCacheManager
{
    public class ChannelRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region CONSTS
        private static string META_END_SUFFIX = "_NAME";
        private static string META_USE_PREFIX = "USE_";
        private static string META_DOUBLE_SUFFIX = "_DOUBLE";
        private static string META_BOOL_SUFFIX = "_BOOL";
        private static string DEFAULT_GROUP_TAG_FREE = "free";
        private static readonly string TAGS = "tags";
        private static readonly string METAS = "metas";
        #endregion

        #region Public Methods

        public static Group BuildGroup(int groupId)
        {
            //Group newGroup = new Group(nGroupID);
            var subGroups = Get_SubGroupsTree(groupId);
            var epgGroupSettings = GetGroupEpgTagsAndMetas(groupId, subGroups);
            var newGroup = new Group(groupId, subGroups, epgGroupSettings);
            
            SetGroupMetas(ref newGroup);

            if (newGroup != null)
            {
                GetGroupsTagsTypes(ref newGroup);
                GetAllGroupChannelIds(newGroup);
                GetGroupLanguages(ref newGroup);
                // get all services related to group
                GetGroupServices(ref newGroup);

                SetGroupDefaults(newGroup);

                var mediaTypeIds = newGroup.GetMediaTypes();

                newGroup.mediaTypes = BuildMediaTypes(mediaTypeIds, groupId);

                SetMediaFileTypes(newGroup);

                //get all PermittedWatchRules by groupID
                SetPermittedWatchRules(ref newGroup);

                SetCatalogConfiguration(ref newGroup);
            }

            return newGroup;
        }        

        /// <summary>
        /// Regions, Recommendation Engine...
        /// </summary>
        /// <param name="newGroup"></param>
        private static void SetGroupDefaults(Group group)
        {
            CatalogDAL.GetGroupDefaultParameters(group.m_nParentGroupID,
                out var isRegionalizationEnabled,
                out var defaultRegion,
                out var defaultRecommendationEngine,
                out var relatedRecommendationEngine,
                out var searchRecommendationEngine,
                out var relatedRecommendationEngineEnrichments,
                out var searchRecommendationEngineEnrichments,
                out var isGeoAvailabilityEnabled,
                out var isAssetUserRuleEnabled);

            group.isRegionalizationEnabled = isRegionalizationEnabled;
            group.defaultRegion = defaultRegion;
            group.defaultRecommendationEngine = defaultRecommendationEngine;
            group.RelatedRecommendationEngine = relatedRecommendationEngine;
            group.SearchRecommendationEngine = searchRecommendationEngine;
            group.RelatedRecommendationEngineEnrichments = relatedRecommendationEngineEnrichments;
            group.SearchRecommendationEngineEnrichments = searchRecommendationEngineEnrichments;
            group.isGeoAvailabilityWindowingEnabled= isGeoAvailabilityEnabled;
            group.isAssetUserRuleEnabled = isAssetUserRuleEnabled;
        }

        /// <summary>
        /// Tells if regionalization is enabled for this group. 
        /// </summary>
        /// <param name="group"></param>
        private static void SetRegionalizationSettings(Group group)
        {
            bool isRegionalizationEnabled;
            int defaultRegion;

            CatalogDAL.GetRegionalizationSettings(group.m_nParentGroupID,
                out isRegionalizationEnabled, out defaultRegion);

            group.isRegionalizationEnabled = isRegionalizationEnabled;
            group.defaultRegion = defaultRegion;
        }

        private static void GetGroupServices(ref Group group)
        {
            List<int> services = Tvinci.Core.DAL.CatalogDAL.GetGroupServices(group.m_nParentGroupID);
            if (services != null)
            {
                group.AddServices(services);
            }
        }


        private static List<int> Get_SubGroupsTree(int nGroupID)
        {
            List<int> lGroups = new List<int>();

            DataTable dt = DAL.UtilsDal.GetGroupsTree(nGroupID);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                int groupId;
                for (int i = 0; i < dt.DefaultView.Count; i++)
                {
                    groupId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "id");
                    if (groupId != 0)
                    {
                        lGroups.Add(groupId);
                    }
                }
            }

            return lGroups;
        }

        private static void GetGroupLanguages(ref Group group)
        {
            List<LanguageObj> languages = Tvinci.Core.DAL.CatalogDAL.GetGroupLanguages(group.m_nParentGroupID);
            if (languages != null)
            {
                group.AddLanguage(languages);
            }
        }


        private static EpgGroupSettings GetGroupEpgTagsAndMetas(int parentGroupId, List<int> subGroups)
        {
            var epgGroupSettings = new EpgGroupSettings();

            try
            {
                var ds = EpgDal.Get_Group_EPGTagsAndMetas(parentGroupId, subGroups, 0/*return not only searchable*/);
                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    #region metas
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            if (epgGroupSettings.GroupId == 0)
                            {
                                epgGroupSettings.GroupId = ODBCWrapper.Utils.ExtractValue<int>(row, "group_id");
                            }

                            long id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");
                            string name = ODBCWrapper.Utils.GetSafeStr(row["name"]);

                            if (!string.IsNullOrEmpty(name))
                            {
                                epgGroupSettings.m_lMetasName.Add(name.ToLower());
                                epgGroupSettings.metas[id] = name.ToLower();
                                epgGroupSettings.MetasDisplayName.Add(name);
                            }
                        }
                    }
                    #endregion
                    #region tags
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            if (epgGroupSettings.GroupId == 0)
                            {
                                epgGroupSettings.GroupId = ODBCWrapper.Utils.ExtractValue<int>(row, "group_id");
                            }

                            long id = ODBCWrapper.Utils.ExtractValue<long>(row, "ID");
                            string name = ODBCWrapper.Utils.GetSafeStr(row["name"]);                            

                            if (!string.IsNullOrEmpty(name))
                            {
                                epgGroupSettings.m_lTagsName.Add(name.ToLower());
                                epgGroupSettings.tags[id] = name.ToLower();
                                epgGroupSettings.TagsDisplayName.Add(name);
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error - Caught exception when fetching EPG group tags and metas. Ex={ex.Message}", ex);
                epgGroupSettings = null;
            }

            return epgGroupSettings;
        }

        /// <summary>
        /// Gets only the Ids of the channels of the given group
        /// </summary>
        /// <param name="group"></param>
        private static void GetAllGroupChannelIds(Group group)
        {
            var groupChannels = CatalogDAL.Get_GroupChannels(group.m_nParentGroupID, group.m_nSubGroup);
            if (groupChannels != null && groupChannels.DefaultView.Count > 0)
            {
                foreach (DataRow row in groupChannels.Rows)
                {
                    var channelID = ODBCWrapper.Utils.GetIntSafeVal(row, "id");
                    if (channelID != 0)
                    {
                        group.channelIDs.Add(channelID);
                    }
                }
            }
        }

        private static void SetPermittedWatchRules(ref Group newGroup)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(newGroup.m_nParentGroupID, newGroup.m_nSubGroup);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    newGroup.m_sPermittedWatchRules.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow, "RuleID"));
                }
            }
        }

        /// <summary>
        /// Initializes the mapping of the table groups_media_type
        /// </summary>
        /// <param name="group"></param>
        private static void SetMediaFileTypes(Group group)
        {
            if (group.groupMediaFileTypeToFileType == null)
            {
                group.groupMediaFileTypeToFileType = new Dictionary<int, int>();
            }

            DataTable table = Tvinci.Core.DAL.CatalogDAL.GetGroupsMediaType(group.GetSubTreeGroupIds());

            foreach (DataRow groupMediaFile in table.Rows)
            {
                group.groupMediaFileTypeToFileType.Add(
                    ODBCWrapper.Utils.ExtractInteger(groupMediaFile, "ID"),
                    ODBCWrapper.Utils.ExtractInteger(groupMediaFile, "MEDIA_TYPE_ID"));
            }
        }

        /// <summary>
        /// Selects the channel by channel ID and its parent group ID
        /// </summary>
        /// <param name="nChannelId"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static Channel GetChannel(int nChannelId, Group group, bool isAlsoInActive = false)
        {
            log.DebugFormat("GetChannel Started for nChannelId={0}", nChannelId);

            Channel channel = null;
            DataSet dataSet = Tvinci.Core.DAL.CatalogDAL.GetChannelDetails(new List<int>() { nChannelId }, isAlsoInActive);

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0)
            {
                DataTable channelData = dataSet.Tables[0];
                DataTable mediaTypesTable = null;

                // If there is a table of media types
                if (dataSet.Tables.Count > 1 && dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
                {
                    mediaTypesTable = dataSet.Tables[1];
                }

                if (channelData != null && channelData.Rows != null && channelData.Rows.Count > 0)
                {
                    DataRow rowData = channelData.Rows[0];

                    channel = CreateChannelByDataRow(group, mediaTypesTable, rowData, isAlsoInActive);
                }
            }

            return channel;
        }

        public static List<Channel> GetChannels(List<int> channelIds, Group group, bool isAlsoInActive = false)
        {
            #region - select channel by channelId, and the parent_group_id

            List<Channel> channels = null;

            log.Debug("Getting channels for subscription");

            DataSet dataSet = Tvinci.Core.DAL.CatalogDAL.GetChannelDetails(channelIds, isAlsoInActive);
            DataTable channelsData = dataSet.Tables[0];
            DataTable mediaTypesTable = null;

            // If there is a table of media types
            if (dataSet.Tables.Count > 1 && dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
            {
                mediaTypesTable = dataSet.Tables[1];
            }

            if (channelsData != null && channelsData.Rows != null && channelsData.Rows.Count > 0)
            {
                channels = new List<Channel>();

                foreach (DataRow rowData in channelsData.Rows)
                {
                    Channel channel = CreateChannelByDataRow(group, mediaTypesTable, rowData, isAlsoInActive);

                    if (channel != null)
                    {
                        channels.Add(channel);
                    }
                }
            }

            return channels;

            #endregion
        }

        private static Channel CreateChannelByDataRow(Group group, DataTable mediaTypesTable, DataRow rowData, bool isForMigration = false)
        {
            log.Debug("new channel");

            Channel channel = new Channel();
            if (channel.m_lChannelTags == null)
            {
                channel.m_lChannelTags = new List<SearchValue>();
            }

            channel.m_lManualMedias = new List<ManualMedia>();
            channel.m_nChannelID = ODBCWrapper.Utils.GetIntSafeVal(rowData["Id"]);

            try
            {
                int channelGroupId = ODBCWrapper.Utils.GetIntSafeVal(rowData["group_id"]);
                int isActive = ODBCWrapper.Utils.GetIntSafeVal(rowData["is_active"]);
                int status = ODBCWrapper.Utils.GetIntSafeVal(rowData["status"]);

                // If the channel belongs to the correct group and the channel is in correct status
                if ((group.m_nSubGroup.Contains(channelGroupId) || group.m_nParentGroupID == channelGroupId) &&
                    (isActive == 1 || isForMigration) && (status == 1))
                {
                    channel.m_nIsActive = isActive;
                    channel.m_nStatus = status;
                    channel.m_nGroupID = channelGroupId;
                    channel.m_nParentGroupID = group.m_nParentGroupID;
                    channel.m_nChannelTypeID = ODBCWrapper.Utils.GetIntSafeVal(rowData["channel_type"]);
                    channel.m_sName = ODBCWrapper.Utils.ExtractString(rowData, "name");
                    channel.m_sDescription = ODBCWrapper.Utils.ExtractString(rowData, "description");

                    ChannelType channelType = ChannelType.None;

                    if (Enum.IsDefined(typeof(ChannelType), channel.m_nChannelTypeID))
                    {
                        channelType = (ChannelType)channel.m_nChannelTypeID;
                    }

                    #region Media Types

                    int mediaType = ODBCWrapper.Utils.GetIntSafeVal(rowData["MEDIA_TYPE_ID"]);

                    channel.m_nMediaType = new List<int>();

                    if (mediaTypesTable != null)
                    {
                        List<DataRow> mediaTypes = mediaTypesTable.Select("CHANNEL_ID = " + channel.m_nChannelID).ToList();

                        foreach (DataRow drMediaType in mediaTypes)
                        {
                            channel.m_nMediaType.Add(ODBCWrapper.Utils.GetIntSafeVal(drMediaType, "MEDIA_TYPE_ID"));
                        }
                    }

                    // The 0 trick is relevant only to older channels
                    if (channelType != ChannelType.KSQL && channel.m_nMediaType.Count == 0)
                    {
                        if (mediaType != -1)
                        {
                            channel.m_nMediaType.Add(mediaType);
                        }
                        else
                        {
                            channel.m_nMediaType.Add(0);
                        }
                    }

                    #endregion

                    #region Ordering Parameters

                    var orderByInt = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_type"]);
                    channel.OrderingParameters = ChannelDataRowMapper.BuildOrderingParameters(rowData);

                    if (group.m_oMetasValuesByGroupId.ContainsKey(channel.m_nGroupID))
                    {
                        UpdateOrderingParameters(channel.OrderingParameters, orderByInt, group.m_oMetasValuesByGroupId[channel.m_nGroupID]);
                    }

                    #endregion

                    #region Is And

                    int isAnd = ODBCWrapper.Utils.GetIntSafeVal(rowData["IS_AND"]);

                    log.Debug("Channel " + channel.m_nChannelID + " active: " + channel.m_nIsActive + " and status: " + channel.m_nStatus);

                    if (isAnd == 1)
                    {
                        channel.m_eCutWith = CutWith.AND;
                    }

                    #endregion

                    switch (channelType)
                    {
                        case ChannelType.None:
                            break;
                        case ChannelType.Automatic:
                            {
                                // If automatic channel, grab tags values
                                #region Automatic
                                // Matching meta values against meta mapping dictionary
                                if (group.m_oMetasValuesByGroupId.ContainsKey(channel.m_nGroupID))
                                {
                                    log.Info("Got mapped value for group " + channel.m_nGroupID + " in channel " + channel.m_nChannelID);
                                    Dictionary<string, string> mappedValuesForGroupId = group.m_oMetasValuesByGroupId[channel.m_nGroupID];

                                    if (mappedValuesForGroupId == null || mappedValuesForGroupId.Count == 0)
                                    {
                                        log.Info("llll" + channel.m_nGroupID + " in channel " + channel.m_nChannelID);
                                    }

                                    foreach (KeyValuePair<string, string> mapping in mappedValuesForGroupId)
                                    {
                                        string sMetaParameter = mapping.Key;
                                        string sMappedMetaParameter = mapping.Value;

                                        bool bIsValidSearchValue = true;

                                        if (sMetaParameter.Contains(META_DOUBLE_SUFFIX) || sMetaParameter.Contains(META_BOOL_SUFFIX))
                                        {
                                            int nUse = ODBCWrapper.Utils.GetIntSafeVal(rowData[META_USE_PREFIX + sMetaParameter]);

                                            if (nUse == 0)
                                            {
                                                bIsValidSearchValue = false;
                                            }
                                        }

                                        if (bIsValidSearchValue)
                                        {
                                            string oMeta = ODBCWrapper.Utils.GetSafeStr(rowData, sMetaParameter);
                                            if (!string.IsNullOrEmpty(oMeta))
                                            {
                                                bool bIsAlreadyExist = false;
                                                SearchValue searchedSearchValue = channel.m_lChannelTags.Find(o => o.m_sKey.Equals(sMappedMetaParameter));
                                                if (searchedSearchValue == null)
                                                {
                                                    SearchValue oNewSearchValue = new SearchValue();
                                                    CreateSearchValueObject(ref oNewSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                                    channel.m_lChannelTags.Add(oNewSearchValue);
                                                }
                                                else
                                                {
                                                    bIsAlreadyExist = true;
                                                    CreateSearchValueObject(ref searchedSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                                }
                                            }
                                        }
                                    }
                                }

                                // Collect all tags
                                log.Info("Collecting tags in channel " + channel.m_nChannelID);

                                GetChannelTags(channel, group);
                                log.Info("Finished Collecting tags in channel " + channel.m_nChannelID);

                                break;

                                #endregion
                            }
                        case ChannelType.Manual:
                            {
                                #region Manual

                                List<ManualMedia> lManualMedias;
                                channel.m_lChannelTags = GetMediasForManualChannel(channel.m_nChannelID, out lManualMedias);

                                if (lManualMedias != null)
                                {
                                    channel.m_lManualMedias = lManualMedias.ToList();
                                }

                                break;

                                #endregion
                            }
                        case ChannelType.Watcher:
                            {
                                // ? Saw this in DB, don't know what it means...
                                break;
                            }
                        case ChannelType.KSQL:
                            {
                                channel.filterQuery = ODBCWrapper.Utils.ExtractString(rowData, "KSQL_FILTER");
                                string groupBy = ODBCWrapper.Utils.ExtractString(rowData, "GROUP_BY");
                                if (!string.IsNullOrEmpty(groupBy))
                                {
                                    channel.searchGroupBy = new SearchAggregationGroupBy()
                                    {
                                        groupBy = new List<string>() { groupBy }
                                    };
                                }
                                BooleanPhraseNode node = null;
                                var parseStatus = BooleanPhraseNode.ParseSearchExpression(channel.filterQuery, ref node);

                                if (parseStatus.Code != 0)
                                {
                                    log.WarnFormat("KSQL channel {0} has invalid KSQL expression: {1}", channel.m_nChannelID, channel.filterQuery);
                                }

                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }

                    if (group.m_oMetasValuesByGroupId.ContainsKey(channel.m_nGroupID))
                    {
                        UpdateOrderingParameters(channel.OrderingParameters, group.m_oMetasValuesByGroupId[channel.m_nGroupID]);
                    }
                }
                else
                {
                    channel = null;
                }

                channel.SupportSegmentBasedOrdering = ODBCWrapper.Utils.ExtractBoolean(rowData, "SUPPORT_SEGMENT_BASED_ORDERING");
                channel.AssetUserRuleId = ODBCWrapper.Utils.GetLongSafeVal(rowData, "ASSET_RULE_ID");
                
                channel.m_OrderObject = ChannelDataRowMapper.BuildOrderObj(channel.OrderingParameters.First());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed CreateChannelByDataRow for channel id: {0}", channel != null ? channel.m_nChannelID.ToString() : "channel is null"), ex);
                channel = null;
            }

            return channel;
        }

        #endregion

        #region Private Methods

        private static void UpdateOrderingParameters(IEnumerable<AssetOrder> orderingParameters, int orderBy, IReadOnlyDictionary<string, string> metasValues)
        {
            if (metasValues == null)
            {
                return;
            }

            if (orderingParameters.First() is AssetOrderByMeta assetOrderByMeta && string.IsNullOrEmpty(assetOrderByMeta.MetaName))
            {
                var metasEnumName = Enum.GetName(typeof(MetasEnum), orderBy);
                if (metasEnumName != null && metasValues.TryGetValue(metasEnumName, out var metaName))
                {
                    assetOrderByMeta.MetaName = metaName;
                }
            }
        }

        private static void UpdateOrderingParameters(IEnumerable<AssetOrder> orderingParameters, IReadOnlyDictionary<string, string> metasValues)
        {
            if (metasValues == null)
            {
                return;
            }

            foreach (var assetOrderByMeta in orderingParameters.OfType<AssetOrderByMeta>())
            {
                metasValues.TryGetValue(OrderBy.META.ToString(), out var metaName);
                if (!string.IsNullOrEmpty(metaName))
                {
                    assetOrderByMeta.MetaName = metaName;
                }
            }
        }

        private static void CreateSearchValueObject(ref SearchValue oNewSearchValue, string key, string value, bool isAlreadyExist, string sKeyPrefix)
        {
            if (!isAlreadyExist)
            {
                oNewSearchValue.m_sKey = key;
                List<string> tagValues = new List<string>();
                oNewSearchValue.m_lValue = tagValues;
            }
            oNewSearchValue.m_sKeyPrefix = sKeyPrefix;
            List<string> lCurrentTagsValues = oNewSearchValue.m_lValue.ToList();
            lCurrentTagsValues.Add(value);
            oNewSearchValue.m_lValue = lCurrentTagsValues;
        }

        private static List<SearchValue> GetMediasForManualChannel(int nChannelId, out List<ManualMedia> lManualMedias)
        {
            List<SearchValue> lMediaIds = null;
            DataTable mediaIdsTable = Tvinci.Core.DAL.CatalogDAL.GetMediaIdsByChannelId(nChannelId);
            lManualMedias = null;
            if (mediaIdsTable != null && mediaIdsTable.Rows.Count > 0)
            {
                lManualMedias = new List<ManualMedia>();
                lMediaIds = new List<SearchValue>();
                foreach (DataRow mediaIdRow in mediaIdsTable.Rows)
                {
                    string sMediaID = ODBCWrapper.Utils.GetSafeStr(mediaIdRow, "media_id");
                    int nOrderNum = ODBCWrapper.Utils.GetIntSafeVal(mediaIdRow, "order_num");
                    bool bIsAlreadyExist = false;

                    SearchValue searchedSearchValue = lMediaIds.Find(o => o.m_sKey.Equals("media_id"));
                    if (searchedSearchValue == null)
                    {
                        SearchValue oNewSearchValue = new SearchValue();
                        CreateSearchValueObject(ref oNewSearchValue, "media_id", sMediaID, bIsAlreadyExist, string.Empty);
                        lMediaIds.Add(oNewSearchValue);

                    }
                    else
                    {
                        bIsAlreadyExist = true;
                        CreateSearchValueObject(ref searchedSearchValue, "media_id", sMediaID, bIsAlreadyExist, string.Empty);
                    }

                    lManualMedias.Add(new ManualMedia(sMediaID, nOrderNum));
                }
            }

            return lMediaIds;
        }

        private static void GetChannelTags(Channel oChannel, Group group)
        {
            try
            {
                DataTable tagsValuesByTagTypeIds = Tvinci.Core.DAL.CatalogDAL.GetTagsValuesByTagTypeIds(oChannel.m_nChannelID);

                if (tagsValuesByTagTypeIds != null && tagsValuesByTagTypeIds.Rows.Count > 0)
                {
                    foreach (DataRow tableRow in tagsValuesByTagTypeIds.Rows)
                    {
                        int nTagTypeId = ODBCWrapper.Utils.GetIntSafeVal(tableRow, "tag_type_id");
                        string sTagValue = ODBCWrapper.Utils.GetSafeStr(tableRow, "value");
                        string sTagName = GetTagName(nTagTypeId, group);

                        if (!string.IsNullOrEmpty(sTagValue) && !string.IsNullOrEmpty(sTagName))
                        {
                            SearchValue oSearchValue = new SearchValue();
                            bool bIsAlreadyExist = false;
                            SearchValue searchedSearchValue = oChannel.m_lChannelTags.Find(o => o.m_sKey.Equals(sTagName));
                            if (searchedSearchValue == null)
                            {
                                SearchValue oNewSearchValue = new SearchValue();
                                CreateSearchValueObject(ref oNewSearchValue, sTagName, sTagValue, bIsAlreadyExist, TAGS);
                                oChannel.m_lChannelTags.Add(oNewSearchValue);
                            }
                            else
                            {
                                bIsAlreadyExist = true;
                                CreateSearchValueObject(ref searchedSearchValue, sTagName, sTagValue, bIsAlreadyExist, TAGS);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                //log.Error(ex.Message, ex);
            }
        }

        private static void SetGroupMetas(ref Group group)
        {
            var dtMappedMetaValues = CatalogDAL.GetMappedMetasByGroupId(group.m_nParentGroupID, group.m_nSubGroup);
            
            if (dtMappedMetaValues != null && dtMappedMetaValues.Rows.Count > 0)
            {
                foreach (DataRow metaDataRow in dtMappedMetaValues.Rows)
                {
                    var subGroupNumberId = ODBCWrapper.Utils.GetIntSafeVal(metaDataRow, "Id");
                    if (subGroupNumberId > 0)
                    {
                        if (!group.m_oMetasValuesByGroupId.ContainsKey(subGroupNumberId))
                        {
                            group.m_oMetasValuesByGroupId.Add(subGroupNumberId, new Dictionary<string, string>());
                        }

                        var metaColumn = RemoveMetasSuffixName(ODBCWrapper.Utils.GetSafeStr(metaDataRow, "columnname"));
                        var metaValue = ODBCWrapper.Utils.GetSafeStr(metaDataRow, "value");

                        if (!group.m_oMetasValuesByGroupId[subGroupNumberId].ContainsKey(metaColumn))
                        {
                            group.m_oMetasValuesByGroupId[subGroupNumberId].Add(metaColumn, metaValue);
                        }

                        var loweredMetaValue = metaValue.ToLower();
                        if (!group.MetaValueToTypesMapping.ContainsKey(loweredMetaValue))
                        {
                            group.MetaValueToTypesMapping.Add(loweredMetaValue, IsStringMetaType(metaColumn));
                        }
                        else if (IsStringMetaType(metaColumn))
                        {
                            group.MetaValueToTypesMapping[loweredMetaValue] = true;
                        }
                    }
                }

                // date metas
                var dtDateMetaValues = CatalogDAL.GetDateMetasByGroupId(group.m_nSubGroup);
                if (dtDateMetaValues != null && dtDateMetaValues.Rows != null && dtDateMetaValues.Rows.Count > 0)
                {
                    foreach (DataRow metaDataRow in dtDateMetaValues.Rows)
                    {
                        var groupId = ODBCWrapper.Utils.GetIntSafeVal(metaDataRow, "group_id");
                        if (!group.m_oMetasValuesByGroupId.ContainsKey(groupId))
                        {
                            group.m_oMetasValuesByGroupId.Add(groupId, new Dictionary<string, string>());
                        }

                        var id = ODBCWrapper.Utils.GetSafeStr(metaDataRow, "id");
                        var name = ODBCWrapper.Utils.GetSafeStr(metaDataRow, "name");
                        string metaDateKey = $"date_{id}";

                        if (!group.m_oMetasValuesByGroupId[groupId].ContainsKey(metaDateKey))
                        {
                            group.m_oMetasValuesByGroupId[groupId].Add(metaDateKey, name);
                        }

                        var loweredName = name.ToLower();
                        if (!group.MetaValueToTypesMapping.ContainsKey(loweredName))
                        {
                            group.MetaValueToTypesMapping.Add(loweredName, false);
                        }
                    }
                }
            }
            else if(group.m_nSubGroup.Count > 1)
            {
                log.Error($"Didn't get any metas from GetMappedMetasByGroupId for group {group.m_nParentGroupID}");
                group = null;
            }
        }

        private static bool IsStringMetaType(string metaColumn)
        {
            if (metaColumn.EndsWith(META_BOOL_SUFFIX) || metaColumn.EndsWith(META_DOUBLE_SUFFIX))
            {
                return false;
            }
            
            return true;
        }

        private static void GetGroupsTagsTypes(ref Group group)
        {
            group.m_oGroupTags.Add(0, DEFAULT_GROUP_TAG_FREE);
            group.TagToGroup = new Dictionary<int, int>();
            string sAllGroups = group.GetSubTreeGroupIds();

            if (!string.IsNullOrEmpty(sAllGroups))
            {
                DataTable mediaTagsType = CatalogDAL.GetMediaTagsTypesByGroupIds(sAllGroups);
                if (mediaTagsType != null && mediaTagsType.Rows.Count > 0)
                {
                    foreach (DataRow mediaTagTypeRow in mediaTagsType.Rows)
                    {
                        int nMediaTagTypeId = ODBCWrapper.Utils.GetIntSafeVal(mediaTagTypeRow, "id");
                        string sMediaTagTypeName = ODBCWrapper.Utils.GetSafeStr(mediaTagTypeRow, "name");
                        int groupId = ODBCWrapper.Utils.GetIntSafeVal(mediaTagTypeRow, "group_id");

                        if (!group.m_oGroupTags.ContainsKey(nMediaTagTypeId))
                        {
                            group.m_oGroupTags.Add(nMediaTagTypeId, sMediaTagTypeName);
                        }

                        group.TagToGroup[nMediaTagTypeId] = groupId;
                    }
                }
            }
        }

        /// <summary>
        /// This function is used in order to match between channels META columns and the values recieved from procedure Get_MetasByGroup
        /// </summary>
        /// <param name="metaName">The meta name which holds "name" before removing it</param>
        /// <returns>Meta name without the suffix "_NAME"</returns>
        private static string RemoveMetasSuffixName(string metaName)
        {
            if (!metaName.Equals(string.Empty))
            {
                if (metaName.EndsWith(META_END_SUFFIX))
                {
                    metaName = metaName.Substring(0, metaName.LastIndexOf(META_END_SUFFIX));
                }
            }

            return metaName;
        }

        /// <summary>
        /// This function returns all group ids which are related to their parent group id
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <returns></returns>
        private static string GetSubTreeGroupIds(int parentGroupId)
        {
            string sTreeOfGroupIds = string.Empty;
            DataTable groupIdsTree = Tvinci.Core.DAL.CatalogDAL.GetAllGroupTree(parentGroupId);
            List<string> oIds = new List<string>();

            if (groupIdsTree != null && groupIdsTree.Rows.Count > 0)
            {
                foreach (DataRow idRow in groupIdsTree.Rows)
                {
                    oIds.Add(ODBCWrapper.Utils.GetSafeStr(idRow, "id"));
                }
            }

            if (oIds.Count > 0)
            {
                sTreeOfGroupIds = string.Join(",", oIds);
            }

            return sTreeOfGroupIds;
        }

        private static string GetTagName(int nTagTypeID, Group group)
        {
            string sTagvalue = string.Empty;

            if (group.m_oGroupTags.ContainsKey(nTagTypeID))
            {
                sTagvalue = group.m_oGroupTags[nTagTypeID];
            }

            return sTagvalue;
        }

        #endregion

        /// <summary>
        /// For given Ids, returns a list of media types
        /// </summary>
        /// <param name="typeIds"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        internal static List<MediaType> BuildMediaTypes(List<int> typeIds, int groupId)
        {
            List<MediaType> newMediaTypes = new List<MediaType>();

            // Get table from stored procedure
            DataTable mediaTypesTable = CatalogDAL.GetMediaTypesTable(groupId);

            if (mediaTypesTable != null && mediaTypesTable.Rows != null && mediaTypesTable.Rows.Count > 0)
            {
                foreach (DataRow currentMediaType in mediaTypesTable.Rows)
                {
                    int id = ODBCWrapper.Utils.ExtractInteger(currentMediaType, "ID");

                    // If this Id was requested
                    if (typeIds.Contains(id))
                    {
                        // Get values from row
                        string associationTag = ODBCWrapper.Utils.ExtractString(currentMediaType, "ASSOCIATION_TAG");
                        string description = ODBCWrapper.Utils.ExtractString(currentMediaType, "DESCRIPTION");
                        string name = ODBCWrapper.Utils.ExtractString(currentMediaType, "NAME");
                        bool isLinear = ODBCWrapper.Utils.ExtractBoolean(currentMediaType, "IS_LINEAR");
                        int parentId = ODBCWrapper.Utils.ExtractInteger(currentMediaType, "PARENT_TYPE_ID");

                        // Initialize new media type from row
                        MediaType newType = new MediaType()
                        {
                            id = id,
                            associationTag = associationTag,
                            description = description,
                            isLinear = isLinear,
                            parentId = parentId,
                            name = name
                        };

                        newMediaTypes.Add(newType);
                    }
                }
            }

            return newMediaTypes;
        }

        private static void SetCatalogConfiguration(ref Group newGroup)
        {
            var cpc = DAL.ApiDAL.Instance.GetCatalogPartnerConfig(newGroup.m_nParentGroupID);
            if(cpc != null)
            {
                newGroup.isTagsSingleTranslation = cpc.SingleMultilingualMode.HasValue ? cpc.SingleMultilingualMode.Value : false;
            }
        }
    }
}
