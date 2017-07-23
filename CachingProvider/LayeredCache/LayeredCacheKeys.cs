using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider.LayeredCache
{
    public class LayeredCacheKeys
    {

        #region Constant Keys

        public const string GET_CURRENCIES_KEY = "currencies";

        #endregion

        #region Dynamic Keys - SHOULD START WITH "someMeaningfulName_..." prefix

        public static string GetCheckGeoBlockMediaKey(int groupID, int mediaID)
        {
            return string.Format("mediaGeoBlock_groupId_{0}_mediaId_{1}", groupID, mediaID);
        }

        public static string GetIsMediaExistsToUserTypeKey(int mediaID, int userTypeID)
        {
            return string.Format("mediaUserType_mediaId_{0}_userTypeId_{1}", mediaID, userTypeID);
        }

        public static string GetFileAndMediaBasicDetailsKey(int fileID, int groupId)
        {
            return string.Format("validateFile_fileId_{0}_groupId_{1}", fileID, groupId);
        }

        public static string GetFileByCoGuidKey(string coGuid)
        {
            return string.Format("fileByCoGuid_{0}", coGuid);
        }

        public static string GetGroupMediaConcurrencyRulesKey(int groupID)
        {
            return string.Format("groupMediaConcurrencyRules_groupId_{0}", groupID);
        }

        public static string GetMediaConcurrencyRulesKey(int mediaId)
        {
            return string.Format("mediaConcurrencyRules_mediaId_{0}", mediaId);
        }

        public static string GetKeyForIp(string ip)
        {
            return string.Format("ip_{0}", ip);
        }

        public static string GetKeyForCountryName(string countryName)
        {
            return string.Format("countryName_{0}", countryName);
        }

        public static string GetUserRolesKey(string userId)
        {
            return string.Format("userRoles_userId_{0}", userId);
        }

        public static string GetChannelsContainingMediaKey(int mediaId)
        {
            return string.Format("channelsContainingMedia_mediaId_{0}", mediaId);
        }

        public static string GetDomainEntitlementsKey(int groupId, int domainId)
        {
            return string.Format("domainEntitlements_groupId_{0}_domainId_{1}", groupId, domainId);
        }

        public static string GetFileCdnDataKey(int fileId)
        {
            return string.Format("fileCdnData_fileId_{0}", fileId);
        }

        public static string GetGroupCdnSettingsKey(int groupId)
        {
            return string.Format("groupCdnSettings_groupId_{0}", groupId);
        }

        public static string GetCDNAdapterKey(int groupId, int defaultAdapterId)
        {
            return string.Format("cdnDefaultAdapter_groupId_{0}_adapterId_{1}", groupId, defaultAdapterId);
        }

        public static string GetMediaFilesKey(long mediaId, string assetType)
        {
            return string.Format("mediaFiles_mediaId_{0}_assetType_{1}", mediaId, assetType);
        }

        public static string GetGroupParentalRulesKey(int groupId)
        {
            return string.Format("groupParentalRules_groupId_{0}", groupId);
        }

        public static string GetUserParentalRulesKey(int groupId, string siteGuid)
        {
            return string.Format("userParentalRules_groupId_{0}_userId_{1}", groupId, siteGuid);
        }

        public static string GetMediaParentalRulesKey(int groupId, long mediaId)
        {
            return string.Format("mediaParentalRules_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetLastUseWithCreditForDomainKey(int groupId, long domainId, int mediaId)
        {
            return string.Format("domainPlayUses_groupId_{0}_domainId_{1}_mediaId_{2}", groupId, domainId, mediaId);
        }

        public static string GetEpgParentalRulesKey(int groupId, long epgId)
        {
            return string.Format("epgParentalRules_groupId_{0}_epgId_{1}", groupId, epgId);
        }

        public static string GetMediaIdForAssetKey(string assetId, string assetType)
        {
            return string.Format("mediaIdForAsset_assetId_{0}_assetType_{1}", assetId, assetType);
        }

        public static string GetRecordingPlaybackSettingsKey(int groupId, int mediaId)
        {
            return string.Format("recordingPlayBackSettings_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetPriceCodeByCountryAndCurrencyKey(int groupId, int priceCodeId, string countryCode, string currencyCode)
        {
            return string.Format("priceCodeByCountryAndCurrency_g_{0}_pc_{1}_co_{2}_cu_{3}", groupId, priceCodeId, countryCode, currencyCode);
        }

        public static string GetDiscountModuleCodeByCountryAndCurrencyKey(int groupId, int discountCodeId, string countryCode, string currencyCode)
        {
            return string.Format("discountModuleByCountryAndCurrency_g_{0}_dm_{1}_co_{2}_cu_{3}", groupId, discountCodeId, countryCode, currencyCode);
        }

        public static string GetGroupDefaultCurrencyKey(int groupId)
        {
            return string.Format("groupDefaultCurrency_groupId_{0}", groupId);
        }

        public static string GetGroupAdsControlKey(int groupId)
        {
            return string.Format("groupAdsControl_groupId_{0}", groupId);
        }

        public static string GetEpgChannelExternalIdKey(int groupId, string epgChannelId)
        {
            return string.Format("epgChannelExternalId_groupId_{0}_epgChannelId_{1}", groupId, epgChannelId);
        }

        public static string GetExternalIdEpgChannelKey(int groupId, string cdvrId)
        {
            return string.Format("ExternalIdEpgChannel_groupId_{0}_cdvrId_{1}", groupId, cdvrId);
        }

        public static string GetMediaFilesByMediaIdKey(int groupId, int mediaId)
        {
            return string.Format("mediaFilesByMediaId_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetProxyIpKey(string ip)
        {
            return string.Format("proxyIp_{0}", ip);
        }

        public static string GetSeriesRemindersKey(int groupId, long seriesReminderId)
        {
            return string.Format("SeriesReminder_groupId_{0}_id_{1}", groupId, seriesReminderId);
        }

        public static Dictionary<string, string> GetSeriesRemindersKeysMap(int groupId, List<long> seriesReminderIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (seriesReminderIds != null && seriesReminderIds.Count > 0)
            {
                seriesReminderIds = seriesReminderIds.Distinct().ToList();
                foreach (long id in seriesReminderIds)
                {
                    result.Add(GetSeriesRemindersKey(groupId, id), id.ToString());
                }
            }

            return result;
        }

        public static string GetAliasMappingFields(int groupId)
        {
            return string.Format("AliasMappingFields_groupId_{0}", groupId);
        }

        public static Dictionary<string, string> GetRemindersKeysMap(int groupId, List<long> reminderIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (reminderIds != null && reminderIds.Count > 0)
            {
                reminderIds = reminderIds.Distinct().ToList();
                foreach (long id in reminderIds)
                {
                    result.Add(GetRemindersKey(groupId, id), id.ToString());
                }
            }

            return result;
        }

        public static string GetRemindersKey(int groupId, long reminderId)
        {
            return string.Format("Reminder_groupId_{0}_id_{1}", groupId, reminderId);
        }

        public static string GetAnnouncementsKey(int groupId)
        {
            return string.Format("announecements_groupId_{0}", groupId);
        }

        public static string GetSubscriptionSetKey(int groupId, long setId)
        {
            return string.Format("subscriptionSet_groupId_{0}_setId_{1}", groupId, setId );
        }

        public static Dictionary<string, string> GetSubscriptionSetsKeysMap(int groupId, List<long> setIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (setIds != null && setIds.Count > 0)
            {
                setIds = setIds.Distinct().ToList();
                foreach (long id in setIds)
                {
                    result.Add(GetSubscriptionSetKey(groupId, id), id.ToString());
                }
            }

            return result;
        }

        public static string GetGroupPriceCodesKey(int groupId)
        {
            return string.Format("priceCodes_groupId_{0}", groupId);
        }

        #endregion

        #region Invalidation Keys - SHOULD START WITH "invalidationKey..." prefix

        public static string GetUserRolesInvalidationKey(string userId)
        {
            return string.Format("invalidationKeyUserRoles_userId_{0}", userId);
        }

        public static string GetCancelSubscriptionInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyCancelSubscription_domainId_{0}", domainId);
        }

        public static string GetCancelSubscriptionRenewalInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyCancelSubscriptionRenewal_domainId_{0}", domainId);
        }

        public static string GetCancelTransactionInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyCancelTransaction_domainId_{0}", domainId);
        }

        public static string GetPurchaseInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyPurchase_domainId_{0}", domainId);
        }

        public static string GetGrantEntitlementInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyGrant_domainId_{0}", domainId);
        }

        public static string GetCancelServiceNowInvalidationKey(int domainId)
        {
            return string.Format("invalidationKeyCancelServiceNow_domainId_{0}", domainId);
        }

        public static string GetRenewInvalidationKey(long domainId)
        {
            return string.Format("invalidationKeyRenew_domainId_{0}", domainId);
        }

        public static string GetRemindersInvalidationKey(int groupId, long reminderId)
        {
            return string.Format("invalidationKeyReminder_groupId_{0}_id_{1}", groupId, reminderId);
        }

        public static string GetLastUseWithCreditForDomainInvalidationKey(int groupId, long domainId, int mediaId)
        {
            return string.Format("invalidationKeyDomainPlayUses_groupId_{0}_domainId_{1}_mediaId_{2}", groupId, domainId, mediaId);
        }

        public static string GetGroupChannelsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupChannels_groupId_{0}", groupId);
        }

        public static string GetMediaInvalidationKey(int groupId, long mediaId)
        {
            return string.Format("invalidationKeyMedia_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetSeriesRemindersInvalidationKey(int groupId, long seriesReminderId)
        {
            return string.Format("invalidationKeySeriesReminder_groupId_{0}_id_{1}", groupId, seriesReminderId);
        }

        public static Dictionary<string, List<string>> GetSeriesRemindersInvalidationKeysMap(int groupId, List<long> seriesReminderIds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (seriesReminderIds != null && seriesReminderIds.Count > 0)
            {
                seriesReminderIds = seriesReminderIds.Distinct().ToList();
                foreach (long id in seriesReminderIds)
                {
                    result.Add(GetSeriesRemindersKey(groupId, id), new List<string>() { GetSeriesRemindersInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static string GetPricingSettingsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyPricingSettings_groupId_{0}", groupId);
        }

        public static string GetAliasMappingFieldsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyAliasMappingFields_groupId_{0}", groupId);
        }

        public static Dictionary<string, List<string>> GetRemindersInvalidationKeysMap(int groupId, List<long> reminderIds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (reminderIds != null && reminderIds.Count > 0)
            {
                reminderIds = reminderIds.Distinct().ToList();
                foreach (long id in reminderIds)
                {
                    result.Add(GetRemindersKey(groupId, id), new List<string>() { GetRemindersInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static string GetAnnouncementsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyAnnounecements_groupId_{0}", groupId);
        }
       
        public static string GetSubscriptionSetInvalidationKey(int groupId, long setId)
        {
            return string.Format("invalidationKeySubscriptionSet_groupId_{0}_setId_{1}", groupId, setId);
        }

        public static Dictionary<string, List<string>> GetSubscriptionSetsInvalidationKeysMap(int groupId, List<long> setIds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (setIds != null && setIds.Count > 0)
            {
                setIds = setIds.Distinct().ToList();
                foreach (long id in setIds)
                {
                    result.Add(GetSubscriptionSetKey(groupId, id), new List<string>() { GetSubscriptionSetInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static string GetGroupPriceCodesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupPriceCodes_groupId_{0}", groupId);
        }

        #endregion        

    }
}
