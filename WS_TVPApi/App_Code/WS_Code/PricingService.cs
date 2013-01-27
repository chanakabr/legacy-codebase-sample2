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
using TVPPro.SiteManager.TvinciPlatform.Pricing;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class PricingService : System.Web.Services.WebService, IPricingService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(PricingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get PPV Module data")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPPVModuleData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetPPVModuleData-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new ApiPricingService(groupID, initObj.Platform).GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetPPVModuleData->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetPPVModuleData-> 'Unknown group' Username: {0}, Password: {1}, PPVCode: {2}", initObj.ApiUser, initObj.ApiPass, ppvCode);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Get all subscriptions contains media file")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] subs = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetSubscriptionsContainingMedia-> [{0}, {1}], Params:[mediaId: {2}, fileId: {3}]", groupId, initObj.Platform, iFileID);

            if (groupId > 0)
            {
                try
                {
                    subs = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingMediaFile(iMediaID, iFileID);
                }
                catch (Exception ex)
                {
                    logger.Error("GetSubscriptionsContainingMedia->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetSubscriptionsContainingMedia-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return subs;
        }

        [WebMethod(EnableSession = true, Description = "Get Copun status according to coupon code")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {        
            TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData couponData = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCouponStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetCouponStatus-> [{0}, {1}], Params:[CouponCode: {2}]", groupId, initObj.Platform, sCouponCode);

            if (groupId > 0)
            {
                try
                {
                    couponData = new ApiPricingService(groupId, initObj.Platform).GetCouponStatus(sCouponCode);

                }
                catch (Exception ex)
                {
                    logger.Error("GetCouponStatus->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetCouponStatus-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return couponData;
        }

        [WebMethod(EnableSession = true, Description = "Set Coupon used")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus SetCouponUsed(InitializationObject initObj, string sCouponCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus couponStatus = TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus.NotExists;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetCouponUsed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SetCouponUsed-> [{0}, {1}], Params:[CouponCode: {2}]", groupId, initObj.Platform, sCouponCode);

            if (groupId > 0)
            {
                try
                {
                    couponStatus = new ApiPricingService(groupId, initObj.Platform).SetCouponUsed(sCouponCode, initObj.SiteGuid); 
                }
                catch (Exception ex)
                {
                    logger.Error("SetCouponUsed->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SetCouponUsed-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }
            
            return couponStatus;
        }

        [WebMethod(EnableSession = true, Description = "Get campaigns by type")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] GetCampaignsByType(InitializationObject initObj, CampaignTrigger trigger, bool isAlsoInactive)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] campaigns = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCampaignsByType", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetCampaignsByType-> [{0}, {1}],  Params:[trigger: {2}, isAlsoInactive: {3}]", trigger , isAlsoInactive);

            if (groupId > 0)
            {
                try
                {
                    campaigns = new ApiPricingService(groupId, initObj.Platform).GetCampaignsByType(trigger, isAlsoInactive, initObj.UDID);  
                }
                catch (Exception ex)
                {
                    logger.Error("GetCampaignsByType->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("GetCampaignsByType-> 'Unknown group' Username: {0}, Password: {1}", initObj.ApiUser, initObj.ApiPass);
            }

            return campaigns;
        }

        #endregion
    }
}
