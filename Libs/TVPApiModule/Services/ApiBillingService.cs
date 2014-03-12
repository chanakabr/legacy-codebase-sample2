using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Billing;

namespace TVPApiModule.Services
{
    public class ApiBillingService 
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiBillingService));

        private TVPPro.SiteManager.TvinciPlatform.Billing.module m_Module;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiBillingService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultPassword;
            
            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods
        public AdyenBillingDetail GetLastBillingUserInfo(string siteGuid, int billingMethod)
        {
            AdyenBillingDetail response = null;
            try
            {
                response = m_Module.GetLastBillingUserInfo(m_wsUserName, m_wsPassword, siteGuid, billingMethod);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLastBillingUserInfo, Error Message: {0}, params: siteGuid: {1}", ex.Message, siteGuid);
            }

            return response;
        }

        public string GetClientMerchantSig(string sParamaters)
        {
            string response = null;
            try
            {
                response = m_Module.GetClientMerchantSig(m_wsUserName, m_wsPassword, sParamaters);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetClientMerchantSig, Error Message: {0}, params: sParamaters: {1}", ex.Message, sParamaters);
            }

            return response;
        }

        public TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail GetLastBillingTypeUserInfo(string sSiteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.Billing.AdyenBillingDetail lastBillingInfo = null;
            try
            {
                lastBillingInfo = m_Module.GetLastBillingTypeUserInfo(m_wsUserName, m_wsPassword,sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLastBillingTypeUserInfo, Error Message: {0}, params: sSiteGuid: {1}", ex.Message, sSiteGuid);
            }

            return lastBillingInfo;
        }

        #endregion
    }
}
