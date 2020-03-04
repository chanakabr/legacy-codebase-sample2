using AdapaterCommon.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class AdapterPlaybackContextOptions
    {
        [DataMember]

        public long AdapterId { get; set; }

        [DataMember]
        public int PartnerId { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Udid { get; set; }

        [DataMember]
        public string IP { get; set; }

        [DataMember]
        public long TimeStamp { get; set; }

        [DataMember]
        public string Signature { get; set; }

        [DataMember]
        public List<KeyValue> AdapterData { get; set; }

        [DataMember]
        public AdapterPlaybackContext PlaybackContext { get; set; }
    }
}