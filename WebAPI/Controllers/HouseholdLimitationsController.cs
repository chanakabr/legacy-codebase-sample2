using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Models.Domains;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdLimitations/action")]
    public class HouseholdLimitationsController : ApiController
    {
        /// <summary>
        /// Get the limitation module by id
        /// </summary>
        /// <param name="id">Household limitations module identifier</param>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaHouseholdLimitations Get(int id)
        {
            KalturaHouseholdLimitations response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.DomainsClient().GetDomainLimitationModule(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}