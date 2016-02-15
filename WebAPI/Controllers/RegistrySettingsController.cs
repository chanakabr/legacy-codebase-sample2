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
    [RoutePrefix("_service/RegistrySettings/action")]
    public class RegistrySettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the registry settings.        
        /// </summary>        
        /// <remarks>        
        /// </remarks>
        /// <returns>The registry settings that apply for the partner</returns>
        /// 
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaRegistrySettings> List()
        {
            List<KalturaRegistrySettings>  response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.ApiClient().GetAllRegistry(groupId);              
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}