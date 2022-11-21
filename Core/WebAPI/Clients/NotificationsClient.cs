using ApiLogic.Notification.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using AutoMapper;
using Core.Notification;
using Phx.Lib.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TVinciShared;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Models.Notifications;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;
using WebAPI.ObjectsConvertor.Extensions;
using KalturaTopicNotificationListResponse = WebAPI.Models.Notifications.KalturaTopicNotificationListResponse;

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

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetNotificationPartnerSettings(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (!response.Status.IsOkStatusCode())
            {
                throw new ClientException(response.Status);
            }

            return Mapper.Map<KalturaPartnerNotificationSettings>(response.settings);
        }

        internal KalturaSmsAdapterProfile GenerateSmsAdapaterSharedSecret(int groupId, int smsAdapterId, int updaterId)
        {
            var response = new SmsAdapterProfile();
            var _response = new SmsAdaptersResponse();
            try
            {
                using (var km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var sharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);
                    _response = SmsManager.Instance.SetSmsAdapterSharedSecret(groupId, smsAdapterId, sharedSecret, updaterId);
                    var selected = _response.SmsAdapters?.Where(adapter => adapter.Id == smsAdapterId).FirstOrDefault();
                    response = new SmsAdapterProfile
                    {
                        AdapterUrl = selected.AdapterUrl,
                        Id = selected.Id,
                        ExternalIdentifier = selected.ExternalIdentifier,
                        GroupId = selected.GroupId,
                        IsActive = selected.IsActive == 1,
                        Name = selected.Name,
                        Settings = selected.Settings,
                        SharedSecret = selected.SharedSecret
                    };
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while InsertSmsAdapter. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (_response.RespStatus.Code != (int)StatusCode.OK)
            {
                log.ErrorFormat("Error while InsertSmsAdapter. groupID: {0}, message: {1}", groupId, _response.RespStatus.Message);
                throw new ClientException(_response.RespStatus);
            }

            return Mapper.Map<KalturaSmsAdapterProfile>(response);
        }

        internal bool DispatchEventNotification(int groupId, KalturaEventNotificationObjectScope scope)
        {
            Func<bool> notifyFunc = () =>
            {
                var eneot = Mapper.Map<EventNotificationObjectScope>(scope);
                eneot.EventObject.GroupId = groupId;
                return eneot.EventObject.Notify(type: eneot.EventObject.GetType().Name.ToLower());
            };

            return ClientUtils.GetBoolResponseFromWS(notifyFunc);
        }

        internal bool SendEmail(int groupId, KalturaEmailMessage emailMessage)
        {
            bool response = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    DynamicMailRequest mailMessage = NotificationMapping.ConvertEmailMessage(emailMessage);

                    mailMessage.groupId = groupId;
                    response = Core.Api.Module.SendMailTemplate(groupId, mailMessage);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while sending email to user.  groupID: {0},emailMessage: {1}, exception: {2}", groupId, JsonConvert.SerializeObject(emailMessage), ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || !response)
                throw new ClientException(StatusCode.Error);

            return true;
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }
            else
            {
                settings = AutoMapper.Mapper.Map<KalturaNotificationSettings>(response.settings);

                return settings;
            }
        }

        internal bool Update(int groupId, KalturaNotificationsPartnerSettings settingsDto)
        {
            Status response = null;
            try
            {
                var settings = Mapper.Map<NotificationPartnerSettings>(settingsDto);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.UpdateNotificationPartnerSettings(groupId, settings);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling notification service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
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
                throw new ClientException(StatusCode.Error);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }
            else
            {
                success = true;
            }

            return success;
        }

        internal KalturaAnnouncement AddAnnouncement(int groupId, KalturaAnnouncement announcement)
        {
            AddMessageAnnouncementResponse response = null;

            var messageAnnouncement = Mapper.Map<MessageAnnouncement>(announcement);

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
                throw new ClientException(response.Status);
            }

            KalturaAnnouncement result = Mapper.Map<KalturaAnnouncement>(response.Announcement);
            return result;
        }

        internal KalturaAnnouncement UpdateAnnouncement(int groupId, int announcementId, KalturaAnnouncement announcement)
        {
            MessageAnnouncementResponse response = null;
            MessageAnnouncement messageAnnouncement = Mapper.Map<MessageAnnouncement>(announcement);

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
                throw new ClientException(response.Status);
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
                throw new ClientException(response);
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
                throw new ClientException(response);
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
                throw new ClientException(response);
            }
            return true;
        }

        internal KalturaAnnouncementListResponse GetAnnouncements(int groupId, int pageSize, int pageIndex, KalturaAnnouncementFilter kalturaFilter)
        {
            GetAllMessageAnnouncementsResponse response = null;

            try
            {
                var filter = Mapper.Map<MessageAnnouncementFilter>(kalturaFilter);
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAllMessageAnnouncements(groupId, pageSize, pageIndex, filter);
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
                throw new ClientException(response.Status);
            }

            var result = Mapper.Map<List<KalturaAnnouncement>>(response.messageAnnouncements);
            var ret = new KalturaAnnouncementListResponse() { Announcements = result, TotalCount = response.totalCount };
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
                throw new ClientException(response.Status);
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
                    Core.Notification.Module.AddInitiateNotificationActionToQueue(groupId, action, int.Parse(userId), udid, pushToken);
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
                throw new ClientException(response.Status);
            }
            result = AutoMapper.Mapper.Map<KalturaMessageTemplate>(response.MessageTemplate);
            return result;
        }

        internal KalturaFollowTvSeriesListResponse ListUserTvSeriesFollows(int groupId, string userID, int pageSize, int pageIndex, KalturaFollowTvSeriesOrderBy orderBy)
        {
            List<KalturaFollowTvSeries> result = null;
            GetUserFollowsResponse response = null;

            // create order object
            var order = ApiObjects.SearchObjects.OrderDir.DESC;
            if (orderBy == KalturaFollowTvSeriesOrderBy.START_DATE_ASC)
                order = ApiObjects.SearchObjects.OrderDir.ASC;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid UID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserFollows(groupId, userId, pageSize, pageIndex, order, true);
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
                throw new ClientException(response.Status);
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
            var order = ApiObjects.SearchObjects.OrderDir.DESC;
            if (orderBy != null && orderBy.Value == KalturaOrder.oldest_first)
                order = ApiObjects.SearchObjects.OrderDir.ASC;


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
                throw new ClientException(response.Status);
            }

            result = Mapper.Map<List<KalturaFollowDataTvSeries>>(response.Follows);

            ret = new KalturaListFollowDataTvSeriesResponse() { FollowDataList = result, TotalCount = response.TotalCount };
            return ret;
        }

        internal bool DeleteUserTvSeriesFollow(int groupId, string userID, int assetId)
        {
            Status response = null;

            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get asset name
            var mediaInfoResponse = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), KSUtils.ExtractKSPayload().UDID, null, 0, 0,
                                                                                    new List<int>() { assetId }, KalturaAssetOrderBy.START_DATE_DESC);

            FollowDataTvSeries followData = new FollowDataTvSeries();
            followData.AssetId = assetId;

            string name = string.Empty;
            if (mediaInfoResponse.Objects.Count > 0 && mediaInfoResponse.Objects[0] != null && mediaInfoResponse.Objects[0].Name != null)
            {
                name = !string.IsNullOrEmpty(mediaInfoResponse.Objects[0].Name.GetDefaultLanugageValue()) ? mediaInfoResponse.Objects[0].Name.GetDefaultLanugageValue() : string.Empty;
            }
            followData.Title = name;

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
                throw new ClientException(response);
            }

            return true;
        }

        internal KalturaFollowTvSeries AddUserTvSeriesFollow(int groupId, string userID, int asset_id)
        {
            GenericResponse<FollowDataBase> response = null;
            FollowDataTvSeries followDataNotification = null;
            KalturaFollowDataTvSeries followData = new KalturaFollowDataTvSeries();
            followData.AssetId = asset_id;


            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get asset name
            var mediaInfoResponse = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(), KSUtils.ExtractKSPayload().UDID, null, 0, 0,
                                                                                    new List<int>() { followData.AssetId }, KalturaAssetOrderBy.START_DATE_DESC);

            followData.Status = 1;
            followData.Title = mediaInfoResponse.Objects[0].Name.GetDefaultLanugageValue();
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
                throw new ClientException(response.Status);
            }

            KalturaFollowTvSeries result = AutoMapper.Mapper.Map<KalturaFollowTvSeries>(response.Object);
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
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Ids != null && response.Ids.Count > 0)
            {
                result = Mapper.Map<List<KalturaPersonalFollowFeed>>(response.Ids);
            }
            ret = new KalturaPersonalFollowFeedResponse() { PersonalFollowFeed = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal KalturaInboxMessageListResponse GetInboxMessageList(int groupId, long domainId, string userID, int pageSize, int pageIndex, List<KalturaInboxMessageType> typeIn, long createdAtGreaterThanOrEqual, long createdAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            List<KalturaInboxMessage> result = null;
            KalturaInboxMessageListResponse ret = null;

            if (typeIn == null || typeIn.Count == 0)
            {
                typeIn = new List<KalturaInboxMessageType>();
                typeIn = Enum.GetValues(typeof(KalturaInboxMessageType)).Cast<KalturaInboxMessageType>().ToList();
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
                    response = Core.Notification.Module.GetInboxMessages(groupId, domainId, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessages.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.InboxMessages != null && response.InboxMessages.Count > 0)
            {
                result = Mapper.Map<List<KalturaInboxMessage>>(response.InboxMessages);
            }
            ret = new KalturaInboxMessageListResponse() { InboxMessages = result, TotalCount = response.TotalCount };

            return ret;
        }

        [Obsolete]
        internal KalturaInboxMessageResponse GetInboxMessages(int groupId, long domainId, string userID, int pageSize, int pageIndex, List<KalturaInboxMessageType> typeIn, long createdAtGreaterThanOrEqual, long createdAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            List<KalturaInboxMessage> result = null;
            KalturaInboxMessageResponse ret = null;

            if (typeIn == null || typeIn.Count == 0)
            {
                typeIn = new List<KalturaInboxMessageType>();
                typeIn = Enum.GetValues(typeof(KalturaInboxMessageType)).Cast<KalturaInboxMessageType>().ToList();
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
                    response = Core.Notification.Module.GetInboxMessages(groupId, domainId, userId, pageSize, pageIndex, convertedtypeIn, createdAtGreaterThanOrEqual, createdAtLessThanOrEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessages.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.InboxMessages != null && response.InboxMessages.Count > 0)
            {
                result = Mapper.Map<List<KalturaInboxMessage>>(response.InboxMessages);
            }
            ret = new KalturaInboxMessageResponse() { InboxMessages = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal bool UpdateInboxMessage(int groupId, long domainId, string userID, string messageId, KalturaInboxMessageStatus status)
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
                    response = Core.Notification.Module.UpdateInboxMessage(groupId, domainId, userId, messageId, NotificationMapping.ConvertInboxMessageStatus(status));
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
                throw new ClientException(response);
            }

            return true;
        }

        internal KalturaInboxMessage GetInboxMessage(int groupId, long domainId, string userID, string id)
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
                    response = Core.Notification.Module.GetInboxMessage(groupId, domainId, userId, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetInboxMessage.  groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(response);
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
                throw new ClientException(response);
            }

            return true;
        }

        internal KalturaTopic GetTopic(int groupId, int id)
        {
            KalturaTopic result = null;
            AnnouncementsResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetAnnouncement(groupId, id);
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while GetAnnouncement. groupID: {groupId}, exception: {ex}");
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Announcements != null && response.Announcements.Count > 0)
            {
                result = Mapper.Map<KalturaTopic>(response.Announcements[0]);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(response);
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
                        case KalturaNotificationType.series_reminder:
                            response = Core.Notification.Module.RegisterPushSeriesReminderParameters(groupId, long.Parse(id), hash, ip);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.NotificationId != 0)
                ret = Mapper.Map<KalturaRegistryResponse>(response);
            else
                ret = new KalturaRegistryResponse();

            return ret;
        }

        internal bool SendPush(int groupId, int userId, KalturaPushMessage kalturaPushMessage)
        {
            Func<PushMessage, Status> sendPushFunc =
              (PushMessage pushMessage) => Core.Notification.Module.SendUserPush(groupId, userId, pushMessage);

            ClientUtils.GetResponseStatusFromWS<KalturaPushMessage, PushMessage>(sendPushFunc, kalturaPushMessage);

            return true;
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
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status);
            }

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                kalturaReminder = Mapper.Map<KalturaAssetReminder>(response.Reminders[0]);
            }

            return kalturaReminder;
        }

        internal bool DeleteReminder(int userId, int groupId, long reminderId, KalturaReminderType type)
        {
            Status response = null;

            // get group ID
            ReminderType wsReminderType = NotificationMapping.ConvertReminderType(type);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.DeleteUserReminder(groupId, userId, reminderId, wsReminderType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while DeleteReminder.  groupID: {0}, userId: {1}, reminderId: {2}, exception: {3}", groupId, userId, reminderId, ex);
                ErrorUtils.HandleWSException(ex);
            }
            if (response.Code != (int)StatusCode.OK)
            {
                // Bad response received from WS
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
            }

            return true;
        }

        internal KalturaReminderListResponse GetReminders(int groupId, string userID, string filter, int pageSize, int pageIndex, KalturaAssetReminderOrderBy orderBy)
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                result = new List<KalturaReminder>();
                foreach (var reminder in response.Reminders)
                {
                    result.Add(Mapper.Map<KalturaAssetReminder>(reminder));
                }
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            engagementAdapter = Mapper.Map<KalturaEngagementAdapter>(response);
            return engagementAdapter;
        }

        internal List<KalturaEngagement> GetEngagements(int groupId, List<KalturaEngagementType> typeIn, long? sendTimeLessThanOrEqual)
        {
            List<KalturaEngagement> list = null;
            EngagementResponseList response = null;

            if (typeIn == null || typeIn.Count == 0)
            {
                typeIn = new List<KalturaEngagementType>();
                typeIn = Enum.GetValues(typeof(KalturaEngagementType)).Cast<KalturaEngagementType>().ToList();
            }

            List<eEngagementType> convertedtypeIn = typeIn.Select(x => NotificationMapping.ConvertEngagementType(x)).ToList();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetEngagements(groupId, convertedtypeIn, DateUtils.UtcUnixTimestampSecondsToDateTime(sendTimeLessThanOrEqual));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetEngagements. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response);
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
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            kalturaEngagement = Mapper.Map<KalturaEngagement>(response.Engagement);
            return kalturaEngagement;
        }

        internal KalturaReminderListResponse GetSeriesReminders(int groupId, string userId, List<string> seriesIds, List<long> seasonNumbers, long? epgChannelId,
            int pageSize, int pageIndex)
        {
            SeriesRemindersResponse response = null;
            List<KalturaReminder> result = null;
            KalturaReminderListResponse ret = null;

            int userIdInt = 0;
            if (!int.TryParse(userId, out userIdInt))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid user ID");
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserSeriesReminders(groupId, userIdInt, seriesIds, seasonNumbers, epgChannelId, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetSeriesReminders. groupID: {0}, exception: {1}", groupId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status);
            }

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                result = new List<KalturaReminder>();
                foreach (DbSeriesReminder reminder in response.Reminders)
                {
                    result.Add(Mapper.Map<KalturaSeriesReminder>(reminder));
                }
            }
            ret = new KalturaReminderListResponse() { Reminders = result, TotalCount = response.TotalCount };

            return ret;
        }

        internal KalturaReminder AddSeriesReminder(int groupId, string userId, KalturaSeriesReminder reminder)
        {
            RemindersResponse response = null;
            KalturaReminder kalturaReminder = null;
            DbSeriesReminder dbSeriesReminder = null;

            int userID = 0;
            if (!int.TryParse(userId, out userID))
            {
                throw new ClientException((int)StatusCode.UserIDInvalid, "Invalid Username");
            }

            // get group ID


            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    dbSeriesReminder = Mapper.Map<DbSeriesReminder>(reminder);
                    dbSeriesReminder.GroupId = groupId;
                    response = Core.Notification.Module.AddUserSeriesReminder(groupId, userID, dbSeriesReminder);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status);
            }

            if (response.Reminders != null && response.Reminders.Count > 0)
            {
                kalturaReminder = Mapper.Map<KalturaSeriesReminder>(response.Reminders[0]);
            }

            return kalturaReminder;
        }


        internal int GetUserIdByToken(int groupId, string token)
        {
            IntResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Notification.Module.GetUserIdByToken(groupId, token);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {0}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException(StatusCode.Error);
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status);
            }

            return response.Value;
        }

        internal bool SendSms(int groupId, int userId, string message, string phoneNumber,
            SerializableDictionary<string, KalturaStringValue> adapterData)
        {
            Status response = new Status();
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    var keyValueList = new List<ApiObjects.KeyValuePair>();

                    if (adapterData != null)
                    {
                        keyValueList = adapterData.Select(p => new ApiObjects.KeyValuePair { key = p.Key, value = p.Value.value }).ToList();
                    }

                    response = Core.Notification.Module.SendUserSms(groupId, userId, message, phoneNumber, keyValueList);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while sending push to user.  groupID: {0}, userId: {1}, message: {2}, exception: {3}", groupId, userId, message, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
                throw new ClientException(StatusCode.Error);

            if (response.Code != (int)StatusCode.OK)
                throw new ClientException(response);

            return true;
        }

        internal KalturaTopicNotification AddTopicNotification(int groupId, KalturaTopicNotification topicNotification, string userId)
        {
            Func<TopicNotification, GenericResponse<TopicNotification>> addTopicNotificationFunc = (TopicNotification topicNotificationToAdd) =>
               Core.Notification.Module.AddTopicNotification(groupId, topicNotificationToAdd, long.Parse(userId));

            KalturaTopicNotification result =
                ClientUtils.GetResponseFromWS<KalturaTopicNotification, TopicNotification>(topicNotification, addTopicNotificationFunc);

            return result;
        }

        internal KalturaTopicNotification UpdateTopicNotification(int groupId, KalturaTopicNotification topicNotification, string userId)
        {
            Func<TopicNotification, GenericResponse<TopicNotification>> updateTopicNotificationFunc = (TopicNotification topicNotificationToUpdate) =>
               Core.Notification.Module.UpdateTopicNotification(groupId, topicNotificationToUpdate, long.Parse(userId));

            KalturaTopicNotification result =
                ClientUtils.GetResponseFromWS<KalturaTopicNotification, TopicNotification>(topicNotification, updateTopicNotificationFunc);

            return result;
        }

        internal void DeleteTopicNotification(int groupId, long id, string userId)
        {
            Func<Status> deleteTopicNotificationFunc = () => Core.Notification.Module.DeleteTopicNotification(groupId, id, long.Parse(userId));
            ClientUtils.GetResponseStatusFromWS(deleteTopicNotificationFunc);
        }

        internal KalturaTopicNotificationListResponse GetTopicNotifications(int groupId, KalturaSubscribeReference subscribeReference)
        {
            KalturaTopicNotificationListResponse result = new KalturaTopicNotificationListResponse();

            Func<GenericListResponse<TopicNotification>> getTopicNotificationsFunc = () =>
               Core.Notification.Module.GetTopicNotifications(groupId, AutoMapper.Mapper.Map<SubscribeReference>(subscribeReference));

            KalturaGenericListResponse<KalturaTopicNotification> response =
                ClientUtils.GetResponseListFromWS<KalturaTopicNotification, TopicNotification>(getTopicNotificationsFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal void SubscribeUserToTopicNotification(int groupId, string userId, long topicNotificationId)
        {
            Func<Status> subscribeUserToTopicNotificationFunc = () => Core.Notification.Module.SubscribeUserToTopicNotification(groupId, topicNotificationId, long.Parse(userId));
            ClientUtils.GetResponseStatusFromWS(subscribeUserToTopicNotificationFunc);
        }

        internal void UnsubscribeUserFromTopicNotification(int groupId, string userId, long topicNotificationId)
        {
            Func<Status> unsubscribeUserFromTopicNotificationFunc = () => Core.Notification.Module.UnsubscribeUserFromTopicNotification(groupId, topicNotificationId, long.Parse(userId));
            ClientUtils.GetResponseStatusFromWS(unsubscribeUserFromTopicNotificationFunc);
        }

        internal KalturaTopicNotificationMessage AddTopicNotificationMessage(int groupId, KalturaTopicNotificationMessage topicNotificationMessage, string userId)
        {
            Func<TopicNotificationMessage, GenericResponse<TopicNotificationMessage>> addTopicNotificationMessageFunc = (TopicNotificationMessage topicNotificationMessageToAdd) =>
               Core.Notification.Module.AddTopicNotificationMessage(groupId, topicNotificationMessageToAdd, long.Parse(userId));

            KalturaTopicNotificationMessage result =
                ClientUtils.GetResponseFromWS<KalturaTopicNotificationMessage, TopicNotificationMessage>(topicNotificationMessage, addTopicNotificationMessageFunc);

            return result;
        }
        internal KalturaTopicNotificationMessage UpdateTopicNotificationMessage(int groupId, int id, KalturaTopicNotificationMessage topicNotificationMessage, string userId)
        {
            Func<TopicNotificationMessage, GenericResponse<TopicNotificationMessage>> updateTopicNotificationMessageFunc = (TopicNotificationMessage topicNotificationMessageToUpdate) =>
               Core.Notification.Module.UpdateTopicNotificationMessage(groupId, topicNotificationMessageToUpdate, long.Parse(userId));

            KalturaTopicNotificationMessage result =
                ClientUtils.GetResponseFromWS<KalturaTopicNotificationMessage, TopicNotificationMessage>(topicNotificationMessage, updateTopicNotificationMessageFunc);

            return result;
        }

        internal void DeleteTopicNotificationMessage(int groupId, long id, string userId)
        {
            Func<Status> deleteTopicNotificationMessageFunc = () => Core.Notification.Module.DeleteTopicNotificationMessage(groupId, id, long.Parse(userId));
            ClientUtils.GetResponseStatusFromWS(deleteTopicNotificationMessageFunc);
        }

        internal KalturaTopicNotificationMessageListResponse GetTopicNotificationMessages(int groupId, long topicNotificationIdEqual, int pageSize, int pageIndex)
        {
            KalturaTopicNotificationMessageListResponse result = new KalturaTopicNotificationMessageListResponse();

            Func<GenericListResponse<TopicNotificationMessage>> getTopicNotificationMessagesFunc = () =>
               Core.Notification.Module.GetTopicNotificationMessages(groupId, topicNotificationIdEqual, pageSize, pageIndex);

            KalturaGenericListResponse<KalturaTopicNotificationMessage> response =
                ClientUtils.GetResponseListFromWS<KalturaTopicNotificationMessage, TopicNotificationMessage>(getTopicNotificationMessagesFunc);

            result.Objects = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
        }

        internal KalturaFollowTvSeries AddKalturaFollowTvSeries(ContextData contextData, KalturaFollowTvSeries kalturaFollowTvSeriesToAdd)
        {
            Func<FollowDataTvSeries, GenericResponse<FollowDataTvSeries>> addFollowTvSeriesFunc = (FollowDataTvSeries followTvSeriesToAdd) =>
                   FollowManager.Instance.Add(contextData, followTvSeriesToAdd);

            KalturaFollowTvSeries result =
                ClientUtils.GetResponseFromWS<KalturaFollowTvSeries, FollowDataTvSeries>(kalturaFollowTvSeriesToAdd, addFollowTvSeriesFunc);

            return result;
        }

        internal bool DeleteKalturaFollowTvSeries(int groupId, long userId, int assetId)
        {
            Func<Status> deleteFollowTvSeriesFunc = () => FollowManager.Delete(groupId, userId, assetId);
            var result = ClientUtils.GetResponseStatusFromWS(deleteFollowTvSeriesFunc);

            return result;
        } 
    }
}