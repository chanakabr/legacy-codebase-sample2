using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    [KnownType(typeof(BumperPlaybackPluginData))]
    public class PlaybackPluginData
    {
    }

    [DataContract]
    public class BumperPlaybackPluginData : PlaybackPluginData
    {
        [DataMember]
        public string URL { get; set; }

        [DataMember]
        public string StreamerType { get; set; }
    }
}