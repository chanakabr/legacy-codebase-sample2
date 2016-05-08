using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/ppv/action")]
    public class PpvController : ApiController
    {
        /// <summary>
        /// Returns ppv object by internal identifier
        /// </summary>
        /// <param name="id">ppv identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, PpvModuleNotFound = 9016</remarks>     
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaPpv Get(string id)
        {
            KalturaPpv response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.PricingClient().GetPPVModuleData(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}