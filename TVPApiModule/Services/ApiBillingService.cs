using System;
using log4net;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;
using TVPApiModule.Context;

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

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.Billing.module Billing
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.Billing.module);
            }
        }

        #endregion

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

            response = Execute(() =>
                {
                    var res = Billing.GetLastBillingUserInfo(m_wsUserName, m_wsPassword, siteGuid, billingMethod);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as AdyenBillingDetail;

            return response;
        }

        public string GetClientMerchantSig(string sParamaters)
        {
            string response = null;

            response = Execute(() =>
                {
                    response = Billing.GetClientMerchantSig(m_wsUserName, m_wsPassword, sParamaters);
                    return response;
                }) as string;

            return response;
        }
        #endregion

        public AdyenBillingDetail GetLastBillingTypeUserInfo(string siteGuid)
        {
            AdyenBillingDetail billingDetail = null;

            billingDetail = Execute(() =>
                {
                    var res = Billing.GetLastBillingTypeUserInfo(m_wsUserName, m_wsPassword, siteGuid);
                    if (res != null)
                    {
                        billingDetail = res.ToApiObject();
                    }

                    return billingDetail;
                }) as AdyenBillingDetail;

            return billingDetail;
        }
    }
}
