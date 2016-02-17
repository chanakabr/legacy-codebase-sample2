using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Notification;
using WebAPI.Notifications;
using WebAPI.Utils;

namespace WebAPI.Clients
{
    public class NotificationsClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public NotificationsClient()
        {

        }

        #region Properties

        protected WebAPI.Notifications.NotificationServiceClient Notification
        {
            get
            {
                return (Module as WebAPI.Notifications.NotificationServiceClient);
            }
        }

        #endregion

        internal KalturaPartnerNotificationSettings Get(int groupId)
        {

            Group group = GroupsManager.GetGroup(groupId);
            NotificationPartnerSettingsResponse response = null;
            KalturaPartnerNotificationSettings settings = null;

            try
            {
                log.Debug(string.Format("Username={0}, Password={1}", group.NotificationsCredentials.Username, group.NotificationsCredentials.Password));

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetNotificationPartnerSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password);
                }
                log.Debug("return from Notification.GetNotificationPartnerSettings");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. ws address: {0}, exception: {1}",
                    Notification.Endpoint != null && Notification.Endpoint.Address != null &&
                            Notification.Endpoint.Address.Uri != null ? Notification.Endpoint.Address.Uri.ToString() : 
                string.Empty, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            else
            {
                settings = AutoMapper.Mapper.Map<KalturaPartnerNotificationSettings>(response.settings);

                return settings;
            }
        }

        internal KalturaNotificationSettings Get(int groupId, int userId)
        {

            Group group = GroupsManager.GetGroup(groupId);
            NotificationSettingsResponse response = null;
            KalturaNotificationSettings settings = null;

            try
            {
                log.Debug(string.Format("Username={0}, Password={1}", group.NotificationsCredentials.Username, group.NotificationsCredentials.Password));

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetNotificationSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId);
                }
                log.Debug("return from Notification.UpdateNotificationPartnerSettings");

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. ws address: {0}, exception: {1}",
                    Notification.Endpoint != null && Notification.Endpoint.Address != null &&
                            Notification.Endpoint.Address.Uri != null ? Notification.Endpoint.Address.Uri.ToString() :
                string.Empty, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                log.Debug("response is null Notification.UpdateNotificationPartnerSettings");
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            else
            {
                settings = AutoMapper.Mapper.Map<KalturaNotificationSettings>(response.settings);

                return settings;
            }
        }

        internal bool Update(int groupId, KalturaPartnerNotificationSettings settings)
        {
            bool success = false;
            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    NotificationPartnerSettings settingsObj = null;
                    settingsObj = AutoMapper.Mapper.Map<NotificationPartnerSettings>(settings);
                    Group group = GroupsManager.GetGroup(groupId);

                               response = Notification.UpdateNotificationPartnerSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, settingsObj);
                  
                }                
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. ws address: {0}, exception: {1}",
                    Notification.Endpoint != null && Notification.Endpoint.Address != null &&
                            Notification.Endpoint.Address.Uri != null ? Notification.Endpoint.Address.Uri.ToString() :
                string.Empty, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;

        }

        internal bool Update(int groupId, int userId, KalturaNotificationSettings settings)
        {
            bool success = false;
            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    NotificationSettings settingsObj = null;
                    settingsObj = AutoMapper.Mapper.Map<NotificationSettings>(settings);
                    Group group = GroupsManager.GetGroup(groupId);

                    response = Notification.UpdateNotificationSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId.ToString(), settingsObj);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. ws address: {0}, exception: {1}",
                    Notification.Endpoint != null && Notification.Endpoint.Address != null &&
                            Notification.Endpoint.Address.Uri != null ? Notification.Endpoint.Address.Uri.ToString() :
                string.Empty, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }
            else
            {
                success = true;
            }

            return success;
        }
    }
}