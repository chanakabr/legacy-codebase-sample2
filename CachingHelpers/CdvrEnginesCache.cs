using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingHelpers
{
    public class CdvrEnginesCache : BaseCacheHelper<CDVRAdapter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static CdvrEnginesCache instance;

        public static CdvrEnginesCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new CdvrEnginesCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private CdvrEnginesCache()
            : base()
        {

        }

        #endregion

        #region Override Methods

        protected override CDVRAdapter BuildValue(params object[] parameters)
        {
            int adapterId = (int)parameters[0];
            int groupId = (int)parameters[1];

            return DAL.ConditionalAccessDAL.GetCDVRAdapter(groupId, adapterId);
        }

        public CDVRAdapter GetCdvrAdapter(int groupId, int adapterId)
        {
            string cacheKey = string.Format("{0}_cdvr_adapter_{1}", version, adapterId);
            string mutexName = string.Concat("Group Cdvr Adapter GID_", groupId);

            return base.Get(cacheKey, mutexName, adapterId, groupId);
        }

        #endregion
    }
}
