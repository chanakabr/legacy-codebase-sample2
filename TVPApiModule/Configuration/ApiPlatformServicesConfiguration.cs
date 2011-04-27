using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using System.Configuration;
using TVPPro.Configuration.PlatformServices;

namespace TVPApi.Configuration.PlatformServices
{
    public class ApiPlatformServicesConfiguration : ConfigurationManager<PlatformServicesData>
    {

        public ApiPlatformServicesConfiguration()
        {
            base.SyncFromFile(ConfigurationManager.AppSettings["TVPPro.Configuration.PlatformServices"], true);
        }

        public ApiPlatformServicesConfiguration(string syncFile)
        {
            base.SyncFromFile(syncFile, true);
            m_syncFile = syncFile;
        }
    }
}
