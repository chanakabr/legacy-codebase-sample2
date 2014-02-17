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
using TVPApiModule.Objects.Responses;
using TVPApiModule.Helper;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ConditionalAccessService : System.Web.Services.WebService//, IConditionalAccessService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ConditionalAccessService));

        [WebMethod(EnableSession = true, Description = "Activate Campaign with information")]
        public TVPApiModule.Objects.Responses.CampaignActionInfo ActivateCampaignWithInfo(InitializationObject initObj, long campID, string hashCode, int mediaID, string mediaLink,
                                                                                                                string senderEmail, string senderName, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            TVPApiModule.Objects.Responses.CampaignActionInfo campaignActionInfo = null;
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
        public int GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetCustomDataID(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType, overrideEndDate);
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
                                                           TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.CampaignActionResult status, TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.VoucherReceipentInfo[] voucherReceipents)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaign(initObj.SiteGuid, campaignID, hashCode, mediaID, mediaLink, 
                        senderEmail, senderName, status, voucherReceipents);
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
        public IEnumerable<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            IEnumerable<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> res = null;
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
        public IEnumerable<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            IEnumerable<TVPApiModule.Objects.Responses.SubscriptionsPricesContainer> res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
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
        public TVPApiModule.Objects.Responses.BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode, string receipt)
        {
            TVPApiModule.Objects.Responses.BillingResponse res = null;
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
        public TVPApiModule.Objects.Responses.BillingResponse CC_ChargeUserForPrePaid(InitializationObject initObj, double price, string currency, string productCode, string ppvModuleCode)
        {
            TVPApiModule.Objects.Responses.BillingResponse res = null;
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

        public IEnumerable<TVPApiModule.Objects.Responses.UserBillingTransactionsResponse> GetUsersBillingHistory(InitializationObject initObj, string[] siteGuids, DateTime startDate, DateTime endDate)
        {
            IEnumerable<TVPApiModule.Objects.Responses.UserBillingTransactionsResponse> res = null;
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

        public IEnumerable<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> GetDomainsBillingHistory(InitializationObject initObj, int[] domainIDs, DateTime startDate, DateTime endDate)
        {
            IEnumerable<TVPApiModule.Objects.Responses.DomainBillingTransactionsResponse> res = null;
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

        public IEnumerable<TVPApiModule.Objects.Responses.PermittedMediaContainer> GetDomainPermittedItems(InitializationObject initObj)
        {
            IEnumerable<TVPApiModule.Objects.Responses.PermittedMediaContainer> res = null;
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

        public IEnumerable<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> GetDomainPermittedSubscriptions(InitializationObject initObj)
        {
            IEnumerable<TVPApiModule.Objects.Responses.PermittedSubscriptionContainer> res = null;
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
    }
}
