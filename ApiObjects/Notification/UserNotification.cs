using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ApiObjects.Notification
{
    // key: user_notification_<GID>_<USER_ID>
    public class UserNotification
    {
        public UserNotification()
        {
            this.Userdevices = new List<UserDevice>();
            this.Announcements = new List<Announcement>();
        }
        public List<UserDevice> Userdevices { get; set; }
        public List<Announcement> Announcements { get; set; }

        public UserNotificationSettings Settings { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ulong cas{ get; set; }
    }
}
