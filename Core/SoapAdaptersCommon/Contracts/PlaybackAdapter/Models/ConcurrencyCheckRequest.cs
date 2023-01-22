using System.Collections.Generic;
using System.Runtime.Serialization;
using PlaybackAdapter;

namespace SoapAdaptersCommon.Contracts.PlaybackAdapter.Models
{
    public class ConcurrencyCheckRequest
    {
        [DataMember]
        public List<PlayingDevice> PlayingDevices  { get; set; }
        [DataMember]
        public List<long> DeviceFamiliesIds  { get; set; }
        [DataMember]
        public Dictionary<string, string> UserDynamicData  { get; set; }
        [DataMember]
        public Dictionary<string, string> AdapterData  { get; set; }
    }
}