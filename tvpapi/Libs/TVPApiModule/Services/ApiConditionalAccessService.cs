using ApiObjects;
using ApiObjects.Billing;
using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess;
using Core.ConditionalAccess.Response;
using KLogMonitor;
using NPVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TVPApi;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.Helper;
using ClientResponseStatus = TVPApiModule.Objects.Responses.ClientResponseStatus;
using LicensedLinkResponse = Core.ConditionalAccess.LicensedLinkResponse;

namespace TVPApiModule.Services
{
    public class ApiConditionalAccessService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private int m_groupID;
        private PlatformType m_platform;

        public ApiConditionalAccessService(int groupID, PlatformType platform)
        {
            m_wsUserName = GroupsManager.GetGroup(groupID).ConditionalAccessCredentials.Username;
            m_wsPassword = GroupsManager.GetGroup(groupID).ConditionalAccessCredentials.Password;

            m_groupID = groupID;
            m_platform = platform;
        }

        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.CC_DummyChargeUserForMediaFile(m_groupID, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, sUDID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CC_DummyChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sCoupon, string sUserIP, string sUserGuid, string sUDID, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.CC_ChargeUserForMediaFile(m_groupID, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon, sUserIP, sExtraParams, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFile, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string ChargeUserForMediaFileUsingCC(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sCoupon, string sUserIP, string sUserGuid, string sUDID, string paymentMethodID, string encryptedCVV)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.CC_ChargeUserForMediaFile(m_groupID, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon, sUserIP, "", string.Empty, string.Empty, sUDID, paymentMethodID, encryptedCVV);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForMediaFileUsingCC, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return string.Format("{0}|{1}|{2}", response.m_oStatus.ToString(), response.m_sRecieptCode, response.m_sStatusDescription);
        }

        public string DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.CC_DummyChargeUserForSubscription(m_groupID, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : DummyChargeUserForSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.CC_ChargeUserForSubscription(m_groupID, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForSubscription, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public string ChargeUserForSubscriptionUsingCC(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string paymentMethodID, string encryptedCVV)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.CC_ChargeUserForSubscription(m_groupID, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, paymentMethodID, encryptedCVV);
                    if (res != null)
                    {
                        response = res.BillingResponse;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChargeUserForSubscriptionUsingCC, Error Message: {0}, Parameters :  User: {1}", ex.Message, sUserGuid);
            }

            return string.Format("{0}|{1}|{2}", response.m_oStatus.ToString(), response.m_sRecieptCode, response.m_sStatusDescription);
        }

        public bool CancelSubscription(string sUserGuid, string sSubscriptionID, int nSubscriptionPurchaseID)
        {
            bool response = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.CancelSubscription(m_groupID, sUserGuid, sSubscriptionID, nSubscriptionPurchaseID);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.InApp_ChargeUserForSubscription(m_groupID, sUserGuid, iPrice, sCurrency, sProductCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sReceipt);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.GetUserPermittedItems(m_groupID, sSiteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedItems, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public MediaFileItemPricesContainer[] GetItemsPrice(int[] fileArray, string sSiteGuid, string sUDID, bool bOnlyLowest)
        {
            MediaFileItemPricesContainer[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.GetItemsPrices(m_groupID, fileArray, sSiteGuid, bOnlyLowest, string.Empty, string.Empty, sUDID, SiteHelper.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetItemsPrice, Error Message: {0}, Parameters :  User: {1}", ex.Message, sSiteGuid);
            }

            return response;
        }

        public MediaFileItemPricesContainer[] GetItemsPrice(int[] fileArray, string sSiteGuid, bool bOnlyLowest)
        {
            return GetItemsPrice(fileArray, sSiteGuid, string.Empty, bOnlyLowest);
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(string sSiteGuid)
        {
            PermittedSubscriptionContainer[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.GetUserPermittedSubscriptions(m_groupID, sSiteGuid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    BillingTransactions response = Core.ConditionalAccess.Module.GetUserBillingHistory(m_groupID, sSiteGuid, startIndex, count, TransactionHistoryOrderBy.CreateDateDesc,
                        DateTime.MinValue, DateTime.MaxValue);
                    if (response != null)
                    {
                        retVal = response.transactions;
                    }
                    return retVal;
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetUserExpiredItems(m_groupID, sSiteGuid, numOfItems);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetUserExpiredSubscriptions(m_groupID, sSiteGuid, numOfItems);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    returnObject = Core.ConditionalAccess.Module.GetSubscriptionsPrices(m_groupID, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
                }
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
        //        returnObject = Core.ConditionalAccess.Module.GetSubscriptionsPricesByIP(m_groupID, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    returnObject = Core.ConditionalAccess.Module.GetUserPrePaidStatus(m_groupID, siteGuid, currencyCode);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    returnObject = Core.ConditionalAccess.Module.PP_ChargeUserForMediaFile(m_groupID, siteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    returnObject = Core.ConditionalAccess.Module.GetLicensedLink(m_groupID, siteGuid, mediaFileID, baseLink, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                }
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
            //GetWSMethodUserPass(eWSMethodName.GetUserPermittedItems, out wsUser, out wsPass);
            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.GetUserCAStatus(m_groupID, siteGuid);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetUserStatus, Error Message: {0}, Parameters :  SiteGuid: {1}", ex.Message, siteGuid);
                }
            }
            return retVal;
        }

        public CampaignActionInfo ActivateCampaignWithInfo(string siteGuid, long campID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           CampaignActionResult status, CampaignActionInfo.VoucherReceipentInfo[] voucherReceipents)
        {
            CampaignActionInfo campaignActionInfo = null;

            try
            {
                CampaignActionInfo campaignActionInfoParam = new CampaignActionInfo()
                {
                    m_siteGuid = int.Parse(siteGuid),
                    m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? 
                    new CampaignActionInfo.SocialInviteInfo() { m_hashCode = hashCode } : default(CampaignActionInfo.SocialInviteInfo),
                    m_mediaID = mediaID,
                    m_mediaLink = mediaLink,
                    m_senderEmail = senderEmail,
                    m_senderName = senderName,
                    m_status = status,
                    m_voucherReceipents = voucherReceipents == null ? null : voucherReceipents.ToList()
                };
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    campaignActionInfo = Core.ConditionalAccess.Module.ActivateCampaignWithInfo(m_groupID, (int)campID, campaignActionInfoParam);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.AD_GetCustomDataID(m_groupID,
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
                                   string overrideEndDate,
                                   string sPreviewModelID)
        {
            int res = 0;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetCustomDataID(m_groupID,
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
                                                      overrideEndDate,
                                                      sPreviewModelID);
                }
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

        public int CreatePurchaseToken(string siteGuid,
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
                           string overrideEndDate,
                            string sPreviewModuleID)
        {
            int res = 0;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetCustomDataID(m_groupID,
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
                                                      overrideEndDate,
                                                      sPreviewModuleID);

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCustomDataID, Error Message: {0}, Parameters : siteGuid - {1}, price - {2}, currencyCode3 - {3}, assetId - {4}, ppvModuleCode - {5}, campaignCode - {6}, couponCode - {7}, paymentMethod - {8}, userIp - {9}, countryCd2 - {10}, languageCode3 - {11}, deviceName - {12}, assetType - {13}, overrideEndDate - {14}, sPreviewModuleID - {15}",
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
                                    overrideEndDate,
                                    sPreviewModuleID);
            }
            return res;
        }

        public bool ActivateCampaign(string siteGuid, int campaignID, CampaignActionInfo actionInfo)
        {
            bool retVal = false;

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.ActivateCampaign(m_groupID, campaignID, actionInfo);
                    }
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

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.GetItemsPricesWithCoupons(m_groupID, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());
                    if (res != null)
                        retVal = res.ItemsPrices;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling web service protocol : GetItemsPricesWithCoupons, Error Message: {0}, Parameters : SiteGuid: {1} sCouponCode : {2}", ex.Message, siteGuid, sCouponCode);
            }
            return retVal;
        }

        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(string siteGuid, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] retVal = null;

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var res = Core.ConditionalAccess.Module.GetSubscriptionsPricesWithCoupon(m_groupID, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());
                        if (res != null)
                            retVal = res.SubscriptionsPrices;
                    }
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

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.IsPermittedItem(m_groupID, siteGuid, mediaId);
                    }
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

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.GetGoogleSignature(m_groupID, customerId);
                    }
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

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        string reason = string.Empty;
                        retVal = Core.ConditionalAccess.Module.IsPermittedSubscription(m_groupID, siteGuid, subId, ref reason);
                    }
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

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.InApp_ChargeUserForMediaFile(m_groupID, siteGuid, price, currency, productCode, ppvModuleCode, string.Empty, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName, ReceiptData);
                    }
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

            if (!string.IsNullOrEmpty(siteGuid))
            {
                try
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        retVal = Core.ConditionalAccess.Module.CC_ChargeUserForPrePaid(m_groupID, siteGuid, price, currency, productCode, ppvModuleCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling web service protocol : CC_ChargeUserForPrePaid, Error Message: {0}, Parameters : SiteGuid: {1} productCode : {2}", ex.Message, siteGuid, productCode);
                }
            }
            return retVal;
        }

        public UserBillingTransactionsResponse[] GetUsersBillingHistory(string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            UserBillingTransactionsResponse[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetUsersBillingHistory(m_groupID, siteGuids, startDate, endDate);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUsersBillingHistory, Error Message: {0}, Parameters :  startDate: {1}, endDate: {2}", ex.Message, startDate, endDate);
            }
            return retVal;
        }

        public DomainsBillingTransactionsResponse GetDomainsBillingHistory(int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            DomainsBillingTransactionsResponse retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetDomainsBillingHistory(m_groupID, domainIDs, startDate, endDate);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainsBillingHistory, Error Message: {0}, Parameters :  startDate: {1}, endDate: {2}", ex.Message, startDate, endDate);
            }
            return retVal;
        }

        public TVPApiModule.Objects.Responses.ConditionalAccess.DomainTransactionsHistoryResponse GetDomainTransactionsHistory(int domainID, DateTime startDate, DateTime endDate)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.DomainTransactionsHistoryResponse retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    DomainTransactionsHistoryResponse response = Core.ConditionalAccess.Module.GetDomainTransactionsHistory(m_groupID, domainID, startDate, endDate, 500, 0, TransactionHistoryOrderBy.CreateDateDesc);
                    if (response != null)
                    {
                        retVal = new Objects.Responses.ConditionalAccess.DomainTransactionsHistoryResponse(response);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainTransactionsHistory, Error Message: {0}, Parameters : domainID {1}, startDate: {2}, endDate: {3}", ex.Message, domainID, startDate, endDate);
            }
            return retVal;
        }

        public PermittedMediaContainer[] GetDomainPermittedItems(int domainID)
        {
            PermittedMediaContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.GetDomainPermittedItems(m_groupID, domainID);
                    if (res != null)
                    {
                        retVal = res.PermittedMediaContainer;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainPermittedItems, Error Message: {0}, Parameters :  domainID: {1}", ex.Message, domainID);
            }
            return retVal;
        }

        public PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(int domainID)
        {
            PermittedSubscriptionContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetDomainPermittedSubscriptions(m_groupID, domainID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainPermittedSubscriptions, Error Message: {0}, Parameters :  domainID: {1}", ex.Message, domainID);
            }
            return retVal;
        }

        public BillingResponse Cellular_ChargeUserForMediaFile(string siteGUID, double price, string currencyCode3, int mediaFileID, string ppvModuleCode, string couponCode, string userIP, string extraParameters, string countryCd2, string languageCode3, string deviceName)
        {
            BillingResponse retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.Cellular_ChargeUserForMediaFile(m_groupID, siteGUID, price, currencyCode3, mediaFileID, ppvModuleCode, couponCode, userIP, extraParameters, countryCd2, languageCode3, deviceName);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : Cellular_ChargeUserForMediaFile, Error Message: {0}, Parameters :  siteGuid: {1}, mediaFileID: {2}", ex.Message, siteGUID, mediaFileID);
            }
            return retVal;
        }

        public BillingResponse Cellular_ChargeUserForSubscription(string siteGUID, double price, string currencyCode3, string subscriptionCode, string couponCode, string userIP, string extraParameters, string countryCd2, string languageCode3, string deviceName)
        {
            BillingResponse retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.Cellular_ChargeUserForSubscription(m_groupID, siteGUID, price, currencyCode3, subscriptionCode, couponCode, userIP, extraParameters, countryCd2, languageCode3, deviceName);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : Cellular_ChargeUserForSubscription, Error Message: {0}, Parameters :  siteGuid: {1}, subscriptionCode: {2}", ex.Message, siteGUID, subscriptionCode);
            }
            return retVal;
        }

        public ChangeSubscriptionStatus ChangeSubscription(string sSiteGuid, int nOldSubscription, int nNewSubscription, string udid)
        {
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    return Core.ConditionalAccess.Module.ChangeSubscription(m_groupID, sSiteGuid, nOldSubscription, nNewSubscription, udid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeSubscription, Error Message: {0}, Parameters :  sSiteGuid : {1}, nOldSubscription : {2}, nNewSubscription : {3}", ex.Message, sSiteGuid, nOldSubscription, nNewSubscription);
            }

            return ChangeSubscriptionStatus.Error;
        }

        public string DummyChargeUserForCollection(double price, string currency, string collectionCode, string couponCode, string userIP, string siteGuid, string extraParameters, string deviceName, string countryCode2, string languageCode3)
        {
            BillingResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.CC_DummyChargeUserForCollection(m_groupID, siteGuid, price, currency, collectionCode, couponCode, userIP, extraParameters, countryCode2, languageCode3, deviceName);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : DummyChargeUserForCollection, Error Message: {0}, Parameters :  User: {1}", ex.Message, siteGuid);
            }

            return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
        }

        public CollectionsPricesContainer[] GetCollectionsPrices(string[] collections, string userGuid, string countryCode2, string languageCode3, string deviceName)
        {
            CollectionsPricesContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.GetCollectionsPrices(m_groupID, collections, userGuid, countryCode2, languageCode3, deviceName, SiteHelper.GetClientIP());
                    if (res != null)
                    {
                        retVal = res.CollectionsPrices;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCollectionsPrices, Error Message: {0}, Parameters :  siteGuid: {1}", ex.Message, userGuid);
            }
            return retVal;
        }

        public CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(string[] collections, string userGuid, string countryCode2, string languageCode3, string deviceName, string couponCode, string clientIp)
        {
            CollectionsPricesContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.ConditionalAccess.Module.GetCollectionsPricesWithCoupon(m_groupID, collections, userGuid, couponCode, countryCode2, languageCode3, deviceName, clientIp);
                    if (res != null)
                    {
                        retVal = res.CollectionsPrices;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetCollectionsPricesWithCoupon, Error Message: {0}, Parameters :  siteGuid: {1}", ex.Message, userGuid);
            }
            return retVal;
        }

        public PermittedCollectionContainer[] GetUserPermittedCollections(string siteGuid)
        {
            PermittedCollectionContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetUserPermittedCollections(m_groupID, siteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedCollections, Error Message: {0}, Parameters :  siteGuid: {1}", ex.Message, siteGuid);
            }
            return retVal;
        }

        public PermittedCollectionContainer[] GetDomainPermittedCollections(int domainId)
        {
            PermittedCollectionContainer[] retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.GetDomainPermittedCollections(m_groupID, domainId);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserPermittedCollections, Error Message: {0}, Parameters :  domainId: {1}", ex.Message, domainId);
            }
            return retVal;
        }

        public BillingResponse ChargeUserForCollection(string siteGuid, double price, string currencyCode3, string collectionCode, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3, string deviceName, string paymentMethodId, string encryptedCvv)
        {
            BillingResponse retVal = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    retVal = Core.ConditionalAccess.Module.CC_ChargeUserForCollection(m_groupID, siteGuid, price, currencyCode3, collectionCode, couponCode, userIP, extraParameters, countryCode2,
                                                                languageCode3, deviceName, paymentMethodId, encryptedCvv);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CC_ChargeUserForCollection, Error Message: {0}, Parameters :  siteGuid: {1}, collectionCode: {2}", ex.Message, siteGuid, collectionCode);
            }

            return retVal;
        }

        public bool CancelTransaction(string siteGuid, int assetId, eTransactionType transactionType, bool bIsForce = false)
        {
            bool isTransactionCancelled = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    isTransactionCancelled = false;
                    Core.ConditionalAccess.Module.CancelTransaction(m_groupID, siteGuid, assetId, transactionType, bIsForce);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CancelTransaction, Error Message: {0}, Parameters :  siteGuid: {1}, assetId: {2}", ex.Message, siteGuid, assetId);
            }

            return isTransactionCancelled;
        }

        public bool WaiverTransaction(string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isWaiverTransactionSucceeded = false;

            try
            {
                var result = Core.ConditionalAccess.Module.WaiverTransaction(m_groupID, siteGuid, assetId, transactionType);
                if (result.Code == 0)
                {
                    isWaiverTransactionSucceeded = true;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : WaiverTransaction, Error Message: {0}, Parameters :  siteGuid: {1}, assetId: {2}", ex.Message, siteGuid, assetId);
            }

            return isWaiverTransactionSucceeded;
        }

        public PermittedCollectionContainer[] GetUserExpiredCollections(string siteGuid, int numOfItems)
        {
            PermittedCollectionContainer[] collections = null;

            try
            {
                collections = Core.ConditionalAccess.Module.GetUserExpiredCollections(m_groupID, siteGuid, numOfItems);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserExpiredCollections, Error Message: {0}, Parameters :  siteGuid: {1}, numOfItems: {2}", ex.Message, siteGuid, numOfItems);
            }

            return collections;
        }

        public LicensedLinkResponse GetLicensedLinks(string siteGuid, int mediaFileID, string baseLink, string udid)
        {
            LicensedLinkResponse res = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetLicensedLinks(m_groupID, siteGuid, mediaFileID, baseLink, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetLicensedLinks, Error Message: {0}, Parameters : User: {1}", ex.Message, siteGuid);
            }
            return res;
        }

        public RecordResponse RecordAsset(string siteGuid, long domainId, string udid, string epgId, int? version)
        {
            RecordResponse res = null;

            try
            {
                RecordNPVRCommand commend = new RecordNPVRCommand()
                {
                    assetID = epgId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend) as RecordResponse;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecordAsset: Error calling webservice protocol : GetNPVRResponse with RecordNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, epgId: {4}",
                    ex.Message, siteGuid, domainId, udid, epgId);
            }
            return res;
        }

        public NPVRResponse CancelAssetRecording(string siteGuid, long domainId, string udid, string recordingId, int? version)
        {
            NPVRResponse res = null;

            try
            {
                CancelNPVRCommand commend = new CancelNPVRCommand()
                {
                    assetID = recordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("CancelAssetRecording: Error calling webservice protocol : GetNPVRResponse with CancelNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, recordingId: {4}",
                    ex.Message, siteGuid, domainId, udid, recordingId);
            }
            return res;
        }

        public NPVRResponse DeleteAssetRecording(string siteGuid, long domainId, string udid, string recordingId, int? version)
        {
            NPVRResponse res = null;

            try
            {
                DeleteNPVRCommand commend = new DeleteNPVRCommand()
                {
                    assetID = recordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("DeleteAssetRecording: Error calling webservice protocol : GetNPVRResponse with DeleteNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, recordingId: {4}",
                    ex.Message, siteGuid, domainId, udid, recordingId);
            }
            return res;
        }

        public QuotaResponse GetNPVRQuota(string siteGuid, long domainId, string udid, int? version)
        {
            QuotaResponse res = null;

            try
            {
                RetrieveQuotaNPVRCommand commend = new RetrieveQuotaNPVRCommand()
                {
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend) as QuotaResponse;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("GetNPVRQuota: Error calling webservice protocol : GetNPVRResponse with RetrieveQuotaNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}",
                    ex.Message, siteGuid, domainId, udid);
            }
            return res;
        }

        public NPVRResponse RecordSeriesByName(string siteGuid, long domainId, string udid, string assetId)
        {
            NPVRResponse res = null;

            try
            {
                RecordSeriesByNameNPVRCommand commend = new RecordSeriesByNameNPVRCommand()
                {
                    assetID = assetId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecordSeriesByName: Error calling webservice protocol : GetNPVRResponse with RecordSeriesByNameNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, assetId: {4}",
                    ex.Message, siteGuid, domainId, udid, assetId);
            }
            return res;
        }

        public NPVRResponse RecordSeriesByProgramId(string siteGuid, long domainId, string udid, string assetId, int? version)
        {
            NPVRResponse res = null;

            try
            {
                RecordSeriesByProgramIdNPVRCommand commend = new RecordSeriesByProgramIdNPVRCommand()
                {
                    assetID = assetId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecordSeriesByProgramId: Error calling webservice protocol : GetNPVRResponse with RecordSeriesByProgramIdNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, assetId: {4}",
                    ex.Message, siteGuid, domainId, udid, assetId);
            }
            return res;
        }


        public NPVRResponse DeleteSeriesRecording(string siteGuid, long domainId, string udid, string seriesRecordingId, int? version)
        {
            NPVRResponse res = null;

            try
            {
                DeleteSeriesNPVRCommand commend = new DeleteSeriesNPVRCommand()
                {
                    assetID = seriesRecordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("CancelSeriesRecording: Error calling webservice protocol : GetNPVRResponse with CancelSeriesNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, seriesRecordingId: {4}",
                    ex.Message, siteGuid, domainId, udid, seriesRecordingId);
            }
            return res;
        }

        public NPVRResponse DeleteRecordingsBy(string siteGuid, long domainId, string udid, string seriesId, string seasonNumber, string channelId,
            List<NPVRRecordingStatus> status)
        {
            NPVRResponse res = null;

            try
            {
                DeleteSeriesRecordingByNPVRCommand commend = new DeleteSeriesRecordingByNPVRCommand()
                {
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    ChannelId = channelId,
                    SeriesId = seriesId,
                    SeasonNumber = seasonNumber,
                    Status = status,
                    Version = 2
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("DeleteRecordingsBy: Error calling webservice protocol : GetNPVRResponse with DeleteSeriesRecordingByNPVRCommand, Error Message: {0}, " +
                    " Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, seriesId: {4}, seasonNumber: {5}, channelId: {6}, status: {7}, version: {8}",
                    ex.Message, siteGuid, domainId, udid, seriesId, seasonNumber, channelId, status != null ? string.Join(",", status.Select(x => x.ToString())) : string.Empty);
            }
            return res;
        }

        public NPVRResponse RecordSeriesBySeriesId(string siteGuid, long domainId, string udid, string seriesId, int seasonNumber, int seasonSeed, int episodeSeed,
            int channelId, List<string> lookupCriteria)
        {
            NPVRResponse res = null;

            try
            {
                RecordSeriesBySeriesIdNPVRCommand commend = new RecordSeriesBySeriesIdNPVRCommand()
                {
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    SeriesId = seriesId,
                    SeasonNumber = seasonNumber,
                    SeasonSeed = seasonSeed,
                    EpisodeSeed = episodeSeed,
                    ChannelId = channelId,
                    LookupCriteria = lookupCriteria,
                    Version = 2
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecordSeriesBySeriesId: Error calling webservice protocol : GetNPVRResponse with RecordSeriesBySeriesIdNPVRCommand, Error Message: {0}, " +
                    " Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, seriesId: {4}, seasonNumber: {5}, seasonSeed: {6}, episodeSeed: {7}, channelId: {8},lookupCriteria:{9} ",
                    ex.Message, siteGuid, domainId, udid, seriesId, seasonNumber, seasonSeed, episodeSeed, channelId, lookupCriteria != null ? string.Join(",", lookupCriteria) : string.Empty);
            }
            return res;
        }

        public NPVRResponse CancelSeriesRecording(string siteGuid, long domainId, string udid, string seriesRecordingId, int? version)
        {
            NPVRResponse res = null;

            try
            {
                CancelSeriesNPVRCommand commend = new CancelSeriesNPVRCommand()
                {
                    assetID = seriesRecordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("CancelSeriesRecording: Error calling webservice protocol : GetNPVRResponse with CancelSeriesNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, seriesRecordingId: {4}",
                    ex.Message, siteGuid, domainId, udid, seriesRecordingId);
            }
            return res;
        }

        public NPVRResponse SetAssetProtectionStatus(string siteGuid, long domainId, string udid, string recordingId, bool isProtect, int? version)
        {
            NPVRResponse res = null;

            try
            {
                ProtectNPVRCommand commend = new ProtectNPVRCommand()
                {
                    assetID = recordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    isProtect = isProtect,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = version
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("SetAssetProtectionStatus: Error calling webservice protocol : GetNPVRResponse with ProtectNPVRCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, recordingId: {4}, isProtect: {5}",
                    ex.Message, siteGuid, domainId, udid, recordingId, isProtect);
            }
            return res;
        }

        public NPVRResponse RecordingWatchStatus(string siteGuid, long domainId, string udid, string recordingId, int alreadyWatched)
        {
            NPVRResponse res = null;

            try
            {
                RecordNPVRAlreadyWatchedCommand commend = new RecordNPVRAlreadyWatchedCommand()
                {
                    assetID = recordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    alreadyWatched = alreadyWatched,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    Version = 2
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecordingWatchStatus: Error calling webservice protocol : GetNPVRResponse with RecordNPVRAlreadyWatchedCommand, Error Message: {0}, Parameters : siteGuid: {1}, domainId: {2}, udid: {3}, recordingId: {4}, alreadyWatched: {5}",
                    ex.Message, siteGuid, domainId, udid, recordingId, alreadyWatched);
            }
            return res;
        }

        public LicensedLinkNPVRResponse GetNPVRLicensedLink(string siteGuid, long domainId, string udid,
            string recordingId, DateTime startTime, int mediaFileID, string basicLink, string userIP, string referrer, string countryCode, string languageCode,
            string couponCode, TVPApiModule.Objects.PlayContextType contextType)
        {
            LicensedLinkNPVRResponse res = null;

            try
            {
                LicensedLinkNPVRCommand commend = new LicensedLinkNPVRCommand()
                {
                    assetID = recordingId,
                    domainID = domainId,
                    siteGuid = siteGuid,
                    udid = udid,
                    startTime = startTime,
                    mediaFileID = mediaFileID,
                    basicLink = basicLink,
                    userIP = userIP,
                    referrer = referrer,
                    countryCd = countryCode,
                    langCd = languageCode,
                    couponCode = couponCode,
                    format = 3,
                    wsPassword = m_wsPassword,
                    wsUsername = m_wsUserName,
                    contextType = ConvertPlayContextType(contextType)
                };

                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.ConditionalAccess.Module.GetNPVRResponse(commend) as LicensedLinkNPVRResponse;
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("GetNPVRLicensedLink: Error calling webservice protocol: GetNPVRResponse with LicensedLinkNPVRCommand.");
                sb.Append(String.Concat(" siteGuid: ", siteGuid));
                sb.Append(String.Concat(" domainId: ", domainId));
                sb.Append(String.Concat(" udid: ", udid));
                sb.Append(String.Concat(" recordingId: ", recordingId));
                sb.Append(String.Concat(" startTime: ", startTime));
                sb.Append(String.Concat(" mediaFileID: ", mediaFileID));
                sb.Append(String.Concat(" basicLink: ", basicLink));
                sb.Append(String.Concat(" userIP: ", userIP));
                sb.Append(String.Concat(" referrer: ", referrer));
                sb.Append(String.Concat(" countryCode: ", countryCode));
                sb.Append(String.Concat(" languageCode: ", languageCode));
                sb.Append(String.Concat(" couponCode: ", couponCode));
                sb.Append(String.Concat(" contextType: ", contextType));
                logger.Error(sb.ToString(), ex);
            }

            return res;
        }        

        public ClientResponseStatus CancelServiceNow(int domainId, int assetId, eTransactionType transactionType, bool bIsForce = false, string udid = null)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.CancelServiceNow(m_groupID, domainId, assetId, transactionType, bIsForce, udid);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CancelServiceNow, Error Message: {0}, Parameters: domain Id: {1}, assetId: {2}", ex.Message, domainId, assetId);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public ClientResponseStatus CancelSubscriptionRenewal(int p_nDomainId, string p_sSubscriptionID, string userId, string udid, string userIp)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.CancelSubscriptionRenewal(m_groupID, p_nDomainId, p_sSubscriptionID, userId, udid, userIp);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CancelSubscriptionRenewal, Error Message: {0}, Parameters :  Domain: {1} Susbcription: {2}",
                    ex.Message, p_nDomainId, p_sSubscriptionID);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public ServicesResponse GetDomainServices(int domainId)
        {
            ServicesResponse response;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.GetDomainServices(m_groupID, domainId);
                    response = new ServicesResponse(result.Services, result.Status);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(
                    "Error calling webservice protocol : GetDomainServices, Error Message: {0}, Parameters :  DomainID: {1}",
                    ex.Message, domainId);
                response = new ServicesResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }
            return response;
        }


        public TVPApiModule.Objects.Responses.LicensedLinkResponse GetEPGLicensedLink(string siteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceUDID, int formatType)
        {
            TVPApiModule.Objects.Responses.LicensedLinkResponse response = null;

            string wsUser = GroupsManager.GetGroup(m_groupID).ConditionalAccessCredentials.Username;
            string wsPassword = GroupsManager.GetGroup(m_groupID).ConditionalAccessCredentials.Password;
            
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.GetEPGLicensedLink(m_groupID, siteGUID, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceUDID, formatType);
                    response = new TVPApiModule.Objects.Responses.LicensedLinkResponse(result);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error while calling webservice protocol : GetEPGLicensedLink, Error Message: {0}, Parameters : MediaFileID : {1}, EPGItemID : {2}, UserIP: {3}", ex.Message, mediaFileID, EPGItemID, userIP);
                response = new TVPApiModule.Objects.Responses.LicensedLinkResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }
            return response;
        }

        public TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse Purchase(string userId, double price, string currency, int contentId, int productId,
            eTransactionType productType, string coupon, string deviceName, int paymentGatewayId, int paymentMethodId)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.Purchase(m_groupID, userId, 0, price, currency, contentId, productId, productType, coupon, SiteHelper.GetClientIP(), deviceName, paymentGatewayId, paymentMethodId, string.Empty);
                    response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse(result);

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(@"Error calling webservice protocol : Purchase, Error Message: {0}, params: userId: {1}, price: {2}, currency: {3}, contentId: {4}, productId: {5} 
                    , productType: {6}, coupon: {7}, deviceName: {8}, pgwId: {9}",
                    ex.Message,                                     // {0}
                    userId != null ? userId : string.Empty,         // {1}                    
                    price,                                          // {2}
                    currency != null ? currency : string.Empty,     // {3}
                    contentId,                                      // {4}
                    productId,                                      // {5}
                    productType.ToString(),                         // {6}
                    coupon != null ? coupon : string.Empty,         // {7}
                    deviceName != null ? deviceName : string.Empty, // {8}
                    paymentGatewayId                                           // {9}                    
                    );
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse ProcessReceipt(string userId, int contentId, int productId, eTransactionType transactionType, string deviceName, string purchaseToken, string paymentGatewayName)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.ProcessReceipt(m_groupID, userId, 0, contentId, productId, transactionType, 
                        SiteHelper.GetClientIP(), deviceName, purchaseToken, paymentGatewayName, null);
                    response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse(result);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(@"Error calling web-service protocol : Purchase, Error Message: {0}, parameters: userId: {1}, contentId: {2}, productId: {3},
                                     transactionType: {4}, deviceName: {5}, purchaseToken: {6}, paymentGatewayName: {7}",
                    ex.Message,                                                         // {0}
                    userId != null ? userId : string.Empty,                             // {1}                    
                    contentId,                                                          // {2}
                    productId,                                                          // {3}
                    transactionType.ToString(),                                         // {4}
                    deviceName != null ? deviceName : string.Empty,                     // {5}
                    purchaseToken != null ? purchaseToken : string.Empty,               // {6}
                    paymentGatewayName != null ? paymentGatewayName : string.Empty);    // {7}                    
            }

            return response;
        }

        public ClientResponseStatus GrantEntitlements(string userId, int contentId, int productId, eTransactionType productType, bool history)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.ConditionalAccess.Module.GrantEntitlements(m_groupID, userId, 0, contentId, productId, productType, SiteHelper.GetClientIP(), string.Empty, history);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat(@"Error calling webservice protocol : GrantEntitlements, Error Message: {0}, params: userId: {1}, contentId: {2}, productId: {3} 
                    , productType: {4}, history: {5}",
                    ex.Message,                                 //{0}
                    userId != null ? userId : string.Empty,     //{1}                    
                    contentId,                                 //{2}
                    productId,                                 //{3}
                    productType.ToString(),                    //{4}
                    history.ToString()                         //{5}                    
                    );
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");

            }

            return clientResponse;
        }


        public AssetItemPriceResponse GetAssetsPrices(string siteGuid, string couponCode, string udid, List<AssetFiles> assetFiles)
        {
            AssetItemPriceResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.ConditionalAccess.Module.GetAssetPrices(m_groupID, siteGuid, couponCode, string.Empty, string.Empty, udid,
                        SiteHelper.GetClientIP(), assetFiles);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAssetsPrices, Error Message: {0}, Parameters :  User: {1}", ex.Message, siteGuid);
            }

            return response;
        }

        private PlayContextType ConvertPlayContextType(Objects.PlayContextType contextType)
        {
            switch (contextType)
            {
                case Objects.PlayContextType.Trailer:
                    return PlayContextType.Trailer;
                case Objects.PlayContextType.CatchUp:
                    return PlayContextType.CatchUp;
                case Objects.PlayContextType.StartOver:
                    return PlayContextType.StartOver;
                case Objects.PlayContextType.Download:
                    return PlayContextType.Download;
                case Objects.PlayContextType.Playback:
                default:
                    return PlayContextType.Playback;
            }
        }
    }
}
