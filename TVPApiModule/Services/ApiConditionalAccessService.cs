using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.Configuration.Site;
using TVPPro.SiteManager.Helper;

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
        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID)
        {
            BillingResponse response = null;

            try
            {                
                response = m_Module.CC_DummyChargeUserForMediaFile(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, sUDID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CC_DummyChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID)
        {
            BillingResponse response = null;

            try
            {
                response = m_Module.CC_DummyChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : DummyChargeUserForSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public PermittedMediaContainer[] GetUserPermittedItems(string sSiteGuid)
        {
            PermittedMediaContainer[] response = null;

            try
            {
                response = m_Module.GetUserPermittedItems(m_wsUserName, m_wsPassword, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public MediaFileItemPricesContainer[] GetItemsPrice(int[] fileArray, string sSiteGuid, bool bOnlyLowest)
        {
            MediaFileItemPricesContainer[] response = null;
            
            try
            {
                response = m_Module.GetItemsPrices(m_wsUserName, m_wsPassword, fileArray, sSiteGuid, bOnlyLowest, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetItemsPrice, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(string sSiteGuid)
        {
            PermittedSubscriptionContainer[] response = null;

            try
            {
                response = m_Module.GetUserPermittedSubscriptions(m_wsUserName, m_wsPassword, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermitedSubscriptions, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public BillingTransactionsResponse GetUserTransactionHistory(string sSiteGuid, int startIndex, int count)
        {
            BillingTransactionsResponse retVal = null;

            try
            {
                retVal = m_Module.GetUserBillingHistory(m_wsUserName, m_wsPassword, sSiteGuid, startIndex, count);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserBillingHistory, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }
            return retVal;
        }

        public PermittedMediaContainer[] GetUserExpiredItems(string sSiteGuid, int numOfItems)
        {
            PermittedMediaContainer[] retVal = null;
            
            try
            {
                retVal = m_Module.GetUserExpiredItems(m_wsUserName, m_wsPassword, sSiteGuid, numOfItems);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserExpiredItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }
            return retVal;
        }

        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(string sSiteGuid, int numOfItems)
        {
            PermittedSubscriptionContainer[] retVal = null;

            try
            {
                retVal = m_Module.GetUserExpiredSubscriptions(m_wsUserName, m_wsPassword, sSiteGuid, numOfItems);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserExpiredSubscriptions, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }
            return retVal;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionsPrices(string sSiteGuid, string[] sSubscriptions, bool LowerPrice)
        {
            SubscriptionsPricesContainer[] returnObject = null;

            try
            {
                returnObject = m_Module.GetSubscriptionsPrices(m_wsUserName, m_wsPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPrices, Error Message: {0}, Parameters : User: {1}", ex.Message, sSiteGuid);
            }
            return returnObject;
        }

        public string[] GetPrepaidBalance(string siteGuid, string currencyCode)
        {
            UserPrePaidContainer returnObject = null;

            try
            {
                returnObject = m_Module.GetUserPrePaidStatus(m_wsUserName, m_wsPassword, siteGuid, currencyCode);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPrepaidBalance, Error Message: {0}, Parameters : User: {1}", ex.Message, siteGuid);
            }

            return new string[] { (returnObject.m_nTotalAmount - returnObject.m_nAmountUsed).ToString(), returnObject.m_sCurrencyCode };
        }

        public PrePaidResponseStatus PP_ChargeUserForMediaFile(string siteGuid, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode, string udid)
        {
            PrePaidResponse returnObject = null;

            try
            {
                returnObject = m_Module.PP_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : PP_ChargeUserForMediaFile, Error Message: {0}, Parameters : User: {1}", ex.Message, siteGuid);
            }

            return returnObject.m_oStatus;
        }

        public string GetMediaLicenseLink(string siteGuid, int mediaFileID, string baseLink, string udid)
        {
            string returnObject = null;

            try
            {
                returnObject = m_Module.GetLicensedLink(m_wsUserName, m_wsPassword, siteGuid, mediaFileID, baseLink, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaLicenseLink, Error Message: {0}, Parameters : User: {1}", ex.Message, siteGuid);
            }

            return returnObject;
        }
        #endregion        
    }
}
