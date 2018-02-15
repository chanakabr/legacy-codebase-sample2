using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ApiObjects.Notification
{
    //  KEY: device_data_<GID>_<UDID>
    public class DeviceNotificationData : NotificationData
    {
        public DeviceNotificationData(string udid) : base()
        {
            this.Udid = Udid;
        }

        public string Udid { get; set; }
        public bool IsLoggedIn { get; set; }
    }
}
