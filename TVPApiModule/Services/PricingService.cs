using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using log4net;

namespace TVPApiModule.Services
{
    public class PricingService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(PricingService));

        private ApiTvinciPlatform.Pricing.mdoule m_Module;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private string m_platform;
        #endregion

        #region C'tor
        public PricingService(int groupID, string platform)
        {
            m_Module = new ApiTvinciPlatform.Pricing.mdoule();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods

        #endregion
    }
}
