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
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;


namespace WebAPI.Controllers
{
    [RoutePrefix("_service/householdQuota/action")]
    public class HouseholdQuotaController : ApiController
    {
        /// <summary>
        /// Returns the household's quota data
        /// </summary>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003</remarks>     
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaHouseholdQuota Get()
        {
            KalturaHouseholdQuota response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long domainId = HouseholdUtils.GetHouseholdIDByKS(groupId);
                string userId = KS.GetFromRequest().UserId;

                // call client                
                response = ClientsManager.ConditionalAccessClient().GetDomainQuota(groupId, userId, domainId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}