using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class AnnouncementsResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<DbAnnouncement> Announcements { get; set; }

        public int TotalCount { get; set; }
    }
}
