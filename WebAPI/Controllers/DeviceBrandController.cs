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
    [RoutePrefix("_service/deviceBrand/action")]
    public class DeviceBrandController : ApiController
    {

        /// <summary>
        /// Return a list of the available device brands.
        /// </summary>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaDeviceBrandListResponse List()
        {
            KalturaDeviceBrandListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client                
                response = ClientsManager.ApiClient().GetDeviceBrandList(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}