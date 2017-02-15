using ApiObjects;
using ApiObjects.CDNAdapter;
using CachingProvider.LayeredCache;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingHelpers
{
    public class CdnAdapterCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();

        #region Singleton

        private static CdnAdapterCache instance = null;

        public static CdnAdapterCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new CdnAdapterCache();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Ctor

        private CdnAdapterCache() { }

        #endregion

        #region Public Methods

        public CDNAdapter GetCdnAdapter(int groupId, int adapterId)
        {
            CDNAdapter adapter = null;
            string key = LayeredCacheKeys.GetCDNAdapterKey(groupId, adapterId);
            bool cacheResult = LayeredCache.Instance.Get<CDNAdapter>(key, ref adapter, Utils.GetCdnAdapter, new Dictionary<string, object>() { { "adapterId", adapterId } },
                groupId, LayeredCacheConfigNames.CDN_ADAPTER_LAYERED_CACHE_CONFIG_NAME);

            if (!cacheResult || adapter == null)
            {
                log.ErrorFormat("Failed GetCdnAdapter, groupId: {0}, adapterId: {1}", groupId, adapterId); 
            }

            return adapter;
        }

        public CDNPartnerSettings GetCdnAdapterSettings(int groupId)
        {
            CDNPartnerSettings settings = null;
            // get group cdn settings
            string key = LayeredCacheKeys.GetGroupCdnSettingsKey(groupId);
            bool cacheResult = LayeredCache.Instance.Get<CDNPartnerSettings>(key, ref settings, Utils.GetCdnSettings, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GROUP_CDN_SETTINGS_LAYERED_CACHE_CONFIG_NAME);

            if (cacheResult && settings != null)
            {
                log.ErrorFormat("Failed GetCdnAdapterSettings, groupId: {0}", groupId); 
            }

            return settings;
        }

        #endregion
    }
}
