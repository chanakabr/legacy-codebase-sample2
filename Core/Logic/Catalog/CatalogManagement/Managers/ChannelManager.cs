using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Api.Managers;
using Core.Catalog.Response;
using GroupsCacheManager;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ApiLogic.Api.Managers.Rule;
using ApiLogic.Catalog;
using GroupsCacheManager.Mappers;
using Tvinci.Core.DAL;
using static ApiObjects.CouchbaseWrapperObjects.CBChannelMetaData;
using ApiObjects.Base;

namespace Core.Catalog.CatalogManagement
{
    public interface IChannelManager
    {
        GenericResponse<Channel> GetChannelById(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets);
        GenericListResponse<Channel> GetChannelsListResponseByChannelIds(ContextData contextData,
            List<int> channelIds, bool isAllowedToViewInactiveAssets, int? totalItems, bool shouldFilterByShop, List<long> assetUserRuleIds = null);
        List<Channel> GetGroupChannels(int groupId);
    }

    public class ChannelManager : IChannelManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const int EPG_ASSET_TYPE = 0;
        private const string ACTION_IS_NOT_ALLOWED = "Action is not allowed";

        private static readonly Lazy<ChannelManager> lazy = new Lazy<ChannelManager>(() => new ChannelManager(), LazyThreadSafetyMode.PublicationOnly);

        public static ChannelManager Instance { get { return lazy.Value; } }

        private ChannelManager()
        {
        }

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
            EnumerableRowCollection<DataRow> channelsAssets;
            if (ds.Tables.Count == 6)
            {
                channelsAssets = ds.Tables[5] != null && ds.Tables[5].Rows != null ? ds.Tables[5].AsEnumerable() : new DataTable().AsEnumerable();
            }
            else
            {
                channelsAssets = channelsMedias;
            }

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

                    List<DataRow> assets = (from row in channelsAssets
                                            where (Int64)row["CHANNEL_ID"] == id
                                            select row).ToList();

                    Channel channel = CreateChannel(id, dr, channelNameTranslations, channelDescriptionTranslations, channelmediaTypes, medias, assets);
                    if (channel != null)
                    {
                        channels.Add(channel);
                    }
                }
            }

            return channels;
        }

        private static Channel CreateChannel(int id, DataRow dr, List<DataRow> nameTranslations, List<DataRow> descriptionTranslations, List<DataRow> mediaTypes,
            List<DataRow> channelMedias, List<DataRow> assets)
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
            channel.HasMetadata = ODBCWrapper.Utils.ExtractBoolean(dr, "HAS_METADATA");
            channel.VirtualAssetId = ODBCWrapper.Utils.GetNullableInt(dr, "VIRTUAL_ASSET_ID");

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
                    int mediaTypeId = ODBCWrapper.Utils.GetIntSafeVal(drMediaType, "MEDIA_TYPE_ID");
                    if (mediaTypeId == APILogic.CRUD.KSQLChannelsManager.EPG_ASSET_TYPE)
                    {
                        mediaTypeId = EPG_ASSET_TYPE;
                    }

                    channel.m_nMediaType.Add(mediaTypeId);
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

            channel.OrderingParameters = ChannelDataRowMapper.BuildOrderingParameters(dr);
            channel.m_OrderObject = ChannelDataRowMapper.BuildOrderObj(channel.OrderingParameters.First());

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
                        bool isMixAssets = false;
                        #region Manual
                        if (assets != null)
                        {
                            if (assets.Count > 0 && ODBCWrapper.Utils.GetNullableInt(assets[0], "asset_type").HasValue)
                            {
                                isMixAssets = true;
                                List<ManualAsset> manualMedias = new List<ManualAsset>();
                                int? assetType;
                                foreach (DataRow row in assets)
                                {
                                    assetType = ODBCWrapper.Utils.GetNullableInt(row, "asset_type");
                                    manualMedias.Add(new ManualAsset()
                                    {
                                        AssetId = ODBCWrapper.Utils.GetIntSafeVal(row, "MEDIA_ID"),
                                        AssetType = assetType.HasValue ? (eAssetTypes)assetType.Value : eAssetTypes.MEDIA,
                                        OrderNum = ODBCWrapper.Utils.GetIntSafeVal(row, "ORDER_NUM")
                                    });
                                }
                                if (manualMedias != null && manualMedias.Count > 0)
                                {
                                    channel.ManualAssets = manualMedias.OrderBy(x => x.OrderNum).ToList();
                                    channel.m_lChannelTags = channel.ManualAssets.Select(x => new SearchValue()
                                    {
                                        m_sKey = x.AssetType == eAssetTypes.MEDIA ? "media_id" : "epg_id",
                                        m_lValue = new List<string>() { x.AssetId.ToString() }
                                    }).ToList();
                                }
                            }
                        }
                        if (channelMedias != null && !isMixAssets)
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
                            if (manualMedias != null && manualMedias.Count > 0)
                            {
                                channel.m_lManualMedias = manualMedias.OrderBy(x => x.m_nOrderNum).ToList();
                                channel.m_lChannelTags = channel.m_lManualMedias.Select(x => new SearchValue() { m_sKey = "media_id", m_lValue = new List<string>() { x.m_sMediaId } }).ToList();
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

                        if (parseStatus == null || parseStatus.Code != 0)
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

            channel.SupportSegmentBasedOrdering = ODBCWrapper.Utils.ExtractBoolean(dr, "SUPPORT_SEGMENT_BASED_ORDERING");
            channel.AssetUserRuleId = ODBCWrapper.Utils.GetLongSafeVal(dr, "ASSET_RULE_ID");

            return channel;
        }

        private static List<Channel> GetChannels(int groupId, List<int> channelIds, bool isAllowedToViewInactiveAssets)
        {
            List<Channel> channels = new List<Channel>();

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
                                                                { "isAllowedToViewInactiveAssets", isAllowedToViewInactiveAssets } }, groupId, LayeredCacheConfigNames.GET_CHANNELS_CACHE_CONFIG_NAME,
                                                                invalidationKeysMap))
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
                if (funcParams != null && funcParams.ContainsKey("channelIds") && funcParams.ContainsKey("isAllowedToViewInactiveAssets") && funcParams.ContainsKey("groupId"))
                {
                    string key = string.Empty;
                    List<int> channelIds;
                    int? groupId = funcParams["groupId"] as int?;
                    bool? isAllowedToViewInactiveAssets = funcParams["isAllowedToViewInactiveAssets"] as bool?;
                    if (funcParams.ContainsKey(LayeredCache.MISSING_KEYS) && funcParams[LayeredCache.MISSING_KEYS] != null)
                    {
                        channelIds = ((List<string>)funcParams[LayeredCache.MISSING_KEYS]).Select(x => int.Parse(x)).ToList();
                    }
                    else
                    {
                        channelIds = funcParams["channelIds"] != null ? funcParams["channelIds"] as List<int> : null;
                    }

                    List<Channel> channels = new List<Channel>();
                    if (channelIds != null && groupId.HasValue && isAllowedToViewInactiveAssets.HasValue)
                    {
                        DataSet ds = CatalogDAL.GetChannelsByIds(groupId.Value, channelIds, isAllowedToViewInactiveAssets.Value);
                        channels = GetChannelListFromDs(ds);

                        // to avoid null reference exception... :|
                        if (channels == null)
                        {
                            channels = new List<Channel>();
                        }

                        var channelsWithMetadata = channels.Where(c => c.HasMetadata).ToList();
                        var channelsIdsWithMetadata = channelsWithMetadata.Select(c => c.m_nChannelID).ToList();

                        if (channelsIdsWithMetadata.Any())
                        {
                            var metadatas = CatalogDAL.GetChannelsMetadataByIds(channelsIdsWithMetadata, eChannelType.Internal);

                            foreach (var item in channelsWithMetadata)
                            {
                                if (metadatas.ContainsKey(item.m_nChannelID))
                                {
                                    item.MetaData = metadatas[item.m_nChannelID];
                                }
                            }
                        }

                        res = channels.Count() == channelIds.Count() || !isAllowedToViewInactiveAssets.Value;
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

        private static string BuildFilterQuery(ICollection<long> mediaIds, ICollection<long> epgIds)
        {
            return new TVinciShared.KsqlBuilder()
                .Or(x =>
                {
                    if (mediaIds.Any())
                    {
                        x.And(y => y.MediaType().AnyMediaIds(mediaIds));
                    }

                    if (epgIds.Any())
                    {
                        x.And(y => y.EpgType().AnyEpgIds(mediaIds));
                    }
                }).Build();
        }

        public GenericListResponse<Channel> GetChannelsListResponseByChannelIds(ContextData contextData, List<int> channelIds, bool isAllowedToViewInactiveAssets,
            int? totalItems, bool shouldFilterByShop, List<long> assetUserRuleIds = null)
        {
            GenericListResponse<Channel> result = new GenericListResponse<Channel>();
            try
            {
                List<Channel> unorderedChannels = ChannelManager.GetChannels(contextData.GroupId, channelIds, isAllowedToViewInactiveAssets);
                if (unorderedChannels == null || unorderedChannels.Count != channelIds.Count)
                {
                    log.ErrorFormat("Failed getting channels from GetChannels, for groupId: {0}, channelIds: {1}", contextData.GroupId, channelIds != null ? string.Join(",", channelIds) : string.Empty);
                    result.SetStatus(eResponseStatus.ElasticSearchReturnedDeleteItem, eResponseStatus.ElasticSearchReturnedDeleteItem.ToString());
                    return result;
                }

                // if it's an operator that also filters according to asset user rule ids
                if (assetUserRuleIds != null && assetUserRuleIds.Any())
                {
                    unorderedChannels = unorderedChannels.Where(x => x.AssetUserRuleId.HasValue && assetUserRuleIds.Contains(x.AssetUserRuleId.Value)).ToList();
                }
                // or it is not an operator and we must limit the search only to the user's shop id
                else
                {
                    long? assetUserRuleId = null;
                    if (shouldFilterByShop)
                    {
                        var userId = contextData.GetCallerUserId();
                        if (userId > 0)
                        {
                            assetUserRuleId =
                                AssetUserRuleManager.GetAssetUserRuleIdWithApplyOnChannelFilterAction(
                                    contextData.GroupId, userId);
                        }
                    }

                    if (assetUserRuleId.HasValue && assetUserRuleId.Value > 0)
                    {
                        unorderedChannels = unorderedChannels.Where(x =>
                            x.AssetUserRuleId.HasValue && x.AssetUserRuleId.Value == assetUserRuleId.Value).ToList();
                    }
                }

                foreach (var channel in unorderedChannels)
                {
                    result.Objects.Add(channel);
                }

                if (result.Objects != null)
                {
                    result.TotalItems = totalItems.HasValue ? totalItems.Value : result.Objects.Count;
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannelsListResponseByChannelIds with groupId: {0}, channelIds: {1}", contextData.GroupId, channelIds != null ? string.Join(",", channelIds) : string.Empty), ex);
            }

            return result;
        }

        private static Status ValidateChannelMediaTypes(int groupId, Channel channel)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                CatalogGroupCache catalogGroupCache;
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling ValidateChannelMediaTypes", groupId);
                    result.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                    return result;
                }

                List<int> groupAssetTypes = new List<int>(catalogGroupCache.AssetStructsMapById.Keys.Select(x => (int)x));
                // add epg type for backward compatibility
                groupAssetTypes.Add(EPG_ASSET_TYPE);
                List<int> noneGroupAssetTypes = new List<int>(channel.m_nMediaType.Except(groupAssetTypes));
                if (noneGroupAssetTypes != null && noneGroupAssetTypes.Count > 0)
                {
                    result.Set((int)eResponseStatus.AssetStructDoesNotExist, string.Format("{0} for the following AssetTypes: {1}",
                                    eResponseStatus.AssetStructDoesNotExist.ToString(), string.Join(",", noneGroupAssetTypes)));
                    return result;
                }
                else if (channel.m_nMediaType.Contains(EPG_ASSET_TYPE))
                {
                    channel.m_nMediaType.Remove(EPG_ASSET_TYPE);
                    channel.m_nMediaType.Add(APILogic.CRUD.KSQLChannelsManager.EPG_ASSET_TYPE);
                }
            }
            catch (Exception)
            {
                log.ErrorFormat("failed ValidateChannelMediaTypes for groupId: {0} when calling UpdateChannel", groupId);
                result.Set((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return result;
        }

        private static long CreateVirtualChannel(int groupId, long userId, Channel channel)
        {
            AssetStruct assetStruct = GetAssetStructIdByChannelType(groupId, channel.m_nChannelTypeID);

            return CreateVirtualChannel(groupId, userId, assetStruct, channel.m_sName, channel.m_nChannelID.ToString(), channel.m_sDescription,
                channel.m_nIsActive, channel.NamesInOtherLanguages, channel.DescriptionInOtherLanguages, channel.AssetUserRuleId);
        }

        private static AssetStruct GetAssetStructIdByChannelType(int groupId, int channelTypeId)
        {
            AssetStruct assetStruct = null;
            CatalogGroupCache catalogGroupCache;
            if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
            {
                log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling GetAssetStructIdByChannelType", groupId);
                return assetStruct;
            }

            if ((ChannelType)channelTypeId == ChannelType.Manual)
            {
                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(AssetManager.MANUAL_ASSET_STRUCT_NAME))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapBySystemName[AssetManager.MANUAL_ASSET_STRUCT_NAME];
                }
            }
            else if ((ChannelType)channelTypeId == ChannelType.KSQL)
            {
                if (catalogGroupCache.AssetStructsMapBySystemName.ContainsKey(AssetManager.DYNAMIC_ASSET_STRUCT_NAME))
                {
                    assetStruct = catalogGroupCache.AssetStructsMapBySystemName[AssetManager.DYNAMIC_ASSET_STRUCT_NAME];
                }
            }

            if (assetStruct != null && (assetStruct.TopicsMapBySystemName == null || assetStruct.TopicsMapBySystemName.Count == 0))
            {
                if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    assetStruct.TopicsMapBySystemName = catalogGroupCache.TopicsMapById.Where(x => assetStruct.MetaIds.Contains(x.Key))
                                                      .OrderBy(x => assetStruct.MetaIds.IndexOf(x.Key))
                                                      .ToDictionary(x => x.Value.SystemName, y => y.Value);
                }
            }

            return assetStruct;
        }

        private static void UpdateVirtualAsset(int groupId, long userId, Channel channel)
        {
            bool needToCreateVirtualAsset = false;
            Asset virtualAsset = GetVirtualAsset(groupId, userId, channel, out needToCreateVirtualAsset);

            if (needToCreateVirtualAsset)
            {
                long virtualAssetId = CreateVirtualChannel(groupId, userId, channel);
                if (virtualAssetId > 0)
                {
                    channel.VirtualAssetId = virtualAssetId;

                    CatalogDAL.UpdateChannelVirtualAssetId(groupId, channel.m_nChannelID, virtualAssetId);

                }
                return;
            }

            if (virtualAsset == null)
            {
                return;
            }

            virtualAsset.Name = channel.m_sName;
            virtualAsset.NamesWithLanguages = channel.NamesInOtherLanguages;
            virtualAsset.Description = channel.m_sDescription;
            virtualAsset.DescriptionsWithLanguages = channel.DescriptionInOtherLanguages;

            GenericResponse<Asset> assetUpdateResponse = AssetManager.Instance.UpdateAsset(groupId, virtualAsset.Id, virtualAsset, userId, false, false, false, true);

            if (!assetUpdateResponse.IsOkStatusCode())
            {
                log.ErrorFormat("Failed update virtual asset {0}, groupId {1}, channelId {2}", virtualAsset.Id, groupId, channel.m_nChannelID);
            }

            return;
        }

        #endregion

        #region Internal Methods

        public GenericResponse<Channel> GetChannelById(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<Channel> response = new GenericResponse<Channel>();
            List<Channel> channels = GetChannels(contextData.GroupId, new List<int>() { channelId }, isAllowedToViewInactiveAssets);
            if (channels != null && channels.Count == 1)
            {
                response.Object = channels.First();
                var userId = contextData.GetCallerUserId();
                if (userId > 0)
                {
                    var ruleId = AssetUserRuleManager.GetAssetUserRuleIdWithApplyOnChannelFilterAction(contextData.GroupId, userId);
                    if (ruleId > 0 && response.Object.AssetUserRuleId != ruleId)
                    {
                        log.DebugFormat("User {0} not allowed on channel {1}. ruleId {2}.", userId, channelId, ruleId);
                        response.SetStatus(eResponseStatus.ActionIsNotAllowed);
                        response.Object = null;
                        return response;
                    }
                }
            }
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        internal bool TryRemoveAssetRuleIdFromChannel(int groupId, long ruleId, long userId)
        {
            bool result = false;
            var contextData = new ContextData(groupId) { UserId = userId };
            var searchChannelsResponse = SearchChannels(contextData, true, string.Empty, null, 0, 500, ChannelOrderBy.Id, OrderDir.ASC, true, new List<long>() { ruleId });
            if (searchChannelsResponse.Objects != null)
            {
                foreach (var channel in searchChannelsResponse.Objects)
                {
                    // remove rule
                    channel.AssetUserRuleId = 0;
                    // update channel
                    var channelResponse = UpdateChannel(groupId, channel.m_nChannelID, channel, UserSearchContext.GetByUserId(userId));
                    if (channelResponse != null && channelResponse.Status != null && channelResponse.Status.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while RemoveAssetRuleIdFromChannel. groupId {0}, channel {1}, rule {2}, user {3}", groupId, channel.m_nChannelID, ruleId, userId);
                    }
                }

                result = true;
            }

            return result;
        }

        internal static long CreateVirtualChannel(int groupId, long userId, AssetStruct assetStruct, string name, string channelId,
            string description, int? isActive, List<LanguageContainer> namesWithLanguages = null, List<LanguageContainer> descriptionsWithLanguages = null, long? assetUserRuleId = null)
        {
            long assetId = 0;

            if (assetStruct != null && assetStruct.TopicsMapBySystemName.ContainsKey(AssetManager.CHANNEL_ID_META_SYSTEM_NAME))
            {
                MediaAsset virtualChannel = new MediaAsset()
                {
                    AssetType = eAssetTypes.MEDIA,
                    IsActive = isActive != null && isActive.HasValue && isActive.Value == 0 ? false : true,
                    CoGuid = Guid.NewGuid().ToString(),
                    Name = name,
                    NamesWithLanguages = namesWithLanguages,
                    Description = description,
                    DescriptionsWithLanguages = descriptionsWithLanguages,
                    CreateDate = DateTime.UtcNow,
                    Metas = new List<Metas>(),
                    MediaType = new MediaType()
                    {
                        m_nTypeID = (int)assetStruct.Id
                    }
                };

                virtualChannel.Metas.Add(new Metas()
                {
                    m_oTagMeta = new TagMeta()
                    {
                        m_sName = AssetManager.CHANNEL_ID_META_SYSTEM_NAME,
                        m_sType = ApiObjects.MetaType.Number.ToString()
                    },
                    m_sValue = channelId
                });

                if (assetUserRuleId > 0)
                {
                    CatalogGroupCache catalogGroupCache = null;
                    if (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        AssetManager.Instance.HandleAssetUserRuleForVirtualAsset(groupId, assetUserRuleId.Value, catalogGroupCache, ref virtualChannel);
                    }
                }

                GenericResponse<Asset> virtualChannelResponse = AssetManager.Instance.AddAsset(groupId, virtualChannel, userId);
                if (!virtualChannelResponse.HasObject())
                {
                    log.ErrorFormat("Failed create Virtual asset for channel id {0}", channelId);
                }
                else
                {
                    assetId = virtualChannelResponse.Object.Id;
                    log.DebugFormat("Success create Virtual asset for channel id {0}, asset id : {1}", channelId, virtualChannelResponse.Object.Id);

                }
            }

            return assetId;
        }

        internal static Asset GetVirtualAsset(int groupId, long userId, Channel channel, out bool needToCreateVirtualAsset)
        {
            needToCreateVirtualAsset = false;
            Asset asset = null;
            GenericResponse<Asset> assetResponse = null;

            if (channel.VirtualAssetId.HasValue)
            {
                assetResponse = AssetManager.Instance.GetAsset(groupId, channel.VirtualAssetId.Value, eAssetTypes.MEDIA, true);

                if (assetResponse.HasObject())
                {
                    return assetResponse.Object;
                }
            }

            AssetStruct assetStruct = GetAssetStructIdByChannelType(groupId, channel.m_nChannelTypeID);

            if (assetStruct == null)
            {
                log.ErrorFormat("Failed UpdateVirtualAsset. AssetStruct is missing groupId {0}, channelId {1}", groupId, channel.m_nChannelTypeID);
                return asset;
            }
            // build ElasticSearch filter
            string filter = string.Format("(and {0}='{1}' asset_type='{2}')", AssetManager.CHANNEL_ID_META_SYSTEM_NAME, channel.m_nChannelID, assetStruct.Id);
            UnifiedSearchResult[] assets = Utils.SearchAssets(groupId, filter, 0, 0, false, false);

            if (assets == null || assets.Length == 0)
            {
                log.DebugFormat("UpdateVirtualAsset. Asset not found. CreateVirtualChannel. groupId {0}, channelId {1}", groupId, channel.m_nChannelTypeID);
                needToCreateVirtualAsset = true;
                return asset;
            }

            assetResponse = AssetManager.Instance.GetAsset(groupId, long.Parse(assets[0].AssetId), eAssetTypes.MEDIA, true);

            if (!assetResponse.HasObject())
            {
                log.ErrorFormat("Failed UpdateVirtualAsset. virtual asset not found. groupId {0}, channelId {1}", groupId, channel.m_nChannelID);
                return asset;
            }

            return assetResponse.Object;
        }


        #endregion

        #region Public Methods

        public List<Channel> GetGroupChannels(int groupId)
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

        public GenericListResponse<Channel> SearchChannels(ContextData contextData, bool isExcatValue, string searchValue, List<int> specificChannelIds, int pageIndex, int pageSize,
            ChannelOrderBy orderBy, OrderDir orderDirection, bool isAllowedToViewInactiveAssets, List<long> assetUserRuleIds = null)
        {
            GenericListResponse<Channel> result = new GenericListResponse<Channel>();
            try
            {
                if (!CatalogManager.Instance.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    result.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return result;
                }

                // get userRules action filter && ApplyOnChannel
                var userId = contextData.GetCallerUserId();
                if (userId > 0)
                {
                    var shopId = AssetUserRuleManager.GetAssetUserRuleIdWithApplyOnChannelFilterAction(contextData.GroupId, userId);
                    if (shopId > 0)
                    {
                        if (assetUserRuleIds == null)
                        {
                            assetUserRuleIds = new List<long>() { shopId };
                        }
                        else if (!assetUserRuleIds.Contains(shopId))
                        {
                            assetUserRuleIds.Add(shopId);
                        }
                    }
                }

                ChannelSearchDefinitions definitions = new ChannelSearchDefinitions()
                {
                    GroupId = contextData.GroupId,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    AutocompleteSearchValue = isExcatValue ? string.Empty : searchValue,
                    ExactSearchValue = isExcatValue ? searchValue : string.Empty,
                    SpecificChannelIds = specificChannelIds != null && specificChannelIds.Count > 0 ? new List<int>(specificChannelIds) : null,
                    OrderBy = orderBy,
                    OrderDirection = orderDirection,
                    isAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets,
                    AssetUserRuleIds = assetUserRuleIds
                };

                var indexManager = IndexManagerFactory.Instance.GetIndexManager(contextData.GroupId);
                int totalItems = 0;
                List<int> channelIds = indexManager.SearchChannels(definitions, ref totalItems);
                result = GetChannelsListResponseByChannelIds(contextData, channelIds, isAllowedToViewInactiveAssets, totalItems, false);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed SearchChannels with groupId: {0}, isExcatValue: {1}, searchValue: {2}", contextData.GroupId, isExcatValue, searchValue), ex);
            }

            return result;
        }

        public static GenericResponse<Channel> AddChannel(int groupId, Channel channelToAdd, UserSearchContext searchContext)
        {
            GenericResponse<Channel> response = new GenericResponse<Channel>();

            try
            {
                bool isGroupExcludedFromTemplatesImplementation = CatalogManager.IsGroupIdExcludedFromTemplatesImplementation(groupId);
                if (!CatalogManager.Instance.DoesGroupUsesTemplates(groupId) && !isGroupExcludedFromTemplatesImplementation)
                {
                    response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return response;
                }

                if (channelToAdd == null)
                {
                    response.SetStatus(eResponseStatus.NoObjectToInsert, APILogic.CRUD.KSQLChannelsManager.NO_KSQL_CHANNEL_TO_INSERT);
                    return response;
                }

                if (!CatalogDAL.ValidateChannelSystemName(groupId, channelToAdd.SystemName))
                {
                    response.SetStatus(eResponseStatus.ChannelSystemNameAlreadyInUse, eResponseStatus.ChannelSystemNameAlreadyInUse.ToString());
                    return response;
                }

                if (string.IsNullOrEmpty(channelToAdd.m_sName))
                {
                    response.SetStatus(eResponseStatus.NameRequired, APILogic.CRUD.KSQLChannelsManager.NAME_REQUIRED);
                    return response;
                }

                // Validate filter query by parsing it for KSQL channel only
                if (channelToAdd.m_nChannelTypeID == (int)ChannelType.KSQL && !string.IsNullOrEmpty(channelToAdd.filterQuery))
                {
                    ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
                    var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(channelToAdd.filterQuery, ref temporaryNode);

                    if (parseStatus == null)
                    {
                        response.SetStatus(eResponseStatus.SyntaxError, "Failed parsing filter query");
                        return response;
                    }
                    else if (parseStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.SetStatus(parseStatus);
                        return response;
                    }

                    channelToAdd.filterTree = temporaryNode;
                }

                List<KeyValuePair<long, int>> mediaIdsToOrderNum = null;

                // validations for groups that use templates implementation (should be all but QA group...)
                if (!isGroupExcludedFromTemplatesImplementation)
                {
                    // Validate asset types
                    if (channelToAdd.m_nChannelTypeID == (int)ChannelType.KSQL && channelToAdd.m_nMediaType != null && channelToAdd.m_nMediaType.Count > 0)
                    {
                        Status validateAssetTypesResult = ValidateChannelMediaTypes(groupId, channelToAdd);
                        if (!validateAssetTypesResult.IsOkStatusCode())
                        {
                            response.Status.Set(validateAssetTypesResult.Code, validateAssetTypesResult.Message);
                            return response;
                        }
                    }

                    // validate medias exist for manual channel only
                    if (channelToAdd.m_nChannelTypeID == (int)ChannelType.Manual)
                    {
                        if (channelToAdd.m_lManualMedias?.Count > 0)
                        {
                            Status assetStatus = ValidateMediasForManualChannel(groupId, channelToAdd.m_lManualMedias, searchContext, out mediaIdsToOrderNum);
                            if (!assetStatus.IsOkStatusCode())
                            {
                                response.SetStatus(assetStatus);
                                return response;
                            }
                        }

                        if (channelToAdd.ManualAssets?.Count > 0)
                        {
                            Status assetStatus = ValidateAssetsForManualChannel(groupId, channelToAdd.ManualAssets, searchContext);
                            if (!assetStatus.IsOkStatusCode())
                            {
                                response.SetStatus(assetStatus);
                                return response;
                            }
                        }
                    }

                    foreach (var orderByMeta in channelToAdd.OrderingParameters.OfType<AssetOrderByMeta>())
                    {
                        if (!string.IsNullOrEmpty(orderByMeta.MetaName)
                            && !CatalogManager.Instance.CheckMetaExists(groupId, orderByMeta.MetaName))
                        {
                            response.SetStatus(eResponseStatus.ChannelMetaOrderByIsInvalid);
                            return response;
                        }
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (channelToAdd.NamesInOtherLanguages != null && channelToAdd.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToAdd.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToDescription = new List<KeyValuePair<string, string>>();
                if (channelToAdd.DescriptionInOtherLanguages != null && channelToAdd.DescriptionInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToAdd.DescriptionInOtherLanguages)
                    {
                        languageCodeToDescription.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                string groupBy = channelToAdd.searchGroupBy != null && channelToAdd.searchGroupBy.groupBy != null && channelToAdd.searchGroupBy.groupBy.Count == 1 ? channelToAdd.searchGroupBy.groupBy.First() : null;

                long assetUserRuleId = AssetUserRuleManager.GetAssetUserRuleIdWithApplyOnChannelFilterAction(groupId, searchContext.UserId);
                if (channelToAdd.AssetUserRuleId.HasValue && channelToAdd.AssetUserRuleId.Value > 0)
                {
                    if (assetUserRuleId > 0 && assetUserRuleId != channelToAdd.AssetUserRuleId)
                    {
                        log.DebugFormat("User {0} not allowed on channel. ruleId {1}.", searchContext.UserId, assetUserRuleId);

                        response.SetStatus(eResponseStatus.ActionIsNotAllowed, ACTION_IS_NOT_ALLOWED);
                        return response;
                    }
                }

                if (channelToAdd.AssetUserRuleId == null || (channelToAdd.AssetUserRuleId.HasValue && channelToAdd.AssetUserRuleId.Value <= 0))
                {
                    if (assetUserRuleId > 0)
                    {
                        channelToAdd.AssetUserRuleId = assetUserRuleId;
                    }
                }

                var assetOrder = channelToAdd.OrderingParameters.First();
                var assetOrderByMeta = assetOrder as AssetOrderByMeta;
                var assetSlidingWindowOrder = assetOrder as AssetSlidingWindowOrder;

                var ds = CatalogDAL.InsertChannel(
                    groupId,
                    channelToAdd.SystemName,
                    channelToAdd.m_sName,
                    channelToAdd.m_sDescription,
                    channelToAdd.m_nIsActive,
                    (int)assetOrder.Field,
                    (int)assetOrder.Direction,
                    assetOrderByMeta?.MetaName,
                    assetSlidingWindowOrder != null,
                    assetSlidingWindowOrder?.SlidingWindowPeriod,
                    channelToAdd.OrderingParameters,
                    channelToAdd.m_nChannelTypeID,
                    channelToAdd.filterQuery,
                    channelToAdd.m_nMediaType,
                    groupBy,
                    languageCodeToName,
                    languageCodeToDescription,
                    mediaIdsToOrderNum,
                    searchContext.UserId,
                    channelToAdd.SupportSegmentBasedOrdering,
                    channelToAdd.AssetUserRuleId,
                    channelToAdd.MetaData != null,
                    channelToAdd.ManualAssets);

                if (ds != null && ds.Tables.Count > 4 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    List<DataRow> nameTranslations = ds.Tables[1] != null && ds.Tables[1].Rows != null ? ds.Tables[1].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> descriptionTranslations = ds.Tables[2] != null && ds.Tables[2].Rows != null ? ds.Tables[2].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaTypes = ds.Tables[3] != null && ds.Tables[3].Rows != null ? ds.Tables[3].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> mediaIds = ds.Tables[4] != null && ds.Tables[4].Rows != null ? ds.Tables[4].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    List<DataRow> assets = ds.Tables[5] != null && ds.Tables[5].Rows != null ? ds.Tables[5].AsEnumerable().ToList() : new DataTable().AsEnumerable().ToList();
                    int id = ODBCWrapper.Utils.GetIntSafeVal(dr["Id"]);

                    if (id > 0)
                    {
                        response.Object = CreateChannel(id, dr, nameTranslations, descriptionTranslations, mediaTypes, mediaIds, assets);

                        if (response.Object.HasMetadata)
                        {
                            CatalogDAL.SaveChannelMetaData(id, eChannelType.Internal, channelToAdd.MetaData);
                            response.Object.MetaData = channelToAdd.MetaData;
                        }
                    }
                }

                if (response.Object != null && response.Object.m_nChannelID > 0)
                {
                    long virtualAssetId = CreateVirtualChannel(groupId, searchContext.UserId, response.Object);

                    if (virtualAssetId > 0)
                    {
                        response.Object.VirtualAssetId = virtualAssetId;

                        CatalogDAL.UpdateChannelVirtualAssetId(groupId, response.Object.m_nChannelID, virtualAssetId);
                    }

                    string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat($"Failed invalidating group channels key for group {groupId}");
                    }

                    bool updateResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertChannel(response.Object.m_nChannelID, response.Object, searchContext.UserId);
                    if (!updateResult)
                    {
                        log.ErrorFormat("Failed update channel index with id: {0} after AddChannel", response.Object.m_nChannelID);
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddChannel for groupId: {0} and channel SystemName: {1}", groupId, channelToAdd.SystemName), ex);
            }

            return response;
        }

        public GenericResponse<Channel> UpdateChannel(int groupId, int channelId, Channel channelToUpdate, UserSearchContext searchContext, bool isForMigration = false, bool isFromAsset = false)
        {
            GenericResponse<Channel> response = new GenericResponse<Channel>();
            Channel currentChannel = null;

            try
            {
                if (!CatalogManager.Instance.DoesGroupUsesTemplates(groupId))
                {
                    response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return response;
                }

                if (channelToUpdate == null)
                {
                    response.SetStatus(eResponseStatus.NoObjectToInsert, APILogic.CRUD.KSQLChannelsManager.NO_KSQL_CHANNEL_TO_INSERT);
                    return response;
                }

                List<KeyValuePair<long, int>> mediaIdsToOrderNum = null;
                int assetTypesValuesInd = 0;

                if (!isForMigration)
                {
                    //isAllowedToViewInactiveAssets = true because only operator can update channel
                    response = GetChannelById(new ContextData(groupId) { UserId = searchContext.UserId }, channelId, true);
                    if (response != null && response.Status != null && response.Status.Code != (int)eResponseStatus.OK)
                    {
                        return response;
                    }

                    currentChannel = response.Object;
                    if (currentChannel == null || currentChannel.m_nChannelTypeID != channelToUpdate.m_nChannelTypeID)
                    {
                        response.SetStatus(eResponseStatus.ChannelDoesNotExist, eResponseStatus.ChannelDoesNotExist.ToString());
                        return response;
                    }

                    if (!string.IsNullOrEmpty(channelToUpdate.SystemName) && currentChannel.SystemName != channelToUpdate.SystemName
                        && !CatalogDAL.ValidateChannelSystemName(groupId, channelToUpdate.SystemName))
                    {
                        response.SetStatus(eResponseStatus.ChannelSystemNameAlreadyInUse, eResponseStatus.ChannelSystemNameAlreadyInUse.ToString());
                        return response;
                    }

                    // Validate filter query by parsing it for KSQL channel only
                    if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.KSQL && !string.IsNullOrEmpty(channelToUpdate.filterQuery))
                    {
                        ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
                        var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(channelToUpdate.filterQuery, ref temporaryNode);

                        if (parseStatus == null)
                        {
                            response.SetStatus(eResponseStatus.SyntaxError, "Failed parsing filter query");
                            return response;
                        }
                        else if (parseStatus.Code != (int)eResponseStatus.OK)
                        {
                            response.SetStatus(parseStatus);
                            return response;
                        }

                        channelToUpdate.filterTree = temporaryNode;
                    }

                    // Validate asset types
                    if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.KSQL && channelToUpdate.m_nMediaType != null)
                    {
                        if (channelToUpdate.m_nMediaType.Count > 0)
                        {
                            Status validateAssetTypesResult = ValidateChannelMediaTypes(groupId, channelToUpdate);
                            if (!validateAssetTypesResult.IsOkStatusCode())
                            {
                                response.Status.Set(validateAssetTypesResult.Code, validateAssetTypesResult.Message);
                                return response;
                            }

                            assetTypesValuesInd = 1; //1 = update
                        }
                        else
                        {
                            assetTypesValuesInd = 2; //2 = clear list
                        }
                    }

                    // validate medias exist for manual channel only
                    if (channelToUpdate.m_nChannelTypeID == (int)ChannelType.Manual)
                    {
                        if (channelToUpdate.m_lManualMedias != null)
                        {
                            mediaIdsToOrderNum = new List<KeyValuePair<long, int>>();
                            if (channelToUpdate.m_lManualMedias.Count > 0)
                            {
                                Status assetStatus = ValidateMediasForManualChannel(groupId, channelToUpdate.m_lManualMedias, searchContext, out mediaIdsToOrderNum);
                                if (!assetStatus.IsOkStatusCode())
                                {
                                    response.SetStatus(assetStatus);
                                    return response;
                                }
                            }
                        }

                        if (channelToUpdate.ManualAssets?.Count > 0)
                        {
                            Status assetStatus = ValidateAssetsForManualChannel(groupId, channelToUpdate.ManualAssets, searchContext);
                            if (!assetStatus.IsOkStatusCode())
                            {
                                response.SetStatus(assetStatus);
                                return response;
                            }
                        }
                    }

                    foreach (var orderByMeta in channelToUpdate.OrderingParameters?.OfType<AssetOrderByMeta>())
                    {
                        if (!string.IsNullOrEmpty(orderByMeta.MetaName)
                            && !CatalogManager.Instance.CheckMetaExists(groupId, orderByMeta.MetaName))
                        {
                            response.SetStatus(eResponseStatus.ChannelMetaOrderByIsInvalid);
                            return response;
                        }
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToName = new List<KeyValuePair<string, string>>();
                if (channelToUpdate.NamesInOtherLanguages != null && channelToUpdate.NamesInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToUpdate.NamesInOtherLanguages)
                    {
                        languageCodeToName.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                List<KeyValuePair<string, string>> languageCodeToDescription = new List<KeyValuePair<string, string>>();
                if (channelToUpdate.DescriptionInOtherLanguages != null && channelToUpdate.DescriptionInOtherLanguages.Count > 0)
                {
                    foreach (LanguageContainer language in channelToUpdate.DescriptionInOtherLanguages)
                    {
                        languageCodeToDescription.Add(new KeyValuePair<string, string>(language.m_sLanguageCode3, language.m_sValue));
                    }
                }

                var groupBy = channelToUpdate.searchGroupBy?.groupBy != null && channelToUpdate.searchGroupBy.groupBy.Count == 1 ? channelToUpdate.searchGroupBy.groupBy.First() : null;

                var assetOrder = channelToUpdate.OrderingParameters.FirstOrDefault();
                var assetOrderByMeta = assetOrder as AssetOrderByMeta;
                var assetSlidingWindowOrder = assetOrder as AssetSlidingWindowOrder;

                int? updatedChannelType = null;
                if (isForMigration)
                {
                    updatedChannelType = channelToUpdate.m_nChannelTypeID;
                }

                bool? hasMetadata = null;
                if (channelToUpdate.MetaData != null)
                {
                    hasMetadata = channelToUpdate.MetaData.Any();
                }

                DataSet ds = CatalogDAL.UpdateChannel(
                    groupId,
                    channelId,
                    channelToUpdate.SystemName,
                    channelToUpdate.m_sName,
                    channelToUpdate.m_sDescription,
                    channelToUpdate.m_nIsActive,
                    (int?)assetOrder?.Field,
                    (int?)assetOrder?.Direction,
                    assetOrderByMeta?.MetaName,
                    assetSlidingWindowOrder != null,
                    assetSlidingWindowOrder?.SlidingWindowPeriod,
                    channelToUpdate.OrderingParameters,
                    channelToUpdate.filterQuery,
                    channelToUpdate.m_nMediaType,
                    groupBy,
                    languageCodeToName,
                    languageCodeToDescription,
                    mediaIdsToOrderNum,
                    searchContext.UserId,
                    channelToUpdate.SupportSegmentBasedOrdering,
                    channelToUpdate.AssetUserRuleId,
                    assetTypesValuesInd,
                    hasMetadata,
                    channelToUpdate.ManualAssets,
                    updatedChannelType);

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
                        var metaData = channelToUpdate.MetaData;

                        if (metaData != null)
                        {
                            if (metaData.Any())
                            {
                                CatalogDAL.SaveChannelMetaData(id, eChannelType.Internal, metaData);
                            }
                            else
                            {
                                CatalogDAL.DeleteChannelMetaData(id, eChannelType.Internal);
                            }
                        }
                        else
                        {
                            metaData = currentChannel?.MetaData;
                        }

                        List<DataRow> assets = null;
                        if (mediaIdsToOrderNum == null)
                        {
                            assets = mediaIds;
                        }

                        response.Object = CreateChannel(id, dr, nameTranslations, descriptionTranslations, mediaTypes, mediaIds, assets);
                        response.Object.MetaData = metaData;
                    }
                }

                if (response.Object != null && response.Object.m_nChannelID > 0)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    if (!isForMigration)
                    {
                        if (!isFromAsset)
                        {
                            UpdateVirtualAsset(groupId, searchContext.UserId, response.Object);
                        }

                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId)))
                        {
                            log.ErrorFormat($"Failed invalidating group channels key for group {groupId}");
                        }

                        if (!LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId)))
                        {
                            log.ErrorFormat("Failed to invalidate channel with id: {0}, invalidationKey: {1} after UpdateChannel",
                                                channelId, LayeredCacheKeys.GetChannelInvalidationKey(groupId, channelId));
                        }

                        bool updateResult = IndexManagerFactory.Instance.GetIndexManager(groupId).UpsertChannel(response.Object.m_nChannelID, response.Object, searchContext.UserId);
                        if (!updateResult)
                        {
                            log.ErrorFormat("Failed update channel index with id: {0} after UpdateChannel", channelId);
                        }
                    }
                }
                else
                {
                    response.SetStatus(eResponseStatus.Error, eResponseStatus.Error.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateChannel for groupId: {0} and channel Id: {1}", groupId, channelId), ex);
            }

            return response;
        }

        public GenericResponse<Channel> GetChannel(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets, bool shouldCheckGroupUsesTemplates = true)
        {
            GenericResponse<Channel> response = new GenericResponse<Channel>();

            try
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(contextData.GroupId);
                if (shouldCheckGroupUsesTemplates && !doesGroupUsesTemplates)
                {
                    response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return response;
                }

                response = GetChannelById(contextData, channelId, isAllowedToViewInactiveAssets);
                if (response != null && response.Status != null && response.Status.Code != (int)eResponseStatus.OK)
                {
                    return response;
                }

                if (response.Object != null)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    if (doesGroupUsesTemplates)
                    {
                        response.SetStatus(eResponseStatus.ChannelDoesNotExist, eResponseStatus.ChannelDoesNotExist.ToString());
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.ObjectNotExist, "KSQL Channel with given ID does not exist");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannel for groupId: {0} and channelId: {1}", contextData.GroupId, channelId), ex);
            }

            return response;
        }

        public Status DeleteChannel(int groupId, int channelId, long userId, bool isFromAsset = false)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (channelId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.IdentifierRequired, APILogic.CRUD.KSQLChannelsManager.ID_REQUIRED);
                    return response;
                }

                //check if channel exists - isAllowedToViewInactiveAssets = true becuase only operator can delete channel
                GenericResponse<Channel> channelResponse = GetChannel(new ContextData(groupId) { UserId = userId }, channelId, true, false);
                if (channelResponse.Status.Code != (int)eResponseStatus.OK)
                {
                    response = channelResponse.Status;
                    return response;
                }

                if (CatalogDAL.DeleteChannel(groupId, channelId, channelResponse.Object.m_nChannelTypeID, userId))
                {
                    CatalogDAL.DeleteChannelMetaData(channelId, eChannelType.Internal);
                    RemoveRelatedEntitiesData(groupId, userId, channelId);

                    string invalidationKey = LayeredCacheKeys.GetGroupChannelsInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat($"Failed invalidating group channels key for group {groupId}");
                    }

                    // in case channel delete or in not active this need to be informed to collection and subscription that contines it
                    // this call should be delete after the moudles separation
                    CollectionManager.Instance.HandleChannelUpdate(groupId, userId, channelId);
                    SubscriptionManager.Instance.HandleChannelUpdate(groupId, channelId);

                    bool deleteResult = false;
                    bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);

                    // delete index only for OPC accounts since previously channels index didn't exist
                    if (doesGroupUsesTemplates)
                    {
                        if (!isFromAsset)
                        {
                            //delete virtual asset
                            bool needToCreateVirtualAsset = false;
                            Asset virtualChannel = GetVirtualAsset(groupId, userId, channelResponse.Object, out needToCreateVirtualAsset);
                            if (virtualChannel != null)
                            {
                                Status status = AssetManager.Instance.DeleteAsset(groupId, virtualChannel.Id, eAssetTypes.MEDIA, userId, true);
                                if (status == null || !status.IsOkStatusCode())
                                {
                                    log.ErrorFormat("Failed delete virtual asset {0}. for channel {1}", virtualChannel.Id, channelId);
                                }
                            }
                        }

                        deleteResult = IndexManagerFactory.Instance.GetIndexManager(groupId).DeleteChannel(channelId);
                    }
                    else
                    {
                        deleteResult = CatalogLogic.UpdateChannelIndex(new List<long>() { channelId }, groupId, eAction.Delete);
                    }

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

                        // update group cache
                        if (!doesGroupUsesTemplates)
                        {
                            string[] keys = new string[1] { APILogic.CRUD.KSQLChannelsManager.BuildChannelCacheKey(groupId, channelId) };
                            if (!TVinciShared.QueueUtils.UpdateCache(groupId, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys))
                            {
                                log.ErrorFormat("Failed to UpdateCache channel with id: {0}, keys: {1} after deleting channel", channelId, keys[0]);
                            }
                        }

                        response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }

                    // remove channel from categories
                    var removeStatus = CategoryItemHandler.Instance.RemoveChannelFromCategories(groupId, channelId, UnifiedChannelType.Internal, userId);
                    if (removeStatus != null && !removeStatus.IsOkStatusCode())
                    {
                        log.Error($"Failed to remove channel {channelId} from categories fr group {groupId}");
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

        private static void RemoveRelatedEntitiesData(int groupId, long userId, long channelId)
        {
            var affectedAssets = CatalogDAL.GetAssociatedAsset(groupId, (int)userId, channelId);
            affectedAssets.ForEach(x => AssetManager.Instance.InvalidateAsset((eAssetTypes)x.Value, groupId, x.Key));
        }

        public GenericListResponse<Channel> GetChannelsContainingMedia(ContextData contextData, long mediaId, int pageIndex, int pageSize,
            ChannelOrderBy orderBy, OrderDir orderDirection, bool isAllowedToViewInactiveAssets)
        {
            GenericListResponse<Channel> result = new GenericListResponse<Channel>();
            try
            {
                if (!CatalogManager.Instance.DoesGroupUsesTemplates(contextData.GroupId))
                {
                    result.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                    return result;
                }

                List<int> channelIds = Utils.GetChannelsContainingMedia(contextData.GroupId, (int)mediaId);
                if (channelIds != null && channelIds.Count > 0)
                {
                    result = SearchChannels(contextData, true, string.Empty, channelIds, pageIndex, pageSize, orderBy, orderDirection, isAllowedToViewInactiveAssets);
                }
                else
                {
                    result.TotalItems = 0;
                    result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetChannelsContainingMedia with groupId: {0}, mediaId: {1}", contextData.GroupId, mediaId), ex);
            }

            return result;
        }

        private static Status ValidateMediasForManualChannel(int groupId, List<GroupsCacheManager.ManualMedia> manualMedias, UserSearchContext searchContext, out List<KeyValuePair<long, int>> mediaIdsToOrderNum)
        {
            mediaIdsToOrderNum = new List<KeyValuePair<long, int>>();
            var assets = new List<ManualAsset>();
            foreach (GroupsCacheManager.ManualMedia manualMedia in manualMedias)
            {
                if (long.TryParse(manualMedia.m_sMediaId, out var mediaId) && mediaId > 0)
                {
                    assets.Add(new ManualAsset { AssetType = eAssetTypes.MEDIA, AssetId = mediaId, OrderNum = manualMedia.m_nOrderNum });
                    mediaIdsToOrderNum.Add(new KeyValuePair<long, int>(mediaId, manualMedia.m_nOrderNum));
                }
            }

            var status = ValidateAssetsForManualChannel(groupId, assets, searchContext);

            return status;
        }

        private static Status ValidateAssetsForManualChannel(long groupId, IReadOnlyCollection<ManualAsset> assets, UserSearchContext searchContext)
        {
            var mediaIds = assets
                .Where(x => x.AssetType == eAssetTypes.MEDIA)
                .Select(x => x.AssetId)
                .ToArray();
            var epgIds = assets
                .Where(x => x.AssetType == eAssetTypes.EPG)
                .Select(x => x.AssetId)
                .ToArray();
            var searchRequest = new UnifiedSearchRequestBuilder(FilterAsset.Instance)
                .WithFilterQuery(BuildFilterQuery(mediaIds, epgIds))
                .Build(groupId, searchContext);
            if (searchRequest == null)
            {
                return Status.Error;
            }

            var searchResponse = SearchProvider.Instance.SearchAssets(searchRequest);
            if (!searchResponse.status.IsOkStatusCode())
            {
                return searchResponse.status;
            }

            var missingMediaIds = mediaIds
                .Concat(epgIds)
                .Except(searchResponse.searchResults.Select(x => long.Parse(x.AssetId)))
                .ToArray();
            if (missingMediaIds.Any())
            {
                return new Status
                {
                    Code = (int)eResponseStatus.AssetDoesNotExist,
                    Message = $"AssetDoesNotExist Ids: {string.Join(",", missingMediaIds)}"
                };
            }

            return new Status(eResponseStatus.OK);
        }

        public GenericListResponse<Channel> GetChannels(ContextData contextData, AssetSearchDefinition assetSearchDefinition, ChannelType? channelType,
            int pageIndex, int pageSize, ChannelOrderBy orderBy, OrderDir orderDirection)
        {
            var response = new GenericListResponse<Channel>();
            HashSet<long> channelIds = null;

            // get userRules action filter && ApplyOnChannel
            var userId = contextData.GetCallerUserId();
            if (userId == 0)
            {
                userId = assetSearchDefinition.UserId;
            }

            long assetUserRuleId = AssetUserRuleManager.GetAssetUserRuleIdWithApplyOnChannelFilterAction(contextData.GroupId, userId);
            if (assetUserRuleId > 0)
            {
                ChannelSearchDefinitions definitions = new ChannelSearchDefinitions()
                {
                    GroupId = contextData.GroupId,
                    OrderBy = orderBy,
                    OrderDirection = orderDirection,
                    isAllowedToViewInactiveAssets = assetSearchDefinition.IsAllowedToViewInactiveAssets,
                    AssetUserRuleIds = new List<long>() { assetUserRuleId }
                };

                var indexManager = IndexManagerFactory.Instance.GetIndexManager(contextData.GroupId);
                int totalItems = 0;
                channelIds = indexManager.SearchChannels(definitions, ref totalItems).Select(x => (long)x).ToHashSet();

                if (totalItems == 0)
                {
                    response.SetStatus(eResponseStatus.OK);
                    return response;
                }
            }

            if (channelType.HasValue)
            {
                assetSearchDefinition.AssetStructId = api.GetChannelAssetStruct(contextData.GroupId, channelType.Value);

                if (assetSearchDefinition.AssetStructId == 0)
                {
                    response.SetStatus(eResponseStatus.AssetStructDoesNotExist, "AssetStruct for channel does not exist.");
                    return response;
                }
            }

            var result = api.Instance.GetObjectVirtualAssetObjectIdsForChannels(contextData.GroupId, assetSearchDefinition, channelIds, pageIndex, pageSize);
            if (result.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
            {
                response.SetStatus(result.Status);
                return response;
            }

            if (result.ObjectIds?.Count > 0)
            {
                response.Objects = GetChannels(contextData.GroupId, result.ObjectIds.Select(x => (int)x).ToList(), assetSearchDefinition.IsAllowedToViewInactiveAssets);
                response.TotalItems = result.TotalItems;
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        #endregion
    }
}