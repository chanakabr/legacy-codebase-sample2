using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
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

        internal KalturaPartnerNotificationSettings Get(int groupId)
        {
            NotificationPartnerSettingsResponse response = null;
            KalturaPartnerNotificationSettings settings = null;

            try
            {
                log.Debug(string.Format("GroupId={0}", groupId));

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetNotificationPartnerSettings(groupId);
                }
                log.Debug("return from Notification.GetNotificationPartnerSettings");
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
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

            
            NotificationSettingsResponse response = null;
            KalturaNotificationSettings settings = null;

            try
            {
                log.Debug(string.Format("GroupId={0}", groupId));
                int user_id = int.Parse(userId);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetNotificationSettings(groupId, user_id);
                }
                log.Debug("return from Notification.UpdateNotificationPartnerSettings");

            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
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
                

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateNotificationPartnerSettings(groupId, settingsObj);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
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
                
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateNotificationSettings(groupId, userId, settingsObj);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
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
                    response = Core.Notification.Module.AddMessageAnnouncement(groupId, messageAnnouncement);
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
                    response = Core.Notification.Module.UpdateMessageAnnouncement(groupId, announcementId, messageAnnouncement);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateMessageAnnouncementStatus(groupId, id, status);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteMessageAnnouncement(groupId, id);
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
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {

                    response = Core.Notification.Module.CreateSystemAnnouncement(groupId);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAllMessageAnnouncements(groupId, pageSize, pageIndex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAllMessageAnnouncements(groupId, pageSize, pageIndex);
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
            

            try
            {
                eUserMessageAction action = string.IsNullOrEmpty(userId) || userId == "0" ? eUserMessageAction.AnonymousPushRegistration : eUserMessageAction.IdentifyPushRegistration;
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    Core.Notification.Module.InitiateNotificationActionAsync(groupId, action, int.Parse(userId), udid, pushToken);
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

            

            try
            {
                MessageTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<MessageTemplate>(followTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.SetMessageTemplate(groupId, apiFollowTemplate);
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

            
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserFollows(groupId, userId, pageSize, pageIndex, order);
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

            
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserFollows(groupId, userId, pageSize, pageIndex, order);
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
            followData.Title = mediaInfoResponse.Objects[0].Name.ToString();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.Unfollow(groupId, userId, followData);
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
            followData.Title = mediaInfoResponse.Objects[0].Name.ToString();
            followDataNotification = Mapper.Map<FollowDataTvSeries>(followData);
            followDataNotification.Type = mediaInfoResponse.Objects[0].getType();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.Follow(groupId, userId, followDataNotification);
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

        internal KalturaMessageTemplate GetMessageTemplate(int groupId, KalturaMessageTemplateType messageTemplateType)
        {
            MessageTemplateResponse response = null;
            KalturaMessageTemplate result = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetMessageTemplate(groupId, NotificationMapping.ConvertTemplateAssetType(messageTemplateType));
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

            

            try
            {
                MessageTemplate apiFollowTemplate = null;
                apiFollowTemplate = AutoMapper.Mapper.Map<MessageTemplate>(messageTemplate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.SetMessageTemplate(groupId, apiFollowTemplate);
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

            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            // Create notifications order object
            OrderObj order = NotificationMapping.ConvertOrderToOrderObj(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserFeeder(groupId, userId, pageSize, pageIndex, order);
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

            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            // Create notifications order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NONE;
            }
            else
            {
                order = NotificationMapping.ConvertOrderToOrderObj(orderBy.Value);
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserFeeder(groupId, userId, pageSize, pageIndex, order);
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

            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetInboxMessages(groupId, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
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

            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetInboxMessages(groupId, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
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
            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateInboxMessage(groupId, userId, messageId, NotificationMapping.ConvertInboxMessageStatus(status));
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

            

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetInboxMessage(groupId, userId, id);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateAnnouncement(groupId, id, NotificationMapping.ConvertAutomaticIssueNotification(automaticIssueNotification));
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteAnnouncement(groupId, id);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAnnouncement(groupId, id);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAnnouncements(groupId, pageSize, pageIndex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAnnouncements(groupId, pageSize, pageIndex);
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

        internal bool DeleteAnnouncementsOlderThan()
        {
            Status response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteAnnouncementsOlderThan();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteAnnouncementsOlderThan.  exception: {0}", ex);
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

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    switch (type)
                    {
                        case KalturaNotificationType.announcement:
                            response = Core.Notification.Module.RegisterPushAnnouncementParameters(groupId, long.Parse(id), hash, ip);
                            break;
                        case KalturaNotificationType.system:
                            if (id.ToLower() == "login")
                                response = Core.Notification.Module.RegisterPushSystemParameters(groupId, hash, ip);
                            else
                                throw new NotImplementedException();
                            break;
                        case KalturaNotificationType.Reminder:
                            response = Core.Notification.Module.RegisterPushReminderParameters(groupId, long.Parse(id), hash, ip);
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
            RemindersResponse response = null;
            KalturaReminder kalturaReminder = null;
            DbReminder dbReminder = null;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    dbReminder = Mapper.Map<DbReminder>(reminder);
                    dbReminder.Reference = reminder.AssetId;
                    dbReminder.GroupId = groupId;
                    response = Core.Notification.Module.AddUserReminder(groupId, userId, dbReminder);
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
                kalturaReminder = Mapper.Map<KalturaAssetReminder>(response.Reminders[0]);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteUserReminder(groupId, userId, reminderId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteReminder.  groupID: {0}, userId: {1}, reminderId: {2}, exception: {3}", groupId, userID, reminderId, ex);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.RemoveUsersNotificationData(groupId, userIds);
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

        internal KalturaReminderListResponse GetReminders(int groupId, string userID, string filter, int pageSize, int pageIndex, KalturaAssetOrderBy orderBy)
        {
            RemindersResponse response = null;
            List<KalturaReminder> result = null;
            KalturaReminderListResponse ret = null;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            // Create notifications order object
            OrderObj order = NotificationMapping.ConvertOrderToOrderObj(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserReminders(groupId, userId, filter, pageSize, pageIndex, order);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetReminders. groupID: {0}, exception: {1}", groupId, ex);
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

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                result = Mapper.Map<List<KalturaReminder>>(response.Reminders);
            }
            ret = new KalturaReminderListResponse() { Reminders = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal List<KalturaEngagementAdapter> GetEngagementAdapters(int groupId)
        {
            List<KalturaEngagementAdapter> list = null;
            EngagementAdapterResponseList response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagementAdapters(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagementAdapters. groupID: {0}, exception: {1}", groupId, ex);
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

            list = Mapper.Map<List<KalturaEngagementAdapter>>(response.EngagementAdapters);

            return list;
        }

        internal KalturaEngagementAdapter GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            KalturaEngagementAdapter engagementAdapter = null;
            EngagementAdapterResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagementAdapter(groupId, engagementAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagementAdapter. groupID: {0}, exception: {1}", groupId, ex);
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

            engagementAdapter = Mapper.Map<KalturaEngagementAdapter>(response);

            return engagementAdapter;
        }

        internal bool DeleteEngagementAdapter(int groupId, int engagementAdapterId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteEngagementAdapter(groupId, engagementAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteEngagementAdapter.  groupID: {0}, engagementAdapterId: {1}, exception: {2}", groupId, engagementAdapterId, ex);
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

        internal KalturaEngagementAdapter InsertEngagementAdapter(int groupId, KalturaEngagementAdapter engagementAdapter)
        {
            EngagementAdapterResponse response = null;
            KalturaEngagementAdapter kalturaEngagementAdapter = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    EngagementAdapter request = Mapper.Map<EngagementAdapter>(engagementAdapter);
                    response = Core.Notification.Module.InsertEngagementAdapter(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertEngagementAdapter.  groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaEngagementAdapter = Mapper.Map<KalturaEngagementAdapter>(response);
            return kalturaEngagementAdapter;
        }

        internal KalturaEngagementAdapter SetEngagementAdapter(int groupId, KalturaEngagementAdapter engagementAdapter)
        {
            EngagementAdapterResponse response = null;
            KalturaEngagementAdapter kalturaEngagementAdapter = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    EngagementAdapter request = Mapper.Map<EngagementAdapter>(engagementAdapter);
                    response = Core.Notification.Module.SetEngagementAdapter(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetEngagementAdapter. groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaEngagementAdapter = Mapper.Map<KalturaEngagementAdapter>(response);
            return kalturaEngagementAdapter;
        }

        internal KalturaEngagementAdapter GenerateEngagementSharedSecret(int groupId, int engagementAdapterId)
        {
            EngagementAdapterResponse response = null;
            KalturaEngagementAdapter engagementAdapter = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GenerateEngagementSharedSecret(groupId, engagementAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GenerateEngagementSharedSecret. groupID: {0}, exception: {1}", groupId, ex);
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

            engagementAdapter = Mapper.Map<KalturaEngagementAdapter>(response);
            return engagementAdapter;
        }

        internal List<KalturaEngagementAdapter> GetEngagementAdapterSettings(int groupId)
        {
            List<KalturaEngagementAdapter> list = null;
            EngagementAdapterSettingsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagementAdapterSettings(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagementAdapterSettings. groupID: {0}, exception: {1}", groupId, ex);
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

            list = Mapper.Map<List<KalturaEngagementAdapter>>(response.EngagementAdapters);

            return list;
        }

        internal bool DeleteEngagementAdapterSettings(int groupId, int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<EngagementAdapterSettings> request = NotificationMapping.ConvertEngagementAdapterSettings(settings);
                    response = Core.Notification.Module.DeleteEngagementAdapterSettings(groupId, engagementAdapterId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteEngagementAdapterSettings.  groupID: {0}, engagementAdapterId: {1}, exception: {2}", groupId, engagementAdapterId, ex);
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

        internal bool InsertEngagementAdapterSettings(int groupId, int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<EngagementAdapterSettings> request = NotificationMapping.ConvertEngagementAdapterSettings(settings);
                    response = Core.Notification.Module.InsertEngagementAdapterSettings(groupId, engagementAdapterId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertEngagementAdapterSettings. groupID: {0}, engagementAdapterId: {1} ,exception: {2}", groupId, engagementAdapterId, ex);
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

        internal bool SetEngagementAdapterSettings(int groupId, int engagementAdapterId, SerializableDictionary<string, KalturaStringValue> settings)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    List<EngagementAdapterSettings> configs = NotificationMapping.ConvertEngagementAdapterSettings(settings);
                    response = Core.Notification.Module.SetEngagementAdapterSettings(groupId, engagementAdapterId, configs);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while SetEngagementAdapterSettings. groupID: {0}, engagementAdapterId: {1}, exception: {2}", groupId, engagementAdapterId, ex);
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

        internal List<KalturaEngagement> GetEngagements(int groupId)
        {
            List<KalturaEngagement> list = null;
            EngagementResponseList response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagements(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagements. groupID: {0}, exception: {1}", groupId, ex);
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

            list = Mapper.Map<List<KalturaEngagement>>(response.Engagements);

            return list;
        }

        internal KalturaEngagement GetEngagement(int groupId, int id)
        {
            KalturaEngagement engagement = null;
            EngagementResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagement(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagement. groupID: {0}, exception: {1}", groupId, ex);
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

            engagement = Mapper.Map<KalturaEngagement>(response.Engagement);

            return engagement;
        }

        internal bool DeleteEngagement(int groupId, int id)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteEngagement(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteEngagement.  groupID: {0}, engagementId: {1}, exception: {2}", groupId, id, ex);
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

        internal KalturaEngagement InsertEngagement(int groupId, KalturaEngagement engagement)
        {
            EngagementResponse response = null;
            KalturaEngagement kalturaEngagement = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    Engagement request = Mapper.Map<Engagement>(engagement);
                    response = Core.Notification.Module.AddEngagement(groupId, request);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertEngagement.  groupID: {0}, exception: {1}", groupId, ex);
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

            kalturaEngagement = Mapper.Map<KalturaEngagement>(response.Engagement);
            return kalturaEngagement;
        }
    }
}