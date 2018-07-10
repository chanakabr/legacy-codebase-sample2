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
    [Service("notificationsPartnerSettings")]
    public class NotificationsPartnerSettingsController : IKalturaController
    {
        /// <summary>
        /// Retrieve the partner notification settings.       
        /// </summary>    
        /// 
        /// <remarks>        
        /// </remarks>
        /// <returns>The notification settings that apply for the partner</returns>
        /// 
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaNotificationsPartnerSettings Get()
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
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaPartnerNotificationSettings GetOldStandard()
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
        [Action("update")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.PushNotificationFalse)]
        [Throws(eResponseStatus.MailNotificationAdapterNotExist)]
        static public bool Update(KalturaNotificationsPartnerSettings settings)
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

        /// <summary>
        /// Update the account notification settings
        /// </summary>
        /// <param name="settings">Account notification settings model</param>
        /// <remarks>        
        /// Possible status codes: 
        /// Push notification false = 8001 
        /// </remarks>
        /// <returns></returns>
        [Action("updateOldStandard")]
        [OldStandardAction("update")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.PushNotificationFalse)]
        static public bool UpdateOldStandard(KalturaPartnerNotificationSettings settings)
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