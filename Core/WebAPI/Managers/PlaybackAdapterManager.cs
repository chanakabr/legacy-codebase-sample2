using ApiLogic.Api.Managers;
using Phx.Lib.Log;
using System.Reflection;
using System.Web;
using KalturaRequestContext;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Utils
{
    public static class PlaybackAdapterManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static KalturaPlaybackContext GetPlaybackAdapterContext(int groupId, string userId, string assetId, KalturaAssetType assetType,
            string udid, string ip, KalturaPlaybackContext kalturaPlaybackContext, KalturaPlaybackContextOptions contextDataParams)
        {
            KalturaPlaybackContext KalturaPlaybackContextResponse = null;
            string id = assetId;
            if (assetType == KalturaAssetType.recording)
            {
                TryConvertRecordingAssetIdFromContext(ref id);
            }

            KalturaAssetRuleFilter filter = new KalturaAssetRuleFilter()
            {
                ActionsContainType = KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER,
                ConditionsContainType = KalturaRuleConditionType.ASSET,
                AssetApplied = new Models.Catalog.KalturaSlimAsset()
                {
                    Id = id,
                    Type = assetType
                }
            };

            long adapterId = 0;
            KalturaAssetRuleListResponse assetRuleListResponse = ClientsManager.ApiClient().GetAssetRules(groupId, filter);
            if (assetRuleListResponse != null &&
                assetRuleListResponse.Objects != null &&
                assetRuleListResponse.Objects.Count > 0 &&
                assetRuleListResponse.Objects[0].Actions != null &&
                assetRuleListResponse.Objects[0].Actions.Count > 0)
            {
                KalturaApplyPlaybackAdapterAction applyPlaybackAdapterAction = assetRuleListResponse.Objects[0].Actions[0] as KalturaApplyPlaybackAdapterAction;
                if (applyPlaybackAdapterAction != null)
                {
                    adapterId = applyPlaybackAdapterAction.AdapterId;
                    if (adapterId <= 0)
                    {
                        log.ErrorFormat("Error while getting playback adapter id. no adapter found. groupId: {0}, userId:{1}", groupId, userId);
                        return KalturaPlaybackContextResponse;
                    }
                }
            }

            if (adapterId == 0)
            {
                // Get default adapter configuration
                var defaultConfig = PartnerConfigurationManager.GetPlaybackConfig(groupId);
                if (defaultConfig != null && defaultConfig.HasObject() && defaultConfig.Object.DefaultAdapters != null)
                {
                    adapterId = GetDefaultAdapterConfiguration(groupId, assetType);
                }
            }

            if (adapterId > 0)
            {
                KalturaPlaybackContext playbackContextResponse = ClientsManager.ApiClient().GetPlaybackAdapterContext(adapterId, groupId,
                    userId, udid, ip, kalturaPlaybackContext, assetId, assetType, contextDataParams);

                if (playbackContextResponse != null)
                {
                    return playbackContextResponse;
                }
            }

            return KalturaPlaybackContextResponse;
        }

        internal static KalturaPlaybackContext GetPlaybackAdapterManifest(int groupId, string assetId, KalturaAssetType assetType, KalturaPlaybackContext kalturaPlaybackContext,
            KalturaPlaybackContextOptions contextDataParams, string userId, string udid, string ip)
        {
            KalturaPlaybackContext KalturaPlaybackContextResponse = null;
            string id = assetId;
            if (assetType == KalturaAssetType.recording)
            {
                TryConvertRecordingAssetIdFromContext(ref id);
            }

            KalturaAssetRuleFilter filter = new KalturaAssetRuleFilter()
            {
                ActionsContainType = KalturaRuleActionType.APPLY_PLAYBACK_ADAPTER,
                ConditionsContainType = KalturaRuleConditionType.ASSET,
                AssetApplied = new Models.Catalog.KalturaSlimAsset()
                {
                    Id = id,
                    Type = assetType
                }
            };

            long adapterId = 0;
            KalturaAssetRuleListResponse assetRuleListResponse = ClientsManager.ApiClient().GetAssetRules(groupId, filter);
            if (assetRuleListResponse != null &&
                assetRuleListResponse.Objects != null &&
                assetRuleListResponse.Objects.Count > 0 &&
                assetRuleListResponse.Objects[0].Actions != null &&
                assetRuleListResponse.Objects[0].Actions.Count > 0)
            {
                KalturaApplyPlaybackAdapterAction applyPlaybackAdapterAction = assetRuleListResponse.Objects[0].Actions[0] as KalturaApplyPlaybackAdapterAction;
                if (applyPlaybackAdapterAction != null)
                {
                    adapterId = applyPlaybackAdapterAction.AdapterId;
                    if (adapterId <= 0)
                    {
                        log.Error($"Error while getting playback adapter id. no adapter found. groupId: {groupId}");
                        return KalturaPlaybackContextResponse;
                    }
                }
            }

            if (adapterId == 0)
            {
                adapterId = GetDefaultAdapterConfiguration(groupId, assetType);
            }

            if (adapterId > 0)
            {
                KalturaPlaybackContext playbackContextResponse = ClientsManager.ApiClient().GetPlaybackAdapterManifest(adapterId, groupId, kalturaPlaybackContext,
                assetId, assetType, contextDataParams, userId, udid, ip);

                if (playbackContextResponse != null)
                {
                    return playbackContextResponse;
                }
            }

            return KalturaPlaybackContextResponse;
        }

        private static long GetDefaultAdapterConfiguration(int groupId, KalturaAssetType assetType)
        {
            long adapterId = 0;
            var defaultConfig = PartnerConfigurationManager.GetPlaybackConfig(groupId);
            if (defaultConfig != null && defaultConfig.HasObject() && defaultConfig.Object.DefaultAdapters != null)
            {
                switch (assetType)
                {
                    case KalturaAssetType.media:
                        adapterId = defaultConfig.Object.DefaultAdapters.MediaAdapterId;
                        break;
                    case KalturaAssetType.recording:
                        adapterId = defaultConfig.Object.DefaultAdapters.RecordingAdapterId;
                        break;
                    case KalturaAssetType.epg:
                        adapterId = defaultConfig.Object.DefaultAdapters.EpgAdapterId;
                        break;
                    default:
                        break;
                }

                if (adapterId > 0)
                {
                    log.Debug($"default playback adapter configuration found. adapterId: {adapterId} ");
                }
            }

            return adapterId;
        }

        private static void TryConvertRecordingAssetIdFromContext(ref string assetId)
        {
            if (RequestContextUtilsInstance.Get().TryGetRecordingConvertId(out var id))
            {
                assetId = id.ToString();
            }
            else
            {
                log.ErrorFormat("Error trying to convert recording assetId {0} to epgId from httpContext key {1}",
                    assetId, RequestContextConstants.RECORDING_CONVERT_KEY);
            }
        }
    }
}