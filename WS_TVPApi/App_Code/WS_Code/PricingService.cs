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
using System.Web;

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

            if (groupID > 0)
            {
                try
                {
                    response = new ApiPricingService(groupID, initObj.Platform).GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
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

        [WebMethod(EnableSession = true, Description = "Get all subscriptions contains media file")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] subs = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    subs = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingMediaFile(iMediaID, iFileID);
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

            return subs;
        }

        [WebMethod(EnableSession = true, Description = "Get Copun status according to coupon code")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {        
            TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData couponData = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCouponStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    couponData = new ApiPricingService(groupId, initObj.Platform).GetCouponStatus(sCouponCode);

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

            return couponData;
        }

        [WebMethod(EnableSession = true, Description = "Set Coupon used")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus SetCouponUsed(InitializationObject initObj, string sCouponCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus couponStatus = TVPPro.SiteManager.TvinciPlatform.Pricing.CouponsStatus.NotExists;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetCouponUsed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    couponStatus = new ApiPricingService(groupId, initObj.Platform).SetCouponUsed(sCouponCode, initObj.SiteGuid); 
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
            
            return couponStatus;
        }

        [WebMethod(EnableSession = true, Description = "Get campaigns by type")]
        public TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] GetCampaignsByType(InitializationObject initObj, CampaignTrigger trigger, bool isAlsoInactive)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] campaigns = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCampaignsByType", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    campaigns = new ApiPricingService(groupId, initObj.Platform).GetCampaignsByType(trigger, isAlsoInactive, initObj.UDID);  
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

            return campaigns;
        }

        [WebMethod(EnableSession = true, Description = "Get subscription data")]
        public List<Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs)
        {
            List<Subscription> res = new List<Subscription>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    foreach (int subID in subIDs)
                    {
                        res.Add(new ApiPricingService(groupId, initObj.Platform).GetSubscriptionData(subID.ToString(), false));
                    }
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

        #endregion
    }
}
