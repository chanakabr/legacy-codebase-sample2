using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Notification;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Web;

namespace WS_Notification
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]

    public class NotificationService : INotificationService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add notification request to the db, using NotificationManager object.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        [ServiceKnownType(typeof(Dictionary<string, List<int>>))]
        [ServiceKnownType(typeof(ExtraParams))]
        public bool AddNotificationRequest(string sWSUserName, string sWSPassword, string siteGuid, NotificationTriggerType triggerType, int nMediaID)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "AddNotificationRequest", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            log.Debug("AddNotificationRequest - " + string.Format("{0},{1},{2},{3}", sWSUserName, sWSPassword, TVinciShared.PageUtils.GetCallerIP(), nGroupID));

            if (nGroupID != 0)
            {
                return Core.Notification.Module.AddNotificationRequest(nGroupID, siteGuid, triggerType, nMediaID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        public bool HandleEpgEvent(int partnerId, List<ulong> programIds)
        {
            string sIP = TVinciShared.PageUtils.GetCallerIP();

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, partnerId);

            log.DebugFormat("EpgEvent - Source IP: {0}, Program ID: {1}, partner ID: {2}", TVinciShared.PageUtils.GetCallerIP(), string.Join(",", programIds.ToArray(), partnerId));
            if (partnerId > 0)
            {
                return Core.Notification.Module.HandleEpgEvent(partnerId, programIds);
            }
            else
                HttpContext.Current.Response.StatusCode = 404;

            return false;
        }

        /// <summary>
        /// Get Device Notification
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nUserID"></param>
        /// <param name="sDeviceUDID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<NotificationMessage> GetDeviceNotifications(string sWSUserName, string sWSPassword, string siteGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, int? messageCount)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "GetDeviceNotifications", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.GetDeviceNotifications(nGroupID, siteGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        public bool SetNotificationMessageViewStatus(string sWSUserName, string sWSPassword, string siteGuid, long? notificationRequestID, long? notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool res = false;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.SetNotificationMessageViewStatus(nGroupID, siteGuid, notificationRequestID, notificationMessageID, viewStatus);
            }
            else
            {
                #region Logging
                log.Error("NotificationService.SetNotificationMessageViewStatus() - " + string.Format("No valid group id. Group ID: {0} , Site Guid: {1}", nGroupID, siteGuid));
                #endregion
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return res;
        }

        public bool SubscribeByTag(string sWSUserName, string sWSPassword, string siteGuid, Dictionary<string, List<string>> tags)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool resault = false;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            int nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.SubscribeByTag(nGroupID, siteGuid, tags);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        public bool UserSettings(string sWSUserName, string sWSPassword, UserSettings userSettings)
        {
            bool resault = false;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            int nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.UserSettings(nGroupID, userSettings);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }

        }

        public bool UnsubscribeFollowUpByTag(string sWSUserName, string sWSPassword, string siteGuid, Dictionary<string, List<string>> tags)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool resault = false;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            int nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.UnsubscribeFollowUpByTag(nGroupID, siteGuid, tags);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        public Dictionary<string, List<string>> GetUserStatusSubscriptions(string sWSUserName, string sWSPassword, string siteGuid)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            Dictionary<string, List<string>> resault;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            int nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.GetUserStatusSubscriptions(nGroupID, siteGuid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        public bool IsTagNotificationExists(string sWSUserName, string sWSPassword, string tagType, string tagValue)
        {
            bool resault = false;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            Int32 nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.IsTagNotificationExists(nGroupID, tagType, tagValue);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        public Dictionary<string, List<string>> GetAllNotifications(string sWSUserName, string sWSPassword, NotificationTriggerType triggerType)
        {
            Dictionary<string, List<string>> resault;
            string sIP = TVinciShared.PageUtils.GetCallerIP();
            int nGroupID = TVinciShared.WS_Utils.GetGroupID("notifications", "SetNotificationMessageViewStatus", sWSUserName, sWSPassword, sIP);

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            if (nGroupID != 0)
            {
                return Core.Notification.Module.GetAllNotifications(nGroupID, triggerType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        //Notifications settings//
        public ApiObjects.Response.Status UpdateNotificationPartnerSettings(string sWSUserName, string sWSPassword, ApiObjects.Notification.NotificationPartnerSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.UpdateNotificationPartnerSettings(groupID, settings);
                }
            }
            catch (Exception)
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public ApiObjects.Notification.NotificationPartnerSettingsResponse GetNotificationPartnerSettings(string sWSUserName, string sWSPassword)
        {
            ApiObjects.Notification.NotificationPartnerSettingsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetNotificationPartnerSettings(groupID);
                }
            }
            catch (Exception)
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Notification.NotificationPartnerSettingsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public ApiObjects.Response.Status UpdateNotificationSettings(string sWSUserName, string sWSPassword, string userId, ApiObjects.Notification.UserNotificationSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.UpdateNotificationSettings(groupID, userId, settings);
                }
            }
            catch (Exception)
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public ApiObjects.Notification.NotificationSettingsResponse GetNotificationSettings(string sWSUserName, string sWSPassword, int userId)
        {
            ApiObjects.Notification.NotificationSettingsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetNotificationSettings(groupID, userId);
                }
            }
            catch (Exception)
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Notification.NotificationSettingsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        /// <summary>
        /// Add a Message Announcement to DB and send to rabbit.
        /// </summary>
        /// <returns></returns>
        // TODO
        public AddMessageAnnouncementResponse AddMessageAnnouncement(string sWSUserName, string sWSPassword, MessageAnnouncement announcement)
        {
            AddMessageAnnouncementResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.AddMessageAnnouncement(groupID, announcement);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddMessageAnnouncement caught an exception: GroupID: {0}, Announcement: {1}, ex: {2}", groupID, announcement, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new AddMessageAnnouncementResponse();
                response.Id = 0;
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        /// <summary>
        /// Sends a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public bool SendMessageAnnouncement(string sWSUserName, string sWSPassword, long startTime, int id)
        {
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.SendMessageAnnouncement(groupID, startTime, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendMessageAnnouncement caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", groupID, id, ex);
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Sends a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public bool SendMessageReminder(string sWSUserName, string sWSPassword, long startTime, int reminderId)
        {
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.SendMessageReminder(groupID, startTime, reminderId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendMessageReminder caught an exception: GroupID: {0}, reminder id: {1}, ex: {2}", groupID, reminderId, ex);
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Updates a Message Announcement.
        /// </summary>
        /// <returns></returns>
        [ServiceKnownType(typeof(MessageAnnouncement))]
        public MessageAnnouncementResponse UpdateMessageAnnouncement(string sWSUserName, string sWSPassword, int announcementId, MessageAnnouncement announcement)
        {
            MessageAnnouncementResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.UpdateMessageAnnouncement(groupID, announcementId, announcement);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateMessageAnnouncement caught an exception: GroupID: {0}, Announcement: {1}, ex: {2}", groupID, announcement, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new MessageAnnouncementResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        /// <summary>
        /// Deletes an announcement (topic)
        /// </summary>
        /// <returns></returns>
        public ApiObjects.Response.Status DeleteAnnouncement(string sWSUserName, string sWSPassword, long announcementId)
        {
            ApiObjects.Response.Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.DeleteAnnouncement(groupID, announcementId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAnnouncement caught an exception: GroupID: {0}, announcementId: {1}, ex: {2}", groupID, announcementId, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        /// <summary>
        /// Updates a Message Announcement status.
        /// </summary>
        /// <returns></returns>
        [ServiceKnownType(typeof(MessageAnnouncement))]
        public ApiObjects.Response.Status UpdateMessageAnnouncementStatus(string sWSUserName, string sWSPassword, long id, bool status)
        {
            ApiObjects.Response.Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.UpdateMessageAnnouncementStatus(groupID, id, status);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateMessageAnnouncementStatus caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", groupID, id, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        /// <summary>
        /// Deletes a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public ApiObjects.Response.Status DeleteMessageAnnouncement(string sWSUserName, string sWSPassword, long id)
        {
            ApiObjects.Response.Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.DeleteMessageAnnouncement(groupID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteMessageAnnouncement caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", groupID, id, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public GetAllMessageAnnouncementsResponse GetAllMessageAnnouncements(string sWSUserName, string sWSPassword, int pageSize, int pageIndex)
        {
            GetAllMessageAnnouncementsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetAllMessageAnnouncements(groupID, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAllMessageAnnouncements caught an exception: GroupID: {0}, ex: {2}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public bool InitiateNotificationAction(string sWSUserName, string sWSPassword, eUserMessageAction userAction, int userId, string udid, string pushToken)
        {
            bool result = false;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            if (groupID == 0)
            {
                log.ErrorFormat("GID wasn't found. pushAction: {0}, userId:{1}, UDID: {2}, sWSUserName: {3}, sWSPassword: {4}",
                    userAction.ToString(),
                    userId,
                    udid,
                    sWSUserName,
                    sWSPassword);
            }
            else
            {
                return Core.Notification.Module.InitiateNotificationAction(groupID, userAction, userId, udid, pushToken);
            }
            return result;
        }

        public ApiObjects.Response.Status CreateSystemAnnouncement(string sWSUserName, string sWSPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID > 0)
                {
                    return Core.Notification.Module.CreateSystemAnnouncement(groupID);
                }
                else
                {
                    log.Error("CreateSystemAnnouncement failed groupid = 0 ");
                }
            }
            catch (Exception ex)
            {
                log.Error("CreateSystemAnnouncement failed", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;

        }

        public MessageTemplateResponse SetMessageTemplate(string sWSUserName, string sWSPassword, MessageTemplate messageTemplate)
        {
            MessageTemplateResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.SetMessageTemplate(groupID, messageTemplate);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SetMessageTemplate caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new MessageTemplateResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        public FollowResponse Follow(string sWSUserName, string sWSPassword, int userId, FollowDataBase followData)
        {
            FollowResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            followData.GroupId = groupID;

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.Follow(groupID, userId, followData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Follow caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public GetUserFollowsResponse GetUserFollows(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, OrderDir order)
        {
            GetUserFollowsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetUserFollows(groupID, userId, pageSize, pageIndex, order);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserFollows caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public Status Unfollow(string sWSUserName, string sWSPassword, int userId, FollowDataBase followData)
        {
            Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.Unfollow(groupID, userId, followData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unfollow caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public IdsResponse Get_FollowedAssetIdsFromAssets(string sWSUserName, string sWSPassword, int groupId, int userId, List<int> assets)
        {
            IdsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.Get_FollowedAssetIdsFromAssets(groupID, userId, assets);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Get_FollowedAssetIdsFromAssets caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public MessageTemplateResponse GetMessageTemplate(string sWSUserName, string sWSPassword, MessageTemplateType assetType)
        {
            MessageTemplateResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetMessageTemplate(groupID, assetType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetMessageTemplate caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public IdListResponse GetUserFeeder(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, OrderObj orderObj)
        {
            IdListResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.GetUserFeeder(groupID, userId, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserFeeder caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public Status UpdateInboxMessage(string sWSUserName, string sWSPassword, int userId, string messageId, eMessageState status)
        {
            Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.UpdateInboxMessage(groupID, userId, messageId, status);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateInboxMessage caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public InboxMessageResponse GetInboxMessage(string sWSUserName, string sWSPassword, int userId, string messageId)
        {
            InboxMessageResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetInboxMessage(groupID, userId, messageId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetInboxMessage caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public InboxMessageResponse GetInboxMessages(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, List<eMessageCategory> messageCategorys, long CreatedAtGreaterThanOrEqual, long CreatedAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetInboxMessages(groupID, userId, pageSize, pageIndex, messageCategorys, CreatedAtGreaterThanOrEqual, CreatedAtLessThanOrEqual);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetInboxMessages caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public Dictionary<string, int> GetAmountOfSubscribersPerAnnouncement(string sWSUserName, string sWSPassword)
        {
            Dictionary<string, int> response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetAmountOfSubscribersPerAnnouncement(groupID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAmountOfSubscribersPerAnnouncement caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public Status UpdateAnnouncement(string sWSUserName, string sWSPassword, int announcementId, eTopicAutomaticIssueNotification topicAutomaticIssueNotification)
        {
            Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.UpdateAnnouncement(groupID, announcementId, topicAutomaticIssueNotification);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateAnnouncement caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public AnnouncementsResponse GetAnnouncement(string sWSUserName, string sWSPassword, int id)
        {
            AnnouncementsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetAnnouncement(groupID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAnnouncement caught an exception: GroupID: {0}, id: {1}, ex: {2}", groupID, id, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public AnnouncementsResponse GetAnnouncements(string sWSUserName, string sWSPassword, int pageSize, int pageIndex)
        {
            AnnouncementsResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.GetAnnouncements(groupID, pageSize, pageIndex);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAnnouncements caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        /// <summary>
        /// Deletes announcements (topics) older than partner configured date. 
        /// </summary>
        /// <returns></returns>
        public ApiObjects.Response.Status DeleteAnnouncementsOlderThan(string sWSUserName, string sWSPassword)
        {
            return Core.Notification.Module.DeleteAnnouncementsOlderThan();
        }

        /// <summary>
        /// Deletes reminder (topics) older than yesterday
        /// </summary>
        /// <returns></returns>
        public ApiObjects.Response.Status DeleteOldReminders(string sWSUserName, string sWSPassword)
        {
            return Core.Notification.Module.DeleteOldReminders();
        }

        public RegistryResponse RegisterPushAnnouncementParameters(string sWSUserName, string sWSPassword, long announcementId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            if (groupID > 0)
            {
                return Core.Notification.Module.RegisterPushAnnouncementParameters(groupID, announcementId, hash, ip);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public RegistryResponse RegisterPushSystemParameters(string sWSUserName, string sWSPassword, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            if (groupID > 0)
            {
                return Core.Notification.Module.RegisterPushSystemParameters(groupID, hash, ip);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public RegistryResponse RegisterPushReminderParameters(string sWSUserName, string sWSPassword, long reminderId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            if (groupID > 0)
            {
                return Core.Notification.Module.RegisterPushReminderParameters(groupID, reminderId, hash, ip);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public Status RemoveUsersNotificationData(string sWSUserName, string sWSPassword, List<string> userIds)
        {
            Status response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.RemoveUsersNotificationData(groupID, userIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveUsersNotificationData caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public RemindersResponse AddUserReminder(string sWSUserName, string sWSPassword, int userId, DbReminder dbReminder)
        {
            RemindersResponse response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);
            try
            {
                if (groupID > 0)
                {
                    return Core.Notification.Module.AddUserReminder(groupID, userId, dbReminder);
                }
                else
                {
                    log.Error("AddUserReminder failed groupId = 0 ");
                }
            }
            catch (Exception ex)
            {
                log.Error("AddUserReminder failed", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;

        }

        public ApiObjects.Response.Status DeleteUserReminder(string sWSUserName, string sWSPassword, int userId, long reminderId, ReminderType reminderType)
        {
            ApiObjects.Response.Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.DeleteUserReminder(groupID, userId, reminderId, reminderType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteUserReminder caught an exception: GroupID: {0}, reminderId: {1}, ex: {2}", groupID, reminderId, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public RemindersResponse GetUserReminders(string sWSUserName, string sWSPassword, int userId, string filter, int pageSize, int pageIndex, OrderObj orderObj)
        {
            RemindersResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.GetUserReminders(groupID, userId, filter, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserReminders caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public EngagementResponse AddEngagement(string wsUserName, string wsSPassword, Engagement engagement)
        {
            EngagementResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", wsUserName, wsSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.AddEngagement(groupID, engagement);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddEngagement caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public ApiObjects.Response.Status SetEngagementAdapterConfiguration(string wsUserName, string wsSPassword, int engagementAdapterId)
        {
            ApiObjects.Response.Status response = null;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", wsUserName, wsSPassword);

            try
            {
                if (groupID != 0)
                {
                    return Core.Notification.Module.SetEngagementAdapterConfiguration(groupID, engagementAdapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SetEngagementAdapterConfiguration caught an exception: GroupID: {0}, engagementAdapterId: {1}, ex: {2}", groupID, engagementAdapterId, ex);
                HttpContext.Current.Response.StatusCode = 404;
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public bool ReSendEngagement(string wsUserName, string wsSPassword, int engagementId)
        {
            bool response = false;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", wsUserName, wsSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.ReSendEngagement(groupID, engagementId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("ReSendEngagement caught an exception: GroupID: {0}, engagementAdapterId: {1}, ex: {2}", groupID, engagementId, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public bool SendEngagement(string wsUserName, string wsSPassword, int engagementId, int startTime)
        {
            bool response = false;
            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", wsUserName, wsSPassword); 

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.SendEngagement(groupID, engagementId, startTime);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendEngagement caught an exception: GroupID: {0}, engagementAdapterId: {1}, ex: {2}", groupID, engagementId, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        public RemindersResponse GetUserSeriesReminders(string sWSUserName, string sWSPassword, int userId, List<string> seriesIds, List<long> seasonNumbers, long epgChannelId,
            int pageSize, int pageIndex, OrderObj orderObj)
        {
            RemindersResponse response = null;

            int groupID = TVinciShared.WS_Utils.GetGroupID("notifications", sWSUserName, sWSPassword);

            try
            {
                if (groupID != 0)
                    return Core.Notification.Module.GetUserSeriesReminders(groupID, userId, seriesIds, seasonNumbers, epgChannelId, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserSeriesReminders caught an exception: GroupID: {0}, ex: {1}", groupID, ex);
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }
    }
}
