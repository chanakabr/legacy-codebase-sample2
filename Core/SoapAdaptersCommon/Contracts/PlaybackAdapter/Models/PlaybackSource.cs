using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class PlaybackSource : MediaFile
    {
        [DataMember]
        public string Format { get; set; }

        [DataMember]
        public string Protocols { get; set; }

        [DataMember]
        public List<DrmPlaybackPluginData> Drm { get; set; }       

        [DataMember]
        public bool IsTokenized { get; set; }

        [DataMember]
        public int? BusinessModuleId { get; set; }

        [DataMember]
        public TransactionType? BusinessModuleType { get; set; }


        public enum TransactionType
        {
            [EnumMember]
            PPV,

            [EnumMember]
            Subscription,

            [EnumMember]
            Collection
        }
    }
}