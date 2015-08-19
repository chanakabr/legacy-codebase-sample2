using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/facebook/action")]
    public class FacebookController : ApiController
    {
        /// <summary>
        /// Returns the facebook application configuration for the partner
        /// </summary>        
        /// <returns></returns>
        [Route("GetConfig"), HttpPost]
        [ApiAuthorize]
        public KalturaFacebookConfig GetConfig()
        {
            KalturaFacebookConfig response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.SocialClient().GetFacebookConfig(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}