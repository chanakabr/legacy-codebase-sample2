using System.Runtime.Serialization;
using AdapaterCommon.Models;

namespace PGAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class PaymentMethodResponse
    {
        [DataMember]
        public bool IsSuccess { get; set; }

        [DataMember]
        public string PGMessage { get; set; }

        [DataMember]
        public string PGStatus { get; set; }
        
        [DataMember]
        public AdapterStatus Status { get; set; }
    }
}
