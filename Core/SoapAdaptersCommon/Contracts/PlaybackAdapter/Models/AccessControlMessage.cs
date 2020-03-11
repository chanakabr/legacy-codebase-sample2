using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class AccessControlMessage
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string Code { get; set; }
    }
}