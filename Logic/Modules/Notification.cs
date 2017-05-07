using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using KLogMonitor;
using Core.Notification;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using KlogMonitorHelper;

namespace Core.Notification
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Add notification request to the db, using NotificationManager object.
        /// </summary>
        /// <param name="groupID"></param>
        /// <param name="userID"></param>
        /// <param name="triggerType"></param>
        /// <returns></returns>
        public static bool AddNotificationRequest(int nGroupID, string siteGuid, NotificationTriggerType triggerType, int nMediaID)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            log.Debug("AddNotificationRequest - " + string.Format("{0}", nGroupID));

            try
            {
                switch (triggerType)
                {
                    case NotificationTriggerType.PaymentFailure:
                    case NotificationTriggerType.Renewal:
                        ApiObjects.Notification.Notification objNotification = NotificationManager.Instance.GetNotification(nGroupID, triggerType);
                        if (objNotification != null) //Indication if notification exist for this group id
                        {
                            NotificationRequest request = NotificationManager.Instance.BuildNotificationRequest(objNotification, siteGuid);
                            NotificationManager.Instance.InsertNotificationRequest(request);
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    case NotificationTriggerType.FollowUpByTag:
                        log.Debug("AddNotificationRequest before GetNotifications");

                        // call new follow flow
                        FollowManager.AddFollowRequest(nGroupID, siteGuid, nMediaID);

                        // call old flow
                        List<FollowUpTagNotification> lNotifications = NotificationManager.Instance.GetNotifications(nGroupID, NotificationTriggerType.FollowUpByTag, nMediaID);

                        if (lNotifications != null) //Indication if notification(s) exist for this 
                        {
                            DataTable request = NotificationManager.Instance.BuildTagNotificationsRequest(lNotifications);
                            NotificationManager.Instance.InsertNotificationTagRequest(request);
                            return true;
                        }
                        else
                        {
                            log.Debug("AddNotificationRequest  GetNotifications count 0");
                            return false;
                        }

                    default:
                        return true;
                }
            }

            catch (Exception ex)
            {
                log.Error("AddNotificationRequest - GroupID=" + nGroupID.ToString() + ",UserID=" + siteGuid + ",Trigger type=" + triggerType.ToString(), ex);
                return false;
            }
        }

        public static bool HandleEpgEvent(int partnerId, List<ulong> programIds)
        {
            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, partnerId);

            log.DebugFormat("EpgEvent - Program ID: {0}, partner ID: {1}", string.Join(",", programIds.ToArray(), partnerId));
            try
            {
                return ReminderManager.HandleEpgEvent(partnerId, programIds);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("EpgEvent - GroupID:{0}, ex: {1}", partnerId, ex);
            }

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
        public static List<NotificationMessage> GetDeviceNotifications(int nGroupID, string siteGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, int? messageCount)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            List<NotificationMessage> resault = null;
            try
            {
                resault = NotificationManager.Instance.GetDeviceNotifications(nGroupID, siteGuid, sDeviceUDID, notificationType, viewStatus, messageCount);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.GetDeviceNotfication()", ex);
                resault = null;
            }
            return resault;
        }

        public static bool SetNotificationMessageViewStatus(int nGroupID, string siteGuid, long? notificationRequestID, long? notificationMessageID, NotificationMessageViewStatus viewStatus)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool res = false;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                res = NotificationManager.Instance.SetNotificationMessageViewStatus(siteGuid, notificationRequestID, notificationMessageID, viewStatus, nGroupID);
            }
            catch (Exception ex)
            {
                #region Logging
                log.Error("NotificationService.SetNotificationMessageViewStatus() - " + string.Format("Exception. Exception msg: {0} , Site Guid: {1} , NotificationRequestID: {2} , NotificationMessageID: {3} , ViewStatus: {4}", ex.Message, siteGuid != null ? siteGuid : "null", notificationRequestID.HasValue ? notificationRequestID.Value.ToString() : "null", notificationMessageID.HasValue ? notificationMessageID.Value.ToString() : "null", viewStatus.ToString()), ex);
                #endregion
            }
            return res;
        }

        public static bool SubscribeByTag(int nGroupID, string siteGuid, Dictionary<string, List<string>> tags)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool resault = false;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                resault = NotificationManager.Instance.FollowUpByTag(siteGuid, tags, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.FollowUpByTag()", ex);
                resault = false;
            }
            return resault;
        }

        public static bool UserSettings(int nGroupID, UserSettings userSettings)
        {
            bool resault = false;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                resault = NotificationManager.Instance.UserSettings(userSettings, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.FollowUpByTag()", ex);
                resault = false;
            }
            return resault;

        }

        public static bool UnsubscribeFollowUpByTag(int nGroupID, string siteGuid, Dictionary<string, List<string>> tags)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            bool resault = false;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                resault = NotificationManager.Instance.UnsubscribeFollowUpByTag(siteGuid, tags, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.UnsubscribeFollowUpByTag()", ex);
                resault = false;
            }
            return resault;
        }

        public static Dictionary<string, List<string>> GetUserStatusSubscriptions(int nGroupID, string siteGuid)
        {
            // add siteguid to logs/monitor
            MonitorLogsHelper.SetContext(Constants.USER_ID, siteGuid != null ? siteGuid : "null");

            Dictionary<string, List<string>> resault;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                resault = NotificationManager.Instance.GetUserStatusSubscriptions(siteGuid, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.GetUserStatusSubscriptions()", ex);
                resault = null;
            }
            return resault;
        }

        public static bool IsTagNotificationExists(int nGroupID, string tagType, string tagValue)
        {
            bool resault = false;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                resault = NotificationManager.Instance.IsTagNotificationExists(tagType, tagValue, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("NotificationService.SetNotificationStatus()", ex);
                resault = false;
            }
            return resault;
        }

        public static Dictionary<string, List<string>> GetAllNotifications(int nGroupID, NotificationTriggerType triggerType)
        {
            Dictionary<string, List<string>> resault;

            // get group ID
            MonitorLogsHelper.SetContext(Constants.GROUP_ID, nGroupID);

            try
            {
                switch (triggerType)
                {
                    case NotificationTriggerType.FollowUpByTag:
                        resault = NotificationManager.Instance.GetAllTagNotifications(nGroupID);
                        break;
                    default:
                        resault = null;
                        break;
                }

            }
            catch (Exception ex)
            {
                log.Error("NotificationService.GetUserStatusSubscriptions()", ex);
                resault = null;
            }
            return resault;
        }

        //Notifications settings//
        public static ApiObjects.Response.Status UpdateNotificationPartnerSettings(int nGroupID, ApiObjects.Notification.NotificationPartnerSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            try
            {
                response = Core.Notification.NotificationSettings.UpdateNotificationPartnerSettings(nGroupID, settings);
            }
            catch (Exception)
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public static ApiObjects.Notification.NotificationPartnerSettingsResponse GetNotificationPartnerSettings(int nGroupID)
        {
            ApiObjects.Notification.NotificationPartnerSettingsResponse response = null;
            try
            {
                response = NotificationCache.Instance().GetPartnerNotificationSettings(nGroupID);
            }
            catch (Exception)
            {
                response = new ApiObjects.Notification.NotificationPartnerSettingsResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public static ApiObjects.Response.Status UpdateNotificationSettings(int nGroupID, string userId, ApiObjects.Notification.UserNotificationSettings settings)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            try
            {
                response = Core.Notification.NotificationSettings.UpdateUserNotificationSettings(nGroupID, userId, settings);
            }
            catch (Exception)
            {
                response = new ApiObjects.Response.Status();
                response.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }
            return response;
        }

        public static ApiObjects.Notification.NotificationSettingsResponse GetNotificationSettings(int nGroupID, int userId)
        {
            ApiObjects.Notification.NotificationSettingsResponse response = null;
            try
            {
                response = Core.Notification.NotificationSettings.GetUserNotificationSettings(nGroupID, userId);
            }
            catch (Exception)
            {
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
        public static AddMessageAnnouncementResponse AddMessageAnnouncement(int nGroupID, MessageAnnouncement announcement)
        {
            AddMessageAnnouncementResponse response = null;


            try
            {
                response = AnnouncementManager.AddMessageAnnouncement(nGroupID, announcement);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddMessageAnnouncement caught an exception: GroupID: {0}, Announcement: {1}, ex: {2}", nGroupID, announcement, ex);
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
        public static bool SendMessageAnnouncement(int nGroupID, long startTime, int id)
        {
            try
            {
                return AnnouncementManager.SendMessageAnnouncement(nGroupID, startTime, id);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendMessageAnnouncement caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", nGroupID, id, ex);
                return false;
            }
        }

        /// <summary>
        /// Sends a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public static bool SendMessageReminder(int nGroupID, long startTime, int reminderId)
        {
            try
            {
                return ReminderManager.SendMessageReminder(nGroupID, startTime, reminderId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SendMessageReminder caught an exception: GroupID: {0}, reminder id: {1}, ex: {2}", nGroupID, reminderId, ex);
                return false;
            }
        }

        /// <summary>
        /// Updates a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public static MessageAnnouncementResponse UpdateMessageAnnouncement(int nGroupID, int announcementId, MessageAnnouncement announcement)
        {
            MessageAnnouncementResponse response = null;
            try
            {
                response = AnnouncementManager.UpdateMessageAnnouncement(nGroupID, announcementId, announcement);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateMessageAnnouncement caught an exception: GroupID: {0}, Announcement: {1}, ex: {2}", nGroupID, announcement, ex);
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
        public static ApiObjects.Response.Status DeleteAnnouncement(int nGroupID, long announcementId)
        {
            ApiObjects.Response.Status response = null;
            try
            {
                response = AnnouncementManager.DeleteAnnouncement(nGroupID, announcementId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAnnouncement caught an exception: GroupID: {0}, announcementId: {1}, ex: {2}", nGroupID, announcementId, ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        /// <summary>
        /// Updates a Message Announcement status.
        /// </summary>
        /// <returns></returns>
        public static ApiObjects.Response.Status UpdateMessageAnnouncementStatus(int nGroupID, long id, bool status)
        {
            ApiObjects.Response.Status response = null;
            try
            {
                response = AnnouncementManager.UpdateMessageSystemAnnouncementStatus(nGroupID, id, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateMessageAnnouncementStatus caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", nGroupID, id, ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        /// <summary>
        /// Deletes a Message Announcement.
        /// </summary>
        /// <returns></returns>
        public static ApiObjects.Response.Status DeleteMessageAnnouncement(int nGroupID, long id)
        {
            ApiObjects.Response.Status response = null;
            try
            {
                response = AnnouncementManager.DeleteMessageAnnouncement(nGroupID, id);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteMessageAnnouncement caught an exception: GroupID: {0}, Announcement id: {1}, ex: {2}", nGroupID, id, ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public static GetAllMessageAnnouncementsResponse GetAllMessageAnnouncements(int nGroupID, int pageSize, int pageIndex)
        {
            GetAllMessageAnnouncementsResponse response = null;
            try
            {
                response = AnnouncementManager.Get_AllMessageAnnouncements(nGroupID, pageSize, pageIndex);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAllMessageAnnouncements caught an exception: GroupID: {0}, ex: {2}", nGroupID, ex);
            }

            return response;
        }

        private delegate bool InitiateNotificationActionCaller(int nGroupID, eUserMessageAction userAction, int userId, string udid, string pushToken);

        public static async Task<bool> InitiateNotificationActionAsync(int nGroupID, eUserMessageAction userAction, int userId, string udid, string pushToken)
        {
            InitiateNotificationActionCaller caller = InitiateNotificationAction;
            return await Task.Run(() => InitiateNotificationAction(nGroupID, userAction, userId, udid, pushToken));
        }

        public static bool InitiateNotificationAction(int nGroupID, eUserMessageAction userAction, int userId, string udid, string pushToken)
        {
            bool result = false;

            try
            {
                result = UserMessageFlow.InitiatePushAction(nGroupID, userAction, userId, udid != null ? udid : string.Empty, pushToken);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while initiating push action. pushAction: {0}, userId:{1}, UDID: {2} token: {3}, ex: {4}",
                    userAction.ToString(),
                    userId,
                    udid,
                    pushToken,
                    ex);
            }
            return result;
        }

        public static ApiObjects.Response.Status CreateSystemAnnouncement(int nGroupID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            try
            {
                response = AnnouncementManager.CreateSystemAnnouncement(nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("CreateSystemAnnouncement failed", ex);
            }
            return response;

        }

        public static MessageTemplateResponse SetMessageTemplate(int nGroupID, MessageTemplate messageTemplate)
        {
            MessageTemplateResponse response = null;

            try
            {
                response = FollowManager.SetMessageTemplate(nGroupID, messageTemplate);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SetMessageTemplate caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
                response = new MessageTemplateResponse();
                response.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.Status.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
            }

            return response;
        }

        public static FollowResponse Follow(int nGroupID, int userId, FollowDataBase followData)
        {
            FollowResponse response = null;
            followData.GroupId = nGroupID;

            try
            {
                response = FollowManager.Follow(nGroupID, userId, followData);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Follow caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static GetUserFollowsResponse GetUserFollows(int nGroupID, int userId, int pageSize, int pageIndex, OrderDir order)
        {
            GetUserFollowsResponse response = null;
            try
            {
                response = FollowManager.Get_UserFollows(nGroupID, userId, pageSize, pageIndex, order);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserFollows caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static Status Unfollow(int nGroupID, int userId, FollowDataBase followData)
        {
            Status response = null;
            try
            {
                response = FollowManager.Unfollow(nGroupID, userId, followData);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unfollow caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static IdsResponse Get_FollowedAssetIdsFromAssets(int nGroupID, int userId, List<int> assets)
        {
            IdsResponse response = null;
            try
            {
                response = FollowManager.Get_FollowedAssetIdsFromAssets(nGroupID, userId, assets);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Get_FollowedAssetIdsFromAssets caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static MessageTemplateResponse GetMessageTemplate(int nGroupID, MessageTemplateType assetType)
        {
            MessageTemplateResponse response = null;
            try
            {
                response = FollowManager.GetMessageTemplate(nGroupID, assetType);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetMessageTemplate caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static IdListResponse GetUserFeeder(int nGroupID, int userId, int pageSize, int pageIndex, OrderObj orderObj)
        {
            IdListResponse response = null;

            try
            {
                response = FollowManager.GetUserFeeder(nGroupID, userId, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserFeeder caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static Status UpdateInboxMessage(int nGroupID, int userId, string messageId, eMessageState status)
        {
            Status response = null;
            try
            {
                response = MessageInboxManger.UpdateInboxMessage(nGroupID, userId, messageId, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateInboxMessage caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static InboxMessageResponse GetInboxMessage(int nGroupID, int userId, string messageId)
        {
            InboxMessageResponse response = null;
            try
            {
                response = MessageInboxManger.GetInboxMessage(nGroupID, userId, messageId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetInboxMessage caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static InboxMessageResponse GetInboxMessages(int nGroupID, int userId, int pageSize, int pageIndex, List<eMessageCategory> messageCategorys, long CreatedAtGreaterThanOrEqual, long CreatedAtLessThanOrEqual)
        {
            InboxMessageResponse response = null;
            try
            {
                response = MessageInboxManger.GetInboxMessages(nGroupID, userId, pageSize, pageIndex, messageCategorys, CreatedAtGreaterThanOrEqual, CreatedAtLessThanOrEqual);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetInboxMessages caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static Dictionary<string, int> GetAmountOfSubscribersPerAnnouncement(int nGroupID)
        {
            Dictionary<string, int> response = null;
            try
            {
                response = AnnouncementManager.GetAmountOfSubscribersPerAnnouncement(nGroupID);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAmountOfSubscribersPerAnnouncement caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static Status UpdateAnnouncement(int nGroupID, int announcementId, eTopicAutomaticIssueNotification topicAutomaticIssueNotification)
        {
            Status response = null;
            try
            {
                response = AnnouncementManager.UpdateAnnouncement(nGroupID, announcementId, topicAutomaticIssueNotification);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateAnnouncement caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static AnnouncementsResponse GetAnnouncement(int nGroupID, int id)
        {
            AnnouncementsResponse response = null;
            try
            {
                response = AnnouncementManager.GetAnnouncement(nGroupID, id);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAnnouncement caught an exception: GroupID: {0}, id: {1}, ex: {2}", nGroupID, id, ex);
            }

            return response;
        }

        public static AnnouncementsResponse GetAnnouncements(int nGroupID, int pageSize, int pageIndex)
        {
            AnnouncementsResponse response = null;
            try
            {
                response = AnnouncementManager.GetAnnouncements(nGroupID, pageSize, pageIndex);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetAnnouncements caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        /// <summary>
        /// Deletes announcements (topics) older than partner configured date. 
        /// </summary>
        /// <returns></returns>
        public static ApiObjects.Response.Status DeleteAnnouncementsOlderThan()
        {
            ApiObjects.Response.Status response = null;
            DateTime currentDate = DateTime.UtcNow;
            double nextIntervalSec = 0;
            bool createNextIteration = true;
            try
            {
                response = AnnouncementManager.DeleteAnnouncementsOlderThan(ref createNextIteration, ref nextIntervalSec);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteAnnouncementsOlderThan caught an exception: ex: {0}", ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            finally
            {
                if (createNextIteration)
                {
                    // creating next notification cleanup iteration
                    log.Debug("creating next notification cleanup iteration");

                    // validate next interval was updated (from default or from CB document)
                    if (nextIntervalSec == 0)
                        nextIntervalSec = AnnouncementManager.NOTIFICATION_CLEANUP_INTERVAL_SEC;

                    response = AnnouncementManager.CreateNextNotificationCleanupIteration(eSetupTask.NotificationSeriesCleanupIteration, currentDate.AddSeconds(nextIntervalSec));
                }
                else
                {
                    // not creating next notification cleanup iteration
                    log.Error("not creating another notification cleanup iteration");
                }
            }
            return response;
        }

        /// <summary>
        /// Deletes reminder (topics) older than yesterday
        /// </summary>
        /// <returns></returns>
        public static ApiObjects.Response.Status DeleteOldReminders()
        {
            ApiObjects.Response.Status response = null;
            DateTime currentDate = DateTime.UtcNow;
            double nextIntervalSec = 0;

            bool createNextIteration = true;
            try
            {
                response = ReminderManager.DeleteOldReminders(ref createNextIteration, ref nextIntervalSec);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteOldReminders caught an exception: ex: {0}", ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
            finally
            {
                if (createNextIteration)
                {
                    // creating next reminder cleanup iteration
                    log.Debug("creating next reminder cleanup iteration");

                    // validate next interval was updated (from default or from CB document)
                    if (nextIntervalSec == 0)
                        nextIntervalSec = ReminderManager.REMINDER_CLEANUP_INTERVAL_SEC;

                    response = AnnouncementManager.CreateNextNotificationCleanupIteration(eSetupTask.ReminderCleanupIteration, currentDate.AddSeconds(nextIntervalSec));
                }
                else
                {
                    // not creating next reminder cleanup iteration
                    log.Error("not creating another reminder cleanup iteration");
                }
            }
            return response;
        }

        public static RegistryResponse RegisterPushAnnouncementParameters(int nGroupID, long announcementId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            response = AnnouncementManager.RegisterPushAnnouncementParameters(nGroupID, announcementId, hash, ip);

            return response;
        }

        public static RegistryResponse RegisterPushSystemParameters(int nGroupID, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            response = AnnouncementManager.RegisterPushSystemParameters(nGroupID, hash, ip);

            return response;
        }

        public static RegistryResponse RegisterPushReminderParameters(int nGroupID, long reminderId, string hash, string ip)
        {
            RegistryResponse response = new RegistryResponse();

            response = ReminderManager.RegisterPushReminderParameters(nGroupID, reminderId, hash, ip);

            return response;
        }

        public static Status RemoveUsersNotificationData(int nGroupID, List<string> userIds)
        {
            Status response = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (userIds != null && userIds.Count != 0)
                {
                    bool result = false;
                    foreach (var userId in userIds)
                    {
                        result = UserMessageFlow.InitiatePushAction(nGroupID, eUserMessageAction.DeleteUser, int.Parse(userId), string.Empty, null);
                        if (!result)
                        {
                            log.ErrorFormat("Failed to remove notification data for userId = {0}", userId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RemoveUsersNotificationData caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static RemindersResponse AddUserReminder(int nGroupID, int userId, DbReminder dbReminder)
        {
            RemindersResponse response = null;
            try
            {
                response = ReminderManager.AddUserReminder(userId, dbReminder, null);
            }
            catch (Exception ex)
            {
                log.Error("AddUserReminder failed", ex);
            }
            return response;

        }

        public static ApiObjects.Response.Status DeleteUserReminder(int nGroupID, int userId, long reminderId, ReminderType reminderType)
        {
            ApiObjects.Response.Status response = null;

            try
            {
                switch (reminderType)
                {
                    case ReminderType.Single:
                        response = ReminderManager.DeleteUserReminder(nGroupID, userId, reminderId);
                        break;
                    case ReminderType.Series:
                        response = ReminderManager.DeleteUserSeriesReminder(nGroupID, userId, reminderId);
                        break;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteUserReminder caught an exception: GroupID: {0}, reminderId: {1}, ex: {2}", nGroupID, reminderId, ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public static ApiObjects.Response.Status DeleteUserSeriesReminder(int nGroupID, int userId, long reminderId)
        {
            ApiObjects.Response.Status response = null;

            try
            {
                response = ReminderManager.DeleteUserReminder(nGroupID, userId, reminderId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteUserReminder caught an exception: GroupID: {0}, reminderId: {1}, ex: {2}", nGroupID, reminderId, ex);
                response = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }

            return response;
        }

        public static RemindersResponse GetUserReminders(int nGroupID, int userId, string filter, int pageSize, int pageIndex, OrderObj orderObj)
        {
            RemindersResponse response = null;

            try
            {
                response = ReminderManager.GetUserReminders(nGroupID, userId, filter, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserReminders caught an exception: GroupID: {0}, ex: {1}", nGroupID, ex);
            }

            return response;
        }

        public static EngagementAdapterResponseList GetEngagementAdapters(int groupId)
        {
            return EngagementManager.GetEngagementAdapters(groupId);
        }

        public static EngagementAdapterResponse GetEngagementAdapter(int groupId, int engagementAdapterId)
        {
            return EngagementManager.GetEngagementAdapter(groupId, engagementAdapterId);
        }

        public static Status DeleteEngagementAdapter(int groupId, int engagementAdapterId)
        {
            return EngagementManager.DeleteEngagementAdapter(groupId, engagementAdapterId);
        }

        public static EngagementAdapterResponse InsertEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            return EngagementManager.InsertEngagementAdapter(groupId, engagementAdapter);
        }

        public static EngagementAdapterResponse SetEngagementAdapter(int groupId, EngagementAdapter engagementAdapter)
        {
            return EngagementManager.SetEngagementAdapter(groupId, engagementAdapter);
        }

        public static EngagementAdapterResponse GenerateEngagementSharedSecret(int groupId, int engagementAdapterId)
        {
            return EngagementManager.GenerateEngagementSharedSecret(groupId, engagementAdapterId);
        }

        public static EngagementAdapterSettingsResponse GetEngagementAdapterSettings(int groupId)
        {
            return EngagementManager.GetEngagementAdapterSettings(groupId);
        }

        public static Status DeleteEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> engagementAdapterSettingsList)
        {
            return EngagementManager.DeleteEngagementAdapterSettings(groupId, engagementAdapterId, engagementAdapterSettingsList);
        }

        public static Status InsertEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> engagementAdapterSettingsList)
        {
            return EngagementManager.InsertEngagementAdapterSettings(groupId, engagementAdapterId, engagementAdapterSettingsList);
        }

        public static Status SetEngagementAdapterSettings(int groupId, int engagementAdapterId, List<EngagementAdapterSettings> engagementAdapterSettingsList)
        {
            return EngagementManager.SetEngagementAdapterSettings(groupId, engagementAdapterId, engagementAdapterSettingsList);
        }

        public static EngagementResponse AddEngagement(int groupId, Engagement engagement)
        {
            return EngagementManager.AddEngagement(groupId, engagement);
        }

        public static Status DeleteEngagement(int groupId, int id)
        {
            return EngagementManager.DeleteEngagement(groupId, id);
        }

        public static EngagementResponse GetEngagement(int groupId, int id)
        {
            return EngagementManager.GetEngagement(groupId, id);
        }

        public static EngagementResponseList GetEngagements(int groupId, List<eEngagementType> convertedtypeIn, DateTime? sendTimeLessThanOrEqual)
        {
            return EngagementManager.GetEngagements(groupId, convertedtypeIn, sendTimeLessThanOrEqual);
        }

        public static bool SendEngagement(int partnerId, int engagementId, int startTime)
        {
            try
            {
                return EngagementManager.SendEngagement(partnerId, engagementId, startTime);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while trying to send engagement. Partner ID: {0}, Engagement ID: {1}, startTime: {2}, Ex: {3}",
                    partnerId,
                    engagementId,
                    TVinciShared.DateUtils.UnixTimeStampToDateTime(startTime),
                    ex);
                return false;
            }
        }

        public static bool SendEngagementBulk(int partnerId, int engagementId, int engagementBulkId, int startTime)
        {
            try
            {
                return EngagementManager.SendEngagementBulk(partnerId, engagementId, engagementBulkId, startTime);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while trying to send bulk engagement. Partner ID: {0}, engagement ID: {1}, engagement bulk ID: {2}, startTime: {3}, Ex: {4}",
                    partnerId,
                    engagementId,
                    engagementId,
                    TVinciShared.DateUtils.UnixTimeStampToDateTime(startTime),
                    ex);
                return false;
            }
        }

        public static ApiObjects.Response.Status SetEngagementAdapterConfiguration(int groupId, int engagementId)
        {
            return EngagementManager.SetEngagementAdapterConfiguration(groupId, engagementId);
        }

        public static bool ReSendEngagement(int partnerId, int engagementId)
        {
            try
            {
                return EngagementManager.ReSendEngagement(partnerId, engagementId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception while trying to resend an engagement. Partner ID: {0}, engagement ID: {1}, Ex: {2}",
                    partnerId,
                    engagementId,
                    ex);
                return false;
            }
        }

        public static RemindersResponse GetUserSeriesReminders(int groupId, int userId, List<string> seriesIds, List<long> seasonNumbers, long? epgChannelId,
            int pageSize, int pageIndex, OrderObj orderObj)
        {
            RemindersResponse response = null;

            try
            {
                response = ReminderManager.GetUserSeriesReminders(groupId, userId, seriesIds, seasonNumbers, epgChannelId, pageSize, pageIndex, orderObj);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserSeriesReminders caught an exception: GroupID: {0}, ex: {1}", groupId, ex);
            }

            return response;
        }
    }
}
