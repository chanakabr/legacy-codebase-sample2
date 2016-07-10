using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdPremiumService/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class HouseholdPremiumServiceController : ApiController
    {
        /// <summary>
        /// Returns all the premium services allowed for the household
        /// </summary>
        /// <returns></returns>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaPremiumService> ListOldStandard()
        {
            List<KalturaPremiumService> response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetDomainServicesOldStandart(groupId, (int)householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all the premium services allowed for the household
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaHouseholdPremiumServiceListResponse List()
        {
            KalturaHouseholdPremiumServiceListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long householdId = HouseholdUtils.GetHouseholdIDByKS(groupId);

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetDomainServices(groupId, (int)householdId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}