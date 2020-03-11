using ApiObjects;
using ApiObjects.Notification;
using DAL;
using System.Collections.Generic;

namespace Core.Notification
{
    public class TestHelper
    {
        public static List<int> GetUsersFollowNotificationView(int groupId, int notificationId)
        {
            return NotificationDal.GetUsersFollowNotificationView(groupId, notificationId);
        }

        public static bool SetUserFollowNotificationData(int groupId, int userId, int notificationId)
        {
            return NotificationDal.SetUserFollowNotificationData(groupId, userId, notificationId);
        }

        public static bool RemoveUserFollowNotification(int groupId, int userId, int notificationId)
        {
            return NotificationDal.RemoveUserFollowNotification(groupId, userId, notificationId);
        }

        public static List<DbAnnouncement> GetAnnouncements(int groupId)
        {
            return NotificationDal.GetAnnouncements(groupId);
        }

        public static bool SetUserInboxMessage(int groupId, InboxMessage message)
        {
            return NotificationDal.SetUserInboxMessage(groupId, message,90);
        }

        public static bool SetSystemAnnouncementMessage(int groupId, InboxMessage message)
        {
            return NotificationDal.SetSystemAnnouncementMessage(groupId, message,90);
        }

        public static InboxMessage GetUserInboxMessage(int groupId, int userId, string messageId)
        {
            return NotificationDal.GetUserInboxMessage(groupId, userId, messageId);
        }

        public static bool UpdateInboxMessageState(int groupId, int userId, string messageId, eMessageState state)
        {
            return NotificationDal.UpdateInboxMessageState(groupId, userId, messageId, state);
        }

        public static List<InboxMessage> GetUserMessagesView(int groupId, long userId, bool onlyUnread, long fromDate)
        {
            return NotificationDal.GetUserMessagesView(groupId, userId, onlyUnread, fromDate);
        }

        public static List<string> GetSystemInboxMessagesView(int groupId, long fromDate)
        {
            return NotificationDal.GetSystemInboxMessagesView(groupId, fromDate);
        }
    }
}
