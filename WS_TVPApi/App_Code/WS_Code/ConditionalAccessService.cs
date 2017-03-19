using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using System.Web;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ConditionalAccessService : System.Web.Services.WebService, IConditionalAccessService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod(EnableSession = true, Description = "Activate Campaign with information")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionInfo ActivateCampaignWithInfo(InitializationObject initObj, long campID, string hashCode, int mediaID, string mediaLink,
                                                                                                                string senderEmail, string senderName, CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            CampaignActionInfo campaignActionInfo = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaignWithInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    campaignActionInfo = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaignWithInfo(initObj.SiteGuid, campID, hashCode, mediaID, mediaLink, senderEmail, senderName, status, voucherReceipents);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return campaignActionInfo;
        }
        [WebMethod(EnableSession = true, Description = "Get customer data")]
        [PrivateMethod]
        public int AD_GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AD_GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).AD_GetCustomDataID(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get customer data")]
        public int GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate, string sPreviewModelID)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetCustomDataID(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType, overrideEndDate, sPreviewModelID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;

                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Create Purchase Token")]
        [PrivateMethod]
        public int CreatePurchaseToken(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate, string previewModuleID)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CreatePurchaseToken", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).CreatePurchaseToken(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType, overrideEndDate, previewModuleID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;

                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Activate Campaign")]
        [PrivateMethod]
        public bool ActivateCampaign(InitializationObject initObj, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    CampaignActionInfo actionInfo = new CampaignActionInfo()
                    {
                        m_siteGuid = int.Parse(initObj.SiteGuid),
                        m_socialInviteInfo = !string.IsNullOrEmpty(hashCode) ? new SocialInviteInfo() { m_hashCode = hashCode } : null,
                        m_mediaID = mediaID,
                        m_mediaLink = mediaLink,
                        m_senderEmail = senderEmail,
                        m_senderName = senderName,
                        m_status = status,
                        m_voucherReceipents = voucherReceipents
                    };

                    res = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaign(initObj.SiteGuid, campaignID, actionInfo);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get items prices with coupon")]
        public MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            MediaFileItemPricesContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemsPricesWithCoupons", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                if (sUserGUID == "0")
                    sUserGUID = string.Empty;

                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() && (!string.IsNullOrEmpty(initObj.SiteGuid) || !string.IsNullOrEmpty(sUserGUID)) &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sUserGUID, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPricesWithCoupons(initObj.SiteGuid, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get subscriptions prices with coupon")]
        [PrivateMethod]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, sUserGUID, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(initObj.SiteGuid, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted item")]
        [PrivateMethod]
        public bool IsPermittedItem(InitializationObject initObj, int mediaId)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsPermittedItem", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).IsPermittedItem(initObj.SiteGuid, mediaId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted subscription")]
        [PrivateMethod]
        public bool IsPermittedSubscription(InitializationObject initObj, int subId)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsPermittedSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).IsPermittedSubscription(initObj.SiteGuid, subId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Gets Google signature")]
        [PrivateMethod]
        public string GetGoogleSignature(InitializationObject initObj, int customerId)
        {
            string res = string.Empty;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetGoogleSignature", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetGoogleSignature(initObj.SiteGuid, customerId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get customer data")]
        [PrivateMethod]
        public BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode, string receipt)
        {
            BillingResponse res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "InApp_ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).InApp_ChargeUserForMediaFile(initObj.SiteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID, receipt);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Charges users for PP")]
        [PrivateMethod]
        public BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode)
        {
            BillingResponse res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CC_ChargeUserForPrePaid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).CC_ChargeUserForPrePaid(initObj.SiteGuid, price, currency, productCode, ppvModuleCode, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Charges license for EPG")]
        [PrivateMethod]
        public string GetEPGLicensedLink(InitializationObject initObj, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            string res = string.Empty;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGLicensedLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    var response = new ApiConditionalAccessService(groupId, initObj.Platform).GetEPGLicensedLink(initObj.SiteGuid, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceName, formatType);
                    if (response != null)
                        res = response.LicensedLink.MainUrl;
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get user's billing history")]
        [PrivateMethod]
        public UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            UserBillingTransactionsResponse[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                // Tokenization: validate siteGuids
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateMultipleSiteGuids(initObj.SiteGuid, siteGuids, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetUsersBillingHistory(siteGuids, startDate, endDate);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get domains' billing history")]
        [PrivateMethod]
        public DomainBillingTransactionsResponse[] GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            DomainBillingTransactionsResponse[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainsBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                // Tokenization: validate domainId
                if (AuthorizationManager.IsTokenizationEnabled() && (domainIDs != null && domainIDs.Length > 0 && domainIDs.Length < 2) &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, domainIDs[0], null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    var temporaryResult = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainsBillingHistory(domainIDs, startDate, endDate);

                    if (temporaryResult != null)
                    {
                        res = temporaryResult.billingTransactions;
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get domain transactions history")]
        [PrivateMethod]
        public TVPApiModule.Objects.Responses.ConditionalAccess.DomainTransactionsHistoryResponse GetDomainTransactionsHistory(InitializationObject initObj, DateTime startDate, DateTime endDate)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.DomainTransactionsHistoryResponse res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainsBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                // Tokenization: validate domainId
                if (AuthorizationManager.IsTokenizationEnabled() && !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainTransactionsHistory(initObj.DomainID, startDate, endDate);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Retrieve domain's permitted media")]
        [PrivateMethod]
        public PermittedMediaContainer[] GetDomainPermittedItems(InitializationObject initObj)
        {
            PermittedMediaContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedItems(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Retrieve domain's permitted subscriptions")]
        [PrivateMethod]
        public PermittedSubscriptionContainer[] GetDomainPermittedSubscriptions(InitializationObject initObj)
        {
            PermittedSubscriptionContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedSubscriptions(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
                HttpContext.Current.Items["Error"] = "Unknown group";
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file using credit card")]
        [PrivateMethod]
        public string ChargeUserForMediaFileUsingCC(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sUserIP, string sCoupon, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            // get the client IP from header/method parameters
            string clientIp = string.IsNullOrEmpty(sUserIP) ? SiteHelper.GetClientIP() : sUserIP;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaFileUsingCC", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForMediaFileUsingCC(iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon, clientIp, initObj.SiteGuid, initObj.UDID, sPaymentMethodID, sEncryptedCVV);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription using credit card")]
        [PrivateMethod]
        public string ChargeUserForMediaSubscriptionUsingCC(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            // get the client IP from header/method parameters
            string clientIp = string.IsNullOrEmpty(sUserIP) ? SiteHelper.GetClientIP() : sUserIP;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaSubscriptionUsingCC", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, 0, sUDID, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForSubscriptionUsingCC(iPrice, sCurrency, sSubscriptionID, sCouponCode, clientIp, initObj.SiteGuid, sExtraParameters, sUDID, sPaymentMethodID, sEncryptedCVV);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Charge User For Media File using Cellular")]
        [PrivateMethod]
        public BillingResponse Cellular_ChargeUserForMediaFile(InitializationObject initObj, double price, string currencyCode3, int mediaFileID, string ppvModuleCode, string couponCode, string extraParameters, string deviceName)
        {
            BillingResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "Cellular_ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).Cellular_ChargeUserForMediaFile(initObj.SiteGuid, price, currencyCode3, mediaFileID, ppvModuleCode, couponCode, SiteHelper.GetClientIP(), extraParameters, string.Empty, string.Empty, deviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Charge User For Subscription using Cellular")]
        [PrivateMethod]
        public BillingResponse Cellular_ChargeUserForSubscription(InitializationObject initObj, double price, string currencyCode3, string subscriptionCode, string couponCode, string extraParameters, string deviceName)
        {
            BillingResponse response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "Cellular_ChargeUserForSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).Cellular_ChargeUserForSubscription(initObj.SiteGuid, price, currencyCode3, subscriptionCode, couponCode, SiteHelper.GetClientIP(), extraParameters, string.Empty, string.Empty, deviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file")]
        [PrivateMethod]
        public string ChargeUserForMediaFileByPaymentMethod(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sCoupon, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon, SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, sExtraParams, sPaymentMethodID, sEncryptedCVV);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
        [PrivateMethod]
        public string ChargeUserForSubscriptionByPaymentMethod(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sExtraParameters, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForSubscription(iPrice, sCurrency, sSubscriptionID, sCouponCode, SiteHelper.GetClientIP(), initObj.SiteGuid, sExtraParameters, initObj.UDID, sPaymentMethodID, sEncryptedCVV);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Change Subscription")]
        [PrivateMethod]
        public ChangeSubscriptionStatus ChangeSubscription(InitializationObject initObj, string sSiteGuid, int nOldSubscription, int nNewSubscription)
        {
            ChangeSubscriptionStatus response = ChangeSubscriptionStatus.Error; ;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChangeSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() && initObj.SiteGuid != sSiteGuid)
                {
                    AuthorizationManager.Instance.returnError(403);
                    return ChangeSubscriptionStatus.Error;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChangeSubscription(sSiteGuid, nOldSubscription, nNewSubscription);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get collections prices")]
        [PrivateMethod]
        public CollectionsPricesContainer[] GetCollectionsPrices(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3)
        {
            CollectionsPricesContainer[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCollectionsPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, userGuid, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetCollectionsPrices(collections, userGuid, countryCode2, languageCode3, initObj.UDID);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get collections prices with coupon")]
        [PrivateMethod]
        public CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3, string couponCode)
        {
            CollectionsPricesContainer[] response = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCollectionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, userGuid, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetCollectionsPricesWithCoupon(collections, userGuid, countryCode2, languageCode3, initObj.UDID, couponCode, clientIp);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Get user permitted collections")]
        [PrivateMethod]
        public PermittedCollectionContainer[] GetUserPermittedCollections(InitializationObject initObj, string siteGuid)
        {
            PermittedCollectionContainer[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedCollections", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedCollections(siteGuid);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Get domain permitted collections")]
        [PrivateMethod]
        public PermittedCollectionContainer[] GetDomainPermittedCollections(InitializationObject initObj)
        {
            PermittedCollectionContainer[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedCollections", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedCollections(initObj.DomainID);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Charge users for collection")]
        [PrivateMethod]
        public BillingResponse ChargeUserForCollection(InitializationObject initObj, double price, string currencyCode3, string collectionCode, string couponCode, string extraParameters, string countryCode2, string languageCode3, string paymentMethodID, string encryptedCvv)
        {
            BillingResponse response = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForCollection", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForCollection(initObj.SiteGuid, price, currencyCode3, collectionCode, couponCode, clientIp, extraParameters, countryCode2, languageCode3, initObj.UDID, paymentMethodID, encryptedCvv);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Dummy Charge users for collection")]
        [PrivateMethod]
        public string DummyChargeUserForCollection(InitializationObject initObj, double price, string currency, string collectionCode, string couponCode, string userIP, string extraParameters, string countryCode2, string languageCode3)
        {
            string response = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DummyChargeUserForCollection", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).DummyChargeUserForCollection(price, currency, collectionCode, couponCode, clientIp, initObj.SiteGuid, extraParameters, initObj.UDID, countryCode2, languageCode3);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Transaction")]
        [PrivateMethod]
        public bool CancelTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType, bool bIsForce)
        {
            bool isTransactionCancelled = false;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelTransaction", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupId, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    isTransactionCancelled = new ApiConditionalAccessService(groupId, initObj.Platform).CancelTransaction(siteGuid, assetId, transactionType, bIsForce);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return isTransactionCancelled;
        }

        [WebMethod(EnableSession = true, Description = "Waiver Transaction")]
        [PrivateMethod]
        public bool WaiverTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isWaiverTransactionSucceeded = false;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "WaiverTransaction", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate if siteGuid is the same as in initObj
                if (AuthorizationManager.IsTokenizationEnabled() && initObj.SiteGuid != siteGuid)
                {
                    AuthorizationManager.Instance.returnError(403);
                    return false;
                }
                try
                {
                    isWaiverTransactionSucceeded = new ApiConditionalAccessService(groupId, initObj.Platform).WaiverTransaction(siteGuid, assetId, transactionType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return isWaiverTransactionSucceeded;
        }

        [WebMethod(EnableSession = true, Description = "Get User Expired Collection")]
        [PrivateMethod]
        public PermittedCollectionContainer[] GetUserExpiredCollections(InitializationObject initObj, string siteGuid, int numOfItems)
        {
            PermittedCollectionContainer[] collections = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredCollection", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                // Tokenization: validate siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, siteGuid, 0, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    collections = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredCollections(siteGuid, numOfItems);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return collections;
        }

        [WebMethod(EnableSession = true, Description = "Returns the CDN URLs to use in case one fails")]
        [PrivateMethod]
        public TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LicensedLinkResponse GetLicensedLinks(InitializationObject initObj, int mediaFileID, string baseLink)
        {
            TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.LicensedLinkResponse links = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredCollection", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    links = new ApiConditionalAccessService(groupId, initObj.Platform).GetLicensedLinks(initObj.SiteGuid, mediaFileID, baseLink, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return links;
        }

        [WebMethod(EnableSession = true, Description = "Issues a record asset request")]
        [PrivateMethod]
        public RecordResponse RecordAsset(InitializationObject initObj, string epgId)
        {
            RecordResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RecordAsset", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).RecordAsset(initObj.SiteGuid, initObj.DomainID, initObj.UDID, epgId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a cancel asset request")]
        [PrivateMethod]
        public NPVRResponse CancelAssetRecording(InitializationObject initObj, string recordingId)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelAssetRecording", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).CancelAssetRecording(initObj.SiteGuid, initObj.DomainID, initObj.UDID, recordingId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a delete asset request")]
        [PrivateMethod]
        public NPVRResponse DeleteAssetRecording(InitializationObject initObj, string recordingId)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DeleteAssetRecording", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).DeleteAssetRecording(initObj.SiteGuid, initObj.DomainID, initObj.UDID, recordingId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a get NPVR quota request")]
        [PrivateMethod]
        public QuotaResponse GetNPVRQuota(InitializationObject initObj)
        {
            QuotaResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetNPVRQuota", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetNPVRQuota(initObj.SiteGuid, initObj.DomainID, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        //[WebMethod(EnableSession = true, Description = "")]
        //public NPVRResponse RecordSeriesByName(InitializationObject initObj, string assetId)
        //{
        //    NPVRResponse res = null;

        //    string clientIp = SiteHelper.GetClientIP();

        //    int groupId = ConnectionHelper.GetGroupID("tvpapi", "RecordSeriesByName", initObj.ApiUser, initObj.ApiPass, clientIp);

        //    if (groupId > 0)
        //    {
        //        try
        //        {
        //            res = new ApiConditionalAccessService(groupId, initObj.Platform).RecordSeriesByName(initObj.SiteGuid, initObj.DomainID, initObj.UDID, assetId);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items["Error"] = ex;
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items["Error"] = "Unknown group";
        //    }

        //    return res;
        //}

        [WebMethod(EnableSession = true, Description = "Issues a record series request")]
        [PrivateMethod]
        public NPVRResponse RecordSeriesByProgramId(InitializationObject initObj, string assetId)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RecordSeriesByProgramId", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).RecordSeriesByProgramId(initObj.SiteGuid, initObj.DomainID, initObj.UDID, assetId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a delete series request")]
        [PrivateMethod]
        public NPVRResponse DeleteSeriesRecording(InitializationObject initObj, string seriesRecordingId)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "DeleteSeriesRecording", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).DeleteSeriesRecording(initObj.SiteGuid, initObj.DomainID, initObj.UDID, seriesRecordingId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a cancel series request")]
        [PrivateMethod]
        public NPVRResponse CancelSeriesRecording(InitializationObject initObj, string seriesRecordingId)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSeriesRecording", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).CancelSeriesRecording(initObj.SiteGuid, initObj.DomainID, initObj.UDID, seriesRecordingId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Issues a cancel series request")]
        [PrivateMethod]
        public NPVRResponse SetAssetProtectionStatus(InitializationObject initObj, string recordingId, bool isProtect)
        {
            NPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetAssetProtectionStatus", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).SetAssetProtectionStatus(initObj.SiteGuid, initObj.DomainID, initObj.UDID, recordingId, isProtect);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Retrieves NPVR Licensed Link")]
        [PrivateMethod]
        public LicensedLinkNPVRResponse GetNPVRLicensedLink(InitializationObject initObj, string recordingId, DateTime startTime, int mediaFileID, string basicLink, string referrer, string couponCode)
        {
            LicensedLinkNPVRResponse res = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetNPVRLicensedLink", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetNPVRLicensedLink(initObj.SiteGuid, initObj.DomainID, initObj.UDID, recordingId, startTime, mediaFileID, basicLink, clientIp, referrer, initObj.Locale.LocaleCountry, initObj.Locale.LocaleLanguage, couponCode);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Cancel household service now")]
        [PrivateMethod]
        public ClientResponseStatus CancelServiceNow(InitializationObject initObj, int domainID, int serviceID, eTransactionType serviceType, bool forceCancel)
        {
            ClientResponseStatus clientResponse;

            string clientIp = SiteHelper.GetClientIP();

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "CancelServiceNow", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, domainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    clientResponse = new ApiConditionalAccessService(nGroupId, initObj.Platform).CancelServiceNow(domainID, serviceID, serviceType, forceCancel);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                clientResponse = ResponseUtils.ReturnBadCredentialsClientResponse();
            }

            return clientResponse;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Subscription")]
        [PrivateMethod]
        public bool CancelSubscription(InitializationObject initObj, string sSubscriptionID, int sSubscriptionPurchaseID)
        {
            bool response = false;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).CancelSubscription(initObj.SiteGuid, sSubscriptionID, sSubscriptionPurchaseID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Subscription Renewal")]
        [PrivateMethod]
        public ClientResponseStatus CancelSubscriptionRenewal(InitializationObject initObj, int domainID, string serviceID)
        {
            ClientResponseStatus clientResponse = new ClientResponseStatus();

            string clientIp = SiteHelper.GetClientIP();

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "CancelSubscriptionRenewal", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, domainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    clientResponse = new ApiConditionalAccessService(nGroupId, initObj.Platform).CancelSubscriptionRenewal(domainID, serviceID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                clientResponse = ResponseUtils.ReturnBadCredentialsClientResponse();
            }

            return clientResponse;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Subscription")]
        [PrivateMethod]
        public ServicesResponse GetDomainServices(InitializationObject initObj, int domainID)
        {
            ServicesResponse response;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainServices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, domainID, null, groupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainServices(domainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new ServicesResponse(); ;
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new ServicesResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Gets link for EPG")]
        [PrivateMethod]
        public TVPApiModule.Objects.Responses.LicensedLinkResponse GetEPGLicensedData(InitializationObject initObj, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, int formatType)
        {
            TVPApiModule.Objects.Responses.LicensedLinkResponse response = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGLicensedResponse", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).
                        GetEPGLicensedLink(initObj.SiteGuid, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, initObj.UDID, formatType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }
            return response;
        }

        [WebMethod(EnableSession = true, Description = "Charge a user’s household for specific content utilizing the household’s pre-assigned payment gateway. Online, one-time charge only of various content types. Upon successful charge entitlements to use the requested content are granted.")]
        [PrivateMethod]
        public TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse Purchase(InitializationObject initObj, string user_id, double price, string currency,
            int content_id, int product_id, string product_type, string coupon, int payment_gateway_id, int payment_method_id)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse response = null;

            if (string.IsNullOrEmpty(product_type) || !Enum.IsDefined(typeof(eTransactionType), product_type))
            {
                response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse();
                response.Status = ResponseUtils.ReturnBadRequestStatus("Invalid parameter format product_type");
                return response;
            }

            eTransactionType productType = (eTransactionType)Enum.Parse(typeof(eTransactionType), product_type);


            int groupID = ConnectionHelper.GetGroupID("tvpapi", "Purchase", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                {
                    return null;
                }

                try
                {
                    response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).Purchase(user_id, price, currency,
                        content_id, product_id, productType, coupon, string.Empty, payment_gateway_id, payment_method_id);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Grant entitlements for a household for specific product or subscription. If a subscription is provided – the grant will apply only till the end of the first renewal period.")]
        [PrivateMethod]
        public ClientResponseStatus GrantEntitlements(InitializationObject initObj, string user_id, int content_id, int product_id, string product_type, bool history)
        {
            TVPApiModule.Objects.Responses.ClientResponseStatus response = null;

            if (string.IsNullOrEmpty(product_type) || !Enum.IsDefined(typeof(eTransactionType), product_type))
            {
                response = new ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadRequestStatus("Invalid parameter format product_type");
                return response;
            }

            eTransactionType productType = (eTransactionType)Enum.Parse(typeof(eTransactionType), product_type);

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GrantEntitlements", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                {
                    return null;
                }

                try
                {
                    response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).GrantEntitlements(user_id, content_id, product_id, productType, history);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.ClientResponseStatus();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Verifies PPV/Subscription/Collection client purchase (such as InApp) and entitles the user.")]
        [PrivateMethod]
        public TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse ProcessReceipt(InitializationObject initObj, int product_id, eTransactionType product_type, string purchase_receipt, string payment_gateway_name, int content_id)
        {
            TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse response = null;


            int groupID = ConnectionHelper.GetGroupID("tvpapi", "Purchase", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and UDID
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                {
                    return null;
                }

                try
                {
                    response = new TVPApiModule.Services.ApiConditionalAccessService(groupID, initObj.Platform).ProcessReceipt(initObj.SiteGuid, content_id, product_id, product_type, string.Empty, purchase_receipt, payment_gateway_name);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new TVPApiModule.Objects.Responses.ConditionalAccess.TransactionResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }

            return response;
        }
    }
}
