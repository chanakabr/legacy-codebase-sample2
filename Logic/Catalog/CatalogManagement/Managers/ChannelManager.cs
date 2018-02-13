using ApiObjects.Response;
using ApiObjects.SearchObjects;
using GroupsCacheManager;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class ChannelManager
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Private Methods

        private static Channel CreateChannelByDataRow(int groupId, DataTable mediaTypesTable, DataRow rowData, CatalogGroupCache catalogGroupCache)
        {
            Channel channel = new Channel();
            if (channel.m_lChannelTags == null)
            {
                channel.m_lChannelTags = new List<SearchValue>();
            }

            channel.m_lManualMedias = new List<GroupsCacheManager.ManualMedia>();
            channel.m_nChannelID = ODBCWrapper.Utils.GetIntSafeVal(rowData["Id"]);
            channel.m_nGroupID = groupId;
            channel.m_nParentGroupID = groupId;
            channel.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(rowData["is_active"]);
            channel.m_nStatus = ODBCWrapper.Utils.GetIntSafeVal(rowData["status"]);
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

            #region Order

            channel.m_OrderObject = new ApiObjects.SearchObjects.OrderObj();
            int orderBy = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_type"]);
            // get order by value 
            string orderByValue = ODBCWrapper.Utils.GetSafeStr(rowData, "ORDER_BY_VALUE");

            // initiate orderBy object 
            UpdateOrderByObjec(orderBy, ref channel, catalogGroupCache, orderByValue);

            int orderDirection = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_dir"]) - 1;
            channel.m_OrderObject.m_eOrderDir =
                (ApiObjects.SearchObjects.OrderDir)ApiObjects.SearchObjects.OrderDir.ToObject(typeof(ApiObjects.SearchObjects.OrderDir), orderDirection);
            channel.m_OrderObject.m_bIsSlidingWindowField = ODBCWrapper.Utils.GetIntSafeVal(rowData["IsSlidingWindow"]) == 1;
            channel.m_OrderObject.lu_min_period_id = ODBCWrapper.Utils.GetIntSafeVal(rowData["SlidingWindowPeriod"]);

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
                case ChannelType.Manual:
                    {
                        #region Manual

                        List<GroupsCacheManager.ManualMedia> lManualMedias;
                        channel.m_lChannelTags = GetMediasForManualChannel(channel.m_nChannelID, out lManualMedias);

                        if (lManualMedias != null)
                        {
                            channel.m_lManualMedias = lManualMedias.ToList();
                        }

                        break;

                        #endregion
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

            // TODO: Lior
            //if (group.m_oMetasValuesByGroupId.ContainsKey(channel.m_nGroupID))
            //{
            //    UpdateOrderByObject(ref channel, group.m_oMetasValuesByGroupId[channel.m_nGroupID]);
            //}

            return channel;
        }

        private static void UpdateOrderByObjec(int nOrderBy, ref Channel oChannel, CatalogGroupCache catalogGroupCache, string orderByValue)
        {
            if (!string.IsNullOrEmpty(orderByValue))
            {
                oChannel.m_OrderObject.m_sOrderValue = orderByValue;
                oChannel.m_OrderObject.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
            }
            else if (nOrderBy >= 1 && nOrderBy <= 30)// all META_STR/META_DOUBLE values
            {
                int nMetaEnum = (nOrderBy);
                string enumName = Enum.GetName(typeof(ApiObjects.Catalog.MetasEnum), nMetaEnum);
                //if (group.m_oMetasValuesByGroupId[oChannel.m_nGroupID].ContainsKey(enumName))
                //{
                //    oChannel.m_OrderObject.m_sOrderValue = group.m_oMetasValuesByGroupId[oChannel.m_nGroupID][enumName];
                //    oChannel.m_OrderObject.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
                //}
            }
            else
            {
                oChannel.m_OrderObject.m_eOrderBy = (ApiObjects.SearchObjects.OrderBy)ApiObjects.SearchObjects.OrderBy.ToObject(typeof(ApiObjects.SearchObjects.OrderBy), nOrderBy);
            }
        }

        private static void UpdateOrderByObject(ref Channel oChannel, Dictionary<string, string> oMetasValues)
        {
            if (oChannel.m_OrderObject != null)
            {
                string sMetaValue = string.Empty;
                if (oChannel.m_OrderObject.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META)
                {
                    oMetasValues.TryGetValue(oChannel.m_OrderObject.m_eOrderBy.ToString(), out sMetaValue);
                    if (!string.IsNullOrEmpty(sMetaValue))
                    {
                        OrderObj oNewOrderObj = new OrderObj();
                        oNewOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
                        oNewOrderObj.m_sOrderValue = sMetaValue;
                        oNewOrderObj.m_eOrderDir = oChannel.m_OrderObject.m_eOrderDir;
                        oChannel.m_OrderObject = oNewOrderObj;
                    }
                }
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

        private static List<Channel> GetChannels(int groupId, List<int> channelIds)
        {
            List<Channel> channels = null;
            DataSet dataSet = Tvinci.Core.DAL.CatalogDAL.GetChannelDetails(channelIds);
            DataTable channelsData = dataSet.Tables[0];
            DataTable mediaTypesTable = null;

            // If there is a table of media types
            if (dataSet.Tables.Count > 1 && dataSet.Tables[1] != null && dataSet.Tables[1].Rows != null && dataSet.Tables[1].Rows.Count > 0)
            {
                mediaTypesTable = dataSet.Tables[1];
            }

            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetChannels", groupId);
                return channels;
            }

            if (channelsData != null && channelsData.Rows != null && channelsData.Rows.Count > 0)
            {
                channels = new List<Channel>();
                foreach (DataRow rowData in channelsData.Rows)
                {
                    Channel channel = CreateChannelByDataRow(groupId, mediaTypesTable, rowData, catalogGroupCache);

                    if (channel != null)
                    {
                        channels.Add(channel);
                    }
                }
            }

            return channels;
        }

        #endregion

        #region Public Methods

        public static ChannelListResponse SearchChannels(int groupId, bool isExcatValue, string searchValue, int pageIndex, int pageSize,
            ApiObjects.SearchObjects.ChannelOrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDirection)
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
                    OrderDirection = orderDirection
                };

                ElasticsearchWrapper wrapper = new ElasticsearchWrapper();
                List<int> channelIds = wrapper.SearchChannels(definitions);
                result.Channels = ChannelManager.GetChannels(groupId, channelIds);
                result.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SearchChannels with groupId: {0}, isExcatValue: {1}, searchValue: {2}", groupId, isExcatValue, searchValue), ex);
            }

            return result;
        }        

        public static ChannelResponse AddManualChannel(int groupId, Channel channelToAdd, long userId)
        {
            ChannelResponse result = new ChannelResponse();
            try
            {
                // validate channel name not in use
                ChannelListResponse SearchChannelsResponse = SearchChannels(groupId, true, channelToAdd.m_sName, 0, 5, ChannelOrderBy.Id, OrderDir.ASC);
                if (SearchChannelsResponse != null && SearchChannelsResponse.Status != null && SearchChannelsResponse.Status.Code == (int)eResponseStatus.OK
                    && SearchChannelsResponse.Channels != null && SearchChannelsResponse.Channels.Count > 0)
                {
                    result.Status = new Status((int)eResponseStatus.ChannelNameAlreadyInUse, eResponseStatus.ChannelNameAlreadyInUse.ToString());
                    return result;
                }

                // validate medias exist
                if (channelToAdd.m_lManualMedias != null && channelToAdd.m_lManualMedias.Count > 0)
                {
                    List<KeyValuePair<ApiObjects.eAssetTypes, long>> assets = new List<KeyValuePair<ApiObjects.eAssetTypes, long>>();                    
                    foreach (string manualMediaId in channelToAdd.m_lManualMedias.Select(x => x.m_sMediaId))
                    {
                        long mediaId;
                        if (long.TryParse(manualMediaId, out mediaId) && mediaId > 0)
                        {
                            assets.Add(new KeyValuePair<ApiObjects.eAssetTypes, long>(ApiObjects.eAssetTypes.MEDIA, mediaId));
                        }
                    }

                    if (assets.Count > 0)
                    {
                        AssetListResponse assetListResponse = AssetManager.GetAssets(groupId, assets);
                        if (assetListResponse != null && assetListResponse.Status != null && assetListResponse.Status.Code == (int)eResponseStatus.OK
                            && assetListResponse.Assets != null && assetListResponse.Assets.Count > 0 && assetListResponse.Assets.Count != channelToAdd.m_lManualMedias.Count)
                        {
                            List<long> missingAssetIds = assets.Select(x => x.Value).Except(assetListResponse.Assets.Select(x => x.Id)).ToList();
                            result.Status = new Status((int)eResponseStatus.AssetDoesNotExist, string.Format("{0} for the following Media Ids: {1}",
                                            eResponseStatus.AssetDoesNotExist.ToString(), string.Join(",", missingAssetIds)));
                            return result;
                        }
                    }                                  
                }

                if (channelToAdd.m_OrderObject.m_eOrderBy == OrderBy.META)
                {
                    if (string.IsNullOrEmpty(channelToAdd.m_OrderObject.m_sOrderValue) || !CatalogManager.CheckMetaExsits(groupId, channelToAdd.m_OrderObject.m_sOrderValue))
                    {
                        result.Status = new Status((int)eResponseStatus.ChannelMetaOrderByIsInvalid, eResponseStatus.ChannelMetaOrderByIsInvalid.ToString());
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddManualChannel for groupId: {0} and channelName: {1}", groupId, channelToAdd.m_sName), ex);
            }

            return result;
        }

        #endregion

    }
}
