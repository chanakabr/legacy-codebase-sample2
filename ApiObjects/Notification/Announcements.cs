using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class Announcement
    {
        public string AnnouncementId { get; set; }
        public string AnnouncementName { get; set; }
        public eAnnouncementRecipientsType RecipientType { get; set; }
        public string AnnouncementExternalId { get; set; }
    }
}
