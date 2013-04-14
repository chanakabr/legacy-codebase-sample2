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
            logger.InfoFormat("ActivateCampaignWithInfo-> [{0}, {1}], Params:[CampID: {2} , hashCode: {3} , mediaID: {4} , mediaLink: {5} , senderEmail: {6} , senderName: {7} , status: {8}]", groupId, initObj.Platform, campID, hashCode, mediaID, mediaLink, senderEmail, senderName, status);
            if (groupId > 0)
            {
                try
                {
                    campaignActionInfo = new ApiConditionalAccessService(groupId, initObj.Platform).ActivateCampaignWithInfo(initObj.SiteGuid, campID, hashCode, mediaID, mediaLink, senderEmail, senderName, status, voucherReceipents);
                }
                catch (Exception ex)
                {
                    logger.Error("ActivateCampaignWithInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ActivateCampaignWithInfo-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return campaignActionInfo;
        }
        [WebMethod(EnableSession = true, Description = "Get customer data")]
        public int AD_GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "AD_GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("Protocol: AD_GetCustomDataID, Parameters : Parameters : price - {0}, currencyCode3 - {1}, assetId - {2}, ppvModuleCode - {3}, campaignCode - {4}, couponCode - {5}, paymentMethod - {6}, userIp - {7}, countryCd2 - {8}, languageCode3 - {9}, deviceName - {10}, assetType - {11}",
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
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).AD_GetCustomDataID(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType);
                }
                catch (Exception ex)
                {
                    logger.Error("AD_GetCustomDataID->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("AD_GetCustomDataID-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get customer data")]
        public int GetCustomDataID(InitializationObject initObj, double price, string currencyCode3, int assetId, string ppvModuleCode, string campaignCode, string couponCode, string paymentMethod, string userIp, string countryCd2, string languageCode3, string deviceName, int assetType, string overrideEndDate)
        {
            int res = 0;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCustomDataID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("Protocol: GetCustomDataID, Parameters : Parameters : price - {0}, currencyCode3 - {1}, assetId - {2}, ppvModuleCode - {3}, campaignCode - {4}, couponCode - {5}, paymentMethod - {6}, userIp - {7}, countryCd2 - {8}, languageCode3 - {9}, deviceName - {10}, assetType - {11}, overrideEndDate - {12}",
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
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetCustomDataID(initObj.SiteGuid, price, currencyCode3, assetId, ppvModuleCode, campaignCode, couponCode, paymentMethod, userIp, countryCd2, languageCode3, deviceName, assetType, overrideEndDate);
                }
                catch (Exception ex)
                {
                    logger.Error("GetCustomDataID->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetCustomDataID-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Activate Campaign")]
        public bool ActivateCampaign(InitializationObject initObj, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                           CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "ActivateCampaign", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("Protocol: ActivateCampaign, Parameters : campaignID : {0} hashCode : {1} mediaID : {2} mediaLink : {3} senderEmail : {4} senderName : {5} status : {6}", campaignID, hashCode, mediaID, mediaLink, senderEmail, senderName, status.ToString());
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
                    logger.Error("ActivateCampaign->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ActivateCampaign-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get items prices with coupon")]
        public MediaFileItemPricesContainer[] GetItemsPricesWithCoupons(InitializationObject initObj, int[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            MediaFileItemPricesContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetItemsPricesWithCoupons", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetItemsPricesWithCoupons, Parameters : SiteGuid : {0} sCouponCode : {1} bOnlyLowest : {2} sCountryCd2 : {3} sLanguageCode3 : {4} sDeviceName : {5}", initObj.SiteGuid, sCouponCode, bOnlyLowest.ToString(), sCountryCd2, sLanguageCode3, sDeviceName);
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetItemsPricesWithCoupons(initObj.SiteGuid, nMediaFiles, sUserGUID, sCouponCode, bOnlyLowest, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    logger.Error("GetItemsPricesWithCoupons->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetItemsPricesWithCoupons-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get subscriptions prices with coupon")]
        public SubscriptionsPricesContainer[] GetSubscriptionsPricesWithCoupon(InitializationObject initObj, string[] sSubscriptions, string sUserGUID, string sCouponCode, string sCountryCd2, string sLanguageCode3, string sDeviceName)
        {
            SubscriptionsPricesContainer[] res = null;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsPricesWithCoupon", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetSubscriptionsPricesWithCoupon, Parameters : SiteGuid : {0} sCouponCode : {1} sCountryCd2 : {2} sLanguageCode3 : {3} sDeviceName : {4}", initObj.SiteGuid, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetSubscriptionsPricesWithCoupon(initObj.SiteGuid, sSubscriptions, sUserGUID, sCouponCode, sCountryCd2, sLanguageCode3, sDeviceName);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSubscriptionsPricesWithCoupon->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSubscriptionsPricesWithCoupon-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted item")]
        public bool IsPermittedItem(InitializationObject initObj, int mediaId)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsPermittedItem", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("IsPermittedItem, Parameters : SiteGuid : {0} mediaId : {1}", initObj.SiteGuid, mediaId);
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).IsPermittedItem(initObj.SiteGuid, mediaId);
                }
                catch (Exception ex)
                {
                    logger.Error("IsPermittedItem->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsPermittedItem-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Is permitted subscription")]
        public bool IsPermittedSubscription(InitializationObject initObj, int subId)
        {
            bool res = false;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "IsPermittedSubscription", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("IsPermittedSubscription, Parameters : SiteGuid : {0} subId : {1}", initObj.SiteGuid, subId);
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).IsPermittedSubscription(initObj.SiteGuid, subId);
                }
                catch (Exception ex)
                {
                    logger.Error("IsPermittedSubscription->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("IsPermittedSubscription-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            return res;
        }

        [WebMethod(EnableSession = true, Description = "Gets Google signature")]
        public string GetGoogleSignature(InitializationObject initObj, int customerId)
        {
            string res = string.Empty;
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetGoogleSignature", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            logger.InfoFormat("GetGoogleSignature, Parameters : SiteGuid : {0} customerId : {1}", initObj.SiteGuid, customerId);
            if (groupId > 0)
            {
                try
                {
                    res = new ApiConditionalAccessService(groupId, initObj.Platform).GetGoogleSignature(initObj.SiteGuid, customerId);
                }
                catch (Exception ex)
                {
                    logger.Error("GetGoogleSignature->", ex);
                }
            }
            else
                logger.ErrorFormat("GetGoogleSignature-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            return res;
        }
    }
}
