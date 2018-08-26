using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class Announcement
    {
        public long AnnouncementId { get; set; }
        public string AnnouncementName { get; set; }
        public long AddedDateSec { get; set; }
    }
}
