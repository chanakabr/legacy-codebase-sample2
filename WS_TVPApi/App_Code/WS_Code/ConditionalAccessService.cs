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
        public BillingResponse InApp_ChargeUserForMediaFile(InitializationObject initObj, double price, string currencyCode3, int productCode, string ppvModuleCode, string receiptData)
        {
            BillingResponse res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "InApp_ChargeUserForMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).InApp_ChargeUserForMediaFile(initObj.SiteGuid, price, currencyCode3, productCode, ppvModuleCode, initObj.UDID, receiptData);
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
