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
    [Service("householdCoupon")]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, CouponWallet, string, CouponWalletFilter>
    {
        // TODO SHIR - talk with Arthur about Throws, description etc in all crud methods
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
        public static KalturaHouseholdCoupon Add(KalturaHouseholdCoupon householdCoupon)
        {
            var response = HouseholdCouponController.DoAdd(householdCoupon);
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
            HouseholdCouponController.DoDelete(code, CouponWalletHandler.Instance);
        }

        /// <summary>
        /// Gets all HouseholdCoupon items for a household
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdCouponListResponse List(KalturaHouseholdCouponFilter filter = null)
        {
            if(filter == null)
            {
                filter = new KalturaHouseholdCouponFilter();
            }

            var response = filter.Execute<KalturaHouseholdCouponListResponse, KalturaHouseholdCoupon>();
            return response;
        }
    }
}