using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using System.Web;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ConditionalAccessService : System.Web.Services.WebService, IConditionalAccessService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ConditionalAccessService));

        [WebMethod(EnableSession = true, Description = "Activate Campaign with information")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return campaignActionInfo;
        }
        [WebMethod(EnableSession = true, Description = "Get customer data")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
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
                    HttpContext.Current.Items.Add("Error", ex);

                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Create Purchase Token")]
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
                    HttpContext.Current.Items.Add("Error", ex);

                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Activate Campaign")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
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
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPricesWithCoupons(initObj.SiteGuid, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get subscriptions prices with coupon")]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(initObj.SiteGuid, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted item")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted subscription")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Gets Google signature")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get customer data")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Charges users for PP")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Charges license for EPG")]
        public string GetEPGLicensedLink(InitializationObject initObj, int mediaFileID, int EPGItemID, DateTime startTime, string basicLink, string userIP, string refferer, string countryCd2, string languageCode3, string deviceName, int formatType)
        {
            string res = string.Empty;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetEPGLicensedLink", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetEPGLicensedLink(initObj.SiteGuid, mediaFileID, EPGItemID, startTime, basicLink, userIP, refferer, countryCd2, languageCode3, deviceName, formatType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get user's billing history")]
        public UserBillingTransactionsResponse[] GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            UserBillingTransactionsResponse[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUsersBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetUsersBillingHistory(siteGuids, startDate, endDate);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get domain's billing history")]
        public DomainBillingTransactionsResponse[] GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            DomainBillingTransactionsResponse[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainsBillingHistory", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainsBillingHistory(domainIDs, startDate, endDate);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Retrieve domain's permitted media")]
        public PermittedMediaContainer[] GetDomainPermittedItems(InitializationObject initObj)
        {
            PermittedMediaContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetDomainPermittedItems(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Retrieve domain's permitted subscriptions")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
                HttpContext.Current.Items.Add("Error", "Unknown group");
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file using credit card")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription using credit card")]
        public string ChargeUserForMediaSubscriptionUsingCC(InitializationObject initObj, double iPrice, string sCurrency, string sSubscriptionID, string sCouponCode, string sUserIP, string sExtraParameters, string sUDID, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            // get the client IP from header/method parameters
            string clientIp = string.IsNullOrEmpty(sUserIP) ? SiteHelper.GetClientIP() : sUserIP;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaSubscriptionUsingCC", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForSubscriptionUsingCC(iPrice, sCurrency, sSubscriptionID, sCouponCode, clientIp, initObj.SiteGuid, sExtraParameters, sUDID, sPaymentMethodID, sEncryptedCVV);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Charge User For Media File using Cellular")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Charge User For Subscription using Cellular")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for file")]
        public string ChargeUserForMediaFileByPaymentMethod(InitializationObject initObj, double iPrice, string sCurrency, int iFileID, string sPPVModuleCode, string sCoupon, string sExtraParams, string sPaymentMethodID, string sEncryptedCVV)
        {
            string response = string.Empty;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChargeUserForMediaFile(iPrice, sCurrency, iFileID, sPPVModuleCode, sCoupon , SiteHelper.GetClientIP(), initObj.SiteGuid, initObj.UDID, sExtraParams, sPaymentMethodID, sEncryptedCVV);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Perform a user purchase for subscription")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Change Subscription")]
        public ChangeSubscriptionStatus ChangeSubscription(InitializationObject initObj, string sSiteGuid, int nOldSubscription, int nNewSubscription)
        {
            ChangeSubscriptionStatus response = ChangeSubscriptionStatus.Error; ;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ChangeSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).ChangeSubscription(sSiteGuid, nOldSubscription, nNewSubscription);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;                       
        }

        [WebMethod(EnableSession = true, Description = "Get collections prices")]
        public CollectionsPricesContainer[] GetCollectionsPrices(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3)
        {
            CollectionsPricesContainer[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCollectionsPrices", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetCollectionsPrices(collections, userGuid, countryCode2, languageCode3, initObj.UDID);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get collections prices with coupon")]
        public CollectionsPricesContainer[] GetCollectionsPricesWithCoupon(InitializationObject initObj, string[] collections, string userGuid, string countryCode2, string languageCode3, string couponCode)
        {
            CollectionsPricesContainer[] response = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCollectionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetCollectionsPricesWithCoupon(collections, userGuid, countryCode2, languageCode3, initObj.UDID, couponCode, clientIp);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Get user permitted collections")]
        public PermittedCollectionContainer[] GetUserPermittedCollections(InitializationObject initObj, string siteGuid)
        {
            PermittedCollectionContainer[] response = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedCollections", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    response = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedCollections(siteGuid);

                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Get domain permitted collections")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }


        [WebMethod(EnableSession = true, Description = "Charge users for collection")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Dummy Charge users for collection")]
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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Cancel Transaction")]
        public bool CancelTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isTransactionCancelled = false;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "CancelTransaction", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    isTransactionCancelled = new ApiConditionalAccessService(groupId, initObj.Platform).CancelTransaction(siteGuid, assetId, transactionType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return isTransactionCancelled;
        }

        [WebMethod(EnableSession = true, Description = "Waiver Transaction")]
        public bool WaiverTransaction(InitializationObject initObj, string siteGuid, int assetId, eTransactionType transactionType)
        {
            bool isWaiverTransactionSucceeded = false;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "WaiverTransaction", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    isWaiverTransactionSucceeded = new ApiConditionalAccessService(groupId, initObj.Platform).WaiverTransaction(siteGuid, assetId, transactionType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return isWaiverTransactionSucceeded;
        }

        [WebMethod(EnableSession = true, Description = "Get User Expired Collection")]
        public PermittedCollectionContainer[] GetUserExpiredCollections(InitializationObject initObj, string siteGuid, int numOfItems)
        {
            PermittedCollectionContainer[] collections = null;

            string clientIp = SiteHelper.GetClientIP();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredCollection", initObj.ApiUser, initObj.ApiPass, clientIp);

            if (groupId > 0)
            {
                try
                {
                    collections = new ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredCollections(siteGuid, numOfItems);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return collections;
        }
    }
}
