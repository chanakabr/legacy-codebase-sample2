using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/notification/action")]
    public class NotificationController : ApiController
    {
        /// <summary>
        /// Registers the device push token to the push service
        /// </summary>
        /// <param name="pushToken">The device-application pair authentication for push delivery</param>
        /// <remarks>
        /// 
        /// </remarks>
        /// <returns></returns>
        [Route("setDevicePushToken"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public bool SetDevicePushToken(string pushToken)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                // validate push token
                if (string.IsNullOrWhiteSpace(pushToken))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "pushToken");

                response = ClientsManager.NotificationClient().SetPush(groupId, userId, udid, pushToken);
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
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaRegistryResponse Register(string identifier, KalturaNotificationType type)
        {
            KalturaRegistryResponse response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                KS.GetFromRequest().ToString();

                if (string.IsNullOrEmpty(identifier))
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "identifier");

                // validate input
                switch (type)
                {
                    case KalturaNotificationType.announcement:
                        long announcentId = 0;
                        if (!long.TryParse(identifier, out announcentId))
                            throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "identifier");
                        break;

                    case KalturaNotificationType.system:
                        if (identifier.ToLower() != "login")
                            throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "identifier");
                        break;
                    case KalturaNotificationType.Reminder:
                        long reminderId = 0;
                        if (!long.TryParse(identifier, out reminderId))
                            throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "identifier");
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