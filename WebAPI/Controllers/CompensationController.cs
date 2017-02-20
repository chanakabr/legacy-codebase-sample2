using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/compensation/action")]
    public class CompensationController : ApiController
    {
        /// <summary>
        /// Adds a new compensation for a household for a given number of iterations of a subscription renewal for a fixed amount / percentage of the renewal price.
        /// </summary>
        /// <param name="id">Purchase ID</param>
        /// <param name="compensation">Compensation parameters</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.SubscriptionNotRenewable)]
        [Throws(eResponseStatus.NotEntitled)]
        [Throws(eResponseStatus.CompensationAlreadyExists)]
        public KalturaCompensation Add(KalturaCompensation compensation)
        {
            KalturaCompensation response = null;

            compensation.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.ConditionalAccessClient().AddCompensation(groupId, userId, compensation);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}