using System.Runtime.Serialization;

namespace AdapaterCommon.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class KeyValue
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Value { get; set; }
    }
}