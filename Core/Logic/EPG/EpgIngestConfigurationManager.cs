using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ApiObjects;
using ApiObjects.Epg;
using CachingProvider.LayeredCache;
using Core.GroupManagers;
using CouchbaseManager;
using Phx.Lib.Log;

namespace ApiLogic.EPG
{
    public interface IEpgPartnerConfigurationManager
    {
        EpgV3PartnerConfiguration GetEpgV3Configuration(int partnerId);

        EpgV2PartnerConfiguration GetEpgV2Configuration(int partnerId);
        bool SetEpgV2Configuration(int partnerId, EpgV2PartnerConfiguration conf);

        bool SetEpgV3Configuration(int partnerId, EpgV3PartnerConfiguration conf);

        /// <summary>
        /// returns a dictionary of language code to a default epgCb that is a template for autofill program
        /// </summary>
        Dictionary<string, EpgCB> GetAutofillTemplate(int partnerId);
    }

    public class EpgPartnerConfigurationManager : IEpgPartnerConfigurationManager
    {
        private readonly ILayeredCache _layeredCache;
        private readonly ICouchbaseManager _appsCbManager;
        private readonly ICouchbaseManager _epgCbManager;

        private static readonly KLogger _log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<EpgPartnerConfigurationManager> _lazy =
            new Lazy<EpgPartnerConfigurationManager>(GetEpgIngestConfigurationManagerInstance, LazyThreadSafetyMode.PublicationOnly);

        public static EpgPartnerConfigurationManager Instance => _lazy.Value;
 

        // The Epg V3 config is set using a dedicated setup job for EpgV3,
        // This job will both migrate the existing version of Epg and will enable V3 by inserting this specific document to Couchbase.
        public EpgV3PartnerConfiguration GetEpgV3Configuration(int partnerId)
        {
            EpgV3PartnerConfiguration result = null;
            var invalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(partnerId);
            var cacheConfigKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationKey(partnerId);
            var isSuccess = _layeredCache.Get(
                cacheConfigKey, ref result, GetEpgV3ConfigurationFromSource,
                new Dictionary<string, object>() { { "partnerId", partnerId } },
                partnerId,
                LayeredCacheConfigNames.GET_EPG_V3_PARTNER_CONFIGURATION,
                new List<string> { invalidationKey });

            return result;
        }

        // Right now key is updated manually without API so no invalidation keys are set
        // if later we have API for updating it we will need invalidation
        public EpgV2PartnerConfiguration GetEpgV2Configuration(int partnerId)
        {
            EpgV2PartnerConfiguration result = null;
            var invalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(partnerId);
            var cacheConfigKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationKey(partnerId);
            var isSuccess = _layeredCache.Get(
                cacheConfigKey, ref result, GetEpgV2ConfigurationFromSource,
                new Dictionary<string, object>() { { "partnerId", partnerId } },
                partnerId,
                LayeredCacheConfigNames.GET_EPG_V2_PARTNER_CONFIGURATION,
                 new List<string> { invalidationKey });

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
                SetEpgV2Configuration(partnerId, result);
            }

            return result;
        }
        
        
        public bool SetEpgV2Configuration(int partnerId, EpgV2PartnerConfiguration conf)
        {
            var result = _appsCbManager.Set(GetEpgV2PartnerConfigurationKey(partnerId), conf, expiration:0);
            var invalidationKey = LayeredCacheKeys.GetEpgV2PartnerConfigurationInvalidationKey(partnerId);
            result &= _layeredCache.SetInvalidationKey(invalidationKey);
            return result;
        }

        public bool SetEpgV3Configuration(int partnerId, EpgV3PartnerConfiguration conf)
        {
            var result = _appsCbManager.Set(GetEpgV3PartnerConfigurationKey(partnerId), conf, expiration: 0);
            var invalidationKey = LayeredCacheKeys.GetEpgV3PartnerConfigurationInvalidationKey(partnerId);
            result &= _layeredCache.SetInvalidationKey(invalidationKey);
            return result;
        }

        public Dictionary<string, EpgCB> GetAutofillTemplate(int partnerId)
        {
            var key = $"autofill_{partnerId}";
            var result = _epgCbManager.Get<Dictionary<string, EpgCB>>(key, true);
            if (result == null)
            {
                _log.Info($"Could not find default auto fill document under key: {key}");
                var langs = GroupLanguageManager.GetGroupLanguages(partnerId);
                result = new Dictionary<string, EpgCB>();
                langs.ForEach(l => {
                    result[l.Code] = new EpgCB
                    {
                        GroupID = partnerId,
                        ParentGroupID = partnerId,
                        IsActive = true,
                        IsAutoFill = true,
                        Status = 1,
                        Name = "No Information",
                        Description = "No Information",
                        Language = l.Code
                    };
                });
            }

            return result;
        }

        private EpgPartnerConfigurationManager(ILayeredCache layeredCache, ICouchbaseManager appsCbManager, ICouchbaseManager epgCbManager)
        {
            _appsCbManager = appsCbManager;
            _epgCbManager = epgCbManager;
            _layeredCache = layeredCache;
        }

        private static EpgPartnerConfigurationManager GetEpgIngestConfigurationManagerInstance()
        {
            var appsCbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            var epgCbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.EPG);

            return new EpgPartnerConfigurationManager(LayeredCache.Instance, appsCbManager, epgCbManager);
        }

        private Tuple<EpgV2PartnerConfiguration, bool> GetEpgV2ConfigurationFromSource(Dictionary<string, object> args)
        {
            if (args?.ContainsKey("partnerId") != true)
            {
                return Tuple.Create<EpgV2PartnerConfiguration, bool>(null, false);
            }

            var partnerId = (int)args["partnerId"];
            var config = _appsCbManager.GetWithVersion<EpgV2PartnerConfiguration>(GetEpgV2PartnerConfigurationKey(partnerId), out _, out var resultStatus);

            // we don't store in cache if config not found because we expect the check of IsEpgV2Enabled to write this doc as soon as it find that key not exists
            // and we don't want to handle invalidation keys because this config is updated manually only.
            if (resultStatus == eResultStatus.ERROR || resultStatus == eResultStatus.KEY_NOT_EXIST)
            {
                return Tuple.Create<EpgV2PartnerConfiguration, bool>(null, false);
            }

            return Tuple.Create(config, true);
        }

        private Tuple<EpgV3PartnerConfiguration, bool> GetEpgV3ConfigurationFromSource(Dictionary<string, object> args)
        {
            var disabledEpgV3Config = new EpgV3PartnerConfiguration();
            if (args?.ContainsKey("partnerId") != true)
            {
                return Tuple.Create(disabledEpgV3Config, false);
            }

            var partnerId = (int)args["partnerId"];
            var config = _appsCbManager.GetWithVersion<EpgV3PartnerConfiguration>(GetEpgV3PartnerConfigurationKey(partnerId), out _, out var resultStatus);

            if (resultStatus == eResultStatus.KEY_NOT_EXIST)
            {
                return Tuple.Create(disabledEpgV3Config, true);

            }
            else if (resultStatus == eResultStatus.ERROR)
            {
                return Tuple.Create(disabledEpgV3Config, false);
            }

            return Tuple.Create(config, true);
        }

        private static string GetEpgV2PartnerConfigurationKey(int partnerId) => $"ingest_v2_configuration_{partnerId}";
        private static string GetEpgV3PartnerConfigurationKey(int partnerId) => $"ingest_v3_configuration_{partnerId}";
    }
}