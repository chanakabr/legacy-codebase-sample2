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
    // TODO ANAT(BEO-6931) - ADD ALL relevant methods for HouseholdCouponController
    [Service("householdCoupon")]
    public class HouseholdCouponController : KalturaCrudController<KalturaHouseholdCoupon, DomainCoupon>
    {
        /// <summary>
        /// householdCoupon add
        /// </summary>
        /// <param name="householdCoupon">householdCoupon details</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaHouseholdCoupon Add(KalturaHouseholdCoupon householdCoupon)
        {
            int groupId = KS.GetFromRequest().GroupId;
            // TODO SHIR - TALK WITH TANTAN ABOUT THIS
            var response = KalturaCrudController<KalturaHouseholdCoupon, DomainCoupon>.Add(groupId, householdCoupon);
            return response;
        }

        /// <summary>
        /// householdCoupon delete
        /// </summary>
        /// <param name="householdCoupon">householdCoupon details</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        static public void Delete(long id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            // TODO SHIR - TALK WITH TANTAN ABOUT THIS
            KalturaCrudController<KalturaHouseholdCoupon, DomainCoupon>.Delete(groupId, id, DomainCouponHandler.Instance);
        }
    }
}