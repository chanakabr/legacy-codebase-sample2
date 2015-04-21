using RestfulTVPApi.Clients.Utils;
using RestfulTVPApi.ConditionalAccess;
using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;

namespace RestfulTVPApi.Clients
{
    public class ConditionalAccessClient : BaseClient
    {
        #region Variables
        private readonly ILog logger = LogManager.GetLogger(typeof(ConditionalAccessClient));

        #endregion

        #region C'tor
        public ConditionalAccessClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
           
        }

        public ConditionalAccessClient()
        {

        }

        #endregion

        #region Properties

        protected RestfulTVPApi.ConditionalAccess.module ConditionalAccess
        {
            get
            {
                return (Module as RestfulTVPApi.ConditionalAccess.module);
            }
        }

        #endregion

        #region Public methods
        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID)
        {
            RestfulTVPApi.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
                {
                    response = ConditionalAccess.CC_DummyChargeUserForMediaFile(WSUserName, WSPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, sUDID);
                    return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }) as string;

            return concatenatedRes;
        }

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            RestfulTVPApi.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
            {
                response = ConditionalAccess.CC_ChargeUserForMediaFile(WSUserName, WSPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, string.Empty, sUserIP, sExtraParams, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
            }) as string;

            return concatenatedRes;            
        }

        public RestfulTVPApi.Objects.Responses.BillingResponse DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse retVal = new Objects.Responses.BillingResponse();

            retVal = Execute(() =>
            {
                RestfulTVPApi.ConditionalAccess.BillingResponse response = ConditionalAccess.CC_DummyChargeUserForSubscription(WSUserName, WSPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID);
                if (response != null)
                    retVal = response.ToApiObject();

                return retVal;
            }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return retVal;
        }

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV)
        {
            RestfulTVPApi.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
                {
                    response = ConditionalAccess.CC_ChargeUserForSubscription(WSUserName, WSPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                    return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }) as string;

            return concatenatedRes;
        }

        public RestfulTVPApi.Objects.Response.Status CancelSubscription(string sUserGuid, string sSubscriptionID, int nSubscriptionPurchaseID)
        {
            RestfulTVPApi.Objects.Response.Status response = new RestfulTVPApi.Objects.Response.Status();

            response = Execute(() =>
                {
                    bool isCanceled = ConditionalAccess.CancelSubscription(WSUserName, WSPassword, sUserGuid, sSubscriptionID, nSubscriptionPurchaseID);       
                    response.status = isCanceled ? StatusObjectCode.OK : StatusObjectCode.Fail;

                    return response;
                }) as RestfulTVPApi.Objects.Response.Status;

            return response;
        }        

        public RestfulTVPApi.Objects.Responses.BillingResponse InAppChargeUserForSubscription(double iPrice, string sCurrency, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sProductCode, string sReceipt)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse response = null;

            response = Execute(() =>
                {
                    var res = ConditionalAccess.InApp_ChargeUserForSubscription(WSUserName, WSPassword, sUserGuid, iPrice, sCurrency, sProductCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sReceipt);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> GetUserPermittedItems(string sSiteGuid)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPermittedItems(WSUserName, WSPassword, sSiteGuid);

                    if (response != null)
                    {
                        retVal = response.Where(pmc => pmc != null).Select(m => m.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer>;

            return retVal;            
        }

        public List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> GetItemsPrice(int[] fileArray, string sSiteGuid, bool bOnlyLowest)
        {
            List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetItemsPrices(WSUserName, WSPassword, fileArray, sSiteGuid, bOnlyLowest, string.Empty, string.Empty, string.Empty, RestfulTVPApi.ServiceInterface.Utils.GetClientIP());

                if (response != null)
                    retVal = response.Where(mf => mf != null).Select(mf => mf.ToApiObject()).ToList();

                return retVal;
            }) as List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> GetUserPermitedSubscriptions(string sSiteGuid)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPermittedSubscriptions(WSUserName, WSPassword, sSiteGuid);

                    if (response != null)
                    {
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public RestfulTVPApi.Objects.Responses.BillingTransactionsResponse GetUserTransactionHistory(string sSiteGuid, int startIndex, int count)
        {
            RestfulTVPApi.Objects.Responses.BillingTransactionsResponse retVal = null;

            retVal = Execute(() =>
                {
                    var res = ConditionalAccess.GetUserBillingHistory(WSUserName, WSPassword, sSiteGuid, startIndex, count);
                    if (res != null)
                        retVal = res.ToApiObject();

                    return retVal;
                }) as RestfulTVPApi.Objects.Responses.BillingTransactionsResponse;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> GetUserExpiredItems(string sSiteGuid, int numOfItems)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserExpiredItems(WSUserName, WSPassword, sSiteGuid, numOfItems);

                    if (response != null)
                    {
                        retVal = response.Where(pm => pm != null).Select(m => m.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> GetUserExpiredSubscriptions(string sSiteGuid, int numOfItems)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserExpiredSubscriptions(WSUserName, WSPassword, sSiteGuid, numOfItems);

                    if (response != null)
                    {
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }
                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer> GetSubscriptionsPrices(string sSiteGuid, string[] sSubscriptions, bool LowerPrice)
        {
            List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer> returnObject = null;

            returnObject = Execute(() =>
                {
                    var response = ConditionalAccess.GetSubscriptionsPrices(WSUserName, WSPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, RestfulTVPApi.ServiceInterface.Utils.GetClientIP());

                    if (response != null)
                        returnObject = response.Where(sp => sp != null).Select(sp => sp.ToApiObject()).ToList();

                    return returnObject;
                }) as List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer>;

            return returnObject;
        }

        //public SubscriptionsPricesContainer[] GetSubscriptionsPricesByIP(string sSiteGuid, string[] sSubscriptions, bool LowerPrice)
        //{
        //    SubscriptionsPricesContainer[] returnObject = null;

        //    try
        //    {
        //        returnObject = m_Module.GetSubscriptionsPricesByIP(m_wsUserName, m_wsPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, Utils.GetClientIP());
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetSubscriptionsPricesByIP, Error Message: {0}, Parameters : User: {1}", ex.Message, sSiteGuid);
        //    }
        //    return returnObject;
        //}

        public List<string> GetPrepaidBalance(string siteGuid, string currencyCode)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPrePaidStatus(WSUserName, WSPassword, siteGuid, currencyCode);

                    if (response != null)
                    {
                        retVal = new List<string>();

                        retVal.Add((response.m_nTotalAmount - response.m_nAmountUsed).ToString());
                        retVal.Add(response.m_sCurrencyCode);
                    }

                    return retVal;
                }) as List<string>;

            return retVal;
        }

        public RestfulTVPApi.Objects.Responses.Enums.PrePaidResponseStatus PP_ChargeUserForMediaFile(string siteGuid, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode, string udid)
        {
            PrePaidResponse returnObject = null;

            RestfulTVPApi.Objects.Responses.Enums.PrePaidResponseStatus prePaidReturnObject = (RestfulTVPApi.Objects.Responses.Enums.PrePaidResponseStatus)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.PrePaidResponseStatus), Execute(() =>
            {
                returnObject = ConditionalAccess.PP_ChargeUserForMediaFile(WSUserName, WSPassword, siteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, RestfulTVPApi.ServiceInterface.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                return (RestfulTVPApi.Objects.Responses.Enums.PrePaidResponseStatus)returnObject.m_oStatus;
            }).ToString());

            return prePaidReturnObject;
        }

        public string GetMediaLicenseLink(string siteGuid, int mediaFileID, string baseLink, string udid)
        {
            string returnObject = null;

            returnObject = Execute(() =>
                {
                    returnObject = ConditionalAccess.GetLicensedLink(WSUserName, WSPassword, siteGuid, mediaFileID, baseLink, RestfulTVPApi.ServiceInterface.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);                    
                    return returnObject;
                }) as string;

            return returnObject;           
        }

        public UserCAStatus GetUserCAStatus(string siteGuid)
        {
            UserCAStatus retVal = UserCAStatus.Annonymus;

            retVal = (UserCAStatus)Enum.Parse(typeof(UserCAStatus), Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        retVal = ConditionalAccess.GetUserCAStatus(WSUserName, WSPassword, siteGuid);
                        logger.InfoFormat("Protocol: GetUserStatus, Parameters : SiteGuid : {0}", siteGuid);
                    }
                    return retVal;
                }).ToString());

            return retVal;            
        }

        public RestfulTVPApi.Objects.Responses.CampaignActionInfo ActivateCampaignWithInfo(string siteGuid, long campID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           RestfulTVPApi.ConditionalAccess.CampaignActionResult status, RestfulTVPApi.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            RestfulTVPApi.Objects.Responses.CampaignActionInfo campaignActionInfo = null;

            campaignActionInfo = Execute(() =>
                {
                    RestfulTVPApi.ConditionalAccess.CampaignActionInfo campaignActionInfoParam = new RestfulTVPApi.ConditionalAccess.CampaignActionInfo()
                        {
                            m_siteGuid = int.Parse(siteGuid),
                            m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new RestfulTVPApi.ConditionalAccess.SocialInviteInfo() { m_hashCode = hashCode } : null,
                            m_mediaID = mediaID,
                            m_mediaLink = mediaLink,
                            m_senderEmail = senderEmail,
                            m_senderName = senderName,
                            m_status = status,
                            m_voucherReceipents = voucherReceipents
                        };
                    var res = ConditionalAccess.ActivateCampaignWithInfo(WSUserName, WSPassword, (int)campID, campaignActionInfoParam);
                    if (res != null)
                        campaignActionInfo = res.ToApiObject();

                    logger.InfoFormat("Protocol: ActivateCampaignWithInfo, Parameters : campID : {0}", campID);

                    return campaignActionInfo;
                }) as RestfulTVPApi.Objects.Responses.CampaignActionInfo;

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

            res = Convert.ToInt32(Execute(() =>
                {
                    res = ConditionalAccess.AD_GetCustomDataID(WSUserName,
                                                          WSPassword,
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

                    return res;
                }));

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

            res = Convert.ToInt32(Execute(() =>
                {
                    res = ConditionalAccess.GetCustomDataID(WSUserName,
                                                          WSPassword,
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
                                                          string.Empty);

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

                    return res;
                }));

            return res;            
        }

        public bool ActivateCampaign(string siteGuid, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           RestfulTVPApi.ConditionalAccess.CampaignActionResult status, RestfulTVPApi.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            bool retVal = false;

            retVal = Convert.ToBoolean(Execute(() =>
                {
                    RestfulTVPApi.ConditionalAccess.CampaignActionInfo actionInfo = new RestfulTVPApi.ConditionalAccess.CampaignActionInfo()
                    {
                        m_siteGuid = int.Parse(siteGuid),
                        m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new RestfulTVPApi.ConditionalAccess.SocialInviteInfo() { m_hashCode = hashCode } : null,
                        m_mediaID = mediaID,
                        m_mediaLink = mediaLink,
                        m_senderEmail = senderEmail,
                        m_senderName = senderName,
                        m_status = status,
                        m_voucherReceipents = voucherReceipents
                    };
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("ActivateCampaign, Parameters : SiteGuid : {0} campaignID : {1} actionInfo : {2}", siteGuid, campaignID, actionInfo.ToString());
                        retVal = ConditionalAccess.ActivateCampaign(WSUserName, WSPassword, campaignID, actionInfo);
                    }

                    return retVal;
                }));

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(string siteGuid, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer> retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("GetItemsPricesWithCoupons, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);

                        var response = ConditionalAccess.GetItemsPricesWithCoupons(WSUserName, WSPassword, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, RestfulTVPApi.ServiceInterface.Utils.GetClientIP());

                        if (response != null)
                            retVal = response.Where(mf => mf != null).Select(mf => mf.ToApiObject()).ToList();

                    }

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.MediaFileItemPricesContainer>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(string[] sSubscriptions, string siteGuid, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer> retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("GetSubscriptionsPricesWithCoupon, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);

                        var response = ConditionalAccess.GetSubscriptionsPricesWithCoupon(WSUserName, WSPassword, sSubscriptions, siteGuid, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, RestfulTVPApi.ServiceInterface.Utils.GetClientIP());

                        if (response != null)
                            retVal = response.Where(sp => sp != null).Select(sp => sp.ToApiObject()).ToList();
                    }

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.SubscriptionsPricesContainer>;

            return retVal;
        }

        public bool IsPermittedItem(string siteGuid, int mediaId)
        {
            bool retVal = false;

            retVal = Convert.ToBoolean(Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("IsPermittedItem, Parameters : SiteGuid : {0} mediaId : {1}", siteGuid, mediaId);

                        retVal = ConditionalAccess.IsPermittedItem(WSUserName, WSPassword, siteGuid, mediaId);
                    }

                    return retVal;
                }));

            return retVal;
        }

        public string GetGoogleSignature(string siteGuid, int customerId)
        {
            string retVal = string.Empty;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("GetGoogleSignature, Parameters : SiteGuid : {0} customerId : {1}", siteGuid, customerId);
                        retVal = ConditionalAccess.GetGoogleSignature(WSUserName, WSPassword, customerId);
                    }

                    return retVal;
                }) as string;

            return retVal;
        }

        public bool IsPermittedSubscription(string siteGuid, int subId)
        {
            bool retVal = false;

            retVal = Convert.ToBoolean(Execute(() =>
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    logger.InfoFormat("IsPermittedSubscription, Parameters : SiteGuid : {0} subId : {1}", siteGuid, subId);

                    string reason = string.Empty;

                    retVal = ConditionalAccess.IsPermittedSubscription(WSUserName, WSPassword, siteGuid, subId, ref reason);
                }

                return retVal;
            }));

            return retVal;        
        }

        public RestfulTVPApi.Objects.Responses.BillingResponse InApp_ChargeUserForMediaFile(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName, string ReceiptData)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("InApp_ChargeUserForMediaFile, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                        var res = ConditionalAccess.InApp_ChargeUserForMediaFile(WSUserName, WSPassword, siteGuid, price, currency, productCode, ppvModuleCode, string.Empty, RestfulTVPApi.ServiceInterface.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName, ReceiptData);
                        if (res != null)
                            retVal = res.ToApiObject();
                    }

                    return retVal;
                }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return retVal;            
        }

        public RestfulTVPApi.Objects.Responses.BillingResponse CC_ChargeUserForPrePaid(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("CC_ChargeUserForPrePaid, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                        var res = ConditionalAccess.CC_ChargeUserForPrePaid(WSUserName, WSPassword, siteGuid, price, currency, productCode, ppvModuleCode, RestfulTVPApi.ServiceInterface.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName);
                        if (res != null)
                            retVal = res.ToApiObject();
                    }

                    return retVal;
                }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return retVal;
        }

        public string GetEPGLicensedLink(string siteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            string res = string.Empty;

            res = Execute(() =>
                {
                    return ConditionalAccess.GetEPGLicensedLink(WSUserName, WSPassword, siteGUID, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceName, formatType);
                }) as string;

            return res;            
        }

        public List<RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse> GetUsersBillingHistory(string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            List<RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUsersBillingHistory(WSUserName, WSPassword, siteGuids, startDate, endDate);
                    if (response != null)
                        retVal = response.Where(ubt => ubt != null).Select(ubt => ubt.ToApiObject()).ToList();

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.UserBillingTransactionsResponse>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            List<RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainsBillingHistory(WSUserName, WSPassword, domainIDs, startDate, endDate);
                    if (response != null)
                        retVal = response.Where(dbt => dbt != null).Select(dbt => dbt.ToApiObject()).ToList();

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.DomainBillingTransactionsResponse>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> GetDomainPermittedItems(int domainID)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainPermittedItems(WSUserName, WSPassword, domainID);
                    if (response != null)
                        retVal = response.Where(pm => pm != null).Select(m => m.ToApiObject()).ToList();

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedMediaContainer>;

            return retVal;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(int domainID)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainPermittedSubscriptions(WSUserName, WSPassword, domainID);
                    if (response != null)
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                    return retVal;
                }) as List<RestfulTVPApi.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public List<CollectionPricesContainer> GetCollectionPrices(string[] collections, string userGuid, string countryCode2, string languageCode3, string deviceName)
        {
            List<RestfulTVPApi.Objects.Responses.CollectionPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetCollectionsPrices(WSUserName, WSPassword, collections, userGuid, countryCode2, languageCode3, deviceName);
                if (response != null)
                    retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                return retVal;
            }) as List<RestfulTVPApi.Objects.Responses.CollectionPricesContainer>;

            return retVal;
        }

        public List<CollectionPricesContainer> GetCollectionPricesWithCoupon(string[] collections, string userGuid, string countryCode2, string languageCode3, string couponCode, string deviceName)
        {
            List<RestfulTVPApi.Objects.Responses.CollectionPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetCollectionsPricesWithCoupon(WSUserName, WSPassword, collections, userGuid, couponCode, countryCode2, languageCode3, deviceName, RestfulTVPApi.ServiceInterface.Utils.GetClientIP());
                if (response != null)
                    retVal = response.Where(cp => cp != null).Select(collection => collection.ToApiObject()).ToList();

                return retVal;
            }) as List<RestfulTVPApi.Objects.Responses.CollectionPricesContainer>;

            return retVal;
        }
      
        public List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer> GetUserPermittedCollections(string siteGuid)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetUserPermittedCollections(WSUserName, WSPassword, siteGuid);

                if (response != null)
                {
                    retVal = response.Where(pc => pc != null).Select(collection => collection.ToApiObject()).ToList();
                    if (retVal != null)
                    {
                        retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                    }
                }

                return retVal;
            }) as List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer>;

            return retVal;
        }

        public List<Objects.Responses.PermittedCollectionContainer> GetDomainPermittedCollections(int domainId)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetDomainPermittedCollections(WSUserName, WSPassword, domainId);
                if (response != null)
                    retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                return retVal;
            }) as List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer>;

            return retVal;
        }

        public RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus ChangeSubscription(string siteGuid, int oldSubscriptionId, int newSubscriptionId)
        {
            RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus status = RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus.Error;

            status = (RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus)Enum.Parse(typeof(RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus), Execute(() =>
            {
                status = (RestfulTVPApi.Objects.Responses.Enums.ChangeSubscriptionStatus)ConditionalAccess.ChangeSubscription(WSUserName, WSPassword, siteGuid, oldSubscriptionId, newSubscriptionId);
                return status;
            }).ToString());

            return status;
        }

        public int CreatePurchaseToken(string siteGuid, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp,
                                       string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate, string previewModuleID)
        {
            int res = 0;

            res = Convert.ToInt32(Execute(() =>
            {
                res = ConditionalAccess.GetCustomDataID(WSUserName, WSPassword, siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3,
                                                        deviceName, assetType, overrideEndDate, previewModuleID);                

                return res;
            }));

            return res;            
        }

        public string DummyChargeUserForCollection(string siteGuid, string collectionId, double price, string currency, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3, string deviceId)
        {
            string res = string.Empty;

            res = Execute(() =>
            {
                var response = ConditionalAccess.CC_DummyChargeUserForCollection(WSUserName, WSPassword, siteGuid, price, currency, collectionId, couponCode, userIP, extraParameters, countryCode2, languageCode3, deviceId);
                if (response != null)
                {
                    res = response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }

                return res;
            }) as string;

            return res;
        }

        public RestfulTVPApi.Objects.Responses.BillingResponse ChargeUserForCollection(string siteGuid, string collectionId, double price, string currency, string encrypteCVV, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3, string deviceId, string paymentMethodId)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse billingResponse = null;

            billingResponse = Execute(() =>
            {
                var response = ConditionalAccess.CC_ChargeUserForCollection(WSUserName, WSPassword, siteGuid, price, currency, collectionId, couponCode, userIP, extraParameters, countryCode2, languageCode3, deviceId, paymentMethodId, encrypteCVV);
                if (response != null)
                {
                    billingResponse = response.ToApiObject();
                }

                return billingResponse;
            }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return billingResponse;
        }

        public RestfulTVPApi.Objects.Responses.BillingResponse CellularChargeUserForSubscription(string siteGuid, double price, string currencyCode, string subscriptionCode, string couponCode, string userIP, string extraParameters, string countryCode, string languageCode3, string deviceName)
        {
            RestfulTVPApi.Objects.Responses.BillingResponse billingResponse = null;

            billingResponse = Execute(() =>
            {
                var response = ConditionalAccess.Cellular_ChargeUserForSubscription(WSUserName, WSPassword, siteGuid, price, currencyCode, subscriptionCode, couponCode, userIP, extraParameters, countryCode, languageCode3, deviceName);
                if (response != null)
                {
                    billingResponse = response.ToApiObject();
                }

                return billingResponse;
            }) as RestfulTVPApi.Objects.Responses.BillingResponse;

            return billingResponse;
        }

        public string ChargeUserForSubscriptionByPaymentMethod(string siteGuid, double price, string currencyCode, string subscriptionCode, string couponCode, string userIP, string extraParameters, string countryCode, string languageCode3, string deviceName, string paymentMethodId, string encryptedCVV)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.CC_ChargeUserForSubscription(WSUserName, WSPassword, siteGuid, price, currencyCode, subscriptionCode, couponCode, userIP, extraParameters, countryCode, languageCode3, deviceName, paymentMethodId, encryptedCVV);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public string ChargeUserForMediaFileByPaymentMethod(double price, string currencyCode, int fileId, string ppvModuleCode, string userIP, string siteGuid, string deviceName, string extraParameters, string paymentMethodId, string encryptedCVV)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.CC_ChargeUserForMediaFile(WSUserName, WSPassword, siteGuid, price, currencyCode, fileId, ppvModuleCode, string.Empty, userIP, extraParameters, string.Empty, string.Empty, deviceName, paymentMethodId, encryptedCVV);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public string CellularChargeUserForMediaFileRequest(double price, string currencyCode, int fileId, string ppvModuleCode, string userIP, string siteGuid, string deviceName, string extraParameters, string couponCode, string languageCode, string countryCode)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.Cellular_ChargeUserForMediaFile(WSUserName, WSPassword, siteGuid, price, currencyCode, fileId, ppvModuleCode, couponCode, userIP, extraParameters, countryCode, languageCode, deviceName);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public string ChargeUserForMediaFileUsingCC(double price, string currency, int fileId, string ppvModuleCode, string couponCode, string userIp, string siteGuid, string udid, string paymentMethodID, string encryptedCVV)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.CC_ChargeUserForMediaFile(WSUserName, WSPassword, siteGuid, price, currency, fileId, ppvModuleCode, couponCode, userIp, string.Empty, string.Empty, string.Empty, udid, paymentMethodID, encryptedCVV);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public string ChargeUserForMediaSubscriptionUsingCC(double price, string currency, string subscriptionId, string couponCode, string userIp, string siteGuid, string udid, string paymentMethodID, string encryptedCVV, string extraParameters, string countryCode, string languageCode)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.CC_ChargeUserForSubscription(WSUserName, WSPassword, siteGuid, price, currency, subscriptionId, couponCode, userIp, extraParameters, countryCode, languageCode, udid, paymentMethodID, encryptedCVV);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer> GetUserExpiredCollections(string siteGuid, int numOfItems)
        {
            List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer> permittedCollections = null;

            permittedCollections = Execute(() =>
            {
                var response = ConditionalAccess.GetUserExpiredCollections(WSUserName, WSPassword, siteGuid, numOfItems);
                if (response != null)
                {
                    permittedCollections = response.Where(pc => pc != null).Select(pc => pc.ToApiObject()).ToList();
                }

                return permittedCollections;
            }) as List<RestfulTVPApi.Objects.Responses.PermittedCollectionContainer>;

            return permittedCollections;
        }

        /*public bool CancelTransaction(string siteGuid, int assetId, eTransactionType transactionType, bool isForce)
        {
            bool isCancelationSucceeded = false;

            isCancelationSucceeded = Convert.ToBoolean(Execute(() =>
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    isCancelationSucceeded = ConditionalAccess.CancelTransaction(m_wsUserName, m_wsPassword, siteGuid, assetId, transactionType, isForce);
                }

                return isCancelationSucceeded;
            }));

            return isCancelationSucceeded;
        }*/

        public bool CancelTransaction(string siteGuid, int assetId, eTransactionType transactionType, bool isForce)
        {
            bool isCancelationSucceeded = false;

            isCancelationSucceeded = Convert.ToBoolean(Execute(() =>
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    isCancelationSucceeded = ConditionalAccess.CancelTransaction(WSUserName, WSPassword, siteGuid, assetId, transactionType, isForce);
                }

                return isCancelationSucceeded;
            }));

            return isCancelationSucceeded;
        }

        public bool WaiverTransaction(string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isWaiverTransactionSucceeded = false;

            isWaiverTransactionSucceeded = Convert.ToBoolean(Execute(() =>
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    isWaiverTransactionSucceeded = ConditionalAccess.WaiverTransaction(WSUserName, WSPassword, siteGuid, assetId, transactionType);
                }

                return isWaiverTransactionSucceeded;
            }));

            return isWaiverTransactionSucceeded;
        }

        /*public Status CancelSubscriptionRenewal(int domain_id, string subscription_id)
        {
            Status response = new Status();

            response = Execute(() =>
            {
                var status = ConditionalAccess.CancelSubscriptionRenewal(m_wsUserName, m_wsPassword, domain_id, subscription_id);
                if (status != null)
                    response = status.ToApiObject();
                return response;
            }) as Status;

            return response;
        }*/
        #endregion
    }
}