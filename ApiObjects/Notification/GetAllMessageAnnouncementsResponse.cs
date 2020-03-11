using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class GetAllMessageAnnouncementsResponse
    {
        public ApiObjects.Response.Status Status;
        public List<MessageAnnouncement> messageAnnouncements;
        public int totalCount;
    }
}
