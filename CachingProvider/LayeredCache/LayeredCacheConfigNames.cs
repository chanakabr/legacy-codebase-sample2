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
        public const string CHECK_GEO_BLOCK_MEDIA_LAYERED_CACHE_CONFIG_NAME = "CheckGeoBlockMedia";
        public const string MEDIA_USER_TYPE_LAYERED_CACHE_CONFIG_NAME = "CheckMediaUserType";
        public const string MEDIA_CONCURRENCY_RULES_LAYERED_CACHE_CONFIG_NAME = "GetMediaConcurrencyRules";        
        public const string GROUP_CDN_SETTINGS_LAYERED_CACHE_CONFIG_NAME = "GroupCDNSettings";
        public const string CDN_ADAPTER_LAYERED_CACHE_CONFIG_NAME = "CdnAdapter";
        public const string GROUP_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "GroupParentalRules";
        public const string USER_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "UserParentalRules";
        public const string MEDIA_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "MediaParentalRules";
        public const string EPG_PARENTAL_RULES_LAYERED_CACHE_CONFIG_NAME = "EpgParentalRules";
        public const string CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME = "GetMediaChannels";

        #endregion

        #region Constant Invalidation Keys

        public const string GET_COUNTRY_BY_IP_INVALIDATION_KEY = "invalidateGetCountryByIp";

        #endregion

    }
}