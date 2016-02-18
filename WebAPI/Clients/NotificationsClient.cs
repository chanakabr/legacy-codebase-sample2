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
                MessageAnnouncementId = announcement.MessageAnnouncementId,
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
                MessageAnnouncementId = announcement.MessageAnnouncementId,
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