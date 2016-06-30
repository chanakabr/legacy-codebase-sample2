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
        /// <param name="identifier">In case type is "announcement", identifier should be the announcement ID. In case type is "system", identifier should be "login" (the login topic)</param>        
        /// <param name="type">"announcement" - TV-Series topic, "system" - login topic</param>     
        [Route("register"), HttpPost]
        [ApiAuthorize]
        public KalturaRegistryResponse Register(string identifier, KalturaNotificationType type)
        {
            KalturaRegistryResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                KS.GetFromRequest().ToString();

                if (string.IsNullOrEmpty(identifier))
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id is empty");

                // validate input
                switch (type)
                {
                    case KalturaNotificationType.ANNOUNCEMENT:
                        long announcentId = 0;
                        if (!long.TryParse(identifier, out announcentId))
                            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "illegal id");
                        break;

                    case KalturaNotificationType.SYSTEM:
                        if (identifier.ToLower() != "login")
                            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "illegal id");
                        break;
                    default:
                        break;
                }

                // call client                
                response = ClientsManager.NotificationClient().Register(groupId, type, identifier, KS.GetFromRequest().ToString(), Utils.Utils.GetClientIP());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}