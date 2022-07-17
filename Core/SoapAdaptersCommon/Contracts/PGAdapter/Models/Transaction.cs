using System.Collections.Generic;
using System.Runtime.Serialization;
using AdapaterCommon.Models;

namespace PGAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class Transaction
    {
        [DataMember]
        public int StateCode { get; set; }

        [DataMember]
        public int FailReasonCode { get; set; }

        [DataMember]
        public string PGStatus { get; set; }

        [DataMember]
        public string PGTransactionID { get; set; }

        [DataMember]
        public string PGMessage { get; set; }

        [DataMember]
        public string PGPayload { get; set; }

        [DataMember]
        public string PaymentMethod { get; set; }

        [DataMember]
        public string PaymentDetails { get; set; }

        [DataMember]
        public long StartDateSeconds { get; set; }

        [DataMember]
        public long EndDateSeconds { get; set; }

        [DataMember]
        public bool AutoRenewing { get; set; }

        [DataMember]
        public List<KeyValue> AdapterData { get; set; } 
        
        [DataMember]
        public int PendingInterval { get; set; }
    }
}