using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.CatalogManagement
{
    public class ChannelManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Methods

        private static List<Channel> GetChannelListFromDs(DataSet ds)
        {
            List<Channel> channels = new List<Channel>();
            if (ds == null || ds.Tables == null || ds.Tables.Count < 5 || ds.Tables[0] == null || ds.Tables[0].Rows == null || ds.Tables[0].Rows.Count == 0)
            {
                log.WarnFormat("GetGroupChannels didn't find any channels");
                return null;
            }

            DataTable channelsTable = ds.Tables[0];
            EnumerableRowCollection<DataRow> nameTranslations = ds.Tables[1] != null && ds.Tables[1].Rows != null ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable();
            EnumerableRowCollection<DataRow> descriptionTranslations = ds.Tables[2] != null && ds.Tables[2].Rows != null ? ds.Tables[2].AsEnumerable() : new DataTable().AsEnumerable();
            EnumerableRowCollection<DataRow> mediaTypes = ds.Tables[3] != null && ds.Tables[3].Rows != null ? ds.Tables[3].AsEnumerable() : new DataTable().AsEnumerable();
            EnumerableRowCollection<DataRow> channelsMedias = ds.Tables[4] != null && ds.Tables[4].Rows != null ? ds.Tables[4].AsEnumerable() : new DataTable().AsEnumerable();
            foreach (DataRow dr in channelsTable.Rows)
            {
                int id = ODBCWrapper.Utils.GetIntSafeVal(dr["Id"]);
                if (id > 0)
                {
                    List<DataRow> channelNameTranslations = (from row in nameTranslations
                                                             where (Int64)row["CHANNEL_ID"] == id
                                                             select row).ToList();
                    List<DataRow> channelDescriptionTranslations = (from row in descriptionTranslations
                                                                    where (Int64)row["CHANNEL_ID"] == id
                                                                    select row).ToList();
                    List<DataRow> channelmediaTypes = (from row in mediaTypes
                                                       where (Int64)row["CHANNEL_ID"] == id
                                                       select row).ToList();
                    List<DataRow> medias = (from row in channelsMedias
                                            where (Int64)row["CHANNEL_ID"] == id
                                            select row).ToList();
                    Channel channel = CreateChannel(id, dr, channelNameTranslations, channelDescriptionTranslations, channelmediaTypes, medias);
                    if (channel != null)
                    {
                        channels.Add(channel);
                    }
                }
            }

            return channels;
        }

        private static Channel CreateChannel(int id, DataRow dr, List<DataRow> nameTranslations, List<DataRow> descriptionTranslations, List<DataRow> mediaTypes, List<DataRow> channelMedias)
        {
            Channel channel = new Channel();
            if (channel.m_lChannelTags == null)
            {
                channel.m_lChannelTags = new List<SearchValue>();
            }

            channel.m_lManualMedias = new List<GroupsCacheManager.ManualMedia>();
            channel.m_nChannelID = id;
            channel.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_ID");
            channel.m_nParentGroupID = channel.m_nGroupID;
            channel.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]);
            channel.m_nStatus = ODBCWrapper.Utils.GetIntSafeVal(dr["status"]);
            channel.m_nChannelTypeID = ODBCWrapper.Utils.GetIntSafeVal(dr["channel_type"]);
            channel.m_sName = ODBCWrapper.Utils.ExtractString(dr, "name");
            channel.m_sDescription = ODBCWrapper.Utils.ExtractString(dr, "description");
            channel.SystemName = ODBCWrapper.Utils.ExtractString(dr, "SYSTEM_NAME");
            channel.CreateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
            channel.UpdateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");

            #region translated names

            channel.NamesInOtherLanguages = new List<LanguageContainer>();
            if (nameTranslations != null && nameTranslations.Count > 0)
            {                
                foreach (DataRow nameDr in nameTranslations)
                {
                    string code3 = ODBCWrapper.Utils.GetSafeStr(nameDr, "CODE3");
                    string translation = ODBCWrapper.Utils.GetSafeStr(nameDr, "TRANSLATION");
                    channel.NamesInOtherLanguages.Add(new LanguageContainer(code3, translation, false));
                }
            }

            #endregion

            #region translated descriptions

            channel.DescriptionInOtherLanguages = new List<LanguageContainer>();
            if (descriptionTranslations != null && descriptionTranslations.Count > 0)
            {
                foreach (DataRow descDr in descriptionTranslations)
                {
                    string code3 = ODBCWrapper.Utils.GetSafeStr(descDr, "CODE3");
                    string translation = ODBCWrapper.Utils.GetSafeStr(descDr, "TRANSLATION");
                    channel.DescriptionInOtherLanguages.Add(new LanguageContainer(code3, translation, false));
                }
            }

            #endregion

            ChannelType channelType = ChannelType.None;
            if (Enum.IsDefined(typeof(ChannelType), channel.m_nChannelTypeID))
            {
                channelType = (ChannelType)channel.m_nChannelTypeID;
            }

            #region Media Types

            int mediaType = ODBCWrapper.Utils.GetIntSafeVal(dr["MEDIA_TYPE_ID"]);
            channel.m_nMediaType = new List<int>();

            if (mediaTypes != null)
            {
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

            #region Order

            channel.m_OrderObject = new ApiObjects.SearchObjects.OrderObj();
            int orderBy = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_type"]);
            // get order by value 
            string orderByValue = ODBCWrapper.Utils.GetSafeStr(dr, "ORDER_BY_VALUE");

            // initiate orderBy object 
            UpdateOrderByObject(orderBy, ref channel, orderByValue);

            int orderDirection = ODBCWrapper.Utils.GetIntSafeVal(dr["order_by_dir"]) - 1;
            channel.m_OrderObject.m_eOrderDir =
                (ApiObjects.SearchObjects.OrderDir)ApiObjects.SearchObjects.OrderDir.ToObject(typeof(ApiObjects.SearchObjects.OrderDir), orderDirection);
            channel.m_OrderObject.m_bIsSlidingWindowField = ODBCWrapper.Utils.GetIntSafeVal(dr["IsSlidingWindow"]) == 1;
            channel.m_OrderObject.lu_min_period_id = ODBCWrapper.Utils.GetIntSafeVal(dr["SlidingWindowPeriod"]);

            #endregion

            #region Is And

            int isAnd = ODBCWrapper.Utils.GetIntSafeVal(dr["IS_AND"]);

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
                case ChannelType.Manual:
                    {
                        #region Manual
                        if (channelMedias != null)
                        {
                            List<GroupsCacheManager.ManualMedia> manualMedias = new List<GroupsCacheManager.ManualMedia>();
                            HashSet<int> mediaIdsSet = new HashSet<int>();
                            foreach (DataRow mediaRow in channelMedias)
                            {
                                int mediaId = ODBCWrapper.Utils.GetIntSafeVal(mediaRow, "MEDIA_ID");
                                int orderNum = ODBCWrapper.Utils.GetIntSafeVal(mediaRow, "ORDER_NUM");
                                if (!mediaIdsSet.Contains(mediaId))
                                {
                                    mediaIdsSet.Add(mediaId);
                                    manualMedias.Add(new GroupsCacheManager.ManualMedia(mediaId.ToString(), orderNum));
                                }
                            }

                            if (manualMedias != null)
                            {
                                channel.m_lManualMedias = manualMedias.OrderBy(x => x.m_nOrderNum).ToList();
                            }
                        }

                        break;

                        #endregion
                    }
                case ChannelType.KSQL:
                    {
                        channel.filterQuery = ODBCWrapper.Utils.ExtractString(dr, "KSQL_FILTER");
                        string groupBy = ODBCWrapper.Utils.ExtractString(dr, "GROUP_BY");
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

            return channel;
        }

        private static void UpdateOrderByObject(int nOrderBy, ref Channel oChannel, string orderByValue)
        {
            if (!string.IsNullOrEmpty(orderByValue))
            {
                oChannel.m_OrderObject.m_sOrderValue = orderByValue;
                oChannel.m_OrderObject.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
            }
            else
            {
                oChannel.m_OrderObject.m_eOrderBy = (ApiObjects.SearchObjects.OrderBy)ApiObjects.SearchObjects.OrderBy.ToObject(typeof(ApiObjects.SearchObjects.OrderBy), nOrderBy);
            }
        }

        private static List<SearchValue> GetMediasForManualChannel(int channelId, out List<GroupsCacheManager.ManualMedia> lManualMedias)
        {
            List<SearchValue> lMediaIds = null;
            DataTable mediaIdsTable = Tvinci.Core.DAL.CatalogDAL.GetMediaIdsByChannelId(channelId);
            lManualMedias = null;
            if (mediaIdsTable != null && mediaIdsTable.Rows.Count > 0)
            {
                lManualMedias = new List<GroupsCacheManager.ManualMedia>();
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

                    lManualMedias.Add(new GroupsCacheManager.ManualMedia(sMediaID, nOrderNum));
                }
            }

            return lMediaIds;
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

        private static List<Channel> GetChannels(int groupId, List<int> channelIds, bool isOperatorSearch)
        {
            List<Channel> channels = null;
            try
            {
                if (channelIds == null || channelIds.Count == 0)
                {
                    return channels;
                }

                Dictionary<string, Channel> channelMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetChannelsKeysMap(groupId, channelIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetChannelsInvalidationKeysMap(groupId, channelIds);

                if (!LayeredCache.Instance.GetValues<Channel>(keyToOriginalValueMap, ref channelMap, GetChannels, new Dictionary<string, object>() { { "groupId", groupId }, { "channelIds", channelIds },
                                                                { "isOperatorSearch", isOperatorSearch } }, groupId, LayeredCacheConfigNames.GET_CHANNELS_CACHE_CONFIG_NAME, invalidationKeysMap))
                {
                    log.ErrorFormat("Failed getting Channels from LayeredCache, groupId: {0}, channelIds: {1}", groupId, string.Join(",", channelIds));
                }
                else if (channelMap != null)
                {
                    channels = channelMap.Values.ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannels for groupId: {0}, channelIds: {1}", groupId, string.Join(",", channelIds)), ex);
            }

            return channels;
        }

        private static Tuple<Dictionary<string, Channel>, bool> GetChannels(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, Channel> result = new Dictionary<string, Channel>();
            try
            {
                if (funcParams != null && funcParams.ContainsKey("channelIds") && funcParams.ContainsKey("isOperatorSearch") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    List<int> channelIds;
                    int? groupId = funcParams["groupId"] as int?;
                    bool? isOperatorSearch = funcParams["isOperatorSearch"] as bool?;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        channelIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToList();
                    }
                    else
                    {
                        channelIds = funcParams["channelIds"] != null ? funcParams["channelIds"] as List<int> : null;
                    }

                    List<Channel> channels = new List<Channel>();
                    if (channelIds != null && groupId.HasValue && isOperatorSearch.HasValue)
                    {
                        DataSet ds = CatalogDAL.GetChannelsByIds(groupId.Value, channelIds, isOperatorSearch.Value);
                        channels = GetChannelListFromDs(ds);
                        res = channels.Count() == channelIds.Count() || !isOperatorSearch.Value;
                    }

                    if (res)
                    {
                        result = channels.ToDictionary(x => LayeredCacheKeys.GetChannelKey(groupId.Value, x.m_nChannelID), x => x);
                    }
                    else
                    {
                        List<int> missingChannelIds = channels.Select(x => x.m_nChannelID).Except(channelIds).ToList();
                        log.DebugFormat("missingChannelIds: {0}", missingChannelIds);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetChannels failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, Channel>, bool>(result, res);
        }        

        #endregion

        #region Internal Methods

        internal static Channel GetChannelById(int groupId, int channelId, bool isOperatorSearch)
        {
            Channel channel = null;
            List<Channel> channels = GetChannels(groupId, new List<int>() { channelId }, isOperatorSearch);
            if (channels != null && channels.Count == 1)
            {
                channel = channels.First();
            }

            return channel;
        }

        #endregion

        #region Public Methods

        public static List<Channel> GetGroupChannels(int groupId)
        {
            List<Channel> groupChannels = null;
            try
            {
                DataSet ds = CatalogDAL.GetGroupChannels(groupId);
                groupChannels = GetChannelListFromDs(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupChannels for groupId: {0}", groupId), ex);
            }

            return groupChannels;
        }

        public static ChannelListResponse SearchChannels(int groupId, bool isExcatValue, string searchValue, int pageIndex, int pageSize,
            ApiObjects.SearchObjects.ChannelOrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDirection, bool isOperatorSearch)
        {
            ChannelListResponse result = new ChannelListResponse();
            try
            {
                ApiObjects.SearchObjects.ChannelSearchDefinitions definitions = new ApiObjects.SearchObjects.ChannelSearchDefinitions()
                {
                    GroupId = groupId,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    AutocompleteSearchValue = isExcatValue ? string.Empty : searchValue,
                    ExactSearchValue = isExcatValue ? searchValue : string.Empty,
                    OrderBy = orderBy,
                    OrderDirection = orderDirection,
                    IsOperatorSearch = isOperatorSearch
                };

                ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                int totalItems = 0;
                List<int> channelIds = wrapper.SearchChannels(definitions, ref totalItems);
                List<Channel> unorderedChannels = ChannelManager.GetChannels(groupId, channelIds, isOperatorSearch);
                if (unorderedChannels == null || unorderedChannels.Count != channelIds.Count)
                {
                    log.ErrorFormat("Failed getting channels from GetChannels, for groupId: {0}, channelIds: {1}", groupId, channelIds != null ? string.Join(",", channelIds) : string.Empty);
                    result.Status = new Status((int)eResponseStatus.ElasticSearchReturnedDeleteItem, eResponseStatus.ElasticSearchReturnedDeleteItem.ToString());
                    return result;
                }

                Dictionary<int, Channel> mappedChannels = unorderedChannels.ToDictionary(x => x.m_nChannelID, x => x);
                foreach (int channelId in channelIds)
                {
                    result.Channels.Add(mappedChannels[channelId]);
                }

                if (result.Channels != null)
                {
                    result.TotalItems = totalItems;
                    result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SearchChannels with groupId: {0}, isExcatValue: {1}, searchValue: {2}", groupId, isExcatValue, searchValue), ex);
            }

            return result;
        }        

        public static ChannelResponse AddChannel(int groupId, Channel channelToAdd, long userId)
        {
            ChannelResponse response = new ChannelResponse();

            try
            {
                if (channelToAdd == null)
                {
                    response.Status = new Status((int)eResponseStatus.NoObjectToInsert, APILogic.CRUD.KSQLChannelsManager.NO_KSQL_CHANNEL_TO_INSERT);
                    return response;
                }

                if (!CatalogDAL.ValidateChannelSystemName(groupId, channelToAdd.SystemName))
                {
                    response.Status = new Status((int)eResponseStatus.ChannelSystemNameAlreadyInUse, eResponseStatus.ChannelSystemNameAlreadyInUse.ToString());
                    return response;
                }

                if (string.IsNullOrEmpty(channelToAdd.m_sName))
                {
                    response.Status = new Status((int)eResponseStatus.NameRequired, APILogic.CRUD.KSQLChannelsManager.NAME_REQUIRED);
                    return response;
                }

                // Validate filter query by parsing it for KSQL channel only
                if (channelToAdd.m_nChannelTypeID == (int)ChannelType.KSQL && !string.IsNullOrEmpty(channelToAdd.filterQuery))
                {
                    ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
                    var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(channelToAdd.filterQuery, ref temporaryNode);

                    if (parseStatus == null)
                    {
                        response.Status = new Status((int)eResponseStatus.SyntaxError, "Failed parsing filter query");
                        return response;
                    }
                    else if (parseStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = new Status(parseStatus.Code, parseStatus.Message);
                        return response;
                    }

                    channelToAdd.filterTree = temporaryNode;
                }

                // Validate asset types
                if (channelToAdd.m_nChannelTypeID == (int)ChannelType.KSQL && channelToAdd.m_nMediaType != null && channelToAdd.m_nMediaType.Count > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddChannel", groupId);
                        return response;
                    }

                    List<int> noneGroupAssetTypes = channelToAdd.m_nMediaType.Except(catalogGroupCache.AssetStructsMapById.Keys.Select(x => (int)x).ToList()).ToList();
                    if (noneGroupAssetTypes != null && noneGroupAssetTypes.Count > 0)
                    {
                        response.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, string.Format("{0} for the following AssetTypes: {1}",
                                        eResponseStatus.AssetStructDoesNotExist.ToString(), string.Join(",", noneGroupAssetTypes)));
                        return response;
                    }
                }

                List<KeyValuePair<long, int>> mediaIdsToOrderNum = null;
                // validate medias exist for manual channel only
                if (channelToAdd.m_nChannelTypeID == (int)ChannelType.Manual && channelToAdd.m_lManualMedias != null && channelToAdd.m_lManualMedias.Count > 0)
                {
                    mediaIdsToOrderNum = new List<KeyValuePair<long, int>>();
                    List<KeyValuePair<ApiObjects.eAssetTypes, long>> assets = new List<KeyValuePair<ApiObjects.eAssetTypes, long>>();
                    foreach (GroupsCacheManager.ManualMedia manualMedia in channelToAdd.m_lManualMedias)
                    {
                        long mediaId;
                        if (long.TryParse(manualMedia.m_sMediaId, out mediaId) && mediaId > 0)
                        {
                            assets.Add(new KeyValuePair<ApiObjects.eAssetTypes, long>(ApiObjects.eAssetTypes.MEDIA, mediaId));
                            mediaIdsToOrderNum.Add(new KeyValuePair<long, int>(mediaId, manualMedia.m_nOrderNum));
                        }
                    }

                    if (assets.Count > 0)
                    {
                        // isOperatorSearch = true becuase only operator can add channel
                        List<Asset> existingAssets = AssetManager.GetAssets(groupId, assets, true);
                        if (existingAssets == null || existingAssets.Count == 0 || existingAssets.Count != channelToAdd.m_lManualMedias.Count)
                        {
                            List<long> missingAssetIds = existingAssets != null ? assets.Select(x => x.Value).Except(existingAssets.Select(x => x.Id)).ToList() : assets.Select(x => x.Value).ToList();
                            response.Status = new Status((int)eResponseStatus.AssetDoesNotExist, string.Format("{0} for the following Media Ids: {1}",
                                            eResponseStatus.AssetDoesNotExist.ToString(), string.Join(",", missingAssetIds)));
                            return response;
                        }
                    }     
                }

                if (channelToAdd.m_OrderObject.m_eOrderBy == OrderBy.META && !string.IsNullOrEmpty(channelToAdd.m_OrderObject.m_sOrderValue)
                    && !CatalogManager.CheckMetaExsits(groupId, channelToAdd.m_OrderObject.m_sOrderValue))
                {
                    response.Status = new Status((int)eResponseStatus.ChannelMetaOrderByIsInvalid, eResponseStatus.ChannelMetaOrderByIsInvalid.ToString());
                    return response;
                }

                List <KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (channelToAdd.NamesInOtherLanguages != null && channelToAdd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToAdd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToDescription = new List<KeyValuePair<string, string>>();
                if (channelToAdd.DescriptionInOtherLanguages != null && channelToAdd.DescriptionInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToAdd.DescriptionInOtherLanguages)
                    {
                        languageCodeToDescription.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                string groupBy = channelToAdd.searchGroupBy != null && channelToAdd.searchGroupBy.groupBy != null && channelToAdd.searchGroupBy.groupBy.Count == 1 ? channelToAdd.searchGroupBy.groupBy.First() : null;
                DataSet ds = CatalogDAL.InsertChannel(groupId, channelToAdd.SystemName, channelToAdd.m_sName, channelToAdd.m_sDescription, channelToAdd.m_nIsActive, (int)channelToAdd.m_OrderObject.m_eOrderBy,
                                                        (int)channelToAdd.m_OrderObject.m_eOrderDir, channelToAdd.m_OrderObject.m_sOrderValue, channelToAdd.m_nChannelTypeID, channelToAdd.filterQuery,
                                                        channelToAdd.m_nMediaType, groupBy, languageCodeToName, languageCodeToDescription, mediaIdsToOrderNum, userId);
                if (ds != null && ds.Tables.Count > 4 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    List<DataRow> nameTranslations = ds.Tables[1] != null && ds.Tables[1].Rows != null ? ds.Tables[1].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> descriptionTranslations = ds.Tables[2] != null && ds.Tables[2].Rows != null ? ds.Tables[2].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaTypes = ds.Tables[3] != null && ds.Tables[3].Rows != null ? ds.Tables[3].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaIds = ds.Tables[4] != null && ds.Tables[4].Rows != null ? ds.Tables[4].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr["Id"]);
                    if (id > 0)
                    {
                        response.Channel = CreateChannel(id, dr, nameTranslations, descriptionTranslations, mediaTypes, mediaIds);
                    }
                }                

                if (response.Channel != null && response.Channel.m_nChannelID > 0)
                {
                    bool updateResult = IndexManager.UpsertChannel(groupId, response.Channel.m_nChannelID, response.Channel);
                    if (!updateResult)
                    {
                        log.ErrorFormat("Failed update channel index with id: {0} after AddChannel", response.Channel.m_nChannelID);
                    }
                    else
                    {
                        response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddChannel for groupId: {0} and channel SystemName: {1}", groupId, channelToAdd.SystemName), ex);
            }

            return response;
        }

        public static ChannelResponse UpdateChannel(int groupId, int channelId, Channel channelToUpdate, long userId)
        {
            ChannelResponse response = new ChannelResponse();

            try
            {
                if (channelToUpdate == null)
                {
                    response.Status = new Status((int)eResponseStatus.NoObjectToInsert, APILogic.CRUD.KSQLChannelsManager.NO_KSQL_CHANNEL_TO_INSERT);
                    return response;
                }

                //isOperatorSearch = true becuase only operator can update channel
                Channel currentChannel = GetChannelById(groupId, channelId, true);
                if (currentChannel == null || currentChannel.m_nChannelTypeID != channelToUpdate.m_nChannelTypeID)
                {
                    response.Status = new Status((int)eResponseStatus.ChannelDoesNotExist, eResponseStatus.ChannelDoesNotExist.ToString());
                    return response;
                }

                if (!string.IsNullOrEmpty(channelToUpdate.SystemName) && currentChannel.SystemName != channelToUpdate.SystemName
                    && !CatalogDAL.ValidateChannelSystemName(groupId, channelToUpdate.SystemName))
                {
                    response.Status = new Status((int)eResponseStatus.ChannelSystemNameAlreadyInUse, eResponseStatus.ChannelSystemNameAlreadyInUse.ToString());
                    return response;
                }

                // Validate filter query by parsing it for KSQL channel only
                if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.KSQL && !string.IsNullOrEmpty(channelToUpdate.filterQuery))
                {
                    ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
                    var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(channelToUpdate.filterQuery, ref temporaryNode);

                    if (parseStatus == null)
                    {
                        response.Status = new Status((int)eResponseStatus.SyntaxError, "Failed parsing filter query");
                        return response;
                    }
                    else if (parseStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = new Status(parseStatus.Code, parseStatus.Message);
                        return response;
                    }

                    channelToUpdate.filterTree = temporaryNode;
                }

                // Validate asset types
                if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.KSQL && channelToUpdate.m_nMediaType != null && channelToUpdate.m_nMediaType.Count > 0)
                {
                    CatalogGroupCache catalogGroupCache;
                    if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling AddChannel", groupId);
                        return response;
                    }

                    List<int> noneGroupAssetTypes = channelToUpdate.m_nMediaType.Except(catalogGroupCache.AssetStructsMapById.Keys.Select(x => (int)x).ToList()).ToList();
                    if (noneGroupAssetTypes != null && noneGroupAssetTypes.Count > 0)
                    {
                        response.Status = new Status((int)eResponseStatus.AssetStructDoesNotExist, string.Format("{0} for the following AssetTypes: {1}",
                                        eResponseStatus.AssetStructDoesNotExist.ToString(), string.Join(",", noneGroupAssetTypes)));
                        return response;
                    }
                }

                List<KeyValuePair<long, int>> mediaIdsToOrderNum = null;
                // validate medias exist for manual channel only
                if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.Manual && channelToUpdate.m_lManualMedias != null && channelToUpdate.m_lManualMedias.Count > 0)
                {
                    mediaIdsToOrderNum = new List<KeyValuePair<long, int>>();
                    List<KeyValuePair<ApiObjects.eAssetTypes, long>> assets = new List<KeyValuePair<ApiObjects.eAssetTypes, long>>();
                    foreach (GroupsCacheManager.ManualMedia manualMedia in channelToUpdate.m_lManualMedias)
                    {
                        long mediaId;
                        if (long.TryParse(manualMedia.m_sMediaId, out mediaId) && mediaId > 0)
                        {
                            assets.Add(new KeyValuePair<ApiObjects.eAssetTypes, long>(ApiObjects.eAssetTypes.MEDIA, mediaId));
                            mediaIdsToOrderNum.Add(new KeyValuePair<long, int>(mediaId, manualMedia.m_nOrderNum));
                        }
                    }

                    if (assets.Count > 0)
                    {
                        // isOperatorSearch = true becuase only operator can update channel
                        List<Asset> existingAssets = AssetManager.GetAssets(groupId, assets, true);
                        if (existingAssets == null || existingAssets.Count == 0 || existingAssets.Count != channelToUpdate.m_lManualMedias.Count)
                        {
                            List<long> missingAssetIds = existingAssets != null ? assets.Select(x => x.Value).Except(existingAssets.Select(x => x.Id)).ToList() : assets.Select(x => x.Value).ToList();
                            response.Status = new Status((int)eResponseStatus.AssetDoesNotExist, string.Format("{0} for the following Media Ids: {1}",
                                            eResponseStatus.AssetDoesNotExist.ToString(), string.Join(",", missingAssetIds)));
                            return response;
                        }
                    }
                }

                if (channelToUpdate.m_OrderObject != null && channelToUpdate.m_OrderObject.m_eOrderBy == OrderBy.META && !string.IsNullOrEmpty(channelToUpdate.m_OrderObject.m_sOrderValue)
                    && !CatalogManager.CheckMetaExsits(groupId, channelToUpdate.m_OrderObject.m_sOrderValue))
                {
                    response.Status = new Status((int)eResponseStatus.ChannelMetaOrderByIsInvalid, eResponseStatus.ChannelMetaOrderByIsInvalid.ToString());
                    return response;
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (channelToUpdate.NamesInOtherLanguages != null && channelToUpdate.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToDescription = new List<KeyValuePair<string, string>>();
                if (channelToUpdate.DescriptionInOtherLanguages != null && channelToUpdate.DescriptionInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToUpdate.DescriptionInOtherLanguages)
                    {
                        languageCodeToDescription.Add(new KeyValuePair<string, string>(language.LanguageCode, language.Value));
                    }
                }

                string groupBy = channelToUpdate.searchGroupBy != null && channelToUpdate.searchGroupBy.groupBy != null && channelToUpdate.searchGroupBy.groupBy.Count == 1 ? channelToUpdate.searchGroupBy.groupBy.First() : null;
                int? orderByType = null;
                int? orderByDir = null;
                string orderByValue = null;
                if (channelToUpdate.m_OrderObject != null)
                {
                    orderByType = (int)channelToUpdate.m_OrderObject.m_eOrderBy;
                    orderByDir = (int)channelToUpdate.m_OrderObject.m_eOrderDir;
                    orderByValue = channelToUpdate.m_OrderObject.m_sOrderValue;
                }
                DataSet ds = CatalogDAL.UpdateChannel(groupId, channelId, channelToUpdate.SystemName, channelToUpdate.m_sName, channelToUpdate.m_sDescription, channelToUpdate.m_nIsActive, orderByType,
                                                        orderByDir, orderByValue, channelToUpdate.filterQuery, channelToUpdate.m_nMediaType, groupBy, languageCodeToName, languageCodeToDescription,
                                                        mediaIdsToOrderNum, userId);
                if (ds != null && ds.Tables.Count > 4 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    List<DataRow> nameTranslations = ds.Tables[1] != null && ds.Tables[1].Rows != null ? ds.Tables[1].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> descriptionTranslations = ds.Tables[2] != null && ds.Tables[2].Rows != null ? ds.Tables[2].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaTypes = ds.Tables[3] != null && ds.Tables[3].Rows != null ? ds.Tables[3].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaIds = ds.Tables[4] != null && ds.Tables[4].Rows != null ? ds.Tables[4].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr["Id"]);
                    if (id > 0)
                    {
                        response.Channel = CreateChannel(id, dr, nameTranslations, descriptionTranslations, mediaTypes, mediaIds);
                    }
                }

                if (response.Channel != null && response.Channel.m_nChannelID > 0)
                {
                    bool updateResult = IndexManager.UpsertChannel(groupId, response.Channel.m_nChannelID, response.Channel);
                    if (!updateResult)
                    {
                        log.ErrorFormat("Failed update channel index with id: {0} after UpdateChannel", channelId);
                    }
                    else
                    {
                        // invalidate channel
                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId)))
                        {
                            log.ErrorFormat("Failed to invalidate channel with id: {0}, invalidationKey: {1} after UpdateChannel",
                                                channelId, LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId));
                        }

                        response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateChannel for groupId: {0} and channel Id: {1}", groupId, channelId), ex);
            }

            return response;
        }

        public static ChannelResponse GetChannel(int groupId, int channelId, bool isOperatorSearch)
        {
            ChannelResponse response = new ChannelResponse();

            try
            {
                response.Channel = GetChannelById(groupId, channelId, isOperatorSearch);
                if (response.Channel != null)
                {
                    response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response.Status = new Status((int)eResponseStatus.ChannelDoesNotExist, eResponseStatus.ChannelDoesNotExist.ToString());
                }                
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannel for groupId: {0} and channelId: {1}", groupId, channelId), ex);
            }

            return response;
        }

        public static Status DeleteChannel(int groupId, int channelId, long userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (channelId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.IdentifierRequired, APILogic.CRUD.KSQLChannelsManager.ID_REQUIRED);
                    return response;
                }

                //check if channel exists - isOperatorSearch = true becuase only operator can delete channel
                ChannelResponse channelResponse = GetChannel(groupId, channelId, true);                
                if (channelResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response = channelResponse.Status;
                    return response;
                }                

                if (CatalogDAL.DeleteChannel(groupId, channelId, channelResponse.Channel.m_nChannelTypeID, userId))
                {                    
                    bool deleteResult = IndexManager.DeleteChannel(groupId, channelId);
                    if (!deleteResult)
                    {
                        log.ErrorFormat("Failed update channel index with id: {0} after DeleteChannel", channelId);
                    }
                    else
                    {
                        // invalidate channel
                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId)))
                        {
                            log.ErrorFormat("Failed to invalidate channel with id: {0}, invalidationKey: {1} after deleting channel",
                                                channelId, LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId));
                        }

                        response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                else
                {
                    log.ErrorFormat("Failed to delete channel with id: {0}, groupId: {1}", channelId, groupId);
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, channelId={1}", groupId, channelId), ex);
            }
            return response;
        }

        #endregion

    }
}
