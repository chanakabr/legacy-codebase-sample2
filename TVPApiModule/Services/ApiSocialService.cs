using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;

namespace TVPApiModule.Services
{
    public class ApiSocialService
    {
        #region Fields
        private readonly ILog logger = LogManager.GetLogger(typeof(ApiSocialService));
        static object instanceLock = new object();

        private int m_groupID;
        private PlatformType m_platform;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private TVPPro.SiteManager.TvinciPlatform.Social.module m_Module;

        #endregion

        #region C'tor
        public ApiSocialService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Social.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.SocialService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.SocialService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.SocialService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion

        public TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus DoSocialAction(int mediaID, string siteGuid, TVPPro.SiteManager.TvinciPlatform.Social.SocialAction socialAction, 
            TVPPro.SiteManager.TvinciPlatform.Social.SocialPlatform socialPlatform, string actionParam)
        {
            TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus eRes = TVPPro.SiteManager.TvinciPlatform.Social.SocialActionResponseStatus.UNKNOWN;

            try
            {
                eRes = m_Module.AddUserSocialAction(m_wsUserName, m_wsPassword, mediaID, siteGuid, socialAction, socialPlatform, actionParam);                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in DoSocialAction, Error : {0} Parameters : mediaID {1}, action: {2}, platform: {3}, param: {4}", ex.Message, mediaID,
                    socialAction, socialPlatform, actionParam);
            }

            return eRes;
        }
    }
}
