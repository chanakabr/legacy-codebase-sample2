using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Notification
{
    public class PushMessage
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Sound { get; set; }

        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public string Url { get; set; }
    }
}
