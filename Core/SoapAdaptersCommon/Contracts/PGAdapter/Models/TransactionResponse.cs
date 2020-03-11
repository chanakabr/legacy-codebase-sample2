using System.Runtime.Serialization;
using AdapaterCommon.Models;

namespace PGAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class TransactionResponse
    {
        [DataMember]
        public Transaction Transaction { get; set; }

        [DataMember]
        public AdapterStatus Status { get; set; }
    }
}