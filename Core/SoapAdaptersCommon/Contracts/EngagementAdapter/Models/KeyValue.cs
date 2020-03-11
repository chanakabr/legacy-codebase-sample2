using System.Runtime.Serialization;

namespace EngagementAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/EngagementAdapterCommon.Models")]
    public class KeyValue
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}