using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using KLogMonitor;
using RedisManager;

namespace CanaryDeploymentManager
{
    public interface IElasticsearchCanaryDeploymentManager
    {
        GenericResponse<ElasticsearchCanaryDeploymentConfiguration> GetPartnerConfiguration(int partnerId);
        Status SetMigrationEventsStatus(int partnerId, bool enableMigrationEvents);
        Status SetElasticsearchActiveVersion(int partnerId, ElasticsearchVersion activeVersion);
        Status DeleteCanaryDeploymentConfiguration(int groupId);
        bool IsMigrationEventsEnabled(int partnerId);
        ElasticsearchVersion GetActiveElasticsearchActiveVersion(int partnerId);

    }
    
    public class ElasticsearchCanaryDeploymentManager: IElasticsearchCanaryDeploymentManager
    {
        private readonly ILayeredCache _layeredCache;
        private readonly ICouchbaseManager _cbManager;

        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private static readonly Lazy<ElasticsearchCanaryDeploymentManager> _lazy = 
            new Lazy<ElasticsearchCanaryDeploymentManager>(GetElasticsearchCanaryDeploymentInstance, LazyThreadSafetyMode.PublicationOnly);

        
        
        public static ElasticsearchCanaryDeploymentManager Instance => _lazy.Value;

        private static ElasticsearchCanaryDeploymentManager GetElasticsearchCanaryDeploymentInstance()
        {
            ICouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            return new ElasticsearchCanaryDeploymentManager(LayeredCache.Instance, cbManager);
        }

        public ElasticsearchCanaryDeploymentManager(ILayeredCache layeredCache, ICouchbaseManager cbManager)
        {
            _layeredCache = layeredCache;
            _cbManager = cbManager;
        }

        public GenericResponse<ElasticsearchCanaryDeploymentConfiguration> GetPartnerConfiguration(int partnerId)
        {
            var invalidationKeys = new List<string>() { LayeredCacheKeys.GetElasticsearchCanaryDeploymentConfigurationInvalidationKey(partnerId) };
            // group id 0 also affects other group ids so we should consider its invalidation key as well
            if (partnerId != 0)
            {
                invalidationKeys.Add(LayeredCacheKeys.GetMicroserviceCanaryDeploymentConfigurationInvalidationKey(0));
            }

            GenericResponse<ElasticsearchCanaryDeploymentConfiguration> result = null;
            var cacheConfigKey = LayeredCacheKeys.GetElasticsearchCanaryDeploymentConfigurationKey(partnerId);
            var isSuccess = _layeredCache.Get(
                cacheConfigKey, ref result, GetElasticsearchConfigFromSource,
                new Dictionary<string, object>() {{ "partnerId", partnerId }},
                partnerId,
                LayeredCacheConfigNames.GET_MICROSERVICES_CANARY_CONFIGURATION, invalidationKeys);
                
            if (!isSuccess)
            {
                _log.Error($"Failed getting elasticsearch canary deployment configuration from layeredCache");
            }

            return result;
        }

        public Status SetMigrationEventsStatus(int partnerId, bool enableMigrationEvents)
        {
            var config = GetPartnerConfiguration(partnerId);
            config.Object.EnableMigrationEvents = enableMigrationEvents;
            var isSuccess = SetPartnerConfiguration(partnerId, config.Object);
            return isSuccess ? Status.Ok : Status.Error;
        }

        public Status SetElasticsearchActiveVersion(int partnerId, ElasticsearchVersion activeVersion)
        {
            var config = GetPartnerConfiguration(partnerId);
            config.Object.ElasticsearchActiveVersion = activeVersion;
            var isSuccess = SetPartnerConfiguration(partnerId, config.Object);
            return isSuccess ? Status.Ok : Status.Error;
        }

        public Status DeleteCanaryDeploymentConfiguration(int groupId)
        {
            var res = new Status(eResponseStatus.FailedToDeleteGroupCanaryDeploymentConfiguration, $"Failed To delete canary deployment configuration for groupId {groupId}");
            try
            {
                var key = GetElasticsearchCanaryConfigurationKey(groupId);
                // if key doesn't exist or it was deleted successfully
                if (!_cbManager.IsKeyExists(key) || _cbManager.Remove(key)) 
                {
                    SetInvalidationKey(groupId);
                    res.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to delete elasticsearch canary deployment configuration for groupId {groupId}", ex);
            }

            return res;
        }
        
        private bool SetPartnerConfiguration(int partnerId, ElasticsearchCanaryDeploymentConfiguration config)
        {
            var key = GetElasticsearchCanaryConfigurationKey(partnerId);
            if (_cbManager.Set(key, config, expiration: 0))
            {
                SetInvalidationKey(partnerId);
                return true;
            }

            return false;
        }

        public bool IsMigrationEventsEnabled(int partnerId)
        {
            var res = GetPartnerConfigurationOrDefault(partnerId);
            return res.EnableMigrationEvents;
        }

        public ElasticsearchVersion GetActiveElasticsearchActiveVersion(int partnerId)
        {
            var res = GetPartnerConfigurationOrDefault(partnerId);
            return res.ElasticsearchActiveVersion;
        }
        
        private ElasticsearchCanaryDeploymentConfiguration GetPartnerConfigurationOrDefault(int partnerId)
        {
            var res = GetPartnerConfiguration(partnerId);
            if (res.Status.Code == (int)eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet)
            {
                var configRes = GetPartnerConfiguration(0);
                return configRes.Object;
            }

            return res.Object;
        }

        private Tuple<GenericResponse<ElasticsearchCanaryDeploymentConfiguration>, bool> GetElasticsearchConfigFromSource(Dictionary<string, object> args)
        {
            var response = new GenericResponse<ElasticsearchCanaryDeploymentConfiguration>();
            response.SetStatus(Status.Ok);
            
            if (args?.ContainsKey("partnerId") != true)
            {
                return Tuple.Create<GenericResponse<ElasticsearchCanaryDeploymentConfiguration>, bool>(null, false);
            }

            var partnerId = (int) args["partnerId"];
            var key = GetElasticsearchCanaryConfigurationKey(partnerId);

            if (!_cbManager.IsKeyExists(key))
            {
                if (partnerId == 0)
                {
                    response.Object = new ElasticsearchCanaryDeploymentConfiguration();
                }
                else
                {
                    response.SetStatus(eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet, "Group canary deployment configuration not set yet, check groupId 0 instead to see default value");
                }
            }
            else
            {
                response.Object = _cbManager.Get<ElasticsearchCanaryDeploymentConfiguration>(key);
            }
            
            return Tuple.Create(response,true);
        }

        private void SetInvalidationKey(int partnerId)
        {
            var invalidationKey = LayeredCacheKeys.GetElasticsearchCanaryDeploymentConfigurationInvalidationKey(partnerId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                _log.Error($"Failed to set invalidation key for elasticsearch canary deployment configuration with invalidationKey: {invalidationKey}");
            }
        }
        
        private string GetElasticsearchCanaryConfigurationKey(int partnerId)
        {
            return $"elasticsearch_canary_configuration_{partnerId}";
        }
    }
}