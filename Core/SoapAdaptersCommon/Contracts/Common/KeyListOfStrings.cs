using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AdapaterCommon.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class KeyListOfStrings
    {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public List<string> Value { get; set; }
    }
}