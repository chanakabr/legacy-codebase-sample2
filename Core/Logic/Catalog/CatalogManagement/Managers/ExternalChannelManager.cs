using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;
using static ApiObjects.CouchbaseWrapperObjects.CBChannelMetaData;

namespace Core.Catalog.CatalogManagement
{
    public interface IExternalChannelManager
    {
        GenericResponse<ExternalChannel> GetChannelById(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets);
    }

    public class ExternalChannelManager : IExternalChannelManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<ExternalChannelManager> lazy = new Lazy<ExternalChannelManager>(() => new ExternalChannelManager(), LazyThreadSafetyMode.PublicationOnly);

        public static ExternalChannelManager Instance { get { return lazy.Value; } }

        private ExternalChannelManager()
        {
        }

        public GenericResponse<ExternalChannel> GetChannelById(ContextData contextData, int channelId, bool isAllowedToViewInactiveAssets)
        {
            GenericResponse<ExternalChannel> response = new GenericResponse<ExternalChannel>();
            List<ExternalChannel> channels = GetChannels(contextData.GroupId, new List<int>() { channelId }, isAllowedToViewInactiveAssets);
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

                response.SetStatus(eResponseStatus.OK);
            }
            return response;
        }

        private static List<ExternalChannel> GetChannels(int groupId, List<int> channelIds, bool isAllowedToViewInactiveAssets)
        {
            List<ExternalChannel> channels = new List<ExternalChannel>();

            try
            {
                if (channelIds == null || channelIds.Count == 0)
                {
                    return channels;
                }

                Dictionary<string, ExternalChannel> channelMap = null;
                Dictionary<string, string> keyToOriginalValueMap = LayeredCacheKeys.GetExternalChannelsKeysMap(groupId, channelIds);
                Dictionary<string, List<string>> invalidationKeysMap = LayeredCacheKeys.GetExternalChannelsInvalidationKeysMap(groupId, channelIds);

                if (!LayeredCache.Instance.GetValues<ExternalChannel>(keyToOriginalValueMap, ref channelMap, GetExternalChannels, new Dictionary<string, object>() { { "groupId", groupId }, { "channelIds", channelIds },
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

        private static Tuple<Dictionary<string, ExternalChannel>, bool> GetExternalChannels(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, ExternalChannel> result = new Dictionary<string, ExternalChannel>();
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

                    List<ExternalChannel> channels = new List<ExternalChannel>();
                    if (channelIds != null && groupId.HasValue && isAllowedToViewInactiveAssets.HasValue)
                    {
                        DataSet ds = CatalogDAL.GetExternalChannelsByIds(groupId.Value, channelIds.Select(x => (long)x).ToList<long>(), isAllowedToViewInactiveAssets.Value);
                        channels = CatalogDAL.SetExternalChannels(ds);

                        // to avoid null reference exception... :|
                        if (channels == null)
                        {
                            channels = new List<ExternalChannel>();
                        }

                        var channelsWithMetadata = channels.Where(c => c.HasMetadata).ToList();
                        var channelsIdsWithMetadata = channelsWithMetadata.Select(c => c.ID).ToList();

                        if (channelsIdsWithMetadata.Any())
                        {
                            var metadatas = CatalogDAL.GetChannelsMetadataByIds(channelIds, eChannelType.External);

                            foreach (var item in channelsWithMetadata)
                            {
                                if (metadatas.ContainsKey(item.ID))
                                {
                                    item.MetaData = metadatas[item.ID];
                                }
                            }
                        }

                        res = channels.Count() == channelIds.Count() || !isAllowedToViewInactiveAssets.Value;
                    }

                    if (res)
                    {
                        result = channels.ToDictionary(x => LayeredCacheKeys.GetExternalChannelKey(groupId.Value, x.ID), x => x);
                    }
                    else
                    {
                        List<int> missingChannelIds = channels.Select(x => x.ID).Except(channelIds).ToList();
                        log.Debug($"missingChannelIds: {missingChannelIds}");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetExternalChannels failed params : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<Dictionary<string, ExternalChannel>, bool>(result, res);
        }
    }
}