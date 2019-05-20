using ApiObjects;
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
    public class DrmAdapterCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static object locker = new object();

        #region Singleton

        private static DrmAdapterCache instance = null;

        public static DrmAdapterCache Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (locker)
                    {
                        if (instance == null)
                        {
                            instance = new DrmAdapterCache();
                        }
                    }
                }

                return instance;
            }
        }

        #endregion

        #region Ctor

        private DrmAdapterCache() { }

        #endregion

        #region Public Methods

        public DrmAdapter GetDrmAdapter(int groupId, int adapterId)
        {
            DrmAdapter adapter = null;
            string key = LayeredCacheKeys.GetDrmAdapterKey(groupId, adapterId);
            bool cacheResult = LayeredCache.Instance.Get<DrmAdapter>(key, ref adapter, Utils.GetDrmAdapter, new Dictionary<string, object>() { { "adapterId", adapterId } },
                groupId, LayeredCacheConfigNames.DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetDrmAdapterInvalidationKey(groupId, adapterId) });

            if (!cacheResult || adapter == null)
            {
                log.ErrorFormat("Failed GetDrmAdapter, groupId: {0}, adapterId: {1}", groupId, adapterId); 
            }

            return adapter;
        }

        public int GetGroupDrmAdapterId(int groupId)
        {
            int adapterId = 0;
            
            string key = LayeredCacheKeys.GetGroupDrmAdapterIdKey(groupId);
            bool cacheResult = LayeredCache.Instance.Get<int>(key, ref adapterId, Utils.GetGroupAdapterId, new Dictionary<string, object>() { { "groupId", groupId } },
                groupId, LayeredCacheConfigNames.GROUP_DRM_ADAPTER_LAYERED_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetGroupDrmAdapterIdInvalidationKey(groupId) });

            if (cacheResult)
            {
                log.ErrorFormat("Failed GetGroupDrmAdapterId, groupId: {0}", groupId);
            }

            return adapterId;
        }

        #endregion
    }
}
