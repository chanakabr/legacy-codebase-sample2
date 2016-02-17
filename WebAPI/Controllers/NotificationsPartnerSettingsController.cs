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
    [RoutePrefix("_service/notificationsPartnerSettings/action")]
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
        public KalturaPartnerNotificationSettings Get()
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
        /// update the partner notification settings.      
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the partner</returns>
        /// 
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public bool Update(KalturaPartnerNotificationSettings settings)
        {
            bool response = false;

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