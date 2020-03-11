using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Services
{
    public class PricingService
    {
        #region Fields
        private static object lockObject = new object();
        private TvinciPlatform.Pricing.module m_Module;
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string wsUserName = string.Empty;
        private string wsPassword = string.Empty;
        #endregion

        #region Constractor
        private PricingService()
        {
            m_Module = new TvinciPlatform.Pricing.module();
            m_Module.Url = PlatformServicesConfiguration.Instance.Data.PricingService.URL;
            wsUserName = PlatformServicesConfiguration.Instance.Data.PricingService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.PricingService.DefaultPassword;

			logger.Info("Starting PricingService with URL:" + PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.URL);
        }
        #endregion

        #region Properties
        private static PricingService m_Instance;
        public static PricingService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (lockObject)
                    {
                        m_Instance = new PricingService();
                    }
                }

                return m_Instance;
            }
        }
        #endregion

        #region public methods

        public PrePaidModule GetPrePaidModuleData(int ppCode)
        {
            PrePaidModule ppModule = null;

            string sKey = string.Format("{0}", ppCode.ToString());
            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is PrePaidModule) return oFromCache as PrePaidModule;

            try
            {
                ppModule = m_Module.GetPrePaidModuleData(wsUserName, wsPassword, ppCode, string.Empty, string.Empty, string.Empty);
                DataHelper.SetCacheObject(sKey, ppModule);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPrePaidModuleData, Error Message: {0}, Parameters :  ppCode: {1}", ex.Message, ppCode);
            }

            return ppModule;
        }

        public Subscription[] GetSubscriptionsContainingMediaFile(int iMediaID, int iMediaFileID)
        {
            Subscription[] subscriptions = null;

            string sKey = string.Empty;
            sKey = string.Format("{0}_{1}_{2}", UsersService.Instance.GetUserID(), iMediaID.ToString(), iMediaFileID.ToString());

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is Subscription[]) return oFromCache as Subscription[];

            try
            {
                subscriptions = m_Module.GetSubscriptionsContainingMediaFile(wsUserName, wsPassword, iMediaID, iMediaFileID);
                if( subscriptions != null )
                    DataHelper.SetCacheObject(sKey, subscriptions);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMediaFile, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return subscriptions;
        }

        public int[] GetSubscriptionIDsContainingMediaFiles(int iMediaID, int iMediaFileID)
        {
            int[] subscriptionIDs = null;

            string sKey = string.Empty;
            sKey = string.Format("{0}_{1}_{2}", UsersService.Instance.GetUserID(), iMediaID.ToString(), iMediaFileID.ToString());

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is int[]) return oFromCache as int[];

            try
            {
                IdsResponse response = m_Module.GetSubscriptionIDsContainingMediaFile(wsUserName, wsPassword, iMediaID, iMediaFileID);
                if (response != null)
                {
                    subscriptionIDs = response.Ids;
                }

                if (subscriptionIDs != null)
                    DataHelper.SetCacheObject(sKey, subscriptionIDs);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionIDsContainingMediaFiles, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return subscriptionIDs;
        }

        public string GetSubscriptionsContainingMediaSTR(int iMediaID, int iMediaFileID)
        {
            string sRet = string.Empty;

            try
            {
                sRet = m_Module.GetSubscriptionsContainingMediaSTR(wsUserName, wsPassword, iMediaID, iMediaFileID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMediaSTR, Error Message: {0}, Parameters :  iMediaID: {1}, iMediaFileID : {2}", ex.Message, iMediaID, iMediaFileID);
            }

            return sRet;
        }

        public int[] GetSubscriptionMediaList(string subscriptionID, int fileType, string device)
        {
            int[] res = null;
            try
            {
                res = m_Module.GetSubscriptionMediaList(wsUserName, wsPassword, subscriptionID, fileType, device);
                logger.InfoFormat("Protocol: GetSubscriptionMediaList, Parameters : SubscriptionID : {0}, FileType : {1}, Device : {2}", subscriptionID, fileType, device);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionMediaList, Error Message: {0}, Parameters :  SubscriptionID: {1}, FileType : {1}, Device : {2}", ex.Message, subscriptionID, fileType, device);
            }
            return res;
        }

        public MediaFilePPVModule[] GetPPVModuleListForMediaFile(int[] mediaFiles, string country, string languageCode, string device)
        {
            MediaFilePPVModule[] res = null;

            string sKey = string.Empty;
            StringBuilder sbMediaFiles = new System.Text.StringBuilder();
            foreach (int media in mediaFiles) sbMediaFiles.Append(media.ToString() + ",");
            sKey = string.Format("{0}_{1}_{2}", sbMediaFiles.ToString(), country, languageCode, device);

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is MediaFilePPVModule[]) return oFromCache as MediaFilePPVModule[];

            try
            {
                res = m_Module.GetPPVModuleListForMediaFiles(wsUserName, wsPassword, mediaFiles, country, languageCode, device);
                DataHelper.SetCacheObject(sKey, res);
                logger.InfoFormat("Protocol: GetPPVModuleListForMediaFile, Parameters : mediaFiles : {0}", mediaFiles);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionMediaList, Error Message: {0}, Parameters :  MediaFiles: {1}, FileType : {1}", ex.Message, mediaFiles);
            }
            return res;
        }

        public Subscription GetSubscriptionDetailsByCode(string SubscriptionCode)
        {
            // Get user and password for method
            
            Subscription res = null;

            try
            {
                if (SessionHelper.LocaleInfo != null)
                {
                    res = m_Module.GetSubscriptionData(wsUserName, wsPassword, SubscriptionCode, SessionHelper.LocaleInfo.LocaleCountry, SessionHelper.LocaleInfo.LocaleLanguage, SessionHelper.LocaleInfo.LocaleDevice, false);
                }
                else
                {
                    res = m_Module.GetSubscriptionData(wsUserName, wsPassword, SubscriptionCode, string.Empty, string.Empty, string.Empty, false);
                }

                logger.InfoFormat("Protocol: GetSubscriptionDetailsByCode, Parameters : userID : {0}", UsersService.Instance.GetUserID());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionDetailsByCode, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public Subscription[] GetSubscriptionsForSingleItem(int MediaId, int FileTypeId)
        {
            //return array of Subscription objects the item related to.
            Subscription[] res = null;

            try
            {
                res = m_Module.GetSubscriptionsContainingMedia(wsUserName, wsPassword, MediaId, FileTypeId);


                logger.InfoFormat("Protocol: GetSubscriptionsContainingMedia, Parameters : MediaId : {0}, FileTypeId : {1}", MediaId.ToString(), FileTypeId.ToString());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMedia, Error Message: {0}, Parameters :  MediaId : {1}, FileTypeId : {2}", ex.Message, MediaId.ToString(), FileTypeId.ToString());
            }

            return res;
        }

        public Subscription[] GetIndexedSubscriptionsForSingleItem(int MediaId, int FileTypeId, int iCount)
        {
            //return array of Subscription objects the item related to.
            Subscription[] res = null;

            try
            {
                res = m_Module.GetIndexedSubscriptionsContainingMedia(wsUserName, wsPassword, MediaId, FileTypeId, iCount);

                logger.InfoFormat("Protocol: GetSubscriptionsContainingMedia, Parameters : MediaId : {0}, FileTypeId : {1}", MediaId.ToString(), FileTypeId.ToString());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsContainingMedia, Error Message: {0}, Parameters :  MediaId : {1}, FileTypeId : {2}", ex.Message, MediaId.ToString(), FileTypeId.ToString());
            }

            return res;
        }

        public void SetCouponUsed(string sCouponCode)
        {
            try
            {
                CouponsStatus status = m_Module.SetCouponUsed(wsUserName, wsPassword, sCouponCode, UsersService.Instance.GetUserID());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetCouponUsed, Error Message: {0}, Parameters :  sCouponCode : {1}, SiteGUID : {2}", ex.Message, sCouponCode, UsersService.Instance.GetUserID());
            }
        }

        public CouponData GetCouponStatus(string sCouponCode)
        {
            try
            {
                var res = m_Module.GetCouponStatus(wsUserName, wsPassword, sCouponCode);
              
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCouponStatus, Error Message: {0}, Parameters :  sCouponCode : {1}, SiteGUID : {2}", ex.Message, sCouponCode, UsersService.Instance.GetUserID());
            }

            return null;
        }

        public Campaign[] GetMediaCampaigns(int mediaID)
        {
            try
            {
                return m_Module.GetMediaCampaigns(wsUserName, wsPassword, mediaID, string.Empty, string.Empty, string.Empty, false);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaCampaigns, Error Message: {0}, Parameters :  mediaID : {1}, SiteGUID : {2}", ex.Message, mediaID, UsersService.Instance.GetUserID());
            }

            return null;
        }

        public Campaign[] GetCampaignsByType(CampaignTrigger trigger, bool isAlsoInactive)
        {
            try
            {
                return m_Module.GetCampaignsByType(wsUserName, wsPassword, trigger, string.Empty, string.Empty, string.Empty, isAlsoInactive);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaCampaigns, Error Message: {0}, Parameters :  SiteGUID : {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return null;
        }

        public PPVModule GetPPVModuleData(string PPVCode)
        {
            try
            {
                return m_Module.GetPPVModuleData(wsUserName, wsPassword, PPVCode, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaCampaigns, Error Message: {0}, Parameters :  SiteGUID : {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return null;
        }

        #endregion
    }
}
