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

        internal KalturaNotificationSettings Get(int groupId, string userId)
        {

            Group group = GroupsManager.GetGroup(groupId);
            NotificationSettingsResponse response = null;
            KalturaNotificationSettings settings = null;

            try
            {
                log.Debug(string.Format("Username={0}, Password={1}", group.NotificationsCredentials.Username, group.NotificationsCredentials.Password));
                int user_id = int.Parse(userId);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {                   
                    response = Notification.GetNotificationSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, user_id);
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

                NotificationPartnerSettings settingsObj = null;
                settingsObj = AutoMapper.Mapper.Map<NotificationPartnerSettings>(settings);
                Group group = GroupsManager.GetGroup(groupId);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
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

        internal bool Update(int groupId, string userId, KalturaNotificationSettings settings)
        {
            bool success = false;
            Status response = null;
            try
            {
                NotificationSettings settingsObj = null;
                settingsObj = AutoMapper.Mapper.Map<NotificationSettings>(settings);
                Group group = GroupsManager.GetGroup(groupId);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {


                    response = Notification.UpdateNotificationSettings(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, settingsObj);
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

        internal int AddAnnouncement(int groupId, Models.Notifications.KalturaAnnouncement announcement)
        {
            AddMessageAnnouncementResponse response = null;
            Group group = GroupsManager.GetGroup(groupId);

            eAnnouncementRecipientsType recipients = eAnnouncementRecipientsType.Other;
            switch (announcement.Recipients)
            {
                case Models.Notifications.KalturaAnnouncementRecipientsType.All:
                    recipients = eAnnouncementRecipientsType.All;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.Guests:
                    recipients = eAnnouncementRecipientsType.Guests;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.LoggedIn:
                    recipients = eAnnouncementRecipientsType.LoggedIn;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.Other:
                    recipients = eAnnouncementRecipientsType.Other;
                    break;
            }

            eAnnouncementStatus status = eAnnouncementStatus.NotSent;
            switch (announcement.Status)
            {
                case Models.Notifications.KalturaAnnouncementStatus.Aborted:
                    status = eAnnouncementStatus.Aborted;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.NotSent:
                    status = eAnnouncementStatus.NotSent;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.Sending:
                    status = eAnnouncementStatus.Sending;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.Sent:
                    status = eAnnouncementStatus.Sent;
                    break;
            }

            MessageAnnouncement messageAnnouncement = new MessageAnnouncement()
            {
                Enabled = announcement.Enabled,
                Message = announcement.Message,
                MessageAnnouncementId = announcement.Id,
                Name = announcement.Name,
                Recipients = recipients,
                StartTime = announcement.StartTime,
                Status = status,
                Timezone = announcement.Timezone
            };

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.AddMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, groupId, messageAnnouncement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddAnnouncement.  groupID: {0}, announcement: {1}, exception: {1}", groupId, announcement, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            return response.Id;
        }

        internal bool UpdateAnnouncement(int groupId, Models.Notifications.KalturaAnnouncement announcement)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            eAnnouncementRecipientsType recipients = eAnnouncementRecipientsType.Other;
            switch (announcement.Recipients)
            {
                case Models.Notifications.KalturaAnnouncementRecipientsType.All:
                    recipients = eAnnouncementRecipientsType.All;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.Guests:
                    recipients = eAnnouncementRecipientsType.Guests;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.LoggedIn:
                    recipients = eAnnouncementRecipientsType.LoggedIn;
                    break;
                case Models.Notifications.KalturaAnnouncementRecipientsType.Other:
                    recipients = eAnnouncementRecipientsType.Other;
                    break;
            }

            eAnnouncementStatus status = eAnnouncementStatus.NotSent;
            switch (announcement.Status)
            {
                case Models.Notifications.KalturaAnnouncementStatus.Aborted:
                    status = eAnnouncementStatus.Aborted;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.NotSent:
                    status = eAnnouncementStatus.NotSent;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.Sending:
                    status = eAnnouncementStatus.Sending;
                    break;
                case Models.Notifications.KalturaAnnouncementStatus.Sent:
                    status = eAnnouncementStatus.Sent;
                    break;
            }

            MessageAnnouncement messageAnnouncement = new MessageAnnouncement()
            {
                Enabled = announcement.Enabled,
                Message = announcement.Message,
                MessageAnnouncementId = announcement.Id,
                Name = announcement.Name,
                Recipients = recipients,
                StartTime = announcement.StartTime,
                Status = status,
                Timezone = announcement.Timezone
            };

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.UpdateMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, groupId, messageAnnouncement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAnnouncement.  groupID: {0}, announcement: {1}, exception: {1}", groupId, announcement, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal bool UpdateAnnouncementStatus(int groupId, int id, bool status)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.UpdateMessageAnnouncementStatus(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, groupId, id, status);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAnnouncementStatus.  groupID: {0}, id: {1}, exception: {1}", groupId, id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal bool DeleteAnnouncement(int groupId, int id)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.DeleteMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncement.  groupID: {0}, id: {1}, exception: {1}", groupId, id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }
    }
}