using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
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
        /// <param name="partner_id"></param>
        /// <returns></returns>
        [Route("GetConfig"), HttpPost]
        public KalturaFacebookConfig GetConfig(string partner_id)
        {
            KalturaFacebookConfig response = null;
            int groupId = int.Parse(partner_id);

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