using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("mediaConcurrencyRule")]
    public class MediaConcurrencyRuleController : IKalturaController
    {
        /// <summary>
        /// Get the list of meta mappings for the partner
        /// </summary>        
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaMediaConcurrencyRuleListResponse List()
        {
            KalturaMediaConcurrencyRuleListResponse response = null;
            
            int groupId = KSManager.GetKSFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetMediaConcurrencyRules(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}