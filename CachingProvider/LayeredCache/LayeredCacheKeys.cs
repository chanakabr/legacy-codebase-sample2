using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    public class LayeredCacheKeys
    {

        public static string GetCheckGeoBlockMediaKey(int groupID, int mediaID)
        {
            return string.Format("groupId_{0}_mediaId_{1}", groupID, mediaID);
        }

        public static string GetIsMediaExistsToUserTypeKey(int mediaID, int userTypeID)
        {
            return string.Format("mediaId_{0}_userTypeId_{1}", mediaID, userTypeID);
        }

        public static string GetFileAndMediaBasicDetailsKey(int fileID, int groupId)
        {
            return string.Format("validate_fileId_groupId_{0}_{1}", fileID, groupId);
        }

        public static string GetFileCoGuidKey(string coGuid)
        {
            return string.Format("fileCoGuid_{0}", coGuid);
        }        

        public static string GetGroupMediaConcurrencyRulesKey(int groupID)
        {
            return string.Format("group_{0}_media_concurrency_rules", groupID);
        }

        public static string GetMediaConcurrencyRulesKey(int mediaId)
        {
            return string.Format("media_{0}_concurrency_rules", mediaId);
        }

        public static string GetKeyForIp(string ip)
        {
            return string.Format("ip_{0}", ip);
        }

        public static string GetUserRolesKey(string userId)
        {
            return string.Format("roles_userId_{0}", userId);
        }

        public static string GetAddRoleInvalidationKey(string userId)
        {
            return string.Format("add_role_userId_{0}", userId);
        }

        public static string GetChannelsContainingMediaKey(int mediaId)
        {
            return string.Format("channels_containing_media_{0}", mediaId);
        }

        public static string GetDomainEntitlementsKey(int groupId, int domainId)
        {
            return string.Format("domainEntitlements_groupId_{0}_domainId_{1}", groupId, domainId);
        }

        public static string GetFileCdnDataKey(int fileId)
        {
            return string.Format("cdn_fileId_{0}", fileId);
        }

        public static string GetGroupCdnSettingsKey(int groupId)
        {
            return string.Format("group_{0}_cdn_settings", groupId);
        }

        public static string GetCDNAdapterKey(int groupId, int defaultAdapterId)
        {
            return string.Format("group_{0}_cdn_default_adapter_{1}", groupId, defaultAdapterId);
        }

        public static string GetCancelSubscriptionInvalidationKey(long domainId)
        {
            return string.Format("cancel_subscription_domainId_{0}", domainId);
        }

        public static string GetCancelTransactionInvalidationKey(long domainId)
        {
            return string.Format("cancel_transaction_domainId_{0}", domainId);
        }

        public static string GetPurchaseInvalidationKey(long domainId)
        {
            return string.Format("purchase_domainId_{0}", domainId);
        }

        public static string GetGrantEntitlementInvalidationKey(long domainId)
        {
            return string.Format("grant_domainId_{0}", domainId);
        }

        public static string GetCancelServiceNowInvalidationKey(int domainId)
        {
            return string.Format("cancel_now_domainId_{0}", domainId);
        }

        public static string GetRenewInvalidationKey(long domainId)
        {
            return string.Format("renew_domainId_{0}", domainId);
        }

        public static string GetMediaFilesKey(long mediaId, string assetType)
        {
            return string.Format("files_mediaId_{0}_assetType_{1}", mediaId, assetType);
        }

        public static string GetGroupParentalRulesKey(int groupId)
        {
            return string.Format("parental_rules_group_{0}", groupId);
        }

        public static string GetUserParentalRulesKey(int groupId, string siteGuid)
        {
            return string.Format("parental_rules_user_{0}_group_{1}", siteGuid, groupId);
        }

        public static string GetMediaParentalRulesKey(int groupId, long mediaId)
        {
            return string.Format("parental_rules_media_{0}_group_{1}", mediaId, groupId);
        }

        public static string GetLastUseWithCreditForDomainKey(int groupId, long domainId, int mediaId)
        {
            return string.Format("domainPlayUses_groupId_{0}_domainId_{1}_mediaId_{2}", groupId, domainId, mediaId);
        }

        public static string GetLastUseWithCreditForDomainInvalidationKey(int groupId, long domainId, int mediaId)
        {
            return string.Format("domainPlayUses_InvalidationKey_groupId_{0}_domainId_{1}_mediaId_{2}", groupId, domainId, mediaId);
        }

        public static string GetEpgParentalRulesKey(int groupId, long epgId)
        {
            return string.Format("parental_rules_epg_{0}_group_{1}", epgId, groupId);
        }

        public static string GetMediaIdForAssetKey(string assetId, string assetType)
        {
            return string.Format("mediaIdForAsset_assetId_{0}_assetType_{1}", assetId, assetType);
        }

        public static string GetRecordingPlaybackSettingsKey(int groupId, int mediaId)
        {
            return string.Format("recordingPlayBackSettings_groupId_{0}mediaId_{1}", groupId, mediaId);
        }

        public static string GetGroupChannelsInvalidationKey(int groupId)
        {
            return string.Format("groupChannelsInvalidationKey_groupId_{0}", groupId);
        }

        public static string GetMediaInvalidationKey(int groupId, long mediaId)
        {
            return string.Format("mediaInvalidationKey_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetEpgChannelExternalIdKey(int groupId, string epgChannelId)
        {
            return string.Format("epgChannelExternalId_groupId_{0}_epgChannelId_{1}", groupId, epgChannelId);
        }
        public static string GetExternalIdEpgChannelKey(int groupId, string cdvrId)
        {
            return string.Format("ExternalIdEpgChannel_groupId_{0}_cdvrId_{1}", groupId, cdvrId);
        }
    }
}
