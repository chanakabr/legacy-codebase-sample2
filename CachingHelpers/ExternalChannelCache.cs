using ApiObjects;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Tvinci.Core.DAL;

namespace CachingHelpers
{
    public class ExternalChannelCache : BaseCacheHelper<ExternalChannel>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Singleton

        private static ExternalChannelCache instance;

        public static ExternalChannelCache Instance()
        {
            if (instance == null)
            {
                lock (locker)
                {
                    if (instance == null)
                    {
                        instance = new ExternalChannelCache();
                    }
                }
            }

            return instance;
        }

        #endregion

        #region Ctor

        private ExternalChannelCache()
            : base()
        {

        }

        #endregion

        #region Override Methods

        protected override ExternalChannel BuildValue(params object[] parameters)
        {
            int groupId = (int)parameters[0];
            string channelId = (string)parameters[1];

            return CatalogDAL.GetExternalChannel(groupId, channelId);
        }

        public ExternalChannel GetChannel(int groupId, string channelId)
        {
            string cacheKey = string.Format("external_channel_{0}_{1}", groupId, channelId);
            string mutexName = string.Concat("Group ExternalChannels GID_", groupId);

            return base.Get(cacheKey, mutexName, groupId, channelId);
        }

        #endregion
    }
}
