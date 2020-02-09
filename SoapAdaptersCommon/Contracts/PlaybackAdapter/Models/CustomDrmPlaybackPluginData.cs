using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class CustomDrmPlaybackPluginData : DrmPlaybackPluginData
    {
        [DataMember]
        public string Data { get; set; }
    }
}

