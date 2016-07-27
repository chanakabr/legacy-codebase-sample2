using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NotificationObj
{
    
    public class MessageAnnouncementResponse
    {
        public Status Status { get; set; }
        public MessageAnnouncement Announcement { get; set; }
    }

    public class AddMessageAnnouncementResponse : MessageAnnouncementResponse
    {
        public int Id { get; set; }
    }
}
