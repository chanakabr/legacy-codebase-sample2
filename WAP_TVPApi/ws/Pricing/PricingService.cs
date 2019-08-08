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
using System.Web;
using KLogMonitor;
using System.Reflection;
using Core.Pricing;
using ApiObjects.Pricing;

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
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region public methods

        [WebMethod(EnableSession = true, Description = "Get PPV Module data")]
        public PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            PPVModule response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPPVModuleData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new ApiPricingService(groupID, initObj.Platform).GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
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

        [WebMethod(EnableSession = true, Description = "Get all subscriptions contains media file")]
        public Subscription[] GetSubscriptionsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            Subscription[] subs = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingMedia", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    subs = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingMediaFile(iMediaID, iFileID);
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

            return subs;
        }

        [WebMethod(EnableSession = true, Description = "Get all subscriptions ID's contains media file")]
        public int[] GetSubscriptionIDsContainingMediaFile(InitializationObject initObj, int iMediaID, int iFileID)
        {
            int[] subIds = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionIDsContainingMediaFile", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    subIds = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionIDsContainingMediaFile(iMediaID, iFileID);
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

            return subIds;
        }

        [WebMethod(EnableSession = true, Description = "Get Coupon status according to coupon code")]
        public CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {
            CouponData couponData = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCouponStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    couponData = new ApiPricingService(groupId, initObj.Platform).GetCouponStatus(sCouponCode);

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

            return couponData;
        }

        [WebMethod(EnableSession = true, Description = "Set Coupon used")]
        public CouponsStatus SetCouponUsed(InitializationObject initObj, string sCouponCode)
        {
            CouponsStatus couponStatus = CouponsStatus.NotExists;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "SetCouponUsed", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    couponStatus = new ApiPricingService(groupId, initObj.Platform).SetCouponUsed(sCouponCode, initObj.SiteGuid);
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

            return couponStatus;
        }

        [WebMethod(EnableSession = true, Description = "Get campaigns by type")]
        public Campaign[] GetCampaignsByType(InitializationObject initObj, CampaignTrigger trigger, bool isAlsoInactive)
        {
            Campaign[] campaigns = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCampaignsByType", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    campaigns = new ApiPricingService(groupId, initObj.Platform).GetCampaignsByType(trigger, isAlsoInactive, initObj.UDID);
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
                    var service = new ApiPricingService(groupId, initObj.Platform);

                    Subscription[] arrSubscriptionObjects = service.GetSubscriptionsData(subIDs.Select(sub => sub.ToString()).ToArray());

                    res = arrSubscriptionObjects.ToList();
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

        [WebMethod(EnableSession = true, Description = "Get subscriptions by user types")]
        public List<Subscription> GetSubscriptionsContainingUserTypes(InitializationObject initObj, int isActive, int[] userTypesIDs)
        {
            List<Subscription> res = new List<Subscription>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetSubscriptionsContainingUserTypes", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    Subscription[] arrSusbcriptiopns = new ApiPricingService(groupId, initObj.Platform).GetSubscriptionsContainingUserTypes(isActive, userTypesIDs);
                    if (arrSusbcriptiopns != null && arrSusbcriptiopns.Length > 0)
                    {
                        res = arrSusbcriptiopns.ToList();
                    }
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

        [WebMethod(EnableSession = true, Description = "Get collection data")]
        public Collection GetCollectionData(InitializationObject initObj, string collectionId, string countryCd2, string languageCode3, string deviceName, bool bGetAlsoUnActive)
        {
            Collection collection = new Collection();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCollectionData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                try
                {
                    collection = new ApiPricingService(groupId, initObj.Platform).GetCollectionData(collectionId, countryCd2, languageCode3, deviceName, bGetAlsoUnActive);
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

            return collection;
        }

        #endregion

    }
}
