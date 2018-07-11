using System;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/mediaConcurrencyRule/action")]
    public class MediaConcurrencyRuleController : ApiController
    {
        /// <summary>
        /// Get the list of meta mappings for the partner
        /// </summary>        
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaMediaConcurrencyRuleListResponse List()
        {
            KalturaMediaConcurrencyRuleListResponse response = null;
            
            int groupId = KS.GetFromRequest().GroupId;

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