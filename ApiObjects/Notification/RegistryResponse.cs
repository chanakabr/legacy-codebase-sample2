using ApiObjects.Response;
using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class RegistryResponse
    {
        public Status Status { get; set; }
        public long AnnouncementId { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }
    }
}
