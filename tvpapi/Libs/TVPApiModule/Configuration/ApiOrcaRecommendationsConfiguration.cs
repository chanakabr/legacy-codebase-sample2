using Phx.Lib.Appconfig;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Tvinci.Configuration;
using TVPPro.Configuration.OrcaRecommendations;

namespace TVPApi.Configuration.OrcaConfiguration
{
    public class ApiOrcaRecommendationsConfiguration : ConfigurationManager<OrcaRecommendations>
    {
        public ApiOrcaRecommendationsConfiguration()
        {
            base.SyncFromFile("", true);
        }

        public ApiOrcaRecommendationsConfiguration(string syncFile)
        {
                base.SyncFromFile(syncFile, true);
                m_syncFile = syncFile;
        }
    }
}
