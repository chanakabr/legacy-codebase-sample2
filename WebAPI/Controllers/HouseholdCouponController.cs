using ApiObjects.Pricing;
using ApiObjects.Response;
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
    // TODO ANAT(BEO-6931) - ADD ALL relevant methods for HouseholdCouponController
    [Service("householdCoupon")]
    public class HouseholdCouponController : IKalturaController
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
            KalturaHouseholdCoupon response = null;
            int groupId = KS.GetFromRequest().GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                householdCoupon.ValidateForAdd();
               
                response = ClientUtils.Add<KalturaHouseholdCoupon, CouponWalt>(groupId, householdCoupon,
                                                                                new Dictionary<string, object>()
                                                                {{ "householdId", householdId }
                                                                                });
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Romve coupon from household
        /// </summary>
        /// <param name="code">Coupon code</param>
        [Action("delete")]
        [ApiAuthorize]        
        static public void Delete(string code)
        {
            try
            {
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

        }

        /// <summary>
        /// Lists all topic notifications in the system.
        /// </summary>
        /// <param name="filter">Filter options</param>
        /// <param name="pager">Paging the request</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaHouseholdCouponListResponse List()
        {
            KalturaHouseholdCouponListResponse response = null;

            

            try
            {
               // response = ClientUtils.List<KalturaHouseholdCoupon, DomainCoupon>(groupId, householdCoupon);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}