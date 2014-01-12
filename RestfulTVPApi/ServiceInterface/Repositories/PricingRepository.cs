using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;

namespace RestfulTVPApi.ServiceInterface
{
    public class PricingRepository : IPricingRepository
    {
        public TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData couponData = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetCouponStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                couponData = new ApiPricingService(groupId, initObj.Platform).GetCouponStatus(sCouponCode);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return couponData;
        }

        //Ofir - Should udid be a param?
        public TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPPVModuleData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new ApiPricingService(groupID, initObj.Platform).GetPPVModuleData(ppvCode, string.Empty, string.Empty, initObj.UDID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }
    }
}