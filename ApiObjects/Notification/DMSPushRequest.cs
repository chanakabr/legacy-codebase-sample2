using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Notification
{
    public class DMSPushRequest
    {
        public string udid { get; set; }
        public string app_name { get; set; }
        public long message_box_id { get; set; }
        public string link { get; set; }
        public string collapseKey { get; set; }
        public bool delayWhileIdle { get; set; }
        public long timeToLiveSeconds { get; set; }
        public int badge { get; set; }
        public string messageBody { get; set; }
        public string launchImageFile { get; set; }
        public string soundFile { get; set; }
        public bool hideActionButton { get; set; }
    }
}
