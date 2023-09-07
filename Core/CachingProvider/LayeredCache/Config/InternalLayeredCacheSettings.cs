using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CachingProvider.LayeredCache
{
    internal class InternalLayeredCacheSettings
    {
        private static readonly List<LayeredCacheConfig> INMEM_10M =
            new List<LayeredCacheConfig>
            {
                new InMemoryLayeredCacheConfig
                {
                    Type = LayeredCacheType.InMemoryCache,
                    TTL = 600
                },
                new CbLayeredCacheConfig
                {
                    Bucket = "Cache",
                    Type = LayeredCacheType.CbMemCache,
                    TTL = 86400
                }
            };

        internal static readonly IReadOnlyDictionary<string, List<LayeredCacheConfig>> CacheSettings =
            new ReadOnlyDictionary<string, List<LayeredCacheConfig>>(
                new Dictionary<string, List<LayeredCacheConfig>>
                {
                    {
                        LayeredCacheConfigNames.GET_BULK_UPLOADS_FROM_CACHE, new List<LayeredCacheConfig>
                        {
                            new InMemoryLayeredCacheConfig
                            {
                                Type = LayeredCacheType.InMemoryCache,
                                TTL = 3600
                            },
                            new CompressionCbLayeredCacheConfig
                            {
                                Bucket = "Cache",
                                Type = LayeredCacheType.CbMemCache,
                                TTL = 86400
                            }
                        }
                    }
                });

        internal static readonly IReadOnlyDictionary<string, List<LayeredCacheConfig>> InvalidationCacheSettings =
            new ReadOnlyDictionary<string, List<LayeredCacheConfig>>(
                new Dictionary<string, List<LayeredCacheConfig>> 
        {
            // {
            //     LayeredCacheConfigNames.GET_SUBSCRIPTIONS, INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_SUBSCRIPTION, INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_FEATURES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_PERMISSION_ITEMS_BY_GROUP_ID,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_PERMISSION_ITEMS_TO_FEATURES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_PERMISSIONS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.LIST_CAMPAIGNS_BY_GROUP_ID,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_ALL_BUSINESS_MODULE_RULE_ACTION_TYPES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_ALL_BUSINESS_MODULE_RULE_IDS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.MEDIA_CONCURRENCY_RULES_BY_LIMITATION_MODULE_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GENERAL_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_LINEAR_MEDIA_REGIONS_NAME_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_REGIONS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_DEVICE_FAMILIES_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_DEVICE_BRANDS_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_SUBSCRIPTION_ITEMS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_CATEGORIES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_CATALOG_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_PLAYBACK_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.CDN_ADAPTER_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GROUP_CDN_SETTINGS_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_COMMERCE_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_ALL_EPG_PICTURES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GROUP_PLAYBACK_PROFILES_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_DEVICE_CONCURRENCY_PRIORITY_FROM_CB,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.DLM_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_ROLES_BY_GROUP_ID,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_LANGUAGES,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_DEFAULT_PARENTAL_SETTINGS_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.PROCEDURES_ROUTING_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_ALL_LINEAR_MEDIA,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_COLLECTIONS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_PROGRAM_ASSET_GROUP_OFFERS,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_PRICE_PLAN_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_USAGE_MODULE_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_MEDIA_FILE_TYPES_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_CATALOG_GROUP_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_MEDIA_ID_GROUP_FILE_MAPPER_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_SECURITY_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GROUP_MANAGER_GET_GROUP_CONFIG_NAME,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_OBJECT_VIRTUAL_ASSET_PARTNER_CONFIG,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.GET_CAMPAIGN_BY_ID,
            //     INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.MEDIA_FILES_LAYERED_CACHE_CONFIG_NAME, INMEM_10M
            // },
            // {
            //     LayeredCacheConfigNames.CHANNELS_CONTAINING_MEDIA_LAYERED_CACHE_CONFIG_NAME,
            //     INMEM_10M
            // },
            {
                LayeredCacheConfigNames.GET_PPV_FOR_FILE, INMEM_10M
            },
            {
                 LayeredCacheConfigNames.GET_GROUP_DISCOUNTS_LAYERED_CACHE_CONFIG_NAME,
                 INMEM_10M
            },
        });
    }
}