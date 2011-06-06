using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace TVPApiModule.Services
{
    public class ApiConditionalAccessService
    {
        #region Variables
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ApiConditionalAccessService));

        private TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module m_Module;
        
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiConditionalAccessService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion

        #region Public methods
        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid)
        {
            BillingResponse response = null;

            try
            {
                response = m_Module.CC_DummyChargeUserForMediaFile(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public PermittedMediaContainer[] GetUserPermittedItems(string sSiteGuid)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, MediaFileItemPricesContainer> GetItemsPrice(int[] MediasArray, string userGuid, bool bOnlyLowest)
        {
            throw new NotImplementedException();
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(string guid)
        {
            throw new NotImplementedException();
        }
        #endregion

        
    }
}
