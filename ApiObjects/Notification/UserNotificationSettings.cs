using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class UserNotificationSettings
    {
        public bool? EnablePush { get; set; }

        public bool? EnableInbox { get; set; }

        public bool? EnableMail { get; set; }

        public UserFollowSettings FollowSettings { get; set; }

        public UserNotificationSettings()
        {
            EnableInbox = true;
            EnableMail = true;
            EnablePush = true;
            FollowSettings = new UserFollowSettings();
        }
    }
}
