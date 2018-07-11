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
    [Service("registrySettings")]
    public class RegistrySettingsController : IKalturaController
    {
        /// <summary>
        /// Retrieve the registry settings.        
        /// </summary>        
        /// <remarks>        
        /// </remarks>
        /// <returns>The registry settings that apply for the partner</returns>
        /// 
        [Action("list")]
        [ApiAuthorize]
        static public KalturaRegistrySettingsListResponse List()
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
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public List<KalturaRegistrySettings> ListOldStandard()
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