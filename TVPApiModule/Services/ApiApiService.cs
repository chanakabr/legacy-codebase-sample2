using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;

namespace TVPApiModule.Services
{
    public class ApiApiService 
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiService));

        private TVPPro.SiteManager.TvinciPlatform.api.API m_Module;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiApiService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods
        public MediaMarkObject GetMediaMark(string sSiteGuid, int iMediaID)
        {
            MediaMarkObject mediaMark = null;
            try
            {
                mediaMark = m_Module.GetMediaMark(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaMark, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, sSiteGuid);
            }

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, string sSiteGuid, SocialAction Action, SocialPlatform socialPlatform)
        {
            bool bRet = false;
            try
            {
                bRet = m_Module.AddUserSocialAction(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid, Action, socialPlatform);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserSocialAction, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, sSiteGuid);
            }

            return bRet;
        }
        #endregion
    }
}
