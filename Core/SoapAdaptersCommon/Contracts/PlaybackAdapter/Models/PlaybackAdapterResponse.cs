using AdapaterCommon.Models;
using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    [DataContract]
    public class PlaybackAdapterResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }
        [DataMember]
        public AdapterPlaybackContext PlaybackContext { get; set; }
    }
}