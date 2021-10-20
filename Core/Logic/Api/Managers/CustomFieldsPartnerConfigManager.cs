using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface ICustomFieldsPartnerConfigManager
    {
        Status UpdateConfig(int groupId, CustomFieldsPartnerConfig partnerConfig);
        CustomFieldsPartnerConfig GetCustomFieldsConfigFromCache(int groupId);
        bool ExistingClientTag(int groupId, string clientTag);
    }

    public class CustomFieldsPartnerConfigManager: ICustomFieldsPartnerConfigManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CustomFieldsPartnerConfigManager> lazy = new Lazy<CustomFieldsPartnerConfigManager>(() =>
            new CustomFieldsPartnerConfigManager(CustomFieldsPartnerRepository.Instance,
                                            LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static ICustomFieldsPartnerConfigManager Instance { get { return lazy.Value; } }

        private readonly ICustomFieldsPartnerRepository _repository;
        private readonly ILayeredCache _layeredCache;

        public CustomFieldsPartnerConfigManager(ICustomFieldsPartnerRepository repository,
                                           ILayeredCache layeredCache)
        {
            _repository = repository;
            _layeredCache = layeredCache;
        }

        public Status UpdateConfig(int groupId, CustomFieldsPartnerConfig partnerConfig)
        {
            var response = new Status(eResponseStatus.Error);

            if (!_repository.SaveCustomFieldsPartnerConfig(groupId, partnerConfig))
            {
                var error = $"Error while save CustomFieldsPartnerConfig. groupId: {groupId}.";
                log.Error(error);
                response.Set(eResponseStatus.Error, error);
                return response;
            }

            string invalidationKey = LayeredCacheKeys.GetCustomFieldsPartnerConfigInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.Error($"Failed to set invalidation key for CustomFieldsPartnerConfig with invalidationKey: {invalidationKey}.");
            }

            response.Set(eResponseStatus.OK);

            return response;
        }

        public CustomFieldsPartnerConfig GetCustomFieldsConfigFromCache(int groupId)
        {
            CustomFieldsPartnerConfig response = null;

            string key = LayeredCacheKeys.GetCustomFieldsPartnerConfigKey(groupId);
            var invalidationKey = new List<string>() { LayeredCacheKeys.GetCustomFieldsPartnerConfigInvalidationKey(groupId) };

            if (!_layeredCache.Get(key,
                                 ref response,
                                 arg => Tuple.Create(_repository.GetCustomFieldsPartnerConfig(groupId), true),
                                 null,
                                 groupId,
                                 LayeredCacheConfigNames.GET_CUSTOM_FIELDS_PARTNER_CONFIG,
                                 invalidationKey,
                                 true))
            {
                log.Error($"Failed getting GetCustomFieldsConfig from LayeredCache, groupId: {groupId}, key: {key}");
            }

            return response;
        }

        /// <summary>
        /// Check if a given client tag is configured to skip alias
        /// </summary>
        public bool ExistingClientTag(int groupId, string clientTag)
        {
            if (string.IsNullOrEmpty(clientTag))
                return false;

            var _config = GetCustomFieldsConfigFromCache(groupId);
            if (_config != null && _config.ClientTagsToIgnoreMetaAlias?.Count > 0)
            {
                return _config.ClientTagsToIgnoreMetaAlias.Any(x => x == clientTag);
            }
            return false;
        }
    }
}
