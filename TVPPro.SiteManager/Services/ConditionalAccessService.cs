using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Context;
using System.Web;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Services
{
    public class ConditionalAccessService
    {
        #region Fields
        private static object lockObject = new object();
        private TvinciPlatform.ConditionalAccess.module m_Module;
        private string wsUserName = string.Empty;
        private string wsPassword = string.Empty;
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        enum eWSMethodName
        {
            GetUserPermittedItems,
            GetItemsPrice,
            GetUserPermitedSubscriptions,
            GetUserCAStatus,
            GetUserTransactionHistory,
            CancelSubscription,
            GetUserExpiredSubscriptions,
            RenewCancledSubscription
        }

        public enum ePaymentType
        {
            PPV,
            Package
        }

        public enum ePaymentMethod
        {
            [EnumAsStringValue("paypal")]
            PayPal,
            [EnumAsStringValue("ideal")]
            IDeal,
            [EnumAsStringValue("directdebit_NL")]
            DirectDebit
        }
        #endregion

        #region Constructor
        private ConditionalAccessService()
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module();
            m_Module.Url = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.URL;

            logger.Info("Starting ConditionalAccessService with URL:" + PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.URL);
        }
        #endregion

        #region Properties
        private static ConditionalAccessService m_Instance;
        public static ConditionalAccessService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (lockObject)
                    {
                        m_Instance = new ConditionalAccessService();
                    }
                }

                return m_Instance;
            }
        }
        #endregion

        #region Public Methods
        public BillingResponse GetPaymentPopupURL(ePaymentType type, ePaymentMethod method, string sSiteGUID, double dPrice, string sCurrencyCode, int iMediaFileID, string sModuleCode)
        {
            return GetPaymentPopupURL(type, method, sSiteGUID, dPrice, sCurrencyCode, iMediaFileID, sModuleCode, string.Empty);
        }

        public BillingResponse GetPaymentPopupURL(ePaymentType type, ePaymentMethod method, string sSiteGUID, double dPrice, string sCurrencyCode, int iMediaFileID, string sModuleCode, string couponCode)
        {
            BillingResponse billingResponse = null;

            // Get user and password for method
            string wsUser;
            string wsPassword;
            //PermittedMediaContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            string sLocaleLanguage = string.Empty;
            string sLocaleCountry = string.Empty;
            string sLocaleDevice = string.Empty;
            if (SessionHelper.LocaleInfo != null)
            {
                sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
            }

            try
            {
                switch (type)
                {
                    case ePaymentType.PPV:
                        billingResponse = m_Module.PU_GetPPVPopupPaymentMethodURL(wsUser, wsPassword, sSiteGUID, dPrice, sCurrencyCode, iMediaFileID, sModuleCode, couponCode, StringEnum.GetStringValue(method), SiteHelper.GetSiteBaseURL(true), sLocaleCountry, sLocaleLanguage, sLocaleDevice);
                        break;
                    case ePaymentType.Package:
                        billingResponse = m_Module.PU_GetSubscriptionPopupPaymentMethodURL(wsUser, wsPassword, sSiteGUID, dPrice, sCurrencyCode, iMediaFileID.ToString(), couponCode, StringEnum.GetStringValue(method), SiteHelper.GetSiteBaseURL(true), sLocaleCountry, sLocaleLanguage, sLocaleDevice);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPaymentPopupURL, Error Message: {0}, Parameters :  type: {1}", ex.Message, type.ToString());
            }

            return billingResponse;
        }

        public PermittedMediaContainer[] GetUserPermittedItems()
        {
            return GetUserPermittedItems(UsersService.Instance.GetUserID());
        }

        public PermittedMediaContainer[] GetUserExpiredItems(int NumOfItems)
        {
            return GetUserExpiredItems(UsersService.Instance.GetUserID(), NumOfItems);
        }

        public PermittedMediaContainer[] GetUserPermittedItems(string sID)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            PermittedMediaContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                res = m_Module.GetUserPermittedItems(wsUser, wsPassword, sID);


                logger.InfoFormat("Protocol: GetUserPermittedItems, Parameters : userID : {0}", sID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public PermittedMediaContainer[] GetUserExpiredItems(string sID, int NumOfItems)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            PermittedMediaContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                res = m_Module.GetUserExpiredItems(wsUser, wsPassword, sID, NumOfItems);


                logger.InfoFormat("Protocol: GetUserExpiredItems, Parameters : userID : {0}", sID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserExpiredItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public PermittedMediaContainer[] GetUserPermittedItems(string sID, string wsUser, string wsPass)
        {
            // Get user and password for method
            PermittedMediaContainer[] res = null;
            try
            {
                res = m_Module.GetUserPermittedItems(wsUser, wsPass, sID);

                logger.InfoFormat("Protocol: GetUserPermittedItems, Parameters : userID : {0}", sID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public bool IsUserPermittedItem(int mediaID)
        {
            // Get user and password for method
            if (UsersService.Instance.UserContext != null && UsersService.Instance.UserContext.UserResponse != null && UsersService.Instance.UserContext.UserResponse.m_user != null)
            {
                logger.InfoFormat("IsPermittedItem - m_user is not null - site guid is {0}", UsersService.Instance.UserContext.UserResponse.m_user.m_sSiteGUID);
                return IsUserPermittedItem(UsersService.Instance.UserContext.UserResponse.m_user.m_sSiteGUID, mediaID);
            }
            logger.InfoFormat("IsPermittedItem - m_user is null - site guid is {0}", UsersService.Instance.GetUserID());
            return IsUserPermittedItem(UsersService.Instance.GetUserID(), mediaID);
        }

        public bool IsUserPermittedItem(string sID, int mediaID)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            bool res = false;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                res = m_Module.IsPermittedItem(wsUser, wsPassword, sID, mediaID);


                logger.InfoFormat("Protocol: IsUserPermittedItem, Parameters : userID : {0}", sID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : IsUserPermittedItem, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public bool IsUserPermittedSubscription(int subID, ref string reason)
        {
            if (UsersService.Instance.UserContext != null && UsersService.Instance.UserContext.UserResponse != null && UsersService.Instance.UserContext.UserResponse.m_user != null)
            {
                logger.InfoFormat("IsPermittedSub - m_user is not null - site guid : {0}", UsersService.Instance.UserContext.UserResponse.m_user.m_sSiteGUID);
                return IsUserPermittedSubscription(UsersService.Instance.UserContext.UserResponse.m_user.m_sSiteGUID, subID, ref reason);
            }
            logger.InfoFormat("IsPermittedSub - m_user is null - site guid : {0}", UsersService.Instance.GetUserID());
            return IsUserPermittedSubscription(UsersService.Instance.GetUserID(), subID, ref reason);
        }

        public bool IsUserPermittedSubscription(string sID, int subID, ref string reason)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            bool res = false;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                res = m_Module.IsPermittedSubscription(wsUser, wsPassword, sID, subID, ref reason);


                logger.InfoFormat("Protocol: IsPermittedSubscription, Parameters : userID : {0}", sID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : IsPermittedSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public UserCAStatus GetUserCAStatus(string siteGuid)
        {
            UserCAStatus retVal = UserCAStatus.Annonymus;
            string wsUser = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultUser;
            string wsPass = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPass);
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    retVal = m_Module.GetUserCAStatus(wsUser, wsPass, siteGuid);
                    logger.InfoFormat("Protocol: GetUserStatus, Parameters : SiteGuid : {0}", siteGuid);

                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetUserStatus, Error Message: {0}, Parameters :  SiteGuid: {1}", ex.Message, siteGuid);
                }
            }

            return retVal;
        }

        public UserCAStatus GetUserCAStatus()
        {
            UserCAStatus retVal = UserCAStatus.Annonymus;

            string sKey = string.Format("GetUserCAStatus_{0}", UsersService.Instance.GetUserID());
            // return object from cache if exist
            object oFromCache = HttpRuntime.Cache[sKey];

            if (oFromCache != null && oFromCache is UserCAStatus) return (UserCAStatus)oFromCache;

            string wsUser = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultUser;
            string wsPass = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPass);
            if (UsersService.Instance.UserContext.UserResponse != null && UsersService.Instance.UserContext.UserResponse.m_user != null)
            {
                string siteGuid = UsersService.Instance.GetUserID();
                try
                {
                    retVal = m_Module.GetUserCAStatus(wsUser, wsPass, siteGuid);

                    HttpRuntime.Cache.Add(sKey, retVal, null, DateTime.UtcNow.AddSeconds(30), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Low, null);
                    logger.InfoFormat("Protocol: GetUserCAStatus, Parameters : SiteGuid : {0}", siteGuid);

                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetItemsPrice, Error Message: {0}, Parameters :  SiteGuid: {1}", ex.Message, siteGuid);
                }
            }
            else
            {
                retVal = UserCAStatus.Annonymus;
            }
            return retVal;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionPricesWithCoupon(string sSubscriptions, string couponCode, bool LowerPrice)
        {
            SubscriptionsPricesContainer[] returnObject = null;
            string wsUser;
            string wsPassword;

            GetWSMethodUserPass(eWSMethodName.GetItemsPrice, out wsUser, out wsPassword);

            try
            {
                returnObject = m_Module.GetSubscriptionsPricesSTWithCoupon(wsUser, wsPassword, sSubscriptions, UsersService.Instance.GetUserID(), couponCode, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionPricesWithCoupon, Error Message: {0}, Parameters : User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }
            return returnObject;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionsPrices(string guid, string[] sSubscriptions, bool LowerPrice)
        {
            SubscriptionsPricesContainer[] returnObject = null;
            string wsUser;
            string wsPassword;

            GetWSMethodUserPass(eWSMethodName.GetItemsPrice, out wsUser, out wsPassword);

            try
            {
                returnObject = m_Module.GetSubscriptionsPrices(wsUser, wsPassword, sSubscriptions, guid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPrices, Error Message: {0}, Parameters : User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }
            return returnObject;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionsPrices(string[] sSubscriptions, bool LowerPrice)
        {
            SubscriptionsPricesContainer[] returnObject = null;
            string wsUser;
            string wsPassword;

            GetWSMethodUserPass(eWSMethodName.GetItemsPrice, out wsUser, out wsPassword);

            try
            {
                returnObject = m_Module.GetSubscriptionsPrices(wsUser, wsPassword, sSubscriptions, UsersService.Instance.GetUserID(), string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPrices, Error Message: {0}, Parameters : User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }
            return returnObject;
        }

        public Dictionary<int, MediaFileItemPricesContainer> GetItemsPrice(int[] MediaFiles, bool LowestPrice)
        {
            string sKey = string.Empty;
            if (string.IsNullOrEmpty(UsersService.Instance.GetUserID()))
            {
                StringBuilder builder = new StringBuilder();
                Array.ForEach(MediaFiles, x => builder.Append(x));
                string sMediaList = builder.ToString();

                sKey = string.Format("{0}_{1}_{2}_{3}", UsersService.Instance.GetUserID(), sMediaList, LowestPrice.ToString(), SessionHelper.LocaleInfo != null ? SessionHelper.LocaleInfo.GetType().GUID.ToString() : string.Empty);

                // return object from cache if exist
                object oFromCache = DataHelper.GetCacheObject(sKey);
                if (oFromCache != null && oFromCache is Dictionary<int, MediaFileItemPricesContainer>) return (oFromCache as Dictionary<int, MediaFileItemPricesContainer>);
            }

            // Get user and password for method
            string wsUser;
            string wsPassword;
            MediaFileItemPricesContainer[] res = null;
            StringBuilder sb = new StringBuilder();

            GetWSMethodUserPass(eWSMethodName.GetItemsPrice, out wsUser, out wsPassword);

            try
            {
                if (SessionHelper.LocaleInfo != null)
                {
                    res = m_Module.GetItemsPrices(wsUser, wsPassword, MediaFiles, UsersService.Instance.GetUserID(), LowestPrice, SessionHelper.LocaleInfo.LocaleCountry.ToString(), SessionHelper.LocaleInfo.LocaleLanguage, SessionHelper.LocaleInfo.LocaleDevice, SiteHelper.GetClientIP());
                }
                else
                {
                    res = m_Module.GetItemsPrices(wsUser, wsPassword, MediaFiles, UsersService.Instance.GetUserID(), LowestPrice, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
                }

                if (string.IsNullOrEmpty(UsersService.Instance.GetUserID()))
                {
                    DataHelper.SetCacheObject(sKey, MapItemsPrice(res));
                }

                foreach (int media in MediaFiles)
                {
                    sb.Append(media.ToString() + ";");
                }
                logger.InfoFormat("Protocol: GetItemsPrice, Parameters : Medias : {0}, userID : {1}", sb.ToString(), UsersService.Instance.GetUserID());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetItemsPrice, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return MapItemsPrice(res);
        }

        public Dictionary<int, MediaFileItemPricesContainer> GetItemsPrice(int[] MediaFiles, string wsUser, string wsPass, string userGuid, bool LowestPrice)
        {
            string sKey = string.Empty;
            MediaFileItemPricesContainer[] res = null;
            StringBuilder sb = new StringBuilder();

            try
            {

                res = m_Module.GetItemsPrices(wsUser, wsPass, MediaFiles, userGuid, LowestPrice, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());

                foreach (int media in MediaFiles)
                {
                    sb.Append(media.ToString() + ";");
                }


            }
            catch (Exception)
            {
                //Handle Error
            }

            return MapItemsPrice(res);
        }

        public Dictionary<int, MediaFileItemPricesContainer> GetItemsPriceWithCoupon(int[] iMediaFiles, string sCouponCode, bool bOnlyLowest)
        {
            MediaFileItemPricesContainer[] result = null;

            // Get user and password for method
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetItemsPrice, out wsUser, out wsPassword);

            try
            {
                if (SessionHelper.LocaleInfo != null)
                {
                    var res = m_Module.GetItemsPricesWithCoupons(wsUser, wsPassword, iMediaFiles, UsersService.Instance.GetUserID(), sCouponCode, bOnlyLowest, SessionHelper.LocaleInfo.LocaleCountry.ToString(), SessionHelper.LocaleInfo.LocaleLanguage, SessionHelper.LocaleInfo.LocaleDevice, SiteHelper.GetClientIP());
                    if (res != null)
                        result = res.ItemsPrices;
                }
                else
                {
                    var res = m_Module.GetItemsPricesWithCoupons(wsUser, wsPassword, iMediaFiles, UsersService.Instance.GetUserID(), sCouponCode, bOnlyLowest, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
                    if (res != null)
                        result = res.ItemsPrices;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetItemsPriceWithCoupons, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return MapItemsPrice(result);
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions()
        {
            return GetUserPermitedSubscriptions(UsersService.Instance.GetUserID());
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(string guid)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            PermittedSubscriptionContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                res = m_Module.GetUserPermittedSubscriptions(wsUser, wsPassword, guid);

                logger.InfoFormat("Protocol: GetUserPermittedSubscriptions, Parameters : userID : {0}", UsersService.Instance.GetUserID());

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedSubscriptions, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return res;
        }

        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(string guid, int numOfItems)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            PermittedSubscriptionContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserExpiredSubscriptions, out wsUser, out wsPassword);

            try
            {
                res = m_Module.GetUserExpiredSubscriptions(wsUser, wsPassword, guid, numOfItems);

                logger.InfoFormat("Protocol: GetUserExpiredSubscriptions, Parameters : userID : {0}", guid);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserExpiredSubscriptions, Error Message: {0}, Parameters :  User: {1}", ex.Message, guid);
            }

            return res;
        }

        public bool CancelSubscription(string sUserGuid, string sSubID, Int32 nSubPurchaseID)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            bool res = false;
            GetWSMethodUserPass(eWSMethodName.CancelSubscription, out wsUser, out wsPassword);

            try
            {
                res = m_Module.CancelSubscription(wsUser, wsPassword, sUserGuid, sSubID, nSubPurchaseID);

                logger.InfoFormat("Protocol: CancelSubscription, Parameters : User : {0} subID : {1}", sUserGuid, sSubID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CancelSubscription, Error Message: {0}, Parameters :  User: {1} subID : {2}", ex.Message, sUserGuid, sSubID);
            }

            return res;
        }

        public bool RenewCancledSubscription(string sUserGuid, string sSubID, Int32 nSubPurchaseID)
        {
            // Get user and password for method
            string wsUser;
            string wsPassword;
            bool res = false;
            GetWSMethodUserPass(eWSMethodName.RenewCancledSubscription, out wsUser, out wsPassword);

            try
            {
                res = m_Module.RenewCancledSubscription(wsUser, wsPassword, sUserGuid, sSubID, nSubPurchaseID);

                logger.InfoFormat("Protocol: RenewCancledSubscription, Parameters : User : {0} subID : {1}", sUserGuid, sSubID);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RenewCancledSubscription, Error Message: {0}, Parameters :  User: {1} subID : {2}", ex.Message, sUserGuid, sSubID);
            }

            return res;
        }


        public BillingResponse ChargeUser_ForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCouponCode, string sCCVariant, ref string sExternalReceiptCode, ref string sReceiptCode)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;

                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                if (sCCVariant == "M1")
                {
                    response = m_Module.Cellular_ChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, sPPVModuleCode, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice);
                }
                else
                {
                    var res = m_Module.CC_ChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, sPPVModuleCode, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice, string.Empty, string.Empty);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling web service protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            sExternalReceiptCode = response.m_sExternalReceiptCode;
            sReceiptCode = response.m_sRecieptCode;
            return response;
        }

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCouponCode, ref string sExternalReceiptCode, ref string sReceiptCode)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;

                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                var res = m_Module.CC_ChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, sPPVModuleCode, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice, string.Empty, string.Empty);
                if (res != null)
                {
                    response = res.BillingResponse;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling web service protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            sExternalReceiptCode = response.m_sExternalReceiptCode;
            sReceiptCode = response.m_sRecieptCode;
            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCouponCode)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;

                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }
                var res = m_Module.CC_ChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, sPPVModuleCode, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice, string.Empty, string.Empty);
                if (res != null)
                {
                    response = res.BillingResponse;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string couponCode)
        {
            return DummyChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, sUserIP, couponCode, string.Empty);
        }

        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string couponCode, string sExtraParameters)
        {
            string result = string.Empty;
            BillingResponse response = Dummy_ChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, sUserIP, couponCode, sExtraParameters);
            if (response != null)
            {
                result = response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
            }
            return result;
        }

        public BillingResponse Dummy_ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string couponCode, string sExtraParameters)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {

                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                response = m_Module.CC_DummyChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, sPPVModuleCode, couponCode, sUserIP, sExtraParameters, sLocaleCountry, sLocaleLanguage, sLocaleDevice);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response;
        }

        public string DummyChargeUserForPrePaid(double iPrice, string sCurrency, string iFileID, string sPPVModuleCode, string sUserIP, string couponCode)
        {
            return DummyChargeUserForPrePaid(iPrice, sCurrency, iFileID, sPPVModuleCode, sUserIP, couponCode, string.Empty);
        }


        public string DummyChargeUserForPrePaid(double iPrice, string sCurrency, string iFileID, string sPPVModuleCode, string sUserIP, string couponCode, string sExtraParameters)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {

                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                response = m_Module.CC_DummyChargeUserForPrePaid(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, iFileID, couponCode, sUserIP, sExtraParameters, sLocaleCountry, sLocaleLanguage, sLocaleDevice);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : DummyChargeUserForPrePaid, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(int startIndex, int count)
        {
            BillingTransactionsResponse retVal = null;
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserTransactionHistory, out wsUser, out wsPassword);
            try
            {
                BillingTransactions response = m_Module.GetUserBillingHistory(wsUser, wsPassword, UsersService.Instance.GetUserID(), startIndex, count, TransactionHistoryOrderBy.CreateDateDesc);
                if (response != null)
                {
                    retVal = response.transactions;
                }
                return retVal;

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserBillingHistory, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }
            return retVal;
        }

        public BillingResponse ChargeUser_ForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string sCouponCode, string sCCVariant, ref string sExternalCode, ref string sReceiptCode, string sExtraParams)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                if (sCCVariant == "M1")
                {
                    response = m_Module.Cellular_ChargeUserForSubscription(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParams, sLocaleCountry, sLocaleLanguage, sLocaleDevice);
                }
                else
                {
                    var res = m_Module.CC_ChargeUserForSubscription(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParams, sLocaleCountry, sLocaleLanguage, sLocaleDevice, string.Empty, string.Empty);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }


            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            if (response != null)
            {
                sExternalCode = response.m_sExternalReceiptCode;
                sReceiptCode = response.m_sRecieptCode;
            }
            return response;
        }

        public BillingResponse ChargeUser_ForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string sCouponCode, string sCCVariant, ref string sExternalCode, ref string sReceiptCode)
        {
            return ChargeUser_ForSubscription(iPrice, sCurrency, sSubscriptionID, sUserIP, sCouponCode, sCCVariant, ref sExternalCode, ref sReceiptCode, string.Empty);
        }

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string sCouponCode, ref string sExternalCode, ref string sReceiptCode)
        {
            string result = string.Empty;
            BillingResponse response = ChargeUser_ForSubscription(iPrice, sCurrency, sSubscriptionID, sUserIP, sCouponCode, string.Empty, ref sExternalCode, ref sReceiptCode);

            if (response != null)
            {
                sExternalCode = response.m_sExternalReceiptCode;
                sReceiptCode = response.m_sRecieptCode;
                result = response.m_oStatus.ToString();
            }
            return result;
        }

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string sCouponCode)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }
                var res = m_Module.CC_ChargeUserForSubscription(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice, string.Empty, string.Empty);
                if (res != null)
                {
                    response = res.BillingResponse;
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response.m_oStatus.ToString();
        }

        public string ChargeUserForPrepaid(double iPrice, string sCurrency, string sPrepaidId, string sUserIP, string sCouponCode)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }
                response = m_Module.CC_ChargeUserForPrePaid(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, sPrepaidId, sCouponCode, sUserIP, "", sLocaleCountry, sLocaleLanguage, sLocaleDevice);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForPrepaid, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response.m_oStatus.ToString();
        }

        public string DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string couponCode)
        {
            return DummyChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sUserIP, couponCode, string.Empty);
        }

        public string DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string couponCode, string sExtraParameters)
        {
            string result = string.Empty;
            BillingResponse response = null;
            response = Dummy_ChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sUserIP, couponCode, sExtraParameters);
            if (response != null)
            {
                result = response.m_oStatus.ToString();
            }
            return result;
        }

        public BillingResponse Dummy_ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sUserIP, string couponCode, string sExtraParameters)
        {
            BillingResponse response = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }
                response = m_Module.CC_DummyChargeUserForSubscription(wsUser, wsPassword, UsersService.Instance.GetUserID(), iPrice, sCurrency, sSubscriptionID, couponCode, sUserIP, sExtraParameters, sLocaleCountry, sLocaleLanguage, sLocaleDevice);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return response;
        }


        public string GetLicensedLinkWithCoupon(int iFileID, string sBaseURL, string sUserIP, string sCoupon)
        {
            string sRet = string.Empty;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermitedSubscriptions, out wsUser, out wsPassword);

            try
            {
                string sLocaleLanguage = string.Empty;
                string sLocaleCountry = string.Empty;
                string sLocaleDevice = string.Empty;
                if (SessionHelper.LocaleInfo != null)
                {
                    sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                    sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                    sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
                }

                //XXX: 
                sLocaleDevice = SessionHelper.DeviceDNA;

                sRet = m_Module.GetLicensedLinkWithCoupon(wsUser, wsPassword, UsersService.Instance.GetUserID(), iFileID, sBaseURL, sUserIP, string.Empty, sLocaleCountry, sLocaleLanguage, sLocaleDevice, sCoupon);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLicensedLinkWithCoupon, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return sRet;
        }

        public int GetCustomDataID(int type, string method, double dPrice, string sCurrencyCode, int campID, string couponCode, int assetID, string sModuleCode, string userIP, string overrideEndDate, string previewModuleID)
        {
            int retVal = 0;

            // Get user and password for method
            string wsUser;
            string wsPassword;
            //PermittedMediaContainer[] res = null;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            string sLocaleLanguage = string.Empty;
            string sLocaleCountry = string.Empty;
            string sLocaleDevice = string.Empty;
            if (SessionHelper.LocaleInfo != null)
            {
                sLocaleLanguage = SessionHelper.LocaleInfo.LocaleLanguage;
                sLocaleCountry = SessionHelper.LocaleInfo.LocaleCountry;
                sLocaleDevice = SessionHelper.LocaleInfo.LocaleDevice;
            }

            try
            {
                retVal = m_Module.GetCustomDataID(wsUser, wsPassword, UsersService.Instance.GetUserID(), dPrice, sCurrencyCode, assetID, sModuleCode, campID.ToString(), couponCode, method, userIP, sLocaleCountry, sLocaleLanguage, sLocaleDevice, type, overrideEndDate, previewModuleID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCustomDataID, Error Message: {0}, Parameters :  type: {1}", ex.Message, type.ToString());
            }

            return retVal;
        }

        public PrePaidHistoryResponse GetUserPrePaidHistory(int numOfItems)
        {
            PrePaidHistoryResponse retVal = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                retVal = m_Module.GetUserPrePaidHistory(wsUser, wsPassword, UsersService.Instance.GetUserID(), numOfItems);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPrePaidHistory, Error Message: {0}, Parameters :  ", ex.Message);
            }

            return retVal;
        }

        public PrePaidResponse PP_ChargeUserForMediaFile(double price, string currencyCode, int fileId, int ppvModuleCode, string couponCode, string userIP)
        {
            PrePaidResponse retVal = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                retVal = m_Module.PP_ChargeUserForMediaFile(wsUser, wsPassword, UsersService.Instance.GetUserID(), price, currencyCode, fileId, ppvModuleCode.ToString(), couponCode, userIP, string.Empty,
                    string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : PP_ChargeUserForMediaFile, Error Message: {0}, Parameters :  ", ex.Message);
            }

            return retVal;
        }

        public PrePaidResponse PP_ChargeUserForSubscription(double price, string currencyCode, int subCode, string couponCode, string userIP)
        {
            PrePaidResponse retVal = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                retVal = m_Module.PP_ChargeUserForSubscription(wsUser, wsPassword, UsersService.Instance.GetUserID(), price, currencyCode, subCode.ToString(), couponCode, userIP, string.Empty,
                    string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : PP_ChargeUserForSubscription, Error Message: {0}, Parameters :  ", ex.Message);
            }

            return retVal;
        }

        public PrePaidPricesContainer[] GetPrePaidPrices(string[] prepaids, string currencyCode, string couponCode)
        {
            PrePaidPricesContainer[] retVal = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                retVal = m_Module.GetPrePaidPrices(wsUser, wsPassword, prepaids, UsersService.Instance.GetUserID(), couponCode, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPrePaidPrices, Error Message: {0}, Parameters :  ", ex.Message);
            }

            return retVal;
        }

        public UserPrePaidContainer GetUserPrePaidStatus(string currencyCode)
        {
            UserPrePaidContainer retVal = null;

            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                retVal = m_Module.GetUserPrePaidStatus(wsUser, wsPassword, UsersService.Instance.GetUserID(), currencyCode);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPrePaidStatus, Error Message: {0}, Parameters :  ", ex.Message);
            }

            return retVal;
        }

        public bool ActivateCampaign(long campID, string mediaLink, int mediaID, string voucherRecName, string voucherRecEmail, string voucherSendName, string voucherSendEmail)
        {
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                return m_Module.ActivateCampaign(wsUser, wsPassword, (int)campID, new CampaignActionInfo()
                {
                    m_mediaLink = mediaLink,
                    m_mediaID = mediaID,
                    m_siteGuid = int.Parse(UsersService.Instance.GetUserID()),
                    m_senderName = voucherSendName,
                    m_senderEmail = voucherSendEmail,
                    m_voucherReceipents = new VoucherReceipentInfo[] { new VoucherReceipentInfo() { m_emailAdd = voucherRecEmail, m_receipentName = voucherRecName } }
                });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ActivateCampaign, Error Message: {0}, Parameters :  mediaID : {1}, SiteGUID : {2}, campID: {3}", ex.Message, mediaID, UsersService.Instance.GetUserID(), campID);
            }

            return false;
        }

        public CampaignActionInfo ActivateCampaignWithInfo(long campID, string inviteHash)
        {
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                return m_Module.ActivateCampaignWithInfo(wsUser, wsPassword, (int)campID, new CampaignActionInfo()
                {
                    m_siteGuid = int.Parse(UsersService.Instance.GetUserID()),
                    m_socialInviteInfo = !string.IsNullOrEmpty(inviteHash) ? new SocialInviteInfo() { m_hashCode = inviteHash } : null
                });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ActivateCampaignWithInfo, Error Message: {0}, Parameters : SiteGUID : {1}, campID: {2}", ex.Message, UsersService.Instance.GetUserID(), campID);
            }

            return null;
        }

        public string GetItemLeftViewLifeCycle(string m_wsUserCa, string m_wsPassCa, string m_mediaFileGuid, string m_userName, string m_country, string m_language, string m_device, bool bIsCoGuid)
        {
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                return m_Module.GetItemLeftViewLifeCycle(wsUser, wsPassword, m_mediaFileGuid, m_userName, bIsCoGuid, m_country, m_language, m_device);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetItemLeftViewLifeCycle, Error Message: {0}, Parameters :  mediaGuid : {1}, SiteGUID : {2}", ex.Message, m_mediaFileGuid, m_userName);
            }

            return null;
        }

        public ChangeSubscriptionStatus ChangeSubscription(string sSiteGuid, int nOldSubscription, int nNewSubscription)
        {
            string wsUser;
            string wsPassword;
            GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPassword);

            try
            {
                return m_Module.ChangeSubscription(wsUser, wsPassword, sSiteGuid, nOldSubscription, nNewSubscription);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeSubscription, Error Message: {0}, Parameters :  sSiteGuid : {1}, nOldSubscription : {2}, nNewSubscription : {3}", ex.Message, sSiteGuid, nOldSubscription, nNewSubscription);
            }

            return ChangeSubscriptionStatus.Error;
        }


        #endregion
        #region Private Methods
        private void GetWSMethodUserPass(eWSMethodName WSMethodName, out string User, out string Pass)
        {
            // Get user and password for method
            MethodCredentials MethodName = new MethodCredentials();

            // in case username/password already exsist in members wsUserName, wsPassword (used in TVPApi);
            if (!string.IsNullOrEmpty(wsUserName) && !string.IsNullOrEmpty(wsPassword))
            {
                User = wsUserName;
                Pass = wsPassword;
                return;
            }

            switch (WSMethodName)
            {
                case eWSMethodName.GetUserPermittedItems:
                    MethodName = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.GetUserPermittedItems;
                    break;
                case eWSMethodName.GetItemsPrice:
                    //  MethodName = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.;
                    break;
                case eWSMethodName.GetUserCAStatus:
                    break;
            }

            if (!string.IsNullOrEmpty(MethodName.UserName) && !string.IsNullOrEmpty(MethodName.Password))
            {
                User = MethodName.UserName;
                Pass = MethodName.Password;
            }
            else
            {
                User = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultUser;
                Pass = PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.DefaultPassword;
            }

            logger.InfoFormat("Calling web service for GetUserPermittedItems method with user:{0} and pass:{1}", User, Pass);
        }

        private Dictionary<int, MediaFileItemPricesContainer> MapItemsPrice(MediaFileItemPricesContainer[] MediaFileItemContainer)
        {
            Dictionary<int, MediaFileItemPricesContainer> ItemsDictionary = new Dictionary<int, MediaFileItemPricesContainer>();

            try
            {
                foreach (MediaFileItemPricesContainer MediaFileItemPrices in MediaFileItemContainer)
                {
                    if (!ItemsDictionary.ContainsKey(MediaFileItemPrices.m_nMediaFileID))
                        ItemsDictionary.Add(MediaFileItemPrices.m_nMediaFileID, MediaFileItemPrices);
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error in ConditionalAccessService : MapItemsPrice, Error Message: {0}, Parameters :  User: {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return ItemsDictionary;
        }


        #endregion
    }
}