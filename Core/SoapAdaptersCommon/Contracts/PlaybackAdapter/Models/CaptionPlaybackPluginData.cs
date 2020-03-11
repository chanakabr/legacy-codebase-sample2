using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class CaptionPlaybackPluginData
    {
        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public string Label { get; set; }

        [DataMember]
        public string Format { get; set; }
    }
}