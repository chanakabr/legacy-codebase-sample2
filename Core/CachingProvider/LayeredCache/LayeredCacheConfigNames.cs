namespace CachingProvider.LayeredCache
{
    public class LayeredCacheConfigNames
    {
        #region Constant Config Names

        public const string MEDIA_FILE_ID_BY_CO_GUID_LAYERED_CACHE_CONFIG_NAME = "GetMediaFileIDByCoGuid";
        public const string VALIDATE_MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME = "ValidateMediaFiles";
        public const string GET_MEDIA_ID_GROUP_FILE_MAPPER_LAYERED_CACHE_CONFIG_NAME = "MediaIdGroupFileTypeMapper";
        public const string GET_DOMAIN_ENTITLEMENTS_LAYERED_CACHE_CONFIG_NAME = "TryGetDomainEntitlementsFromCache";
        public const string MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME = "GetMediaFiles";
        public const string MEDIA_ID_FOR_ASSET_LAYERED_CACHE_CONFIG_NAME = "GetMediaIdForAsset";
        public const string GET_RECORDING_PLAYBACK_SETTINGS_LAYERED_CACHE_CONFIG_NAME = "GetRecordingPlaybackSettingsByLinearMediaId";
        public const string COUNTRY_BY_IP_LAYERED_CACHE_CONFIG_NAME = "GetCountryByIp";
        public const string COUNTRY_BY_COUNTRY_NAME_LAYERED_CACHE_CONFIG_NAME = "GetCountryByCountryName";
        public const string CHECK_GEO_BLOCK_MEDIA_LAYERED_CACHE_CONFIG_NAME = "CheckGeoBlockMedia";
        public const string MEDIA_USER_TYPE_LAYERED_CACHE_CONFIG_NAME = "CheckMediaUserType";
        public const string MEDIA_CONCURRENCY_RULES_LAYERED_CACHE_CONFIG_NAME = "GetMediaConcurrencyRules";
        public const string MEDIA_CONCURRENCY_RULES_BY_LIMITATION_MODULE_CACHE_CONFIG_NAME = "GetMediaConcurrencyRulesByDomainLimitionModule";
        public const string GROUP_CDN_SETTINGS_LAYERED_CACHE_CONFIG_NAME = "GroupCDNSettings";
        public const string CDN_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "CdnAdapter";
        public const string GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "GroupParentalRules";
        public const string USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "UserParentalRules";
        public const string MEDIA_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "MediaParentalRules";
        public const string MEDIA_ASSET_USER_RULES_LAYERED_CACHE_CONFIG_NAME = "GetMediaAssetUserRules";
        public const string EPG_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "EpgParentalRules";
        public const string CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME = "GetMediaChannels";
        public const string FILE_CDN_DATA_LAYERED_CACHE_CONFIG_NAME = "GetFileCdnData";
        public const string PRICE_CODE_LOCALE_LAYERED_CACHE_CONFIG_NAME = "GetPriceCodeByCountyAndCurrency";
        public const string GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME = "GetAllCurrencies";
        public const string GET_DEFAULT_GROUP_CURRENCY_LAYERED_CACHE_CONFIG_NAME = "GetGroupDefaultCurrency";
        public const string GET_GROUP_ADS_CONTROL_CACHE_CONFIG_NAME = "GetGroupAdsControl";
        public const string GET_EPG_CHANNEL_CDVR_ID = "GetEpgChannelCdvrId";
        public const string MEDIA_FILES_BY_MEDIA_ID_LAYERED_CACHE_CONFIG_NAME = "GetMediaFilesByMediaId";
        public const string IS_PROXY_BLOCKED_FOR_IP_LAYERED_CACHE_CONFIG_NAME = "IsProxyAllowed";
        public const string GET_SERIES_REMINDERS_CACHE_CONFIG_NAME = "GetSeriesReminders";
        public const string GET_ALIAS_MAPPING_FIELDS_CACHE_CONFIG_NAME = "GetAliasMappingFields";
        public const string GET_REMINDERS_CACHE_CONFIG_NAME = "GetReminders";
        public const string GET_ANNOUNCEMENTS_LAYERED_CACHE_CONFIG_NAME = "TryGetAnnouncements";
        public const string GET_SUBSCRIPTION_SETS_CACHE_CONFIG_NAME = "GetSubscriptionSets";
        public const string GET_GROUP_PRICE_CODES_LAYERED_CACHE_CONFIG_NAME = "GetGroupPriceCodes";
        public const string GET_GROUP_UNIFIED_BILLING_CYCLE = "GetGroupUnifiedBillingCycle";        
        public const string GET_ROLES_BY_GROUP_ID = "GetRolesByGroupId";
        public const string GET_GROUP_PERMISSION_ITEMS_BY_GROUP_ID = "GetGroupPermissionItemsDictionary";
        public const string GET_PERMISSION_ITEMS_TO_FEATURES = "GetPermissionItemsToFeaturesDictionary";
        public const string GET_GROUP_FEATURES = "GetGroupFeatures";
        public const string DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "DrmAdapter";
        public const string GROUP_DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "GroupDrmAdapter";
        public const string DOES_GROUP_USES_TEMPLATES_CACHE_CONFIG_NAME = "DoesGroupUsesTemplates";
        public const string GET_CATALOG_GROUP_CACHE_CONFIG_NAME = "GetCatalogGroupCache";
        public const string GET_LINEAR_MEDIA_REGIONS_NAME_CACHE_CONFIG_NAME = "GetLinearMediaRegions";
        public const string GET_GROUP_DEVICE_RULES_CACHE_CONFIG_NAME = "GetGroupDeviceRules";
        public const string GET_GROUP_GEO_BLOCK_RULES_CACHE_CONFIG_NAME = "GetGroupGeoBlockRules";        
        public const string GET_ASSETS_LIST_CACHE_CONFIG_NAME = "GetAssets";
        public const string GET_ASSETS_WITH_LANGUAGE_LIST_CACHE_CONFIG_NAME = "GetAssetsWithLanguage";
        public const string GET_IMAGE_TYPE_CACHE_CONFIG_NAME = "GetImageType";
        public const string GET_RATIOS_CACHE_CONFIG_NAME = "GetRatios";
        public const string GET_MEDIA_FILE_TYPES_CONFIG_NAME = "GetMediaFileTypess";
        public const string GET_GROUP_DEFAULT_IMAGES_CACHE_CONFIG_NAME = "GetGroupDefaultImages";
        public const string GET_CHANNELS_CACHE_CONFIG_NAME = "GetChannels";
        public const string GET_TSTV_ACCOUNT_SETTINGS_CACHE_CONFIG_NAME = "GetTimeShiftedTvPartnerSettings";
        public const string GET_DEVICE_RULES_BY_BRAND_ID_CACHE_CONFIG_NAME = "GetDeviceRulesByBrandId";
        public const string GET_USER_WATCHED_MEDIA_IDS_LAYERED_CACHE_CONFIG_NAME = "GetUserWatchedMediaIds";
        public const string UNIFIED_SEARCH_WITH_PERSONAL_DATA = "UnifiedSearchWithPersonalData";
        public const string GET_COUPONS_GROUP = "GetCouponsGroup";
        public const string GET_MEDIA_COUNTRIES = "GetMediaCountries";
        public const string GET_ALL_ASSET_RULES = "GetAllAssetRules";
        public const string GET_ALL_ASSET_RULES_FROM_DB = "GetAllAssetRulesDB";
        public const string GET_ASSET_RULES_BY_ASSET = "GetAssetRulesByAsset";
        public const string GET_ALL_COUNTRY_LIST_LAYERED_CACHE_CONFIG_NAME = "GetAllCountryList";
        public const string GET_ASSET_USER_RULE_IDS_BY_GROUP = "GetAssetUserRuleIdsByGroupDB";
        public const string GET_ASSET_USER_RULE = "GetAssetUserRulesCB";
        public const string GET_USER_TO_ASSET_USER_RULE_IDS = "GetUserToAssetUserRuleIdsDB";
        public const string GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME = "GetGroupDiscounts";
        public const string GET_GROUP_PRICE_PLAN_LAYERED_CACHE_CONFIG_NAME = "GetPricePlan";
        public const string GET_GROUP_USAGE_MODULE_LAYERED_CACHE_CONFIG_NAME = "GetUsageModule";
        public const string GET_DEVICE_CONCURRENCY_PRIORITY_FROM_CB = "GetDeviceConcurrencyPriorityCB";
        public const string GET_ALL_LINEAR_MEDIA = "GetAllLinearMedia";
        public const string GET_ADJACENT_PROGRAMS = "GetAdjacentPrograms";
        public const string USER_LAYERED_CACHE_CONFIG_NAME = "GetUser";
        public const string DOMAIN_LAYERED_CACHE_CONFIG_NAME = "GetDomain";
        public const string DLM_LAYERED_CACHE_CONFIG_NAME = "GetDlm";
        public const string GROUP_FEATURES_LAYERED_CACHE_CONFIG_NAME = "GetGroupFeatureStatus";
        public const string GET_DOMAIN_BUNDLES_LAYERED_CACHE_CONFIG_NAME = "GetDomainBundles";
        public const string GET_PPV_FOR_FILE = "GetPPVModuleForMediaFile";
        public const string PPV_MODULES_CACHE_CONFIG_NAME = "PPVModules";
        public const string PPV_MODULE_CACHE_CONFIG_NAME = "PPVModule";
        public const string GET_BUSINESS_MODULE_RULE = "GetBusinessModuleRule";
        public const string GET_ALL_BUSINESS_MODULE_RULE_IDS = "GetAllBusinessModuleRuleIds";
        public const string GET_ALL_BUSINESS_MODULE_RULE_ACTION_TYPES = "GetAllBusinessModuleRuleActionTypes";
        public const string GET_SSO_ADAPATER_BY_GROUP_ID_CACHE_CONFIG_NAME = "GetSSOAdapaterByGroupId";
        public const string GROUP_MANAGER_GET_GROUP_CONFIG_NAME = "GroupManagerGetGroup";
        public const string PHOENIX_GROUPS_MANAGER_CACHE_CONFIG_NAME = "PhoenixGroupsManager";
        public const string QUERY_CACHE_CONFIG_NAME = "QueryCache";
        public const string PROCEDURES_ROUTING_CONFIG_NAME = "ProceduresRouting";
        public const string QUERIES_ROUTING_CONFIG_NAME = "QueriesRouting";
        public const string GROUP_WATCH_PERMISSION_RULES_LAYERED_CACHE_CONFIG_NAME = "GetGroupPermittedWatchRules";
        public const string GROUP_PLAYBACK_PROFILES_LAYERED_CACHE_CONFIG_NAME = "GetGroupPlaybackProfiles";
        public const string GET_EPG_ASSETS_CACHE_CONFIG_NAME = "GetEpgAssets";        
        public const string API_GET_MEDIA_FILE_TYPE = "GetMediaFileTypeID";
        public const string API_GET_MAPPED_MEDIA_FILES = "GetMappedMediaFiles";
        public const string GET_ALL_EPG_PICTURES = "GetAllEpgPictures";
        public const string GET_DOMAIN_RECORDINGS_LAYERED_CACHE_CONFIG_NAME = "GetDomainRecordingsLayeredCacheConfigName";
        public const string GET_ALL_LANGUAGE_LIST_LAYERED_CACHE_CONFIG_NAME = "GetAllLanguageList";
        public const string ASSET_STATS_CONFIG_NAME = "AssetStats";
        public const string ASSET_STATS_SORT_CONFIG_NAME = "AssetStatsSort";
        public const string GET_BULK_UPLOADS_FROM_CACHE = "GetBulkUploadsFromCache";
        public const string GET_GROUP_LANGUAGES = "GetGroupLanguages";
        public const string GET_TOPIC_NOTIFICATIONS_LAYERED_CACHE_CONFIG_NAME = "TryGetTopicNotifications";
        public const string GET_USER_ROLES_TO_PASSWORD_POLICY = "GetUserRolesToPasswordPolicy";
        public const string GET_GROUP_REGIONS = "GetGroupRegions";
        public const string GET_GENERAL_PARTNER_CONFIG = "GetGeneralPartnerConfig";
        public const string GET_GROUP_PROGRAM_ASSET_GROUP_OFFERS = "GetProgramAssetGroupOffersIds";
        public const string GET_GROUP_COLLECTIONS = "GetCollectionsIds";
        public const string GET_GROUP_SUBSCRIPTION = "GetSubscriptionIds";
        public const string GET_OBJECT_VIRTUAL_ASSET_PARTNER_CONFIG = "GetObjectVirtualAssetPartnerConfig";
        public const string GET_GROUP_SEGMENTATION_TYPES = "GetGroupSegmentationTypes";
        public const string GET_SEGMENTATION_TYPE = "GetSegmentationType";
        public const string GET_GROUP_SEGMENTATION_TYPES_OF_ACTION = "GetGroupSegmentationTypesOfAction";
        public const string GET_COMMERCE_PARTNER_CONFIG = "GetCommercePartnerConfigDB";
        public const string GET_SECURITY_PARTNER_CONFIG = "GetSecurityPartnerConfig";
        public const string GET_GROUP_CATEGORIES = "GetGroupCategories";
        public const string GET_CATEGORY_ITEM = "GetCategoryItem";
        public const string GET_IOT_CLIENT_CONFIGURATION = "GetIotClientConfiguration";
        public const string GET_PLAYBACK_PARTNER_CONFIG = "GetPlaybackPartnerConfigDB";
        public const string GET_PAYMENT_PARTNER_CONFIG = "GetPaymentPartnerConfigDB";
        public const string GET_DEVICE_REFERENCE_DATA = "GetDeviceReferenceData";
        public const string GET_SMS_ADAPATER_BY_GROUP_ID_CACHE_CONFIG_NAME = "GetSMSAdapaterByGroupId";
        public const string GET_GROUP_PERMISSIONS = "GetGroupPermissions";
        public const string GET_CATALOG_PARTNER_CONFIG = "GetCatalogPartnerConfigDB";
        public const string GET_DEFAULT_PARENTAL_SETTINGS_PARTNER_CONFIG = "GetDefaultParentalSettingsPartnerConfigDB";
        public const string LIST_CAMPAIGNS_BY_GROUP_ID = "ListCampaignsByGroupIdDB";
        public const string GET_CAMPAIGN_BY_ID = "Get_CampaignsByIdDB";
        public const string GET_DYNAMIC_LIST_BY_ID = "Get_DynamicListByIdDB";
        public const string GET_DYNAMIC_LIST_MAP = "Get_DynamicListMap";
        public const string BUILD_CATEGORY_VERSION = "BuildCategoryVersion";
        public const string GET_CATEGORY_VERSION_DEFAULTS = "GetCategoryVersionDefaults";
        public const string GET_CATEGORY_VERSIONS_OF_TREE = "GetCategoryVersionsOfTree";
        public const string GET_KS_VALIDATION = "Get_KsValidation";
        public const string GET_OPC_PARTNER_CONFIG = "GetOpcPartnerConfig";
        public const string GET_DOMAIN_SUBSCRIPTION_PURCHASE = "GetDomainSubscriptionPurchase";
        public const string GET_GROUP_PREVIEW_MODULES = "GetGroupPreviewModules";
        public const string GET_LABELS_CACHE_CONFIG_NAME = "GetLabels";
        public const string GET_USER_SESSION_PROFILES = "GetUserSessionProfiles";
        public const string GET_SESSION_CHARACTERISTIC = "GetSessionCharacteristic";
        public const string GET_GROUP_SERVICES_LAYERED_CACHE_CONFIG_NAME = "GetGroupPremiumServices";
        public const string GET_SERVICES_LAYERED_CACHE_CONFIG_NAME = "GetAllPremiumServices";
        public const string GET_SUBSCRIPTIONS = "GetSubscriptions";
        public const string GET_GROUP_SUBSCRIPTION_ITEMS = "GetGroupSubscriptionItems";
        public const string GET_CUSTOM_FIELDS_PARTNER_CONFIG = "GetCustomFieldsPartnerConfigDB";
        public const string GET_GROUP_USING_ALIAS_NAMES = "GetGroupUsingAliasNames";
        public const string LIST_SEARCH_PRIORITY_GROUPS_MAPPINGS = "ListSearchPriorityGroupsMappings";
        public const string GET_DEVICE_FAMILIES_CACHE_CONFIG_NAME = "GetDeviceFamilies";
        public const string GET_DEVICE_BRANDS_CACHE_CONFIG_NAME = "GetDeviceBrands";
        public const string GET_LIVE_TO_VOD_FULL_CONFIGURATION_CACHE_CONFIG_NAME = "GetLiveToVodFullConfiguration";

        // DON'T you ever change the value of this static var even when its not aligned, changing will require sync integration and change tcm config
        public const string GET_MICROSERVICES_CANARY_CONFIGURATION = "GetCanaryConfiguration";
        public const string GET_ELASTICSEARCH_CANARY_CONFIGURATION = "GetElasticsearchCanaryConfiguration";

        public const string GET_EPG_V2_PARTNER_CONFIGURATION = "GetEpgV2PartnerConfiguration";
        public const string GET_EPG_V3_PARTNER_CONFIGURATION = "GetEpgV3PartnerConfiguration";
        public const string GET_EPG_V3_ALIAS_INDEX_BINDING_CONFIGURATION = "GetEpgV3IndexAliasBindingConfiguration";

        public const string GET_PROGRAM_ASSET_GROUP_OFFERS = "GetProgramAssetGroupOffers";
        public const string GET_MEDIA_SUPPRESSED_INDEXES = "GetMediaSuppressedIndexesDB";

        public const string GET_ALL_MESSAGE_ANNOUNCEMENTS = "GetAllMessageAnnouncements";
        public const string GET_USER_MESSAGES_STATUS = "GetUserMessagesStatus";
        public const string GET_MESSAGE_FOLLOW_ANNOUNCEMENT_DB = "GetMessageFollowAnnouncementDB";

        public const string GET_ASSETFILE_BY_ID = "GetAssetFileById";
        public const string GET_ASSETFILES_BY_ASSETID = "GetAssetFilesByAssetId";

        #endregion

        #region Constant Invalidation Keys

        public const string GET_COUNTRY_BY_IP_INVALIDATION_KEY = "invalidateGetCountryByIp";
        public const string GET_PROXY_IP_INVALIDATION_KEY = "invalidateProxyIp";

        #endregion
    }
}

