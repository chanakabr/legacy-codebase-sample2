using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class PushWebParameter
    {
        public long AnnouncementId { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }

        public PushWebParameter()
        {
        }

        public PushWebParameter(long id, string token, string url)
        {
            AnnouncementId = id;
            Key = token;
            Url = url;
        }
    }
}
