using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
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

        internal bool AddAnnouncement(int groupId, Models.Notifications.KalturaAnnouncement announcement)
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
                    response = Notification.AddMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, messageAnnouncement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while AddAnnouncement.  groupID: {0}, announcement: {1}, exception: {2}", groupId, announcement, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            return true;
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
                    response = Notification.UpdateMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, messageAnnouncement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAnnouncement.  groupID: {0}, announcement: {1}, exception: {2}", groupId, announcement, ex);
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
                    response = Notification.UpdateMessageAnnouncementStatus(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, id, status);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAnnouncementStatus.  groupID: {0}, id: {1}, exception: {2}", groupId, id, ex);
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
                    response = Notification.DeleteMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncement.  groupID: {0}, id: {1}, exception: {2}", groupId, id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }


        internal bool CreateSystemAnnouncement(int groupId)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {

                    response = Notification.CreateSystemAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password);
                }
            }
            catch (Exception ex)
            {

                log.ErrorFormat("Error while CreateSystemAnnouncement.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }
            return true;
        }

        internal KalturaMessageAnnouncementListResponse GetAllAnnouncements(int groupId, int pageSize, int pageIndex)
        {
            List<KalturaAnnouncement> result = null;
            GetAllMessageAnnouncementsResponse response = null;
            KalturaMessageAnnouncementListResponse ret;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetAllMessageAnnouncements(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncement.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<List<KalturaAnnouncement>>(response.messageAnnouncements);

            ret = new KalturaMessageAnnouncementListResponse() { Announcements = result, TotalCount = response.totalCount };
            return ret;
        }

        internal bool SetPush(int groupId, string userId, string udid, string pushToken)
        {
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                eUserMessageAction action = string.IsNullOrEmpty(userId) || userId == "0" ? eUserMessageAction.AnonymousPushRegistration : eUserMessageAction.IdentifyPushRegistration;
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    Notification.InitiateNotificationActionAsync(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, action, int.Parse(userId), udid, pushToken);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetPush. groupID: {0}, userId: {1}, udid: {2}", groupId, userId, udid, ex);
                ErrorUtils.HandleWSException(ex);
                return false;
            }
            return true;
        }

        internal KalturaFollowTemplate InsertFollowTemplate(int groupId, KalturaFollowTemplate followTemplate)
        {
            FollowTemplateResponse response = null;
            KalturaFollowTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                FollowTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<FollowTemplate>(followTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.SetFollowTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, apiFollowTemplate);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertFollowTemplate.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            result = AutoMapper.Mapper.Map<KalturaFollowTemplate>(response.FollowTemplate);
            return result;
        }

        internal KalturaListFollowDataResponse GetUserTvSeriesFollows(int groupId, string userID, int pageSize, int pageIndex)
        {
            List<KalturaFollowDataBase> result = null;
            GetUserFollowsResponse response = null;
            KalturaListFollowDataResponse ret;

            Group group = GroupsManager.GetGroup(groupId);
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetUserFollows(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserTvSeriesFollows.  groupID: {0}, userId: {1}, exception: {2}", groupId, userID, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<List<KalturaFollowDataBase>>(response.Follows);

            ret = new KalturaListFollowDataResponse() { FollowDataList = result, TotalCount = response.TotalCount };
            return ret;
        }

        internal bool DeleteUserTvSeriesFollow(int groupId, string userID, int asset_id)
        {
            Status response = null;

            Group group = GroupsManager.GetGroup(groupId);
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    FollowDataTvSeries followData = new FollowDataTvSeries();
                    followData.AssetId = asset_id;
                    response = Notification.Unfollow(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, followData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserTvSeriesFollows.  groupID: {0}, userId: {1}, exception: {2}", groupId, userID, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal bool AddUserTvSeriesFollow(int groupId, string userID, int asset_id)
        {
            Status response = null;
            FollowDataTvSeries followDataNotification = null;
            KalturaFollowDataTvSeries followData = new KalturaFollowDataTvSeries();
            followData.AssetId = asset_id;

            Group group = GroupsManager.GetGroup(groupId);
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get asset name
            var mediaInfoResponse = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), KSUtils.ExtractKSPayload().UDID, null,
                                0, 0, new List<int>() { followData.AssetId }, new List<KalturaCatalogWith>());

            if (mediaInfoResponse == null || mediaInfoResponse.Objects == null || mediaInfoResponse.Objects.Count == 0)
                throw new NotFoundException();

            
            followData.Status = 1;
            followData.Title = string.Format("{0}_{1}", mediaInfoResponse.Objects[0].Name,groupId);

            followDataNotification = Mapper.Map<FollowDataTvSeries>(followData);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.Follow(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, followDataNotification);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserTvSeriesFollows.  groupID: {0}, userId: {1}, exception: {2}", groupId, userID, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaFollowTemplate GetFollowTemplate(int groupId)
        {
            FollowTemplateResponse response = null;
            KalturaFollowTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetFollowTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertFollowTemplate.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaFollowTemplate>(response.FollowTemplate);
            return result;
        }


        internal KalturaFollowTemplate SetFollowTemplate(int groupId, KalturaFollowTemplate followTemplate)
        {
            FollowTemplateResponse response = null;
            KalturaFollowTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                FollowTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<FollowTemplate>(followTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.SetFollowTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, apiFollowTemplate);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetFollowTemplate.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code == (int)StatusCode.OK)
                result = AutoMapper.Mapper.Map<KalturaFollowTemplate>(response.FollowTemplate);
            
            return result;
        }
    }
}