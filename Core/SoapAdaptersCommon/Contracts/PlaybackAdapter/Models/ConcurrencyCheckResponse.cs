using System.Collections.Generic;
using System.Runtime.Serialization;
using PlaybackAdapter;

namespace SoapAdaptersCommon.Contracts.PlaybackAdapter.Models
{
    public class ConcurrencyCheckResponse
    {
        [DataMember]
        public bool AllowedToPlay { get; set; }
        [DataMember]
        public string CancelDeviceUdid { get; set; }
    }
}