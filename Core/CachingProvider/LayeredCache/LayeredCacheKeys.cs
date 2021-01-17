using System.Collections.Generic;
using System.Linq;

namespace CachingProvider.LayeredCache
{
    public class LayeredCacheKeys
    {
        #region Constant Keys

        public const string GET_CURRENCIES_KEY = "currencies";
        public const string PERMISSION_MANAGER_INVALIDATION_KEY = "invalidationKey_permissionsManager";
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

        public static string GetDomainBundlesKey(int groupId, int domainId)
        {
            return string.Format("domainBundlesV3_groupId_{0}_domainId_{1}", groupId, domainId);
        }

        public static string GetGroupLanguagesCacheKey(int groupId)
        {
            return string.Format("groupLanguages_groupId_{0}", groupId);

        }

        public static string GetPermissionsRolesIdsKey(int groupId)
        {
            return string.Format("getPermissionRoleIds_V1_groupId_{0}", groupId);
        }

        public static string GetGroupPermissionItemsDictionaryKey(int groupId)
        {
            return string.Format("groupPermissionItemsDictionaryKey_groupId_{0}", groupId);
        }

        public static string GetGroupIotClientConfig(int groupId)
        {
            return string.Format("groupIotClientConfig_groupId_{0}", groupId);
        }

        public static string GetPermissionItemsToFeaturesDictionaryKey(int groupId)
        {
            return string.Format("permissionItemsToFeaturesDictionaryKey_groupId_{0}", groupId);
        }

        public static string GetGroupFeaturesKey(int groupId)
        {
            return string.Format("groupFeaturesKey_groupId_{0}", groupId);
        }

        public static string GetGroupPermissionsKey(int groupId)
        {
            return string.Format("groupPermissionsKey_groupId_{0}", groupId);
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
            return string.Format("mediaFiles_mediaId_{0}_assetType_{1}_V1", mediaId, assetType);
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

        public static string GetMediaAssetUserRulesKey(int groupId, long mediaId)
        {
            return string.Format("mediaAssetUserRules_groupId_{0}_mediaId_{1}", groupId, mediaId);
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
            return string.Format("subscriptionSetV2_groupId_{0}_setId_{1}", groupId, setId);
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

        public static string GetLinearMediaRegionsKey(int groupId)
        {
            return string.Format("LinearMediaRegions_groupId_{0}", groupId);
        }

        public static string GetCatalogGroupCacheKey(int groupId)
        {
            return string.Format("CatalogGroupCacheV5_groupId_{0}", groupId);
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
            return string.Format("AssetV4_type_{0}_id_{1}", assetType, id);
        }

        public static string GetAssetWithLanguageKey(string assetType, string id, int languageId)
        {
            return string.Format("Asset_V1_type_{0}_id_{1}_lang_{2}", assetType, id, languageId);
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

        public static string GetPPVModuleKey(long ppvModule, bool shouldShrink = false)
        {
            return string.Format("PPVModule_{0}_shouldShrink_{1}", ppvModule, shouldShrink);
        }

        public static string GetGroupPPVModuleIdsKey(int groupId)
        {
            return string.Format("PPVModules_groupId_{0}", groupId);
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

        public static Dictionary<string, string> GetAssetsWithLanguageKeyMap(string assetType, List<string> assetIds, int languageId)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (assetIds != null && assetIds.Count > 0)
            {
                assetIds = assetIds.Distinct().ToList();
                foreach (string id in assetIds)
                {
                    result.Add(GetAssetWithLanguageKey(assetType, id, languageId), id.ToString());
                }
            }

            return result;
        }

        public static Dictionary<string, string> GetMappedMediaFileKeys(int groupId, List<int> mediaFilesIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (mediaFilesIds != null && mediaFilesIds.Count > 0)
            {
                mediaFilesIds = mediaFilesIds.Distinct().ToList();
                foreach (int mediaFileId in mediaFilesIds)
                {
                    result.Add(GetMappedMediaFileKey(groupId, mediaFileId), mediaFileId.ToString());
                }
            }

            return result;
        }

        public static string GetTstvAccountSettingsKey(int groupId)
        {
            return string.Format("TstvAccountSettingsV2_groupId_{0}", groupId);
        }

        public static string GetIsTstvSettingsExistsKey(int groupId)
        {
            return string.Format("TstvAccountSettingsExists_groupId_{0}", groupId);
        }

        public static string GetDeviceRulesByBrandIdKey(int groupId, int brandId)
        {
            return string.Format("deviceRules_groupId_{0}_brandId_{1}", groupId, brandId);
        }

        public static string GetUserWatchedMediaIdsKey(int userId)
        {
            return string.Format("user_watched_media_ids_user_{0}", userId);
        }

        public static string GetCouponsGroupKey(long couponsGroupId, int groupId)
        {
            return string.Format("coupons_group_id_{0}_groupId_{1}", couponsGroupId, groupId);
        }

        public static string GetCouponsGroupsKey(int groupId)
        {
            return string.Format("coupons_groups_groupId_{0}", groupId);
        }

        public static string GetMediaCountriesKey(long mediaId)
        {
            return string.Format("media_countries_{0}", mediaId);
        }

        public static string GetAllCountryListKey()
        {
            return "allCountryList";
        }

        public static string GetDiscountsKey(int groupId)
        {
            return string.Format("discounts_groupId_{0}", groupId);
        }

        public static string GetAllAssetRulesKey(int groupId, int conditionType, int? actionType)
        {
            if (actionType.HasValue)
            {
                return string.Format("all_asset_rules_groupId_{0}_conditionType_{1}_actionType_{2}", groupId, conditionType, actionType.Value);
            }

            return string.Format("all_asset_rules_groupId_{0}_conditionType_{1}", groupId, conditionType);
        }

        public static string GetAllAssetRulesFromDBKey()
        {
            return string.Format("all_asset_rules_from_db");
        }

        public static string GetAssetRuleKey(long ruleId)
        {
            return string.Format("asset_rules_{0}", ruleId);
        }

        public static string GetAssetRulesByAssetKey(string assetId, int assetType, int assetRuleConditionType)
        {
            return string.Format("asset_rules_by_assetId_{0}_assetType_{1}_conditionType_{2}", assetId, assetType, assetRuleConditionType);
        }

        public static string GetAssetRulesByAssetKey(string assetId, int assetType, int assetRuleConditionType, int assetRuleActionType)
        {
            return string.Format("asset_rules_by_Id_{0}_type_{1}_condition_{2}_action_{3}", assetId, assetType, assetRuleConditionType, assetRuleActionType);
        }

        public static string GetAssetUserRuleIdsGroupKey(int groupId)
        {
            return string.Format("asset_user_rule_ids_groupId_{0}", groupId);
        }

        public static string GetAssetUserRuleKey(long ruleId)
        {
            return string.Format("asset_user_rule_{0}", ruleId);
        }

        public static string GetUserToAssetUserRuleIdsKey(long userId)
        {
            return string.Format("asset_user_rule_ids_userId_{0}", userId);
        }

        public static string GetUserKey(int userId, int groupId)
        {
            return string.Format("group_{0}_user_{1}", groupId, userId);
        }

        public static string GetDomainKey(int domainId)
        {
            return string.Format("domain_V3_{0}", domainId);
        }

        public static string GetDlmKey(int dlmId)
        {
            return string.Format("DLM_{0}", dlmId);
        }

        public static string GroupFeaturesKey(int groupId)
        {
            return string.Format("GroupFeatures_{0}", groupId);
        }

        public static string GetAllPpvsKey(int groupId)
        {
            return string.Format("all_ppvs_groupId_{0}", groupId);
        }

        public static string GetAllBusinessModuleRuleIdsKey(int groupId)
        {
            return string.Format("all_business_module_rules_ids_groupId_{0}", groupId);
        }

        public static string GetBusinessModuleRuleKey(long id)
        {
            return string.Format("business_module_rule_id_{0}", id);
        }

        public static string GetAllRuleActionTypesKey(int groupId)
        {
            return string.Format("all_business_module_rules_action_type_groupId_{0}", groupId);
        }

        public static string GetAssetStatsSortKey(string assetId, string orderBy)
        {
            return string.Format("asset_stats_sort_{0}_{1}", assetId, orderBy);
        }

        public static string GetSSOAdapaterByGroupKey(int groupId)
        {
            return string.Format("sso_adapter_by_group_{0}", groupId);
        }

        public static string GetDeviceReferenceDataByGroupKey(int groupId)
        {
            return string.Format("device_reference_data_by_group_V1_{0}", groupId);
        }

        public static string GetSmsAdapaterByGroupKey(int groupId)
        {
            return string.Format("sms_adapter_by_group_{0}", groupId);
        }

        public static string GetSSOAdapaterImplementationsKey(int adapterId)
        {
            return string.Format("sso_adapter_implementations_v1_{0}", adapterId);
        }

        public static string GetQueryCacheDefinitionsKey()
        {
            return "query_cache";
        }

        public static string GetDbProceduresRoutingKey()
        {
            return "db_procedures_routing";
        }

        public static string GetDbQueryRoutingKey()
        {
            return "db_query_routing";
        }

        public static string GetGroupWatchPermissionRulesKey(int groupId)
        {
            return string.Format("GroupWatchPermissionRules_{0}", groupId);
        }

        public static string GetGroupPlaybackProfileKey(int groupId)
        {
            return string.Format("GroupPlaybackProfile_{0}", groupId);
        }

        public static string GetMediaFileTypeByIdKey(int groupId, int fileId)
        {
            return string.Format("MediaFileTypeID_{0}_group_{1}", fileId, groupId);
        }

        public static string GetMappedMediaFileKey(int groupId, int fileId)
        {
            return string.Format("MapMediaFileId_{0}_group_{1}", fileId, groupId);
        }

        public static string GetDomainRecordingsKey(long domainId)
        {
            return string.Format("DomainRecordings_domainId_{0}", domainId);
        }

        public static string GetMediaStatsKey(int assetId)
        {
            return string.Format("MediaStats_{0}", assetId);
        }

        public static string GetEPGStatsKey(int assetId)
        {
            return string.Format("EPGStats_{0}", assetId);
        }

        public static string GetBusinessModuleRulesRulesByMediaKey(long mediaId)
        {
            return string.Format("business_module_rules_by_mediaId_{0}", mediaId);
        }

        public static string GetRegionsKey(int groupId)
        {
            return string.Format("Regions_V1_{0}", groupId);
        }

        public static string GetUserRolesToPasswordPolicyKey(int groupId)
        {
            return string.Format("user_roles_to_password_policy_{0}", groupId);
        }

        public static string GetPasswordPolicyKey(long roleId)
        {
            return string.Format("password_policy_roleId_{0}", roleId);
        }

        public static string GetGeneralPartnerConfig(int groupId)
        {
            return string.Format("general_partner_configV1_{0}", groupId);
        }

        public static string GetObjectVirtualAssetPartnerConfig(int groupId)
        {
            return string.Format("object_virtual_asset_config_{0}", groupId);
        }

        public static string GetGroupSegmentationTypesKey(int groupId)
        {
            return string.Format("group_segmentation_types_{0}", groupId);
        }

        public static string GetSegmentationTypeKey(int groupId, long segmentationTypeId)
        {
            return string.Format("segmentation_type_{0}_{1}", groupId, segmentationTypeId);
        }

        public static string GetGroupSegmentationTypeIdsOfActionKey(int groupId, string cacheKey)
        {
            return string.Format("group_segmentation_types_of_action_{0}_{1}", groupId, cacheKey);
        }

        public static string GetCommercePartnerConfigKey(int groupId)
        {
            return string.Format("commerce_partner_config_{0}", groupId);
        }

        public static string GetSecurityPartnerConfigKey(int groupId)
        {
            return $"security_partner_config_{groupId}";
        }
        public static string GetGroupCategoriesKey(int groupId)
        {
            return $"groupCategoriesKey_groupId_{groupId}";
        }

        public static string GetCategoryItemKey(int groupId, long id)
        {
            return $"categoryItem_{groupId}_{id}";
        }

        public static string GetPlaybackPartnerConfigKey(int groupId)
        {
            return string.Format("playback_partner_config_{0}", groupId);
        }

        public static string GetPaymentPartnerConfigKey(int groupId)
        {
            return string.Format("payment_partner_config_{0}", groupId);
        }

        public static string GetCatalogPartnerConfigKey(int groupId)
        {
            return string.Format("catalog_partner_config_{0}", groupId);
        }

        public static string GetGroupCampaignKey(int groupId, int campaignType)
        {
            return $"group_campaign_{groupId}_type_{campaignType}";
        }

        public static string GetCampaignKey(int groupId, long campaignId)
        {
            return $"group_campaign_{groupId}_id_{campaignId}";
        }

        public static string GetDynamicListKey(int groupId, long id)
        {
            return $"group_DynamicList_{groupId}_id_{id}";
        }

        public static string GetDynamicListGroupMappingKey(int groupId, int type)
        {
            return $"group_DynamicList_Mapping_{groupId}_type_{type}";
        }

        public static Dictionary<string, string> GetExternalChannelsKeysMap(int groupId, List<int> channelIds)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            if (channelIds != null && channelIds.Count > 0)
            {
                channelIds = channelIds.Distinct().ToList();
                foreach (int id in channelIds)
                {
                    result.Add(GetExternalChannelKey(groupId, id), id.ToString());
                }
            }

            return result;
        }

        public static string GetExternalChannelKey(int groupId, int channelId)
        {
            return $"ExternalChannel_groupId_{groupId}_Id_{channelId}";
        }

        #endregion

        #region Invalidation Keys - SHOULD START WITH "invalidationKey..." prefix

        public static string GetDomainEntitlementInvalidationKey(int groupId, long domainId)
        {
            return string.Format("invalidationKey_domainEntitlements_groupId_{0}_domainId_{1}", groupId, domainId);
        }

        public static string GetUserRolesInvalidationKey(int partnerId, string userId)
        {
            return string.Format($"{partnerId}_InvalidateUserRoles_{userId}");
        }

        // This key was added to user Get \ initilize method because when its connected to authentication miscroservice
        // it will be invalidated when microservice invalidates the user login history recotd
        public static string GetUserLoginHistoryInvalidationKey(int partnerId, int userId)
        {
            return $"{partnerId}_InvalidateUserLoginHistory_{userId}";
        }

        public static string GetSubscriptionSuspendInvalidationKey(int groupId, long domainId)
        {
            return string.Format("subscription_suspend_groupId_{0}_domainId_{1}", groupId, domainId);
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
            return string.Format("invalidationKeyPriceCode_groupId_{0}_id_{1}", groupId, id);
        }

        public static string GetGroupUnifiedBillingCycleInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyUnifiedBillingCycle_groupId_{0}", groupId);
        }

        public static string GetHouseholdUnifiedBillingCycleInvalidationKey(int groupId, int domainID, long renewLifeCycle)
        {
            return string.Format("invalidationKeyUBCycle_groupId_{0}_domainId_{1}_RLCycle_{2}", groupId, domainID, renewLifeCycle);
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

        public static string GetAssetInvalidationKey(int groupId, string assetType, long id)
        {
            return string.Format("invalidationKey_groupId_{0}_AssetType_{1}_id_{2}", groupId, assetType, id);
        }

        public static string GetMediaConcurrencyRulesDeviceLimitationModuleInvalidationKey(int groupId, int dlmId)
        {
            return string.Format("invalidationKey_groupId_{0}_MCRules_by_DLimitationModule_{1}", groupId, dlmId);
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

        public static string GetSmsAdapaterInvalidationKey(int groupId)
        {
            return $"InvalidationKey_smsAdapter_groupId_{groupId}";
        }

        public static Dictionary<string, List<string>> GetAssetsInvalidationKeysMap(int groupId, string assetType, List<long> ids)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (ids != null && ids.Count > 0)
            {
                ids = ids.Distinct().ToList();
                foreach (long id in ids)
                {
                    result.Add(GetAssetKey(assetType, id), new List<string>() { GetAssetInvalidationKey(groupId, assetType, id) });
                }
            }

            return result;
        }

        public static Dictionary<string, List<string>> GetMediaInvalidationKeysMap(int groupId, string assetType, List<long> ids, int languageId)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (ids != null && ids.Count > 0)
            {
                ids = ids.Distinct().ToList();
                foreach (long id in ids)
                {
                    result.Add(GetAssetWithLanguageKey(assetType, id.ToString(), languageId), new List<string>() { GetMediaInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static Dictionary<string, List<string>> GetEpgInvalidationKeysMap(int groupId, string assetType, List<long> ids, int languageId)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (ids != null && ids.Count > 0)
            {
                ids = ids.Distinct().ToList();
                foreach (long id in ids)
                {
                    result.Add(GetAssetWithLanguageKey(assetType, id.ToString(), languageId), new List<string>() { GetEpgInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        public static string GetUserParentalRuleInvalidationKey(int groupId, string siteGuid)
        {
            return string.Format("invalidationKey_groupId_{0}_UParentalRules_{1}", groupId, siteGuid);
        }

        public static string GetCouponsGroupInvalidationKey(int groupId, long couponsGroup)
        {
            return string.Format("invalidationKey_groupId_{0}_couponsGroupId_{1}", groupId, couponsGroup);
        }

        public static string GetCouponsGroupsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyCouponGroups_groupId_{0}", groupId);
        }

        public static string GetAllAssetRulesInvalidationKey()
        {
            return string.Format("invalidationKey_all_asset_rules");
        }

        public static string GetAllAssetRulesGroupInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_all_asset_rules_groupId_{0}", groupId);
        }

        public static string GetAssetRuleInvalidationKey(int groupId, long ruleId)
        {
            return string.Format("invalidationKey_groupId_{0}_AssetRuleId_{1}", groupId, ruleId);
        }

        public static string GetAssetUserRuleIdsGroupInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_asset_user_rule_ids_groupId_{0}", groupId);
        }

        public static string GetAssetUserRuleInvalidationKey(int groupId, long ruleId)
        {
            return string.Format("invalidationKey_groupId_{0}_assetUserRuleId_{1}", groupId, ruleId);
        }

        public static string GetUserToAssetUserRuleIdsInvalidationKey(int groupId, long userId)
        {
            return string.Format("invalidationKey_groupId_{0}_UserToUserAssetRuleIds_{1}", groupId, userId);
        }

        public static string GetGroupDiscountsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyDiscounts_groupId_{0}", groupId);
        }

        public static string GetEpgInvalidationKey(int groupId, long epgId)
        {
            return string.Format("invalidationKeyEpg_groupId_{0}_epgId_{1}", groupId, epgId);
        }

        public static string GetDeviceConcurrencyPriorityKey(int groupId)
        {
            return string.Format("device_concurrency_Priority_groupId_{0}", groupId);
        }

        public static string GetDeviceConcurrencyPriorityInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_device_concurrency_Priority_groupId_{0}", groupId);
        }

        public static string GetAllLinearMediaKey(int groupId)
        {
            return string.Format("all_linear_media_groupId_{0}", groupId);
        }

        public static string GetAdjacentProgramsKey(int groupId, string epgChannelId)
        {
            return string.Format("adjacentProgramsV1_groupId_{0}_epgChannelId_{1}", groupId, epgChannelId);
        }

        public static string GetAdjacentProgramsInvalidationKey(int groupId, string epgChannelId)
        {
            return string.Format("invalidationKey_adjacent_programs_groupId_{0}_epgChannelId_{1}", groupId, epgChannelId);
        }

        public static string GetGroupParentalRulesInvalidationKey(int groupId)
        {
            return string.Format("InvalidationKey_groupParentalRules_groupId_{0}", groupId);
        }

        public static string GetDlmInvalidationKey(int groupId, int dlmId)
        {
            return string.Format("invalidationKey_groupId_{0}_dlmId_{1}", groupId, dlmId);
        }

        public static string GetGroupFeatureInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupFeatures_{0}", groupId);
        }

        public static string GetAllBusinessModuleRulesGroupInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_all_business_module_rules_groupId_{0}", groupId);
        }

        public static string GetBusinessModuleRuleInvalidationKey(int groupId, long ruleId)
        {
            return string.Format("invalidationKey_groupId_{0}_business_module_rule_{1}", groupId, ruleId);
        }

        public static string GetSSOAdapaterInvalidationKey(int partnerId)
        {
            return $"{partnerId}_InvalidatePartnerSSOAdapterProfiles";
        }

        public static string GetDeviceReferenceDataInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_device_reference_data_{0}", groupId);
        }

        public static string GroupManagerGetGroupInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_group_manager_get_group_{0}", groupId);
        }

        public static string PhoenixGroupsManagerInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_PhoenixGroupsManager_{0}", groupId);
        }

        public static string GetQueryCacheInvalidationKey()
        {
            return "invalidationKey_query_cache";
        }

        public static string GetProceduresRoutingInvalidationKey()
        {
            return "invalidationKey_procedures_routing";
        }

        public static string GetQueriesRoutingInvalidationKey()
        {
            return "invalidationKey_queries_routing";
        }

        public static string GetGroupWatchPermissionRulesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupWatchPermissionRules_{0}", groupId);
        }

        public static string GetGroupPlaybackProfilesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_GroupPlaybackProfiles_{0}", groupId);
        }

        public static string GetMediaFileTypeByIdInvalidationKey(int groupId, int fileId)
        {
            return string.Format("invalidationKey_groupId_{0}_MediaFileTypeID_{1}", groupId, fileId);
        }

        public static string GetAllEpgPicturesKey(int groupId)
        {
            return string.Format("all_epg_pictures_groupId_{0}", groupId);
        }

        public static string GetDomainRecordingsInvalidationKeys(int groupId, long domainId)
        {
            return string.Format("invalidationKeyDomainRecordings_groupId_{0}_DomainId_{1}", groupId, domainId);
        }

        public static string GetBulkUploadsKey(int groupId, string bulkObjectType, int status)
        {
            return string.Format("bulk_uploads_groupId_{0}_bulkObjectType_{1}_status_{2}", groupId, bulkObjectType, status);
        }

        public static string GetBulkUploadsInvalidationKey(int groupId, string bulkObjectType, int status)
        {
            return string.Format("invalidationKey_{0}", GetBulkUploadsKey(groupId, bulkObjectType, status));
        }

        public static string GetTopicNotificationsKey(int groupId, int SubscribeReferenceType)
        {
            return string.Format("TopicNotifications_groupId_{0}_type_{1}", groupId, SubscribeReferenceType);
        }

        public static string GetTopicNotificationsInvalidationKey(int groupId, int SubscribeReferenceType)
        {
            return string.Format("invalidationKeyTopicNotifications_groupId_{0}_type_{1}", groupId, SubscribeReferenceType);
        }

        public static string GetRegionsInvalidationKey(int groupId)
        {
            return string.Format("invalidationKey_Regions_{0}", groupId);
        }

        public static string PermissionsManagerInvalidationKey()
        {
            return PERMISSION_MANAGER_INVALIDATION_KEY;
        }

        public static string GetUserRolesToPasswordPolicyInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyUserRolesToPassPolicy_groupId_{0}", groupId);
        }

        public static string GetPasswordPolicyInvalidationKey(int groupId, long roleId)
        {
            return string.Format("invalidationKeyPassPolicy_groupId_{0}_roleId_{1}", groupId, roleId);
        }

        public static string GetGeneralPartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGeneralPartnerConfig_groupdId_{0}", groupId);
        }

        public static string GetObjectVirtualAssetPartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyObjectVirtualAssetPartnerConfig_groupId_{0}", groupId);
        }

        public static string GetGroupSegmentationTypesInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupSegmentationTypes_groupId_{0}", groupId);
        }


        // todo: arthur map to kafka ...
        public static string GetSegmentationTypeInvalidationKey(int groupId, long segmentationTypeId)
        {
            return string.Format("invalidationKeySegmentationType_groupdId_{0}_segId_{1}", groupId, segmentationTypeId);
        }

        public static string GetGroupSegmentationTypeIdsOfActionInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupSegmentationTypeOfAction_groupId_{0}", groupId);
        }

        public static string GetCommercePartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyCommercePartnerConfig_groupId_{0}", groupId);
        }

        public static string GetSecurityPartnerConfigInvalidationKey(int groupId)
        {
            return $"{groupId}_InvalidatePartnerSecurityConfiguration";
        }

        public static string GetPlaybackPartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyPlaybackPartnerConfig_groupId_{0}", groupId);
        }

        public static string GetPaymentPartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyPaymentPartnerConfig_groupId_{0}", groupId);
        }

        public static string GetDomainDeviceInvalidationKey(int groupId, int domainId, string deviceId)
        {
            return $"{groupId}_InvalidateHouseholdDevice_{domainId}_{deviceId}";
        }

        public static string GetCatalogPartnerConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyCatalogPartnerConfig_groupId_{0}", groupId);
        }

        public static string GetGroupIotClientConfigInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupIotClientConfig_groupId_{0}", groupId);
        }

        public static string GetGroupCampaignInvalidationKey(int groupId, int type)
        {
            return $"invalidationKeyGroupCampaign_groupId_{groupId}_type_{type}";
        }

        public static string GetCampaignInvalidationKey(int groupId, long campaignId)
        {
            return $"invalidationKeyGroupCampaign_groupId_{groupId}_id_{campaignId}";
        }

        public static string GetDynamicListInvalidationKey(int groupId, long id)
        {
            return $"invalidationKeyGroupDynamicList_groupId_{groupId}_id_{id}";
        }

        public static string GetDynamicListGroupMappingInvalidationKey(int groupId, int type)
        {
            return $"invalidationKeygroupDynamicListMapping_groupId_{groupId}_type_{type}";
        }

        public static string GetHouseholdInvalidationKey(int groupId, long householdId)
        {
            return string.Format("invalidationKeyDomain_groupId_{0}_domainId_{1}", groupId, householdId);
        }

        public static string GetHouseholdUserInalidationKey(int groupId, long householId, string siteGuid)
        {
            return string.Format("invalidationKeyDomainUser_groupId_{0}_domainId_{1}_userId_{2}",groupId, householId, siteGuid);
        }

        public static string GetRoleIdInvalidationKey(int groupId, int roleId)
        {
            return string.Format("invalidationKeyRole_groupId_{0}_roleId_{1}", groupId, roleId);
        }

        public static string GetPermissionsRolesIdsInvalidationKey(int partnerId)
        {
            return string.Format($"{partnerId}_InvalidatePartnerRoles");
        }

        public static string GetGroupPermissionItemsDictionaryInvalidationKey(int groupId)
        {
            return string.Format("invalidationKeyGroupPermissionItemsDictionaryKey_groupId_{0}", groupId);
        }

        public static string GetAssetStatsSortInvalidationKey()
        {
            return "invalidation_key_asset_stats_sort";
        }

        public static string GetGroupCategoriesInvalidationKey(int groupId)
        {
            return $"invalidationKeyGroupCategories_groupId_{groupId}";
        }

        public static string GetCategoryIdInvalidationKey(int groupId, long categoryId)
        {
            return $"invalidationKeyCategory_groupId_{groupId}_Id_{categoryId}";
        }

        public static string GetUserInvalidationKey(int partnerId, string userId)
        {
            return string.Format($"{partnerId}_InvalidateOTTUser_{userId}");
        }

        public static string GetUserWatchedMediaIdsInvalidationKey(int groupId, int userId)
        {
            return string.Format("invalidationkeyUserWatchedMediaIds_groupId_{0}_userId_{1}", groupId, userId);
        }

        public static string GetMediaCountriesInvalidationKey(int groupId, long mediaId)
        {
            return string.Format("invalidationkeyMediaCountries_groupId_{0}_mediaId_{1}", groupId, mediaId);
        }

        public static string GetExternalChannelInvalidationKey(int groupId, int channelId)
        {
            return $"invalidationKeyExternalChannel_groupId_{groupId}_Id_{channelId}";
        }

        public static Dictionary<string, List<string>> GetExternalChannelsInvalidationKeysMap(int groupId, List<int> channelIds)
        {
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
            if (channelIds != null && channelIds.Count > 0)
            {
                channelIds = channelIds.Distinct().ToList();
                foreach (int id in channelIds)
                {
                    result.Add(GetExternalChannelKey(groupId, id), new List<string>() { GetExternalChannelInvalidationKey(groupId, id) });
                }
            }

            return result;
        }

        #endregion

        #region Invalidation keys functions

        public static List<string> GetDomainBundlesInvalidationKeys(int groupId, int domainId)
        {
            return new List<string>() { GetSubscriptionSuspendInvalidationKey(groupId, domainId), GetDomainEntitlementInvalidationKey(groupId, domainId) };
        }

        // call this when changes on asset may affect your cache
        public static List<string> GetAssetMultipleInvalidationKeys(int groupId, string assetType, long id)
        {
            return new List<string>()
            {
                GetAssetInvalidationKey(groupId, assetType, id),
                GetMediaInvalidationKey(groupId, id)
            };
        }

        #endregion

        public static string GetAllLanguageListKey()
        {
            return "allLanguageList";
        }

    }
}