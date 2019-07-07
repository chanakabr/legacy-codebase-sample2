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
        static public KalturaHouseholdCoupon Add(KalturaHouseholdCoupon householdCoupon)
        {
            KalturaHouseholdCoupon response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                householdCoupon.ValidateForAdd();
                response = ClientUtils.Add<KalturaHouseholdCoupon, DomainCoupon>(groupId, householdCoupon);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}