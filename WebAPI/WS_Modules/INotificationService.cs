using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using Core.Notification;
using ScheduledTasks;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;

namespace WS_Notification
{
    // NOTE: You can use the "Rename" command on the "Refractor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface INotificationService
    {
        [OperationContract]
        bool AddNotificationRequest(string sWSUserName, string sWSPassword, string siteGuid, NotificationTriggerType triggerType, int nMediaID);

        [OperationContract]
        List<NotificationMessage> GetDeviceNotifications(string sWSUserName, string sWSPassword, string siteGuid, string sDeviceUDID, NotificationMessageType notificationType, NotificationMessageViewStatus viewStatus, int? messageCount);

        [OperationContract]
        bool SetNotificationMessageViewStatus(string sWSUserName, string sWSPassword, string siteGuid, long? notificationRequestID, long? notificationMessageID, NotificationMessageViewStatus viewStatus);

        [OperationContract]
        Dictionary<string, List<string>> GetAllNotifications(string sWSUserName, string sWSPassword, NotificationTriggerType triggerType);

        [OperationContract]
        bool SubscribeByTag(string sWSUserName, string sWSPassword, string siteGuid, Dictionary<string, List<string>> tags);//long notificationID);

        [OperationContract]
        bool UserSettings(string sWSUserName, string sWSPassword, UserSettings userSettings);

        [OperationContract]
        bool UnsubscribeFollowUpByTag(string sWSUserName, string sWSPassword, string siteGuid, Dictionary<string, List<string>> tags);

        [OperationContract]
        Dictionary<string, List<string>> GetUserStatusSubscriptions(string sWSUserName, string sWSPassword, string siteGuid);

        [OperationContract]
        bool IsTagNotificationExists(string sWSUserName, string sWSPassword, string tagType, string tagValue);

        [OperationContract]
        ApiObjects.Response.Status UpdateNotificationPartnerSettings(string sWSUserName, string sWSPassword, ApiObjects.Notification.NotificationPartnerSettings settings);

        [OperationContract]
        ApiObjects.Notification.NotificationPartnerSettingsResponse GetNotificationPartnerSettings(string sWSUserName, string sWSPassword);

        [OperationContract]
        ApiObjects.Response.Status UpdateNotificationSettings(string sWSUserName, string sWSPassword, string userId, ApiObjects.Notification.UserNotificationSettings settings);

        [OperationContract]
        ApiObjects.Notification.NotificationSettingsResponse GetNotificationSettings(string sWSUserName, string sWSPassword, int userId);

        [OperationContract]
        [ServiceKnownType(typeof(MessageAnnouncement))]
        AddMessageAnnouncementResponse AddMessageAnnouncement(string sWSUserName, string sWSPassword, MessageAnnouncement announcement);

        [OperationContract]
        bool SendMessageAnnouncement(string sWSUserName, string sWSPassword, long startTime, int id);

        [OperationContract]
        MessageAnnouncementResponse UpdateMessageAnnouncement(string sWSUserName, string sWSPassword, int announcementId, MessageAnnouncement announcement);

        [OperationContract]
        ApiObjects.Response.Status UpdateMessageAnnouncementStatus(string sWSUserName, string sWSPassword, long id, bool status);

        [OperationContract]
        ApiObjects.Response.Status DeleteMessageAnnouncement(string sWSUserName, string sWSPassword, long id);

        [OperationContract]
        bool InitiateNotificationAction(string sWSUserName, string sWSPassword, eUserMessageAction userAction, int userId, string udid, string pushToken);

        [OperationContract]
        ApiObjects.Response.Status CreateSystemAnnouncement(string sWSUserName, string sWSPassword);

        [OperationContract]
        GetAllMessageAnnouncementsResponse GetAllMessageAnnouncements(string sWSUserName, string sWSPassword, int pageSize, int pageIndex);

        [OperationContract]
        MessageTemplateResponse SetMessageTemplate(string sWSUserName, string sWSPassword, MessageTemplate followTemplate);

        [OperationContract]
        MessageTemplateResponse GetMessageTemplate(string sWSUserName, string sWSPassword, MessageTemplateType assetTypes);

        [OperationContract]
        GetUserFollowsResponse GetUserFollows(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, OrderDir order);

        [OperationContract]
        [ServiceKnownType(typeof(FollowDataTvSeries))]
        ApiObjects.Response.Status Unfollow(string sWSUserName, string sWSPassword, int userId, FollowDataBase followData);

        [OperationContract]
        [ServiceKnownType(typeof(FollowDataTvSeries))]
        FollowResponse Follow(string sWSUserName, string sWSPassword, int userId, FollowDataBase followData);

        [OperationContract]
        IdsResponse Get_FollowedAssetIdsFromAssets(string sWSUserName, string sWSPassword, int groupId, int userId, List<int> assets);

        [OperationContract]
        IdListResponse GetUserFeeder(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, OrderObj orderObj);

        [OperationContract]
        ApiObjects.Response.Status UpdateInboxMessage(string sWSUserName, string sWSPassword, int userId, string messageId, eMessageState status);

        [OperationContract]
        InboxMessageResponse GetInboxMessage(string sWSUserName, string sWSPassword, int userId, string messageId);

        [OperationContract]
        InboxMessageResponse GetInboxMessages(string sWSUserName, string sWSPassword, int userId, int pageSize, int pageIndex, List<eMessageCategory> messageCategorys, long CreatedAtGreaterThanOrEqual, long CreatedAtLessThanOrEqual);

        [OperationContract]
        ApiObjects.Response.Status DeleteAnnouncement(string sWSUserName, string sWSPassword, long announcementId);

        [OperationContract]
        Dictionary<string, int> GetAmountOfSubscribersPerAnnouncement(string sWSUserName, string sWSPassword);

        [OperationContract]
        ApiObjects.Response.Status UpdateAnnouncement(string sWSUserName, string sWSPassword, int announcementId, eTopicAutomaticIssueNotification topicAutomaticIssueNotification);

        [OperationContract]
        AnnouncementsResponse GetAnnouncement(string sWSUserName, string sWSPassword, int id);

        [OperationContract]
        AnnouncementsResponse GetAnnouncements(string sWSUserName, string sWSPassword, int pageSize, int pageIndex);

        [OperationContract]
        ApiObjects.Response.Status DeleteAnnouncementsOlderThan(string sWSUserName, string sWSPassword);

        [OperationContract]
        RegistryResponse RegisterPushAnnouncementParameters(string sWSUserName, string sWSPassword, long announcementId, string hash, string ip);

        [OperationContract]
        RegistryResponse RegisterPushSystemParameters(string sWSUserName, string sWSPassword, string hash, string ip);

        [OperationContract]
        ApiObjects.Response.Status RemoveUsersNotificationData(string sWSUserName, string sWSPassword, List<string> userIds);

        [OperationContract]
        bool HandleEpgEvent(int partnerId, List<ulong> programIds);

        [OperationContract]
        RegistryResponse RegisterPushReminderParameters(string sWSUserName, string sWSPassword, long reminderId, string hash, string ip);

        [OperationContract]
        RemindersResponse AddUserReminder(string sWSUserName, string sWSPassword, int userId, DbReminder dbReminder);

        [OperationContract]
        ApiObjects.Response.Status DeleteOldReminders(string sWSUserName, string sWSPassword);

        [OperationContract]
        ApiObjects.Response.Status DeleteUserReminder(string sWSUserName, string sWSPassword, int userId, long reminderId);

        [OperationContract]
        bool SendMessageReminder(string sWSUserName, string sWSPassword, long startTime, int reminderId);

        [OperationContract]
        RemindersResponse GetUserReminders(string sWSUserName, string sWSPassword, int userId, string filter, int pageSize, int pageIndex, OrderObj orderObj);

        [OperationContract]
        EngagementResponse AddEngagement(string wsUserName, string wsSPassword, Engagement engagement);
        
        [OperationContract]
        ApiObjects.Response.Status SetEngagementAdapterConfiguration(string wsUserName, string wsSPassword, int engagementAdapterId);
    }
}
