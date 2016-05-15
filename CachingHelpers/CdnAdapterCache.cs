using ApiObjects.CDNAdapter;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingHelpers
{
    public class CdnAdapterCache : BaseCacheHelper<CDNAdapter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static CdnAdapterCache instance;

        public static CdnAdapterCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CdnAdapterCache("innercache");
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private CdnAdapterCache(string cacheType)
            : base(cacheType)
        {

        }

        #endregion

        #region Override Methods

        protected override CDNAdapter BuildValue(params object[] parameters)
        {
            int adapterId = (int)parameters[0];
            int groupId = (int)parameters[1];

            return DAL.ApiDAL.GetCDNAdapter(adapterId);
        }

        public CDNAdapter GetCdnAdapter(int groupId, int adapterId)
        {
            string cacheKey = string.Format("cdn_adapter_{0}", adapterId);
            string mutexName = string.Concat("Group CDNAdapter GID_", groupId);

            return base.Get(cacheKey, mutexName, adapterId, groupId);
        }

        #endregion
    }
}
