using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Pricing;

namespace TVPApiModule.Services
{
    public class ApiPricingService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private TVPPro.SiteManager.TvinciPlatform.Pricing.module m_Module;
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private int m_groupID;
        private PlatformType m_platform;

        public ApiPricingService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Pricing.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.PricingService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public PPVModule GetPPVModuleData(int ppvCode, string sCountry, string sLanguage, string sDevice)
        {
            PPVModule response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetPPVModuleData(m_wsUserName, m_wsPassword, ppvCode.ToString(), sCountry, sLanguage, sDevice);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPPVModuleData, Error Message: {0} Parameters: ppv: {1}", ex.Message, ppvCode);
            }

            return response;
        }

        public MediaFilePPVModule[] GetPPVModuleListForMediaFiles(int[] mediaFiles, string sCountry, string sLanguage, string sDevice)
        {
            MediaFilePPVModule[] response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetPPVModuleListForMediaFiles(m_wsUserName, m_wsPassword, mediaFiles, sCountry, sLanguage, sDevice);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    subscriptions = m_Module.GetSubscriptionsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    sub = m_Module.GetSubscriptionData(m_wsUserName, m_wsPassword, subCode, string.Empty, string.Empty, string.Empty, getAlsoInactive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionData, Error Message: {0}", ex.Message);
            }

            return sub;
        }

        public Subscription[] GetSubscriptionsData(string[] p_arrSubscriptionCodes)
        {
            Subscription[] arrSubs = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.GetSubscriptionsData(m_wsUserName, m_wsPassword, p_arrSubscriptionCodes, string.Empty, string.Empty, string.Empty);
                    if (res != null)
                        arrSubs = res.Subscriptions;
                }
            }
            catch (Exception ex)
            {

                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsData, Error Message: {0}", ex.Message);
            }

            return (arrSubs);
        }

        public CouponData GetCouponStatus(string sCouponCode)
        {
            CouponData couponData = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = m_Module.GetCouponStatus(m_wsUserName, m_wsPassword, sCouponCode);
                    if (res != null)
                        couponData = res.Coupon;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCouponStatus, Error Message: {0}", ex.Message);
            }

            return couponData;
        }

        public CouponsStatus SetCouponUsed(string sCouponCode, string sSiteGUID)
        {
            CouponsStatus couponStatus = CouponsStatus.NotExists;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    couponStatus = m_Module.SetCouponUsed(m_wsUserName, m_wsPassword, sCouponCode, sSiteGUID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetCouponUsed, Error Message: {0}", ex.Message);
            }

            return couponStatus;
        }

        public Campaign[] GetCampaignsByType(CampaignTrigger trigger, bool isAlsoInactive, string udid)
        {
            Campaign[] campaigns = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    campaigns = m_Module.GetCampaignsByType(m_wsUserName, m_wsPassword, trigger, string.Empty, string.Empty, udid, isAlsoInactive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCampaignsByType, Error Message: {0}", ex.Message);
            }

            return campaigns;
        }

        public int[] GetSubscriptionIDsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            int[] subscriptions = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    IdsResponse response = m_Module.GetSubscriptionIDsContainingMediaFile(m_wsUserName, m_wsPassword, iMediaID, iMediaFileID);
                    if (response != null)
                    {
                        subscriptions = response.Ids;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionIDsContainingMediaFile, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return subscriptions;
        }


        public Subscription[] GetSubscriptionsContainingUserTypes(int isActive, int[] userTypesIDs)
        {
            Subscription[] subscriptions = null;
            string sUserTypesIDs = string.Empty;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    subscriptions = m_Module.GetSubscriptionsContainingUserTypes(m_wsUserName, m_wsPassword, string.Empty, string.Empty, string.Empty, isActive, userTypesIDs);
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

        public Collection GetCollectionData(string collectionId, string countryCd2, string languageCode3, string deviceName, bool bGetAlsoUnActive)
        {
            Collection collection = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    collection = m_Module.GetCollectionData(m_wsUserName, m_wsPassword, collectionId, countryCd2, languageCode3, deviceName, bGetAlsoUnActive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingUserTypes, Error Message: {0}", ex.Message);
            }

            return collection;
        }
    }
}
