using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public const string MEDIA_IF_FOR_ASSET_LAYERED_CACHE_CONFIG_NAME = "GetMediaIdForAsset";
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
        public const string EPG_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "EpgParentalRules";
        public const string CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME = "GetMediaChannels";
        public const string FILE_CDN_DATA_LAYERED_CACHE_CONFIG_NAME = "GetFileCdnData";
        public const string PRICE_CODE_LOCALE_LAYERED_CACHE_CONFIG_NAME = "GetPriceCodeByCountyAndCurrency";
        public const string DISCOUNT_MODULE_LOCALE_LAYERED_CACHE_CONFIG_NAME = "GetDiscountModuleByCountyAndCurrency";
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
        public const string GET_ROLE_BY_ROLE_ID = "GetRoleByRoleId";
        public const string GET_ROLES_BY_GROUP_ID = "GetRolesByGroupId";
        public const string DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "DrmAdapter";
        public const string GROUP_DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "GroupDrmAdapter";
        public const string DOES_GROUP_USES_TEMPLATES_CACHE_CONFIG_NAME = "DoesGroupUsesTemplates";
        public const string GET_CATALOG_GROUP_CACHE_CONFIG_NAME = "GetCatalogGroupCache";
        public const string GET_GROUP_DEVICE_RULES_CACHE_CONFIG_NAME = "GetGroupDeviceRules";
        public const string GET_GROUP_GEO_BLOCK_RULES_CACHE_CONFIG_NAME = "GetGroupGeoBlockRules";
        public const string GET_ASSET_CACHE_CONFIG_NAME = "GetAsset";
        public const string GET_IMAGE_TYPE_CACHE_CONFIG_NAME = "GetImageType";
        public const string GET_RATIOS_CACHE_CONFIG_NAME = "GetRatios";
        public const string GET_ASSET_FILE_TYPES_CONFIG_NAME = "GetAssetFileTypess";

        #endregion

        #region Constant Invalidation Keys

        public const string GET_COUNTRY_BY_IP_INVALIDATION_KEY = "invalidateGetCountryByIp";
        public const string GET_PROXY_IP_INVALIDATION_KEY = "invalidateProxyIp";

        #endregion

    }
}