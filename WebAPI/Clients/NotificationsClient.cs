using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.Notifications;
using WebAPI.ObjectsConvertor.Mapping;
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

        internal bool Update(int groupId, KalturaNotificationsPartnerSettings settings)
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

        internal bool Update(int groupId, string userId, KalturaNotificationsSettings settings)
        {
            bool success = false;
            Status response = null;
            try
            {
                UserNotificationSettings settingsObj = null;
                settingsObj = AutoMapper.Mapper.Map<UserNotificationSettings>(settings);
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

        internal KalturaAnnouncement AddAnnouncement(int groupId, Models.Notifications.KalturaAnnouncement announcement)
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
                Enabled = announcement.getEnabled(),
                Message = announcement.Message,
                MessageAnnouncementId = announcement.getId(),
                Name = announcement.Name,
                Recipients = recipients,
                StartTime = announcement.getStartTime(),
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

            KalturaAnnouncement result = Mapper.Map<KalturaAnnouncement>(response.Announcement);
            return result;
        }

        internal KalturaAnnouncement UpdateAnnouncement(int groupId, int announcementId, Models.Notifications.KalturaAnnouncement announcement)
        {
            MessageAnnouncementResponse response = null;
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
                Enabled = announcement.getEnabled(),
                Message = announcement.Message,
                MessageAnnouncementId = announcement.getId(),
                Name = announcement.Name,
                Recipients = recipients,
                StartTime = announcement.getStartTime(),
                Status = status,
                Timezone = announcement.Timezone
            };

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.UpdateMessageAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, announcementId, messageAnnouncement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateAnnouncement.  groupID: {0}, announcement: {1}, exception: {2}", groupId, announcement, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaAnnouncement result = Mapper.Map<KalturaAnnouncement>(response.Announcement);
            return result;
        }

        internal bool UpdateAnnouncementStatus(int groupId, long id, bool status)
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

        internal bool DeleteAnnouncement(int groupId, long id)
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

        internal KalturaAnnouncementListResponse GetAnnouncements(int groupId, int pageSize, int pageIndex)
        {
            List<KalturaAnnouncement> result = null;
            GetAllMessageAnnouncementsResponse response = null;
            KalturaAnnouncementListResponse ret;

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

            ret = new KalturaAnnouncementListResponse() { Announcements = result, TotalCount = response.totalCount };
            return ret;
        }

        [Obsolete]
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

        internal KalturaMessageTemplate InsertFollowTemplate(int groupId, KalturaMessageTemplate followTemplate)
        {
            MessageTemplateResponse response = null;
            KalturaMessageTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                MessageTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<MessageTemplate>(followTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.SetMessageTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, apiFollowTemplate);
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
            result = AutoMapper.Mapper.Map<KalturaMessageTemplate>(response.MessageTemplate);
            return result;
        }

        internal KalturaFollowTvSeriesListResponse ListUserTvSeriesFollows(int groupId, string userID, int pageSize, int pageIndex, KalturaFollowTvSeriesOrderBy orderBy)
        {
            List<KalturaFollowTvSeries> result = null;
            GetUserFollowsResponse response = null;

            // create order object
            OrderDir order = OrderDir.DESC;
            if (orderBy == KalturaFollowTvSeriesOrderBy.START_DATE_ASC)
                order = OrderDir.ASC;

            Group group = GroupsManager.GetGroup(groupId);
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetUserFollows(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, order);
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

            result = Mapper.Map<List<KalturaFollowTvSeries>>(response.Follows);

            KalturaFollowTvSeriesListResponse ret = new KalturaFollowTvSeriesListResponse() { FollowDataList = result, TotalCount = response.TotalCount };
            return ret;
        }

        [Obsolete]
        internal KalturaListFollowDataTvSeriesResponse GetUserTvSeriesFollows(int groupId, string userID, int pageSize, int pageIndex, KalturaOrder? orderBy)
        {
            List<KalturaFollowDataTvSeries> result = null;
            GetUserFollowsResponse response = null;
            KalturaListFollowDataTvSeriesResponse ret;

            // create order object
            OrderDir order = OrderDir.DESC;
            if (orderBy != null && orderBy.Value == KalturaOrder.oldest_first)
                order = OrderDir.ASC;

            Group group = GroupsManager.GetGroup(groupId);
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetUserFollows(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, order);
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

            result = Mapper.Map<List<KalturaFollowDataTvSeries>>(response.Follows);

            ret = new KalturaListFollowDataTvSeriesResponse() { FollowDataList = result, TotalCount = response.TotalCount };
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

            // get asset name
            var mediaInfoResponse = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), KSUtils.ExtractKSPayload().UDID, null,
                                            0, 0, new List<int>() { asset_id }, new List<KalturaCatalogWith>());

            if (mediaInfoResponse == null ||
                mediaInfoResponse.Objects == null ||
                mediaInfoResponse.Objects.Count == 0)
            {
                throw new ClientException((int)StatusCode.NotFound, "asset not found");
            }

            FollowDataTvSeries followData = new FollowDataTvSeries();
            followData.AssetId = asset_id;
            followData.Title = mediaInfoResponse.Objects[0].Name;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
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

        internal KalturaFollowTvSeries AddUserTvSeriesFollow(int groupId, string userID, int asset_id)
        {
            FollowResponse response = null;
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
            {
                throw new ClientException((int)StatusCode.NotFound, "asset not found");
            }

            followData.Status = 1;
            followData.Title = mediaInfoResponse.Objects[0].Name;
            followDataNotification = Mapper.Map<FollowDataTvSeries>(followData);
            followDataNotification.Type = mediaInfoResponse.Objects[0].getType();

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
            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            KalturaFollowTvSeries result = AutoMapper.Mapper.Map<KalturaFollowTvSeries>(response.Follow);
            return result;
        }

        internal KalturaMessageTemplate GetMessageTemplate(int groupId, KalturaOTTAssetType asset_Type)
        {
            MessageTemplateResponse response = null;
            KalturaMessageTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetMessageTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, NotificationMapping.ConvertOTTAssetType(asset_Type));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetMessageTemplate.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMessageTemplate>(response.MessageTemplate);
            return result;
        }

        internal KalturaMessageTemplate SetMessageTemplate(int groupId, KalturaMessageTemplate messageTemplate)
        {
            MessageTemplateResponse response = null;
            KalturaMessageTemplate result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                MessageTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<MessageTemplate>(messageTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.SetMessageTemplate(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, apiFollowTemplate);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetMessageTemplate.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            result = AutoMapper.Mapper.Map<KalturaMessageTemplate>(response.MessageTemplate);

            return result;
        }

        internal KalturaPersonalFeedListResponse GetUserFeedList(int groupId, string userID, int pageSize, int pageIndex, KalturaPersonalFeedOrderBy orderBy)
        {
            IdListResponse response = null;
            List<KalturaPersonalFeed> result = null;
            KalturaPersonalFeedListResponse ret = null;

            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            // Create notifications order object
            WebAPI.Notifications.OrderObj order = NotificationMapping.ConvertOrderToOrderObj(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetUserFeeder(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, order);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserFeeder.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.Ids != null && response.Ids.Count > 0)
            {
                result = Mapper.Map<List<KalturaPersonalFeed>>(response.Ids);
            }
            ret = new KalturaPersonalFeedListResponse() { PersonalFollowFeed = result, TotalCount = response.TotalCount };

            return ret;
        }

        [Obsolete]
        internal KalturaPersonalFollowFeedResponse GetUserFeeder(int groupId, string userID, int pageSize, int pageIndex, KalturaOrder? orderBy)
        {
            IdListResponse response = null;
            List<KalturaPersonalFollowFeed> result = null;
            KalturaPersonalFollowFeedResponse ret = null;

            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            // Create notifications order object
            WebAPI.Notifications.OrderObj order = new WebAPI.Notifications.OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = WebAPI.Notifications.OrderBy.NONE;
            }
            else
            {
                order = NotificationMapping.ConvertOrderToOrderObj(orderBy.Value);
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetUserFeeder(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, order);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserFeeder.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.Ids != null && response.Ids.Count > 0)
            {
                result = Mapper.Map<List<KalturaPersonalFollowFeed>>(response.Ids);
            }
            ret = new KalturaPersonalFollowFeedResponse() { PersonalFollowFeed = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal KalturaInboxMessageListResponse GetInboxMessageList(int groupId, string userID, int pageSize, int pageIndex, List<KalturaInboxMessageType> typeIn, long createdAtGreaterThanOrEqual, long createdAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            List<KalturaInboxMessage> result = null;
            KalturaInboxMessageListResponse ret = null;

            if (typeIn == null || typeIn.Count == 0)
            {
                typeIn = new List<KalturaInboxMessageType>();
                typeIn.Add(KalturaInboxMessageType.Followed);
                typeIn.Add(KalturaInboxMessageType.SystemAnnouncement);
            }

            List<eMessageCategory> convertedtypeIn = typeIn.Select(x => NotificationMapping.ConvertInboxMessageType(x)).ToList();

            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetInboxMessages(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessages.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.InboxMessages != null && response.InboxMessages.Count > 0)
            {
                result = Mapper.Map<List<KalturaInboxMessage>>(response.InboxMessages);
            }
            ret = new KalturaInboxMessageListResponse() { InboxMessages = result, TotalCount = response.TotalCount };

            return ret;
        }

        [Obsolete]
        internal KalturaInboxMessageResponse GetInboxMessages(int groupId, string userID, int pageSize, int pageIndex, List<KalturaInboxMessageType> typeIn, long createdAtGreaterThanOrEqual, long createdAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            List<KalturaInboxMessage> result = null;
            KalturaInboxMessageResponse ret = null;

            if (typeIn == null || typeIn.Count == 0)
            {
                typeIn = new List<KalturaInboxMessageType>();
                typeIn.Add(KalturaInboxMessageType.Followed);
                typeIn.Add(KalturaInboxMessageType.SystemAnnouncement);
            }

            List<eMessageCategory> convertedtypeIn = typeIn.Select(x => NotificationMapping.ConvertInboxMessageType(x)).ToList();

            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetInboxMessages(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessages.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.InboxMessages != null && response.InboxMessages.Count > 0)
            {
                result = Mapper.Map<List<KalturaInboxMessage>>(response.InboxMessages);
            }
            ret = new KalturaInboxMessageResponse() { InboxMessages = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal bool UpdateInboxMessage(int groupId, string userID, string messageId, KalturaInboxMessageStatus status)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.UpdateInboxMessage(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, messageId, NotificationMapping.ConvertInboxMessageStatus(status));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateInboxMessage.  groupID: {0}, messageId: {1}, exception: {2}", groupId, messageId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaInboxMessage GetInboxMessage(int groupId, string userID, string id)
        {
            InboxMessageResponse response = null;
            KalturaInboxMessage result = null;

            Group group = GroupsManager.GetGroup(groupId);

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetInboxMessage(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessage.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.InboxMessages != null && response.InboxMessages.Count > 0)
            {
                result = AutoMapper.Mapper.Map<KalturaInboxMessage>(response.InboxMessages[0]);
            }

            return result;
        }

        internal bool UpdateTopic(int groupId, int id, KalturaTopicAutomaticIssueNotification automaticIssueNotification)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.UpdateAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, id, NotificationMapping.ConvertAutomaticIssueNotification(automaticIssueNotification));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while UpdateTopic.  groupID: {0}, id: {1}, exception: {2}", groupId, id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal bool DeleteTopic(int groupId, int id)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.DeleteAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, id);
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


        internal KalturaTopic GetTopic(int groupId, int id)
        {
            AnnouncementsResponse response = null;
            KalturaTopic result = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetAnnouncement(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetTopic.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.Announcements != null && response.Announcements.Count > 0)
            {
                result = AutoMapper.Mapper.Map<KalturaTopic>(response.Announcements[0]);
            }

            return result;
        }

        internal KalturaTopicListResponse GetTopicsList(int groupId, int pageSize, int pageIndex)
        {
            AnnouncementsResponse response = null;
            List<KalturaTopic> result = null;
            KalturaTopicListResponse ret = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetAnnouncements(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetTopics.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.Announcements != null && response.Announcements.Count > 0)
            {
                result = Mapper.Map<List<KalturaTopic>>(response.Announcements);
            }
            ret = new KalturaTopicListResponse() { Topics = result, TotalCount = response.TotalCount };

            return ret;
        }

        [Obsolete]
        internal KalturaTopicResponse GetTopics(int groupId, int pageSize, int pageIndex)
        {
            AnnouncementsResponse response = null;
            List<KalturaTopic> result = null;
            KalturaTopicResponse ret = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.GetAnnouncements(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetTopics.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.Announcements != null && response.Announcements.Count > 0)
            {
                result = Mapper.Map<List<KalturaTopic>>(response.Announcements);
            }
            ret = new KalturaTopicResponse() { Topics = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal bool DeleteAnnouncementsOlderThan(int groupId)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.DeleteAnnouncementsOlderThan(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncementsOlderThan.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaRegistryResponse Register(int groupId, KalturaNotificationType type, string id, string hash, string ip)
        {
            RegistryResponse response = null;
            KalturaRegistryResponse ret = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    switch (type)
                    {
                        case KalturaNotificationType.announcement:
                            response = Notification.RegisterPushAnnouncementParameters(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, long.Parse(id), hash, ip);
                            break;
                        case KalturaNotificationType.system:
                            if (id.ToLower() == "login")
                                response = Notification.RegisterPushSystemParameters(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, hash, ip);
                            else
                                throw new NotImplementedException();
                            break;
                        case KalturaNotificationType.Reminder:
                            response = Notification.RegisterPushReminderParameters(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, long.Parse(id), hash, ip);                            
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while RegisterPushAnnouncementParameters.  groupID: {0}, notification type: {1}, exception: {2}", groupId, type, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            if (response.NotificationId != 0)
                ret = Mapper.Map<KalturaRegistryResponse>(response);
            else
                ret = new KalturaRegistryResponse();

            return ret;
        }    
        
        internal KalturaReminder AddAssetReminder(int groupId, string userID, KalturaAssetReminder reminder)
        {
            Notifications.RemindersResponse response = null;
            KalturaReminder kalturaReminder = null;
            DbReminder dbReminder = null;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    dbReminder = Mapper.Map<DbReminder>(reminder);
                    dbReminder.Reference = reminder.AssetId;
                    response = Notification.AddUserReminder(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, dbReminder);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                kalturaReminder = Mapper.Map<KalturaReminder>(response.Reminders[0]);
            }

            return kalturaReminder;
        }

        internal bool DeleteReminder(string userID, int groupId, long reminderId)
        {
            Status response = null;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.DeleteUserReminder(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userId, reminderId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteReminder.  groupID: {0}, userId: {1}, reminderId: {2}, exception: {3}", groupId, userID, reminderId,ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal bool RemoveUsersNotificationData(int groupId, List<string> userIds)
        {
            Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Notification.RemoveUsersNotificationData(group.NotificationsCredentials.Username, group.NotificationsCredentials.Password, userIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncementsOlderThan.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaReminderListResponse GetReminders(int groupId, string p1, int p2, int p3, KalturaAssetOrderBy kalturaAssetOrderBy)
        {
            throw new NotImplementedException();
        }
    }
}