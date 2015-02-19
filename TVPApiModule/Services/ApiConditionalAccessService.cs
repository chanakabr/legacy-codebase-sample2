using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Extentions;
using TVPApiModule.Context;
using TVPApiModule.Objects.Responses;

namespace TVPApiModule.Services
{
    public class ApiConditionalAccessService : BaseService
    {
        #region Variables
        private static ILog logger = log4net.LogManager.GetLogger(typeof(ApiConditionalAccessService));

        //private TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiConditionalAccessService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ConditionalAccessService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiConditionalAccessService()
        {

        }

        #endregion

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module ConditionalAccess
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.module);
            }
        }

        #endregion

        //#region Public Static Functions

        //public static ApiConditionalAccessService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.ConditionalAccessService) as ApiConditionalAccessService;
        //}

        //#endregion

        #region Public methods
        public string DummyChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
                {
                    response = ConditionalAccess.CC_DummyChargeUserForMediaFile(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, "", sUserIP, "", string.Empty, string.Empty, sUDID);
                    return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }) as string;

            return concatenatedRes;
        }

        public string ChargeUserForMediaFile(double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sUserGuid, string sUDID, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
            {
                response = ConditionalAccess.CC_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, iFileID, sPPVModuleCode, string.Empty, sUserIP, sExtraParams, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
            }) as string;

            return concatenatedRes;            
        }

        public string DummyChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
            {
                response = ConditionalAccess.CC_DummyChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID);
                return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
            }) as string;

            return concatenatedRes;
        }

        public string ChargeUserForSubscription(double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.BillingResponse response = null;

            string concatenatedRes = Execute(() =>
                {
                    response = ConditionalAccess.CC_ChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sSubscriptionID, sCouponCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sPaymentMethodID, sEncryptedCVV);
                    return response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }) as string;

            return concatenatedRes;
        }

        public bool CancelSubscription(string sUserGuid, string sSubscriptionID, int nSubscriptionPurchaseID)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    response = ConditionalAccess.CancelSubscription(m_wsUserName, m_wsPassword, sUserGuid, sSubscriptionID, nSubscriptionPurchaseID);                    
                    return response;
                }));

            return response;
        }        

        public TVPApiModule.Objects.Responses.BillingResponse InAppChargeUserForSubscription(double iPrice, string sCurrency, string sUserIP, string sUserGuid, string sExtraParameters, string sUDID, string sProductCode, string sReceipt)
        {
            TVPApiModule.Objects.Responses.BillingResponse response = null;

            response = Execute(() =>
                {
                    var res = ConditionalAccess.InApp_ChargeUserForSubscription(m_wsUserName, m_wsPassword, sUserGuid, iPrice, sCurrency, sProductCode, sUserIP, sExtraParameters, string.Empty, string.Empty, sUDID, sReceipt);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as TVPApiModule.Objects.Responses.BillingResponse;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetUserPermittedItems(string sSiteGuid)
        {
            List<TVPApiModule.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPermittedItems(m_wsUserName, m_wsPassword, sSiteGuid);

                    if (response != null)
                    {
                        retVal = response.Where(pmc => pmc != null).Select(m => m.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedMediaContainer>;

            return retVal;            
        }

        public List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> GetItemsPrice(int[] fileArray, string sSiteGuid, bool bOnlyLowest)
        {
            List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetItemsPrices(m_wsUserName, m_wsPassword, fileArray, sSiteGuid, bOnlyLowest, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());

                if (response != null)
                    retVal = response.Where(mf => mf != null).Select(mf => mf.ToApiObject()).ToList();

                return retVal;
            }) as List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetUserPermitedSubscriptions(string sSiteGuid)
        {
            List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPermittedSubscriptions(m_wsUserName, m_wsPassword, sSiteGuid);

                    if (response != null)
                    {
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public TVPApiModule.Objects.Responses.BillingTransactionsResponse GetUserTransactionHistory(string sSiteGuid, int startIndex, int count)
        {
            TVPApiModule.Objects.Responses.BillingTransactionsResponse retVal = null;

            retVal = Execute(() =>
                {
                    var res = ConditionalAccess.GetUserBillingHistory(m_wsUserName, m_wsPassword, sSiteGuid, startIndex, count);
                    if (res != null)
                        retVal = res.ToApiObject();

                    return retVal;
                }) as TVPApiModule.Objects.Responses.BillingTransactionsResponse;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetUserExpiredItems(string sSiteGuid, int numOfItems)
        {
            List<TVPApiModule.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserExpiredItems(m_wsUserName, m_wsPassword, sSiteGuid, numOfItems);

                    if (response != null)
                    {
                        retVal = response.Where(pm => pm != null).Select(m => m.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedMediaContainer>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetUserExpiredSubscriptions(string sSiteGuid, int numOfItems)
        {
            List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserExpiredSubscriptions(m_wsUserName, m_wsPassword, sSiteGuid, numOfItems);

                    if (response != null)
                    {
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();
                        if (retVal != null)
                        {
                            retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                        }
                    }
                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> GetSubscriptionsPrices(string sSiteGuid, string[] sSubscriptions, bool LowerPrice)
        {
            List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> returnObject = null;

            returnObject = Execute(() =>
                {
                    var response = ConditionalAccess.GetSubscriptionsPrices(m_wsUserName, m_wsPassword, sSubscriptions, sSiteGuid, string.Empty, string.Empty, string.Empty, SiteHelper.GetClientIP());

                    if (response != null)
                        returnObject = response.Where(sp => sp != null).Select(sp => sp.ToApiObject()).ToList();

                    return returnObject;
                }) as List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer>;

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

        public List<string> GetPrepaidBalance(string siteGuid, string currencyCode)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUserPrePaidStatus(m_wsUserName, m_wsPassword, siteGuid, currencyCode);

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

        public TVPApiModule.Objects.Responses.PrePaidResponseStatus PP_ChargeUserForMediaFile(string siteGuid, double price, string currency, int mediaFileID, string ppvModuleCode, string couponCode, string udid)
        {
            PrePaidResponse returnObject = null;

            TVPApiModule.Objects.Responses.PrePaidResponseStatus prePaidReturnObject = (TVPApiModule.Objects.Responses.PrePaidResponseStatus)Enum.Parse(typeof(TVPApiModule.Objects.Responses.PrePaidResponseStatus), Execute(() =>
            {
                returnObject = ConditionalAccess.PP_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currency, mediaFileID, ppvModuleCode, couponCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                return (TVPApiModule.Objects.Responses.PrePaidResponseStatus)returnObject.m_oStatus;
            }).ToString());

            return prePaidReturnObject;
        }

        public string GetMediaLicenseLink(string siteGuid, int mediaFileID, string baseLink, string udid)
        {
            string returnObject = null;

            returnObject = Execute(() =>
                {
                    returnObject = ConditionalAccess.GetLicensedLink(m_wsUserName, m_wsPassword, siteGuid, mediaFileID, baseLink, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);                    
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
                        retVal = ConditionalAccess.GetUserCAStatus(m_wsUserName, m_wsPassword, siteGuid);
                        logger.InfoFormat("Protocol: GetUserStatus, Parameters : SiteGuid : {0}", siteGuid);
                    }
                    return retVal;
                }).ToString());

            return retVal;            
        }

        public TVPApiModule.Objects.Responses.CampaignActionInfo ActivateCampaignWithInfo(string siteGuid, long campID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            TVPApiModule.Objects.Responses.CampaignActionInfo campaignActionInfo = null;

            campaignActionInfo = Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo campaignActionInfoParam = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo()
                        {
                            m_siteGuid = int.Parse(siteGuid),
                            m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SocialInviteInfo() { m_hashCode = hashCode } : null,
                            m_mediaID = mediaID,
                            m_mediaLink = mediaLink,
                            m_senderEmail = senderEmail,
                            m_senderName = senderName,
                            m_status = status,
                            m_voucherReceipents = voucherReceipents
                        };
                    var res = ConditionalAccess.ActivateCampaignWithInfo(m_wsUserName, m_wsPassword, (int)campID, campaignActionInfoParam);
                    if (res != null)
                        campaignActionInfo = res.ToApiObject();

                    logger.InfoFormat("Protocol: ActivateCampaignWithInfo, Parameters : campID : {0}", campID);

                    return campaignActionInfo;
                }) as TVPApiModule.Objects.Responses.CampaignActionInfo;

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
                    res = ConditionalAccess.AD_GetCustomDataID(m_wsUserName,
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
                    res = ConditionalAccess.GetCustomDataID(m_wsUserName,
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
                                                           TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            bool retVal = false;

            retVal = Convert.ToBoolean(Execute(() =>
                {
                    TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo actionInfo = new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo()
                    {
                        m_siteGuid = int.Parse(siteGuid),
                        m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.SocialInviteInfo() { m_hashCode = hashCode } : null,
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
                        retVal = ConditionalAccess.ActivateCampaign(m_wsUserName, m_wsPassword, campaignID, actionInfo);
                    }

                    return retVal;
                }));

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(string siteGuid, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("GetItemsPricesWithCoupons, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);

                        var response = ConditionalAccess.GetItemsPricesWithCoupons(m_wsUserName, m_wsPassword, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());

                        if (response != null)
                            retVal = response.Where(mf => mf != null).Select(mf => mf.ToApiObject()).ToList();

                    }

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(string[] sSubscriptions, string siteGuid, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("GetSubscriptionsPricesWithCoupon, Parameters : SiteGuid : {0} sCouponCode : {1}", siteGuid, sCouponCode);

                        var response = ConditionalAccess.GetSubscriptionsPricesWithCoupon(m_wsUserName, m_wsPassword, sSubscriptions, siteGuid, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName, SiteHelper.GetClientIP());

                        if (response != null)
                            retVal = response.Where(sp => sp != null).Select(sp => sp.ToApiObject()).ToList();
                    }

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer>;

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

                        retVal = ConditionalAccess.IsPermittedItem(m_wsUserName, m_wsPassword, siteGuid, mediaId);
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
                        retVal = ConditionalAccess.GetGoogleSignature(m_wsUserName, m_wsPassword, customerId);
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

                    retVal = ConditionalAccess.IsPermittedSubscription(m_wsUserName, m_wsPassword, siteGuid, subId, ref reason);
                }

                return retVal;
            }));

            return retVal;        
        }

        public TVPApiModule.Objects.Responses.BillingResponse InApp_ChargeUserForMediaFile(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName, string ReceiptData)
        {
            TVPApiModule.Objects.Responses.BillingResponse retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("InApp_ChargeUserForMediaFile, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                        var res = ConditionalAccess.InApp_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currency, productCode, ppvModuleCode, string.Empty, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName, ReceiptData);
                        if (res != null)
                            retVal = res.ToApiObject();
                    }

                    return retVal;
                }) as TVPApiModule.Objects.Responses.BillingResponse;

            return retVal;            
        }

        public TVPApiModule.Objects.Responses.BillingResponse CC_ChargeUserForPrePaid(string siteGuid, double price, string currency, string productCode, string ppvModuleCode, string sDeviceName)
        {
            TVPApiModule.Objects.Responses.BillingResponse retVal = null;

            retVal = Execute(() =>
                {
                    if (!string.IsNullOrEmpty(siteGuid))
                    {
                        logger.InfoFormat("CC_ChargeUserForPrePaid, Parameters : SiteGuid : {0} productCode : {1}", siteGuid, productCode);
                        var res = ConditionalAccess.CC_ChargeUserForPrePaid(m_wsUserName, m_wsPassword, siteGuid, price, currency, productCode, ppvModuleCode, SiteHelper.GetClientIP(), string.Empty, string.Empty, string.Empty, sDeviceName);
                        if (res != null)
                            retVal = res.ToApiObject();
                    }

                    return retVal;
                }) as TVPApiModule.Objects.Responses.BillingResponse;

            return retVal;
        }

        public string GetEPGLicensedLink(string siteGUID, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            string res = string.Empty;

            res = Execute(() =>
                {
                    return ConditionalAccess.GetEPGLicensedLink(m_wsUserName, m_wsPassword, siteGUID, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceName, formatType);
                }) as string;

            return res;            
        }

        public List<TVPApiModule.Objects.Responses.UserBillingTransactionsResponse> GetUsersBillingHistory(string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            List<TVPApiModule.Objects.Responses.UserBillingTransactionsResponse> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetUsersBillingHistory(m_wsUserName, m_wsPassword, siteGuids, startDate, endDate);
                    if (response != null)
                        retVal = response.Where(ubt => ubt != null).Select(ubt => ubt.ToApiObject()).ToList();

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.UserBillingTransactionsResponse>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainsBillingHistory(m_wsUserName, m_wsPassword, domainIDs, startDate, endDate);
                    if (response != null)
                        retVal = response.Where(dbt => dbt != null).Select(dbt => dbt.ToApiObject()).ToList();

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetDomainPermittedItems(int domainID)
        {
            List<TVPApiModule.Objects.Responses.PermittedMediaContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainPermittedItems(m_wsUserName, m_wsPassword, domainID);
                    if (response != null)
                        retVal = response.Where(pm => pm != null).Select(m => m.ToApiObject()).ToList();

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedMediaContainer>;

            return retVal;
        }

        public List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(int domainID)
        {
            List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> retVal = null;

            retVal = Execute(() =>
                {
                    var response = ConditionalAccess.GetDomainPermittedSubscriptions(m_wsUserName, m_wsPassword, domainID);
                    if (response != null)
                        retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                    return retVal;
                }) as List<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer>;

            return retVal;
        }

        public List<CollectionPricesContainer> GetCollectionPrices(string[] collections, string userGuid, string countryCode2, string languageCode3, string deviceName)
        {
            List<TVPApiModule.Objects.Responses.CollectionPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetCollectionsPrices(m_wsUserName, m_wsPassword, collections, userGuid, countryCode2, languageCode3, deviceName);
                if (response != null)
                    retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                return retVal;
            }) as List<TVPApiModule.Objects.Responses.CollectionPricesContainer>;

            return retVal;
        }

        public List<CollectionPricesContainer> GetCollectionPricesWithCoupon(string[] collections, string userGuid, string countryCode2, string languageCode3, string couponCode, string deviceName)
        {
            List<TVPApiModule.Objects.Responses.CollectionPricesContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetCollectionsPricesWithCoupon(m_wsUserName, m_wsPassword, collections, userGuid, couponCode, countryCode2, languageCode3, deviceName, SiteHelper.GetClientIP());
                if (response != null)
                    retVal = response.Where(cp => cp != null).Select(collection => collection.ToApiObject()).ToList();

                return retVal;
            }) as List<TVPApiModule.Objects.Responses.CollectionPricesContainer>;

            return retVal;
        }
        
        #endregion

        public List<TVPApiModule.Objects.Responses.PermittedCollectionContainer> GetUserPermittedCollections(string siteGuid)
        {
            List<TVPApiModule.Objects.Responses.PermittedCollectionContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetUserPermittedCollections(m_wsUserName, m_wsPassword, siteGuid);

                if (response != null)
                {
                    retVal = response.Where(pc => pc != null).Select(collection => collection.ToApiObject()).ToList();
                    if (retVal != null)
                    {
                        retVal = retVal.OrderByDescending(r => r.purchase_date.Date).ThenByDescending(r => r.purchase_date.TimeOfDay).ToList();
                    }
                }

                return retVal;
            }) as List<TVPApiModule.Objects.Responses.PermittedCollectionContainer>;

            return retVal;
        }

        public List<Objects.Responses.PermittedCollectionContainer> GetDomainPermittedCollections(int domainId)
        {
            List<TVPApiModule.Objects.Responses.PermittedCollectionContainer> retVal = null;

            retVal = Execute(() =>
            {
                var response = ConditionalAccess.GetDomainPermittedCollections(m_wsUserName, m_wsPassword, domainId);
                if (response != null)
                    retVal = response.Where(ps => ps != null).Select(s => s.ToApiObject()).ToList();

                return retVal;
            }) as List<TVPApiModule.Objects.Responses.PermittedCollectionContainer>;

            return retVal;
        }

        public Objects.Responses.ChangeSubscriptionStatus ChangeSubscription(string siteGuid, int oldSubscriptionId, int newSubscriptionId)
        {
            Objects.Responses.ChangeSubscriptionStatus status = Objects.Responses.ChangeSubscriptionStatus.Error;

            status = (TVPApiModule.Objects.Responses.ChangeSubscriptionStatus)Enum.Parse(typeof(TVPApiModule.Objects.Responses.ChangeSubscriptionStatus), Execute(() =>
            {
                status = (TVPApiModule.Objects.Responses.ChangeSubscriptionStatus)ConditionalAccess.ChangeSubscription(m_wsUserName, m_wsPassword, siteGuid, oldSubscriptionId, newSubscriptionId);
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
                res = ConditionalAccess.GetCustomDataID(m_wsUserName, m_wsPassword, siteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3,
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
                var response = ConditionalAccess.CC_DummyChargeUserForCollection(m_wsUserName, m_wsPassword, siteGuid, price, currency, collectionId, couponCode, userIP, extraParameters, countryCode2, languageCode3, deviceId);
                if (response != null)
                {
                    res = response.m_oStatus.ToString() + "|" + response.m_sRecieptCode;
                }

                return res;
            }) as string;

            return res;
        }

        public TVPApiModule.Objects.Responses.BillingResponse ChargeUserForCollection(string siteGuid, string collectionId, double price, string currency, string encrypteCVV, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3, string deviceId, string paymentMethodId)
        {
            TVPApiModule.Objects.Responses.BillingResponse billingResponse = null;

            billingResponse = Execute(() =>
            {
                var response = ConditionalAccess.CC_ChargeUserForCollection(m_wsUserName, m_wsPassword, siteGuid, price, currency, collectionId, couponCode, userIP, extraParameters, countryCode2, languageCode3, deviceId, paymentMethodId, encrypteCVV);
                if (response != null)
                {
                    billingResponse = response.ToApiObject();
                }

                return billingResponse;
            }) as TVPApiModule.Objects.Responses.BillingResponse;

            return billingResponse;
        }

        public TVPApiModule.Objects.Responses.BillingResponse CellularChargeUserForSubscription(string siteGuid, double price, string currencyCode, string subscriptionCode, string couponCode, string userIP, string extraParameters, string countryCode, string languageCode3, string deviceName)
        {
            TVPApiModule.Objects.Responses.BillingResponse billingResponse = null;

            billingResponse = Execute(() =>
            {
                var response = ConditionalAccess.Cellular_ChargeUserForSubscription(m_wsUserName, m_wsPassword, siteGuid, price, currencyCode, subscriptionCode, couponCode, userIP, extraParameters, countryCode, languageCode3, deviceName);
                if (response != null)
                {
                    billingResponse = response.ToApiObject();
                }

                return billingResponse;
            }) as TVPApiModule.Objects.Responses.BillingResponse;

            return billingResponse;
        }

        public string ChargeUserForSubscriptionByPaymentMethod(string siteGuid, double price, string currencyCode, string subscriptionCode, string couponCode, string userIP, string extraParameters, string countryCode, string languageCode3, string deviceName, string paymentMethodId, string encryptedCVV)
        {
            string response = string.Empty;

            response = Execute(() =>
            {
                var billingResponse = ConditionalAccess.CC_ChargeUserForSubscription(m_wsUserName, m_wsPassword, siteGuid, price, currencyCode, subscriptionCode, couponCode, userIP, extraParameters, countryCode, languageCode3, deviceName, paymentMethodId, encryptedCVV);
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
                var billingResponse = ConditionalAccess.CC_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currencyCode, fileId, ppvModuleCode, string.Empty, userIP, extraParameters, string.Empty, string.Empty, deviceName, paymentMethodId, encryptedCVV);
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
                var billingResponse = ConditionalAccess.Cellular_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currencyCode, fileId, ppvModuleCode, couponCode, userIP, extraParameters, countryCode, languageCode, deviceName);
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
                var billingResponse = ConditionalAccess.CC_ChargeUserForMediaFile(m_wsUserName, m_wsPassword, siteGuid, price, currency, fileId, ppvModuleCode, couponCode, userIp, string.Empty, string.Empty, string.Empty, udid, paymentMethodID, encryptedCVV);
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
                var billingResponse = ConditionalAccess.CC_ChargeUserForSubscription(m_wsUserName, m_wsPassword, siteGuid, price, currency, subscriptionId, couponCode, userIp, extraParameters, countryCode, languageCode, udid, paymentMethodID, encryptedCVV);
                if (billingResponse != null)
                {
                    response = billingResponse.m_oStatus.ToString() + "|" + billingResponse.m_sRecieptCode;
                }

                return response;
            }) as string;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.PermittedCollectionContainer> GetUserExpiredCollections(string siteGuid, int numOfItems)
        {
            List<TVPApiModule.Objects.Responses.PermittedCollectionContainer> permittedCollections = null;

            permittedCollections = Execute(() =>
            {
                var response = ConditionalAccess.GetUserExpiredCollections(m_wsUserName, m_wsPassword, siteGuid, numOfItems);
                if (response != null)
                {
                    permittedCollections = response.Where(pc => pc != null).Select(pc => pc.ToApiObject()).ToList();
                }

                return permittedCollections;
            }) as List<TVPApiModule.Objects.Responses.PermittedCollectionContainer>;

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

        public bool CancelTransaction(string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isCancelationSucceeded = false;

            isCancelationSucceeded = Convert.ToBoolean(Execute(() =>
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    isCancelationSucceeded = ConditionalAccess.CancelTransaction(m_wsUserName, m_wsPassword, siteGuid, assetId, transactionType);
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
                    isWaiverTransactionSucceeded = ConditionalAccess.WaiverTransaction(m_wsUserName, m_wsPassword, siteGuid, assetId, transactionType);
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
    }
}
