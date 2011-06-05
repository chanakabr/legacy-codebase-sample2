using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using TVPApi;
using TVPPro.SiteManager.Context;
using log4net;

namespace TVPApiModule.Services
{
    public class UsersService
    {
        #region Variables
        private readonly ILog logger = LogManager.GetLogger(typeof(UsersService));
        private ApiTvinciPlatform.Users.UsersService m_Module;
        
        private string m_wsUserName;
        private string m_wsPassword;

        private int m_groupID;
        private string m_platform;
        #endregion

        #region C'tor
        public UsersService(int groupID, string platform)
        {
            m_Module = new ApiTvinciPlatform.Users.UsersService();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.UsersService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods

        #endregion
    }
}
