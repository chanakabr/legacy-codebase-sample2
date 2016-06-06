using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/system/action")]
    public class SystemController : ApiController
    {
        /// <summary>
        /// Returns country details by the provided IP, if not provided - by the client IP
        /// </summary>
        /// <param name="ip">IP</param>
        /// <remarks>
        /// Possible status codes:  Country was not found = 4025
        /// </remarks>
        [Route("getCountry"), HttpPost]
        [ApiAuthorize]
        public KalturaCountry GetCountry(string ip = null)
        {
            KalturaCountry response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(ip))
            {
                ip = Utils.Utils.GetClientIP();
            }

            try
            {
                response = ClientsManager.CatalogClient().GetCountryByIp(groupId, ip);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}