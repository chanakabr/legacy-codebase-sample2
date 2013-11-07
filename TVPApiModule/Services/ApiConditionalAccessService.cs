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

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID)
        {
            BillingResponse response = null;

            try
            {
                response = m_Module.CC_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, sUDID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
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

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID)
        {
            BillingResponse response = null;

            try
            {
                response = m_Module.CC_ChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public bool CancelSubscription(string sUserGuid, string sSubscriptionID, int nSubscriptionPurchaseID)
        {
            bool response = false;

            try
            {
                response = m_Module.CancelSubscription(m_wsUserName, m_wsPassword, sUserGuid, sSubscriptionID, nSubscriptionPurchaseID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CancelSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response;
        }

        public BillingResponse InAppChargeUserForSubscription(double iPrice, string sCurrency, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sProductCode, string sReceipt)
        {
            BillingResponse response = null;

            try
            {
                response = m_Module.InApp_ChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sProductCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sReceipt);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : InAppChargeUserForSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response;
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
                response = m_Module.GetItemsPrices(m_wsUserName, m_wsPassword, fileArray, sSiteGuid, bOnlyLowest, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
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
                returnObject = m_Module.GetSubscriptionsPrices(m_wsUserName, m_wsPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPrices, Error Message: {0}, Parameters : User: {1}", ex.Message, sSiteGuid);
            }
            return returnObject;
        }

        //public SubscriptionsPricesContainer[] GetSubscriptionsPricesByIP(string sSiteGuid, string[] sSubscriptions, bool LowerPrice)
        //{
        //    SubscriptionsPricesContainer[] returnObject = null;

        //    try
        //    {
        //        returnObject = m_Module.GetSubscriptionsPricesByIP(m_wsUserName, m_wsPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPricesByIP, Error Message: {0}, Parameters : User: {1}", ex.Message, sSiteGuid);
        //    }
        //    return returnObject;
        //}

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

        public UserCAStatus GetUserCAStatus(string siteGuid)
        {
            UserCAStatus retVal = UserCAStatus.Annonymus;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            //GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPass);
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

        public CampaignActionInfo ActivateCampaignWithInfo(string siteGuid, long campID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            CampaignActionInfo campaignActionInfo = null;

            try
            {
                CampaignActionInfo campaignActionInfoParam = new CampaignActionInfo()
                {
                    m_siteGuid = int.Parse(siteGuid),
                    m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new SocialInviteInfo() { m_hashCode = hashCode } : null,
                    m_mediaID = mediaID,
                    m_mediaLink = mediaLink,
                    m_senderEmail = senderEmail,
                    m_senderName = senderName,
                    m_status = status,
                    m_voucherReceipents = voucherReceipents
                };
                campaignActionInfo = m_Module.ActivateCampaignWithInfo(m_wsUserName, m_wsPassword, (int)campID, campaignActionInfoParam);
                logger.InfoFormat("Protocol: ActivateCampaignWithInfo, Parameters : campID : {0}", campID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ActivateCampaignWithInfo, Error Message: {0}, Parameters :  CampID: {1}", ex.Message, campID);
            }

            return campaignActionInfo;
        }

        public int AD_GetCustomDataID(string siteGuid,
                                      double price,
                                      string currencyCode3,
                                      int assetId,
                                      string ppvModuleCode,
                                      string campaignCode,
                                      string couponCode,
                                      string paymentMethod,
                                      string userIp,
                                      string countryCd2,
                                      string languageCode3,
                                      string deviceName,
                                      int assetType)
        {
            int res = 0;

            try
            {
                res = m_Module.AD_GetCustomDataID(m_wsUserName,
                                                  m_wsPassword,
                                                  siteGuid,
                                                  price,
                                                  currencyCode3,
                                                  assetId,
                                                  ppvModuleCode,
                                                  campaignCode,
                                                  couponCode,
                                                  paymentMethod,
                                                  userIp,
                                                  countryCd2,
                                                  languageCode3,
                                                  deviceName,
                                                  assetType);

                logger.InfoFormat("Protocol: AD_GetCustomDataID, Parameters : Parameters : siteGuid - {0}, price - {1}, currencyCode3 - {2}, assetId - {3}, ppvModuleCode - {4}, campaignCode - {5}, couponCode - {6}, paymentMethod - {7}, userIp - {8}, countryCd2 - {9}, languageCode3 - {10}, deviceName - {11}, assetType - {12}",
                                  siteGuid,
                                  price,
                                  currencyCode3,
                                  assetId,
                                  ppvModuleCode,
                                  campaignCode,
                                  couponCode,
                                  paymentMethod,
                                  userIp,
                                  countryCd2,
                                  languageCode3,
                                  deviceName,
                                  assetType);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AD_GetCustomDataID, Error Message: {0}, Parameters : siteGuid - {1}, price - {2}, currencyCode3 - {3}, assetId - {4}, ppvModuleCode - {5}, campaignCode - {6}, couponCode - {7}, paymentMethod - {8}, userIp - {9}, countryCd2 - {10}, languageCode3 - {11}, deviceName - {12}, assetType - {13}",
                                    ex.Message,
                                    siteGuid,
                                    price,
                                    currencyCode3,
                                    assetId,
                                    ppvModuleCode,
                                    campaignCode,
                                    couponCode,
                                    paymentMethod,
                                    userIp,
                                    countryCd2,
                                    languageCode3,
                                    deviceName,
                                    assetType);
            }
            return res;
        }

        public int GetCustomDataID(string siteGuid,
                                   double price,
                                   string currencyCode3,
                                   int assetId,
                                   string ppvModuleCode,
                                   string campaignCode,
                                   string couponCode,
                                   string paymentMethod,
                                   string userIp,
                                   string countryCd2,
                                   string languageCode3,
                                   string deviceName,
                                   int assetType,
                                   string overrideEndDate)
        {
            int res = 0;

            try
            {
                res = m_Module.GetCustomDataID(m_wsUserName,
                                                  m_wsPassword,
                                                  siteGuid,
                                                  price,
                                                  currencyCode3,
                                                  assetId,
                                                  ppvModuleCode,
                                                  campaignCode,
                                                  couponCode,
                                                  paymentMethod,
                                                  userIp,
                                                  countryCd2,
                                                  languageCode3,
                                                  deviceName,
                                                  assetType,
                                                  overrideEndDate);

                logger.InfoFormat("Protocol: GetCustomDataID, Parameters : Parameters : siteGuid - {0}, price - {1}, currencyCode3 - {2}, assetId - {3}, ppvModuleCode - {4}, campaignCode - {5}, couponCode - {6}, paymentMethod - {7}, userIp - {8}, countryCd2 - {9}, languageCode3 - {10}, deviceName - {11}, assetType - {12}, overrideEndDate - {13}",
                                  siteGuid,
                                  price,
                                  currencyCode3,
                                  assetId,
                                  ppvModuleCode,
                                  campaignCode,
                                  couponCode,
                                  paymentMethod,
                                  userIp,
                                  countryCd2,
                                  languageCode3,
                                  deviceName,
                                  assetType,
                                  overrideEndDate);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCustomDataID, Error Message: {0}, Parameters : siteGuid - {1}, price - {2}, currencyCode3 - {3}, assetId - {4}, ppvModuleCode - {5}, campaignCode - {6}, couponCode - {7}, paymentMethod - {8}, userIp - {9}, countryCd2 - {10}, languageCode3 - {11}, deviceName - {12}, assetType - {13}, overrideEndDate - {14}",
                                    ex.Message,
                                    siteGuid,
                                    price,
                                    currencyCode3,
                                    assetId,
                                    ppvModuleCode,
                                    campaignCode,
                                    couponCode,
                                    paymentMethod,
                                    userIp,
                                    countryCd2,
                                    languageCode3,
                                    deviceName,
                                    assetType,
                                    overrideEndDate);
            }
            return res;
        }

        public bool ActivateCampaign(string siteGuid, int campaignID, CampaignActionInfo actionInfo)
        {
            bool retVal = false;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("ActivateCampaign, Parameters : SiteGuid : {0} campaignID : {1} actionInfo : {2}", siteGuid, campaignID, actionInfo.ToString());
                    retVal = m_Module.ActivateCampaign(wsUser, wsPass, campaignID, actionInfo);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : ActivateCampaign, Error Message: {0}, Parameters : SiteGuid: {1} campaignID : {2} actionInfo : {3}", ex.Message, siteGuid, campaignID, actionInfo.ToString());
                }
            }
            return retVal;
        }

        public MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(string siteGuid, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            MediaFileItemPricesContainer[] retVal = null;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("GetItemsPricesWithCoupons, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);
                    retVal = m_Module.GetItemsPricesWithCoupons(wsUser, wsPass, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : GetItemsPricesWithCoupons, Error Message: {0}, Parameters : SiteGuid: {1} sCouponCode : {2}", ex.Message, siteGuid, sCouponCode);
                }
            }
            return retVal;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(string siteGuid, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] retVal = null;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("GetSubscriptionsPricesWithCoupon, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);
                    retVal = m_Module.GetSubscriptionsPricesWithCoupon(wsUser, wsPass, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : GetSubscriptionsPricesWithCoupon, Error Message: {0}, Parameters : SiteGuid: {1} sCouponCode : {2}", ex.Message, siteGuid, sCouponCode);
                }
            }
            return retVal;
        }

        public bool IsPermittedItem(string siteGuid, int mediaId)
        {
            bool retVal = false;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("IsPermittedItem, Parameters : SiteGuid : {0} mediaId : {1}", siteGuid, mediaId);
                    retVal = m_Module.IsPermittedItem(wsUser, wsPass, siteGuid, mediaId);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : IsPermittedItem, Error Message: {0}, Parameters : SiteGuid: {1} mediaId : {2}", ex.Message, siteGuid, mediaId);
                }
            }
            return retVal;
        }

        public string GetGoogleSignature(string siteGuid, int customerId)
        {
            string retVal = string.Empty;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("GetGoogleSignature, Parameters : SiteGuid : {0} customerId : {1}", siteGuid, customerId);
                    retVal = m_Module.GetGoogleSignature(wsUser, wsPass, customerId);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : GetGoogleSignature, Error Message: {0}, Parameters : SiteGuid: {1} customerId : {2}", ex.Message, siteGuid, customerId);
                }
            }
            return retVal;
        }

        public bool IsPermittedSubscription(string siteGuid, int subId)
        {
            bool retVal = false;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("IsPermittedSubscription, Parameters : SiteGuid : {0} subId : {1}", siteGuid, subId);

                    string reason = string.Empty;

                    retVal = m_Module.IsPermittedSubscription(wsUser, wsPass, siteGuid, subId, ref reason);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : IsPermittedSubscription, Error Message: {0}, Parameters : SiteGuid: {1} subId : {2}", ex.Message, siteGuid, subId);
                }
            }
            return retVal;
        }

        public BillingResponse InApp_ChargeUserForMediaFile(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName, string ReceiptData)
        {
            BillingResponse retVal = null;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("InApp_ChargeUserForMediaFile, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                    retVal = m_Module.InApp_ChargeUserForMediaFile(wsUser, wsPass, siteGuid, price, currency, productCode, ppvModuleCode, string.Empty, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName, ReceiptData);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : InApp_ChargeUserForMediaFile, Error Message: {0}, Parameters : SiteGuid: {1} productCode : {2}", ex.Message, siteGuid, productCode);
                }
            }
            return retVal;
        }

        public BillingResponse CC_ChargeUserForPrePaid(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName)
        {
            BillingResponse retVal = null;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPass = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    logger.InfoFormat("CC_ChargeUserForPrePaid, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                    retVal = m_Module.CC_ChargeUserForPrePaid(wsUser, wsPass, siteGuid, price, currency, productCode, ppvModuleCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : CC_ChargeUserForPrePaid, Error Message: {0}, Parameters : SiteGuid: {1} productCode : {2}", ex.Message, siteGuid, productCode);
                }
            }
            return retVal;
        }

        public string GetEPGLicensedLink(string siteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            string res = string.Empty;
            string wsUser = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            string wsPassword = ConfigManager.GetInstance().GetConfig(m_groupID, m_platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;

            try
            {
                return m_Module.GetEPGLicensedLink(wsUser, wsPassword, siteGUID, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceName, formatType);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetEPGLicensedLink, Error Message: {0}, Parameters : MediaFileID : {1}, EPGItemID : {2}, UserIP: {3}", ex.Message, mediaFileID, EPGItemID, userIP);
            }
            return res;
        }

        public UserBillingTransactionsResponse[] GetUsersBillingHistory(string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            UserBillingTransactionsResponse[] retVal = null;

            try
            {
                retVal = m_Module.GetUsersBillingHistory(m_wsUserName, m_wsPassword, siteGuids, startDate, endDate);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUsersBillingHistory, Error Message: {0}, Parameters :  startDate: {1}, endDate: {2}", ex.Message, startDate, endDate);
            }
            return retVal;
        }

        public DomainBillingTransactionsResponse[] GetDomainsBillingHistory(int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            DomainBillingTransactionsResponse[] retVal = null;

            try
            {
                retVal = m_Module.GetDomainsBillingHistory(m_wsUserName, m_wsPassword, domainIDs, startDate, endDate);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainsBillingHistory, Error Message: {0}, Parameters :  startDate: {1}, endDate: {2}", ex.Message, startDate, endDate);
            }
            return retVal;
        }
        

        #endregion
    }
}
