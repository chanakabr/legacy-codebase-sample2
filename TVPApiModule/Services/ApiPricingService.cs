using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.Pricing;

namespace TVPApiModule.Services
{
    public class ApiPricingService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiPricingService));

        private TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule m_Module;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiPricingService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.mdoule();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods

       

        public MediaFilePPVModule[] GetPPVModuleListForMediaFiles(int[] mediaFiles, string sCountry, string sLanguage, string sDevice)
        {
            MediaFilePPVModule[] response = null;
            try
            {
                response = m_Module.GetPPVModuleListForMediaFiles(m_wsUserName, m_wsPassword, mediaFiles, sCountry, sLanguage, sDevice);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (int media in mediaFiles)                
                    sb.Append(media + ",");
                
                logger.ErrorFormat("Error calling webservice protocol : GetPPVModuleListForMediaFiles, Error Message: {0} Parameters: Medias: {1}", ex.Message, sb.ToString());
            }

            return response;
        }

        public Subscription[] GetSubscriptionsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            Subscription[] subscriptions = null;

            try
            {
                subscriptions = m_Module.GetSubscriptionsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMediaFile, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return subscriptions;
        }

        public Subscription GetSubscriptionData(string subCode, bool getAlsoInactive)
        {
            Subscription sub = null;

            try
            {
                sub = m_Module.GetSubscriptionData(m_wsUserName, m_wsPassword, subCode, string.Empty, string.Empty, string.Empty, getAlsoInactive);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionData, Error Message: {0}", ex.Message);
            }

            return sub;
        }
        #endregion
    }
}
