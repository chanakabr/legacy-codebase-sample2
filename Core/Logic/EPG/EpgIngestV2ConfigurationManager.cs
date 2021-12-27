using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ApiObjects;
using ApiObjects.Epg;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using KLogMonitor;

namespace ApiLogic.EPG
{
    public interface IEpgV2PartnerConfigurationManager
    {
        EpgV2PartnerConfiguration GetConfiguration(int partnerId);
        bool SetConfiguration(int partnerId, EpgV2PartnerConfiguration conf);
    }

    public class EpgV2PartnerConfigurationManager : IEpgV2PartnerConfigurationManager
    {
        private readonly ILayeredCache _layeredCache;
        private readonly ICouchbaseManager _cbManager;

        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<EpgV2PartnerConfigurationManager> _lazy =
            new Lazy<EpgV2PartnerConfigurationManager>(GetEpgIngestV2ConfigurationManagerInstance, LazyThreadSafetyMode.PublicationOnly);

        public static EpgV2PartnerConfigurationManager Instance => _lazy.Value;

        private EpgV2PartnerConfigurationManager(ILayeredCache layeredCache, ICouchbaseManager cbManager)
        {
            _cbManager = cbManager;
            _layeredCache = layeredCache;
        }

        private static EpgV2PartnerConfigurationManager GetEpgIngestV2ConfigurationManagerInstance()
        {
            ICouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            return new EpgV2PartnerConfigurationManager(LayeredCache.Instance, cbManager);
        }

        // Right now key is updated manually without API so no invalidation keys are set
        // if later we have api for updating it we will need invalidation
        public EpgV2PartnerConfiguration GetConfiguration(int partnerId)
        {
            EpgV2PartnerConfiguration result = null;
            var cacheConfigKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationKey(partnerId);
            var isSuccess = _layeredCache.Get(
                cacheConfigKey, ref result, GetConfigurationFromSource,
                new Dictionary<string, object>() { { "partnerId", partnerId } },
                partnerId,
                LayeredCacheConfigNames.GET_EPG_V2_PARTNER_CONFIGURATION);

            if (!isSuccess)
            {
                _log.Error($"Failed getting epg v2 partner configuration from layeredCache, trying to get from legacy table GROUP_FEATURES");
                // if null then fallback to check the old groups features table
               
                // create a new partner config with default confs
                result = new EpgV2PartnerConfiguration();
                var isGroupsFeaturesEnabledFallback = TvinciCache.GroupsFeatures.GetGroupFeatureStatus(partnerId, GroupFeature.EPG_INGEST_V2);
                if (isGroupsFeaturesEnabledFallback)
                {
                    result.IsEpgV2Enabled = true;
                }
            
                // set the config in any way to avoid calling groups features in the future
                SetConfiguration(partnerId, result);
            }

            return result;
        }
        
        private Tuple<EpgV2PartnerConfiguration, bool> GetConfigurationFromSource(Dictionary<string, object> args)
        {
            if (args?.ContainsKey("partnerId") != true)
            {
                return Tuple.Create<EpgV2PartnerConfiguration, bool>(null, false);
            }

            var partnerId = (int)args["partnerId"];
            var config = _cbManager.GetWithVersion<EpgV2PartnerConfiguration>(GetEpgV2PartnerConfigurationKey(partnerId),out _, out var resultStatus);
            
            // we dont store in cache if config not found because we expect the check of IsEpgV2Enabled to write this doc as soon as it find that key not exists
            // and we dont want ot handle invalidation keys because this config is updated manually only.
            if (resultStatus == eResultStatus.ERROR || resultStatus == eResultStatus.KEY_NOT_EXIST)
            {
                return Tuple.Create<EpgV2PartnerConfiguration, bool>(null, false);
            }

            return Tuple.Create(config, true);
        }

        public bool SetConfiguration(int partnerId, EpgV2PartnerConfiguration conf)
        {
            return _cbManager.Set(GetEpgV2PartnerConfigurationKey(partnerId), conf, expiration:0);
        }
        
        private static string GetEpgV2PartnerConfigurationKey(int partnerId) => $"ingest_v2_configuration_{partnerId}";
    }
}