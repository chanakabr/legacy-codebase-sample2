using System.Collections.Generic;
using System.Runtime.Serialization;
using AdapaterCommon.Models;
using PlaybackAdapter.Models;
using SSOAdapter.Models;

namespace PlaybackAdapter
{
    [DataContract]
    public class PlaybackImplementationsResponse
    {
        [DataMember]
        public AdapterStatus Status { get; set; }
        [DataMember]
        public IEnumerable<int> ImplementedMethods { get; set; }
    }
}