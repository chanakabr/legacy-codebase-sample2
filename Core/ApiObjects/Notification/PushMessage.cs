using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ApiObjects.Notification
{
    [DataContract]
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

        [DataMember]
        public string Udid { get; set; }

        [DataMember]
        public List<PushChannel> PushChannels { get; set; }
    }

    public enum PushChannel
    {
        Push,
        Iot
    }
}