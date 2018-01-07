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
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/drmAdapterProfile/action")]
    public class DrmAdapterProfileController : ApiController
    {
        /// <summary>
        /// Returns all DRM adapters for partner
        /// </summary>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaDRMAdapterProfileListResponse List()
        {
            KalturaDRMAdapterProfileListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetDrmAdapters(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}