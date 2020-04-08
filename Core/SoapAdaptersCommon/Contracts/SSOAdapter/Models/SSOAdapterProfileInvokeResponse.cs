using AdapaterCommon.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public partial class SSOAdapterProfileInvokeResponse
    {
        [DataMember]
        public AdapterStatusCode AdapterStatus { get; set; }

        [DataMember]
        public SSOResponseStatus SSOResponseStatus { get; set; }

        [DataMember]
        public List<KeyValue> Response { get; set; }
    }
}
