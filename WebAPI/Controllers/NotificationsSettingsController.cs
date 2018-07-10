using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("notificationsSettings")]
    public class NotificationsSettingsController : IKalturaController
    {
        /// <summary>
        /// Retrieve the user’s notification settings.    
        /// </summary>    
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply</returns>
        /// 
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaNotificationsSettings Get()
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
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply</returns>
        /// 
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaNotificationSettings GetOldStandard()
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
        /// <param name="settings">Notifications settings</param>
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the user</returns>
        /// 
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        static public bool Update(KalturaNotificationsSettings settings)
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
        [Action("updateOldStandard")]
        [OldStandardAction("update")]
        [ApiAuthorize]
        [Obsolete]
        static public bool UpdateOldStandard(KalturaNotificationSettings settings)
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
        /// <param name="settings">Notifications settings</param>
        /// <param name="token">User's token identifier</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the user</returns>
        [Action("updateWithToken")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidToken)]
        static public bool UpdateWithToken(KalturaNotificationsSettings settings, string token, int partnerId)
        {
            bool response = false;

            try
            {
                int userId = ClientsManager.NotificationClient().GetUserIdByToken(partnerId, token);
                response = ClientsManager.NotificationClient().Update(partnerId, userId.ToString(), settings);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}