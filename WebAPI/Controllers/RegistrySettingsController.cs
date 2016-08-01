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
    [RoutePrefix("_service/registrySettings/action")]
    [OldStandardAction("listOldStandard", "list")]
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
        public KalturaRegistrySettingsListResponse List()
        {
            List<KalturaRegistrySettings>  list = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                list = ClientsManager.ApiClient().GetAllRegistry(groupId);              
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return new KalturaRegistrySettingsListResponse() { RegistrySettings = list, TotalCount = list.Count };
        }
    
        /// <summary>
        /// Retrieve the registry settings.        
        /// </summary>        
        /// <remarks>        
        /// </remarks>
        /// <returns>The registry settings that apply for the partner</returns>
        /// 
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaRegistrySettings> ListOldStandard()
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