using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class RegistryParameter
    {
        public long AnnouncementId { get; set; }
        public string Key { get; set; }
        public string Url { get; set; }

        public RegistryParameter()
        {

        }

        public RegistryParameter(long id, string token, string url)
        {
            AnnouncementId = id;
            Key = token;
            Url = url;
        }
    }
}
