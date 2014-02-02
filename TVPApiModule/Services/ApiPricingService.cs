using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using log4net;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPApiModule.Extentions;

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

        public TVPApiModule.Objects.Responses.PPVModule GetPPVModuleData(int ppvCode, string sCountry, string sLanguage, string sDevice)
        {
            TVPApiModule.Objects.Responses.PPVModule response = null;
            try
            {
                var res = m_Module.GetPPVModuleData(m_wsUserName, m_wsPassword, ppvCode.ToString(), sCountry, sLanguage, sDevice);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPPVModuleData, Error Message: {0} Parameters: ppv: {1}", ex.Message, ppvCode);
            }

            return response;
        }

        public List<MediaFilePPVModule> GetPPVModuleListForMediaFiles(int[] mediaFiles, string sCountry, string sLanguage, string sDevice)
        {
            List<MediaFilePPVModule> retVal = null;

            try
            {
                var response = m_Module.GetPPVModuleListForMediaFiles(m_wsUserName, m_wsPassword, mediaFiles, sCountry, sLanguage, sDevice);

                if (response != null)
                    retVal = response.ToList();
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (int media in mediaFiles)                
                    sb.Append(media + ",");
                
                logger.ErrorFormat("Error calling webservice protocol : GetPPVModuleListForMediaFiles, Error Message: {0} Parameters: Medias: {1}", ex.Message, sb.ToString());
            }

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            List<TVPApiModule.Objects.Responses.Subscription> subscriptions = null;

            try
            {
                var response = m_Module.GetSubscriptionsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);

                if (response != null)
                    subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMediaFile, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return subscriptions;
        }

        public TVPApiModule.Objects.Responses.Subscription GetSubscriptionData(string subCode, bool getAlsoInactive)
        {
            TVPApiModule.Objects.Responses.Subscription sub = null;

            try
            {
                var res = m_Module.GetSubscriptionData(m_wsUserName, m_wsPassword, subCode, string.Empty, string.Empty, string.Empty, getAlsoInactive);
                if (res != null)
                    sub = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionData, Error Message: {0}", ex.Message);
            }

            return sub;
        }

        public TVPApiModule.Objects.Responses.CouponData GetCouponStatus(string sCouponCode)
        {
            TVPApiModule.Objects.Responses.CouponData couponData = null;

            try
            {
                var res = m_Module.GetCouponStatus(m_wsUserName, m_wsPassword, sCouponCode);
                if (res != null)
                    couponData = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCouponStatus, Error Message: {0}", ex.Message);
            }

            return couponData ;
        }

        public TVPApiModule.Objects.Responses.CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID)
        {
            TVPApiModule.Objects.Responses.CouponsStatus couponStatus = TVPApiModule.Objects.Responses.CouponsStatus.NotExists;
            
            try
            {
                couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)m_Module.SetCouponUsed(m_wsUserName, m_wsPassword, sCouponCode, sSiteGUID); 
            } 
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetCouponUsed, Error Message: {0}", ex.Message);
            }

            return couponStatus;
        }

        public List<Campaign> GetCampaignsByType(CampaignTrigger trigger, bool isAlsoInactive, string udid)
        {
            List<Campaign> retVal = null;

            try
            {
                var response = m_Module.GetCampaignsByType(m_wsUserName, m_wsPassword, trigger, string.Empty, string.Empty, udid, isAlsoInactive);

                if (response != null)
                    retVal = response.ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCampaignsByType, Error Message: {0}", ex.Message);
            }

            return retVal;
        }

        public List<int> GetSubscriptionIDsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            List<int> retVal = null;

            try
            {
                var response = m_Module.GetSubscriptionIDsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);

                if (response != null)
                    retVal = response.ToList();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionIDsContainingMediaFile, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionsContainingUserTypes(int isActive, int[] userTypesIDs)
        {
            List<TVPApiModule.Objects.Responses.Subscription> subscriptions = null;
            string sUserTypesIDs = string.Empty;

            try
            {                
                var response = m_Module.GetSubscriptionsContainingUserTypes(m_wsUserName, m_wsPassword, string.Empty, string.Empty, string.Empty, isActive, userTypesIDs);
                
                if (response != null)
                {
                    subscriptions = response.Where(s => s != null).Select(s => s.ToApiObject()).ToList();
                }
            }
            catch (Exception ex)
            {
                if (userTypesIDs != null && userTypesIDs.Length > 0)
                {
                    sUserTypesIDs = string.Join(",", userTypesIDs.Select(x => x.ToString()).ToArray());
                }
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingUserTypes, Error Message: {0}, Parameters :  isActive: {1}, userTypesIDs : {2}", ex.Message, isActive, sUserTypesIDs);
            }

            return subscriptions;
        }

        #endregion
    }
}
