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
//using TVPPro.SiteManager.TvinciPlatform.Pricing;
using System.Web;
using TVPApiModule.Helper;

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
    public class PricingService : System.Web.Services.WebService//, IPricingService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(PricingService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get PPV Module data")]
        public TVPApiModule.Objects.Responses.PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            TVPApiModule.Objects.Responses.PPVModule response = null;

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

        //// Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get all subscriptions contains media file")]
        //public TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        //{
        //    TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] subs = null;

        //    int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupId > 0)
        //    {
        //        try
        //        {
        //            subs = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingMediaFile(iMediaID, iFileID);
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return subs;
        //}

        [WebMethod(EnableSession = true, Description = "Get all subscriptions ID's contains media file")]
        public IEnumerable<int> GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            IEnumerable<int> subIds = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionIDsContainingMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    subIds = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionIDsContainingMediaFile(iMediaID, iFileID);
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

            return subIds;
        }

        [WebMethod(EnableSession = true, Description = "Get Coupon status according to coupon code")]
        public TVPApiModule.Objects.Responses.CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {        
            TVPApiModule.Objects.Responses.CouponData couponData = null;

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
        public TVPApiModule.Objects.Responses.CouponsStatus SetCouponUsed(InitializationObject initObj, string sCouponCode)
        {
            TVPApiModule.Objects.Responses.CouponsStatus couponStatus = TVPApiModule.Objects.Responses.CouponsStatus.NotExists;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetCouponUsed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    couponStatus = (TVPApiModule.Objects.Responses.CouponsStatus)new ApiPricingService(groupId, initObj.Platform).SetCouponUsed(sCouponCode, initObj.SiteGuid); 
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

        ////Deprecated!
        //[WebMethod(EnableSession = true, Description = "Get campaigns by type")]
        //public TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] GetCampaignsByType(InitializationObject initObj, CampaignTrigger trigger, bool isAlsoInactive)
        //{
        //    TVPPro.SiteManager.TvinciPlatform.Pricing.Campaign[] campaigns = null;

        //    int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCampaignsByType", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupId > 0)
        //    {
        //        try
        //        {
        //            campaigns = new ApiPricingService(groupId, initObj.Platform).GetCampaignsByType(trigger, isAlsoInactive, initObj.UDID);  
        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }
        //    else
        //    {
        //        HttpContext.Current.Items.Add("Error", "Unknown group");
        //    }

        //    return campaigns;
        //}

        [WebMethod(EnableSession = true, Description = "Get subscription data")]
        public List<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionData(InitializationObject initObj, int[] subIDs)
        {
            List<TVPApiModule.Objects.Responses.Subscription> res = new List<TVPApiModule.Objects.Responses.Subscription>();

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

        [WebMethod(EnableSession = true, Description = "Get subscriptions by user types")]
        public IEnumerable<TVPApiModule.Objects.Responses.Subscription> GetSubscriptionsContainingUserTypes(InitializationObject initObj, int isActive, int[] userTypesIDs)
        {
            IEnumerable<TVPApiModule.Objects.Responses.Subscription> res = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingUserTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    res = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingUserTypes(isActive, userTypesIDs);
                    
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
