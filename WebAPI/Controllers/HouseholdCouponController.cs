using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    // TODO SHIR - CRUD changes
    [Service("householdCoupon")]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, CouponWallet, string, CouponWalletFilter>
    {
        /// <summary>
        /// householdCoupon add
        /// </summary>
        /// <param name="householdCoupon">householdCoupon details</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CouponCodeIsMissing)]
        [Throws(eResponseStatus.CouponCodeAlreadyLoaded)]
        [Throws(eResponseStatus.CouponNotValid)]
        [Throws(eResponseStatus.HouseholdRequired)]        
        static public KalturaHouseholdCoupon Add(KalturaHouseholdCoupon householdCoupon)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);
            var extraParams = new Dictionary<string, object>() { { "householdId", householdId } };
            // TODO SHIR - TALK WITH TANTAN ABOUT THIS (that controller is static)
            var response = KalturaCrudController<KalturaHouseholdCoupon, CouponWallet, string, CouponWalletFilter>.Add(groupId, householdCoupon,extraParams);
            return response;
        }

        /// <summary>
        /// Remove coupon from household
        /// </summary>
        /// <param name="code">Coupon code</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CouponCodeNotInHousehold)]
        static public void Delete(string code)
        {
            int groupId = KS.GetFromRequest().GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);
            var extraParams = new Dictionary<string, object>() { { "householdId", householdId } };
            KalturaCrudController<KalturaHouseholdCoupon, CouponWallet, string, CouponWalletFilter>.Delete(groupId, code, CouponWalletHandler.Instance, extraParams);
        }

        /// <summary>
        /// Lists all topic notifications in the system.
        /// </summary>
        /// <param name="filter">Filter options</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdCouponListResponse List(KalturaHouseholdCouponFilter filter)
        {
            KalturaHouseholdCouponListResponse response = new KalturaHouseholdCouponListResponse();
            var groupId = KS.GetFromRequest().GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            if (filter == null)
                filter = new KalturaHouseholdCouponFilter();

            try
            {
                KalturaGenericListResponse<KalturaHouseholdCoupon> coreResponse = filter.Execute<KalturaHouseholdCoupon>();
                response.Objects = coreResponse.Objects;
                response.TotalCount = coreResponse.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            
            return response;
        }
    }
}