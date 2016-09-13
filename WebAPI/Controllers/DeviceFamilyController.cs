using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/deviceFamily/action")]
    public class DeviceFamilyController : ApiController
    {

        /// <summary>
        /// Return a list of the available device families  series recordings for the household with optional filter by status and KSQL.
        /// </summary>
        /// <param name="filter">Filter parameters for filtering out the result - support order by only - START_DATE_ASC, START_DATE_DESC, ID_ASC,ID_DESC,SERIES_ID_ASC, SERIES_ID_DESC</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, UserNotInDomain = 1005, UserDoesNotExist = 2000, UserSuspended = 2001, UserWithNoDomain = 2024</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaDeviceFamilyListResponse List()
        {
            KalturaDeviceFamilyListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client                
                response = ClientsManager.ApiClient().GetDeviceFamilyList(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}