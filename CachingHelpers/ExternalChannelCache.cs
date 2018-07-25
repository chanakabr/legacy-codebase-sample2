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
            string channelId = (string)parameters[0];

            var externalChannel = CatalogDAL.GetExternalChannel(channelId);

            return externalChannel;
        }

        public ExternalChannel GetChannel(int groupId, string channelId)
        {
            string cacheKey = string.Format("{0}_external_channel_{1}_{2}", version, groupId, channelId);
            return base.Get(cacheKey, channelId);
        }

        #endregion
    }
}
