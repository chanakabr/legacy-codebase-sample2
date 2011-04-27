using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Threading;
using System.Configuration;
using log4net;
using TVPPro.Configuration.Media;

namespace TVPApi.Configuration.Media
{
    public class ApiMediaConfiguration : ConfigurationManager<MediaData>
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(MediaConfiguration));

        public ApiMediaConfiguration()
		{
			base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.Media"], true);
            m_syncFile = ConfigurationManager.AppSettings["TVPPro.Configuration.Media"];
		}

        public ApiMediaConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }
    }
}
