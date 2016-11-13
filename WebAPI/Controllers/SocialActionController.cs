using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Social;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/socialAction/action")]
    public class SocialActionController : ApiController
    {
        /// <summary>
        /// Insert new user social action
        /// </summary>
        /// <param name="socialAction">social Action Object</param>
        /// <remarks>
        /// Possible status codes:
        /// </remarks>
        /// <param name="channel">KSQL channel Object</param>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public KalturaUserSocialActionResponse Add(KalturaSocialAction socialAction)
        {
            KalturaUserSocialActionResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                // call client
                response = ClientsManager.SocialClient().AddSocialAction(groupId, userId, udid, socialAction);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

    }
}