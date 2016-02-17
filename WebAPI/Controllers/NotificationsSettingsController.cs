using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/notificationsSettings/action")]
    public class NotificationsSettingsController : ApiController
    {
        /// <summary>
        /// Retrieve the notification settings.       
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply</returns>
        /// 
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaNotificationSettings Get(int userId)
        {
            KalturaNotificationSettings response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
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
        /// update the notification settings.      
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the user</returns>
        /// 
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaNotificationSettings settings, int userId)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.NotificationClient().Update(groupId, userId,settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

    }
}