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
    [RoutePrefix("_service/notificationsSettings/action")]
    [OldStandardAction("getOldStandard", "get")]
    [OldStandardAction("updateOldStandard", "update")]
    public class NotificationsSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the user’s notification settings.    
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaNotificationsSettings Get()
        {
            KalturaNotificationsSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = (KalturaNotificationsSettings) ClientsManager.NotificationClient().Get(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Retrieve the user’s notification settings.    
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply</returns>
        /// 
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaNotificationSettings GetOldStandard()
        {
            KalturaNotificationSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.NotificationClient().Get(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update the user’s notification settings.      
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the user</returns>
        /// 
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemaValidationType.ACTION_RETURN_TYPE)]
        public bool Update(KalturaNotificationsSettings settings)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.NotificationClient().Update(groupId, userId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update the user’s notification settings.      
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the user</returns>
        /// 
        [Route("updateOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool UpdateOldStandard(KalturaNotificationSettings settings)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string userId = KS.GetFromRequest().UserId;
                // call client                
                response = ClientsManager.NotificationClient().Update(groupId, userId, settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}