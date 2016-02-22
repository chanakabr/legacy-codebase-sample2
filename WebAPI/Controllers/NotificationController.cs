using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/notification/action")]
    public class NotificationController : ApiController
    {
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="push_token">TBD</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("setPush"), HttpPost]
        [ApiAuthorize]
        public bool SetPush(string push_token)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                response = ClientsManager.NotificationClient().SetPush(groupId, userId, udid, push_token);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}