using KLogMonitor;
using System.Reflection;
using System.Web;
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
            string udid, string ip, KalturaPlaybackContext kalturaPlaybackContext, SerializableDictionary<string, Models.General.KalturaStringValue> adapterData)
        {
            KalturaPlaybackContext KalturaPlaybackContextResponse = null;
            string id = assetId;
            if (assetType == KalturaAssetType.recording)
            {
                if (HttpContext.Current != null && HttpContext.Current.Items[Core.ConditionalAccess.PlaybackManager.RECORDING_CONVERT_KEY] != null)
                {
                    id = HttpContext.Current.Items[Core.ConditionalAccess.PlaybackManager.RECORDING_CONVERT_KEY].ToString();
                }
                else
                {
                    log.ErrorFormat("Error trying to convert recording assetId {0} to epgId from httpContext key {1}",
                                        assetId, Core.ConditionalAccess.PlaybackManager.RECORDING_CONVERT_KEY);
                }
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
                    long adapterId = applyPlaybackAdapterAction.AdapterId;
                    if (adapterId <= 0)
                    {
                        log.ErrorFormat("Error while getting playback adapter id. no adapter found. groupId: {0}, userId:{1}", groupId, userId);
                        return KalturaPlaybackContextResponse;
                    }

                    KalturaPlaybackContext playbackContextResponse = ClientsManager.ApiClient().GetPlaybackAdapterContext(adapterId, groupId,
                        userId, udid, ip, kalturaPlaybackContext, adapterData);

                    if (playbackContextResponse != null)
                    {
                        return playbackContextResponse;
                    }
                }
            }

            return KalturaPlaybackContextResponse;
        }
    }
}