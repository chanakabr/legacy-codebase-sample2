using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
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
                // validate push token
                if (string.IsNullOrWhiteSpace(push_token))
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "push token cannot be empty");

                response = ClientsManager.NotificationClient().SetPush(groupId, userId, udid, push_token);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>        
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("initiateCleanup"), HttpPost]
        [ApiAuthorize]
        public bool InitiateCleanup()
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                response = ClientsManager.NotificationClient().DeleteAnnouncementsOlderThan(groupId);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD
        /// </summary>        
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("getLastCleanupDate"), HttpPost]
        [ApiAuthorize]
        public long GetLastCleanupDate()
        {
            long response = 0;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.NotificationClient().GetNotificationLastCleanupDate(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// TBD 
        /// </summary>
        /// <remarks>
        /// Possible status codes:       
        /// </remarks>
        /// <param name="id"></param>        
        [Route("registry"), HttpPost]
        [ApiAuthorize]
        public KalturaRegistryResponse Registry(int id)
        {
            KalturaRegistryResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                KS.GetFromRequest().ToString();

                // validate input
                if (id <= 0)
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id is illegal");

                // call client                
                response = ClientsManager.NotificationClient().Registry(groupId, id, KS.GetFromRequest().ToString(), Utils.Utils.GetClientIP());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}