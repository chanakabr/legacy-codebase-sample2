using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/notificationsPartnerSettings/action")]
    [OldStandardAction("getOldStandard", "get")]
    [OldStandardAction("updateOldStandard", "update")]
    public class NotificationsPartnerSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the partner notification settings.       
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the partner</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaNotificationsPartnerSettings Get()
        {
            KalturaNotificationsPartnerSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = (KalturaNotificationsPartnerSettings)ClientsManager.NotificationClient().Get(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return (KalturaNotificationsPartnerSettings)response;
        }

        /// <summary>
        /// Retrieve the partner notification settings.       
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the partner</returns>
        /// 
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaPartnerNotificationSettings GetOldStandard()
        {
            KalturaPartnerNotificationSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.NotificationClient().Get(groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update the account notification settings
        /// </summary>
        /// <param name="settings">Account notification settings model</param>
        /// <remarks>        
        /// Possible status codes: 
        /// Push notification false = 8001 
        /// </remarks>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemaValidationType.ACTION_RETURN_TYPE)]
        public bool Update(KalturaNotificationsPartnerSettings settings)
        {
            bool response = false;

            if (settings.PushStartHour.HasValue && (settings.PushStartHour.Value < 0 || settings.PushStartHour.Value > 24))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal push start hour");


            if (settings.PushEndHour.HasValue && (settings.PushEndHour.Value < 0 || settings.PushEndHour.Value > 24))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal push end hour");

            if (settings.MessageTTLDays.HasValue && (settings.MessageTTLDays.Value < 1 || settings.MessageTTLDays.Value > 90))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal message TTL");

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.NotificationClient().Update(groupId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update the account notification settings
        /// </summary>
        /// <param name="settings">Account notification settings model</param>
        /// <remarks>        
        /// Possible status codes: 
        /// Push notification false = 8001 
        /// </remarks>
        /// <returns></returns>
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool UpdateOldStandard(KalturaPartnerNotificationSettings settings)
        {
            bool response = false;

            if (settings.PushStartHour.HasValue && (settings.PushStartHour.Value < 0 || settings.PushStartHour.Value > 24))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal push start hour");


            if (settings.PushEndHour.HasValue && (settings.PushEndHour.Value < 0 || settings.PushEndHour.Value > 24))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal push end hour");

            if (settings.MessageTTLDays.HasValue && (settings.MessageTTLDays.Value < 1 || settings.MessageTTLDays.Value > 90))
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal message TTL");

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.NotificationClient().Update(groupId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}