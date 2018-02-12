using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using ApiObjects.Notification;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Notification.Adapters;
using QueueWrapper.Queues.QueueObjects;
using APILogic.AmazonSnsAdapter;
using APILogic.DmsService;

namespace Core.Notification
{
    public class MailAnnouncementsHelper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        public static List<string> GetAllAnnouncementExternalIdsForUser(int groupId, UserNotification userNotificationData)
        {
            List<string> result = new List<string>();

            // validate user enabled push notifications
            if (!NotificationSettings.IsUserMailEnabled(userNotificationData.Settings))
            {
                log.ErrorFormat("User mail notification is disabled. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);
                return result;
            }

            // Get list of announcements to subscribe (VOD)
            result.AddRange(GetUserAnnouncementsExternalIds(groupId, userNotificationData));

            // Get list of reminders to subscribe (EGP)
            result.AddRange(GetUserRemindersExternalIds(groupId, userNotificationData));

            // Get list of series reminders to subscribe (EPG)
            result.AddRange(GetUserSeriesRemindersExternalIds(groupId, userNotificationData));

            // Get list of series interests to subscribe
            result.AddRange(GetUserInterestsExternalIds(groupId, userNotificationData));

            return result;
        }

        private static List<string> GetUserInterestsExternalIds(int groupId, UserNotification userNotificationData)
        {
            List<string> result = new List<string>();

            // get notification list from user object
            List<long> interestIds = new List<long>();
            if (userNotificationData != null && userNotificationData.UserInterests != null)
                interestIds = userNotificationData.UserInterests.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't have any interests. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);

            // get reminders from DB
            var reminders = InterestDal.GetTopicInterestNotificationsByGroupId(groupId, interestIds);

            if (reminders == null || reminders.Count() == 0)
                log.ErrorFormat("Failed to fetch user interests from DB. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);
            else
                result = reminders.Select(r => r.MailExternalId).ToList();

            return result;
        }

        private static List<string> GetUserAnnouncementsExternalIds(int groupId, UserNotification userNotificationData)
        {
            List<string> result = new List<string>();

            // get notification list from user object
            List<long> notificationIds = new List<long>();
            if (userNotificationData != null && userNotificationData.Announcements != null)
                notificationIds = userNotificationData.Announcements.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't follow anything. PID: {0}, UID: {1}", groupId, userNotificationData.UserId);

            // get announcements
            List<DbAnnouncement> announcements = new List<DbAnnouncement>();
            NotificationCache.TryGetAnnouncements(groupId, ref announcements);
            if (announcements != null)
                announcements = announcements.Where(x => x.RecipientsType == eAnnouncementRecipientsType.All || notificationIds.Contains(x.ID)).ToList();

            // build announcements adapter object
            if (announcements == null || announcements.Count == 0)
                log.ErrorFormat("Failed to fetch announcements from DB. login announcement + announcements ID: {0}", JsonConvert.SerializeObject(notificationIds));
            else 
                result = announcements.Select(a => a.MailExternalId).ToList();

            return result;
        }

        private static List<string> GetUserRemindersExternalIds(int groupId, UserNotification userNotificationData)
        {
            List<string> result = new List<string>();

            // get notification list from user object
            List<long> remindersIds = new List<long>();
            if (userNotificationData != null && userNotificationData.Reminders != null)
                remindersIds = userNotificationData.Reminders.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't have any reminders. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);

            // get reminders from DB
            var reminders = NotificationDal.GetReminders(groupId, remindersIds);

            if (reminders == null || reminders.Count() == 0)
                log.ErrorFormat("Failed to fetch user reminders from DB. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);
            else
                result = reminders.Select(r => r.MailExternalId).ToList();

            return result;
        }

        private static List<string> GetUserSeriesRemindersExternalIds(int groupId, UserNotification userNotificationData)
        {
            List<string> result = new List<string>();

            // get notification list from user object
            List<long> remindersIds = new List<long>();
            if (userNotificationData != null && userNotificationData.SeriesReminders != null)
                remindersIds = userNotificationData.Reminders.Select(x => x.AnnouncementId).ToList();
            else
                log.DebugFormat("User doesn't have any series reminders. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);

            // get reminders from DB
            var reminders = NotificationDal.GetSeriesReminders(groupId, remindersIds);

            if (reminders == null || reminders.Count() == 0)
                log.ErrorFormat("Failed to fetch user series reminders from DB. groupId: {0}, userId: {1}", groupId, userNotificationData.UserId);
            else
                result = reminders.Select(r => r.MailExternalId).ToList();

            return result;
        }
    }
}
