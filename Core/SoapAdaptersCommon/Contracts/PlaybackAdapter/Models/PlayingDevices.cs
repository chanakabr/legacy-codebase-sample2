using System.Runtime.Serialization;

namespace PlaybackAdapter
{
    public class PlayingDevice
    {
        [DataMember]
        public string Udid { get; set; }

        [DataMember]
        public string DeviceFamilyId { get; set; }
    }
}