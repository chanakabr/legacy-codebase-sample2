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

        public const string GET_RATIOS_KEY = "Ratios";

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

        public static string GetMediaConcurrencyRulesDeviceLimitationModuleKey(int dlmId)
        {
            return string.Format("mediaConcurrencyRules_dlmId_{0}", dlmId);
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

        public static string GetPermissionsRolesIdsKey(int groupId)
        {
            return string.Format("getPermissionRoleIds_groupId_{0}", groupId);
        }

        public static string GetFileCdnDataKey(int fileId)
        {
            return string.Format("fileCdnData_fileId_{0}", fileId);
        }

        public static string GetGroupCdnSettingsKey(int groupId)
        {
            return string.Format("groupCdnSettings_groupId_{0}", groupId);
        }

        public static string GetCDNAdapterKey(int groupId, int adapterId)
        {
            return string.Format("cdnAdapter_groupId_{0}_adapterId_{1}", groupId, adapterId);
        }

        public static string GetDrmAdapterKey(int groupId, int defaultAdapterId)
        {
            return string.Format("drmAdapter_groupId_{0}_adapterId_{1}", groupId, defaultAdapterId);
        }

        public static string GetGroupDrmAdapterIdKey(int groupId)
        {
            return string.Format("drmGroupAdapter_groupId_{0}", groupId);
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

        public static string GetRoleIdKey(int roleId)
        {
            return string.Format("roleId_{0}", roleId);
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
            return string.Format("subscriptionSet_groupId_{0}_setId_{1}", groupId, setId);
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

        public static string GetGroupUnifiedBillingCycleKey(int groupId)
        {
            return string.Format("unifiedBillingCycle_groupId_{0}", groupId);
        }

        public static string GetDoesGroupUsesTemplatesCacheKey(int groupId)
        {
            return string.Format("DoesGroupUsesTemplates_groupId_{0}", groupId);
        }

        public static string GetCatalogGroupCacheKey(int groupId)
        {
            return string.Format("CatalogGroupCache_groupId_{0}", groupId);
        }

        public static string GetGroupDeviceRulesKey(int groupId)
        {
            return string.Format("GroupDeviceRules_groupId_{0}", groupId);
        }
        
        public static string GetGroupGeoBlockRulesKey(int groupId)
        {
            return string.Format("GroupGeoBlockRules_groupId_{0}", groupId);
        }

        public static string GetAssetKey(string assetType, long id)
        {
            return string.Format("Asset_type_{0}_id_{1}", assetType, id);
        }

        public static string GetGroupImageTypesKey(int groupId)
        {
            return string.Format("GroupImageTypes_groupId_{0}", groupId);
        }

        public static string GetGroupDefaultImagesKey(int groupId)
        {
            return string.Format("GroupDefaultImages_groupId_{0}", groupId);
        }

        public static string GetGroupRatiosKey(int groupId)
        {
            return string.Format("GroupRatios_groupId_{0}", groupId);
        }

        public static string GetGroupMediaFileTypesKey(int groupId)
        {
            return string.Format("GroupMediaFileTypes_groupId_{0}", groupId);
        }

        public static string GetChannelKey(int groupId, int channelId)
        {
            return string.Format("Channel_groupId_{0}_Id_{1}", groupId, channelId);
        }

        public static Dictionary<string, string> GetChannelsKeysMap(int groupId, List<int> channelIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (channelIds != null && channelIds.Count > 0)
            {
                channelIds = channelIds.Distinct().ToList();
                foreach (int id in channelIds)
                {
                    result.Add(GetChannelKey(groupId, id), id.ToString());
                }
            }

            return result;
        }

        public static Dictionary<string, string> GetAssetsKeyMap(string assetType, List<long> assetIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (assetIds != null && assetIds.Count > 0)
            {
                assetIds = assetIds.Distinct().ToList();
                foreach (long id in assetIds)
                {
                    result.Add(GetAssetKey(assetType, id), id.ToString());
                }
            }

            return result;
        }

        public static string GetTstvAccountSettingsKey(int groupId)
        {
            return string.Format("TstvAccountSettings_groupId_{0}", groupId);
        }

        public static string GetExcelTemplateKey(int groupId, string mediaTypeId, bool shouldGenerateFiles, bool shouldGenerateImages)
        {
            return string.Format("ExcelTemplate_groupId_{0}_mediaType_{1}_WithFiles_{2}_WithImages_{3}", groupId, mediaTypeId, shouldGenerateFiles, shouldGenerateImages);
        }

        #endregion

        #region Invalidation Keys - SHOULD START WITH "invalidationKey..." prefix

        public static string GetUserRolesInvalidationKey(string userId)
        {
            return string.Format("add_role_userId_{0}", userId);
        }

        public static string GetCancelSubscriptionInvalidationKey(long domainId)
        {
            return string.Format("cancel_subscription_domainId_{0}", domainId);
        }

        public static string GetCancelSubscriptionRenewalInvalidationKey(long domainId)
        {
            return string.Format("cancel_subscription_renewal_domainId_{0}", domainId);
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

        public static string GetRemindersInvalidationKey(int groupId, long reminderId)
        {
            return string.Format("invalidationKeyReminder_groupId_{0}_id_{1}", groupId, reminderId);
        }

        public static string GetLastUseWithCreditForDomainInvalidationKey(int groupId, long domainId, int mediaId)
        {
            return string.Format("domainPlayUses_InvalidationKey_groupId_{0}_domainId_{1}_mediaId_{2}", groupId, domainId, mediaId);
        }

        public static string GetGroupChannelsInvalidationKey(int groupId)
        {
            return string.Format("groupChannelsInvalidationKey_groupId_{0}", groupId);
        }

        public static string GetMediaInvalidationKey(int groupId, long mediaId)
        {
            return string.Format("mediaInvalidationKey_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetSeriesRemindersInvalidationKey(int groupId, long seriesReminderId)
        {
            return string.Format("invalidationKeySeriesReminder_groupId_{0}_id_{1}", groupId, seriesReminderId);
        }

        public static string GetGroupDrmAdapterIdInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupDrmAdapter_groupId_{0}", groupId);
        }

        public static string GetDrmAdapterInvalidationKey(int groupId, int adapterId)
        {
            return string.Format("invalidationKeyDrmAdapter_groupId_{0}_adapterId_{1}", groupId, adapterId);
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
            return string.Format("PricingSettings_groupId_{0}", groupId);
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

        public static string GetPriceCodeInvalidationKey(int groupId, int id)
        {
            return string.Format("invalidationKeyPriceCode_id_{0}_groupId_{1}", id, groupId);
        }

        public static string GetGroupUnifiedBillingCycleInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyUnifiedBillingCycle_groupId_{0}", groupId);
        }

        public static string GetHouseholdUnifiedBillingCycleInvalidationKey(int domainID, long renewLifeCycle)
        {
            return string.Format("invalidationKeyUnifiedBillingCycle_domainId_{0}_renewLifeCycle", domainID, renewLifeCycle);
        }

        public static string GetDoesGroupUsesTemplatesCacheInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyDoesGroupUsesTemplates_groupId_{0}", groupId);
        }

        public static string GetCatalogGroupCacheInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyCatalogGroupCache_groupId_{0}", groupId);
        }

        public static string GetGroupDeviceRulesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupDeviceRules_groupId_{0}", groupId);
        }

        public static string GetGroupGeoBlockRulesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupGeoBlockRules_groupId_{0}", groupId);
        }

        public static string GetAssetInvalidationKey(string assetType, long id)
        {
            return string.Format("invalidationKey_Asset_type_{0}_id_{1}", assetType, id);
        }

        public static string GetMediaConcurrencyRulesDeviceLimitationModuleInvalidationKey(int groupId, int dlmId)
        {
            return string.Format("invalidationKey_mediaConcurrencyRules_by_domainLimitationModule_{0}", dlmId);
        }

        public static string GetGroupImageTypesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupImageTypes_groupId_{0}", groupId);
        }

        public static string GetGroupDefaultImagesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupDefaultImages_groupId_{0}", groupId);
        }

        public static string GetGroupRatiosInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupRatios_groupId_{0}", groupId);
        }

        public static string GetGroupMediaFileTypesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupMediaFileTypes_groupId_{0}", groupId);
        }

        public static string GetChannelInvalidationKey(int groupId, int channelId)
        {
            return string.Format("invalidationKey_Channel_groupId_{0}_Id_{1}", groupId, channelId);
        }

        public static Dictionary<string, List<string>> GetChannelsInvalidationKeysMap(int groupId, List<int> channelIds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (channelIds != null && channelIds.Count > 0)
            {
                channelIds = channelIds.Distinct().ToList();
                foreach (int id in channelIds)
                {
                    result.Add(GetChannelKey(groupId, id), new List<string>() { GetChannelInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static string GetCDNAdapterInvalidationKey(int groupId, int adapterId)
        {
            return string.Format("InvalidationKey_cdnAdapter_groupId_{0}_adapterId_{1}", groupId, adapterId);
        }

        public static string GetTstvAccountSettingsInvalidationKey(int groupId)
        {
            return string.Format("InvalidationKey_TstvAccountSettings_groupId_{0}", groupId);
        }

        public static Dictionary<string, List<string>> GetAssetsInvalidationKeysMap(string assetType, List<long> ids)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (ids != null && ids.Count > 0)
            {
                ids = ids.Distinct().ToList();
                foreach (long id in ids)
                {
                    result.Add(GetAssetKey(assetType, id), new List<string>() { GetAssetInvalidationKey(assetType, id) });
                }
            }

            return result;
        }

        #region Domains

        public static string GetHouseholdInvalidationKey(long householdId)
        {
            return string.Format("invalidationKey_domain_{0}", householdId);
        }

        public static string GetHouseholdUserInalidationKey(long householId, string siteGuid)
        {
            return string.Format("invalidationKey_domain_{0}_user_{1}", householId, siteGuid);
        }

        public static string GetRoleIdInvalidationKey(int roleId)
        {
            return string.Format("invalidationKey_roleId_{0}", roleId);
        }

        public static string GetPermissionsRolesIdsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_permissionRoleIds_groupId_{0}", groupId);
        }

        #endregion

        #region Users

        public static string GetUserInvalidationKey(string siteGuid)
        {
            return string.Format("invalidationKey_user_{0}", siteGuid);
        }

        #endregion

        public static string GetGroupParentalRulesInvalidationKey(int groupId)
        {
            return string.Format("InvalidationKey_groupParentalRules_groupId_{0}", groupId);
        }

        #endregion

        #region Invalidation keys functions

        public static List<string> GetDomainEntitlementInvalidationKeys(int domainId)
        {
            return new List<string>()
            {
                GetCancelSubscriptionInvalidationKey(domainId),
                GetCancelTransactionInvalidationKey(domainId),
                GetPurchaseInvalidationKey(domainId),
                GetGrantEntitlementInvalidationKey(domainId),
                GetCancelServiceNowInvalidationKey(domainId),
                GetRenewInvalidationKey(domainId),
                GetCancelSubscriptionRenewalInvalidationKey(domainId)
            };
        }

        // call this when changes on asset may affect your cache
        public static List<string> GetAssetMultipleInvalidationKeys(int groupId, string assetType, long id)
        {
            return new List<string>()
            {
                GetAssetInvalidationKey(assetType, id),
                GetMediaInvalidationKey(groupId, id)
            };
        }

        #endregion


    }
}
