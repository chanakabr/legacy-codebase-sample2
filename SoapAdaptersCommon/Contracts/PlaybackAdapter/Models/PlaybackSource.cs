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
        public bool IsTokenized;
    }
}