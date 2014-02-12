using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;
using TVPApiModule.Manager;
using TVPApiModule.Context;
using TVPApiModule.Objects;

namespace TVPApiModule.Services
{
    public class ApiBillingService : BaseService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiBillingService));

        //private TVPPro.SiteManager.TvinciPlatform.Billing.module m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiBillingService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Billing.module();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.BillingService.DefaultPassword;
            
            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiBillingService()
        {
            
        }

        #endregion C'tor

        //#region Public Static Functions

        //public static ApiBillingService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.BillingService) as ApiBillingService;
        //}

        //#endregion

        #region Public methods
        public AdyenBillingDetail GetLastBillingUserInfo(string siteGuid, int billingMethod)
        {
            AdyenBillingDetail response = null;
            try
            {
                var res = (m_Module as TVPPro.SiteManager.TvinciPlatform.Billing.module).GetLastBillingUserInfo(m_wsUserName, m_wsPassword, siteGuid, billingMethod);
                if (res != null)
                    response = res.ToApiObject();
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
                response = (m_Module as TVPPro.SiteManager.TvinciPlatform.Billing.module).GetClientMerchantSig(m_wsUserName, m_wsPassword, sParamaters);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetClientMerchantSig, Error Message: {0}, params: sParamaters: {1}", ex.Message, sParamaters);
            }

            return response;
        }
        #endregion
    }
}
