using System.Runtime.Serialization;

namespace PlaybackAdapter.Models
{
    [DataContract]
    public enum ePlaybackMethods
    {
        [EnumMember]
        GetPlaybackContext = 0,
        [EnumMember]
        GetPlaybackManifest = 1,
        [EnumMember]
        Concurrency = 2
    }
}