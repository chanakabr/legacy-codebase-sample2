using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class FairPlayPlaybackPluginData : DrmPlaybackPluginData
    {
        [DataMember]
        public string Certificate { get; set; }
    }
}