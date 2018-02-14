using Newtonsoft.Json;
using System.Collections.Generic;

namespace ApiObjects.Notification
{
    // key: user_notification_<GID>_<USER_ID>
    public class UserNotification
    {
        public int UserId { get; set; }

        public long CreateDateSec { get; set; }

        public List<UserDevice> devices { get; set; }

        public List<Announcement> Announcements { get; set; }

        public List<Announcement> Reminders { get; set; }

        public List<Announcement> SeriesReminders { get; set; }

        public List<Announcement> UserInterests { get; set; }

        public UserNotificationSettings Settings { get; set; }

        public UserData UserData { get; set; }

        public string SMSNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ulong cas { get; set; }

        public UserNotification(int userId)
        {
            this.UserId = userId;
            this.devices = new List<UserDevice>();
            this.Announcements = new List<Announcement>();
            this.Reminders = new List<Announcement>();
            this.SeriesReminders = new List<Announcement>();
            this.UserInterests = new List<Announcement>();
            this.Settings = new UserNotificationSettings();
            this.UserData = new UserData();
        }
    }
}
