using System.Runtime.Serialization;

namespace PGAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public class TransactionProductDetails
    {
        [DataMember]
        public string productid { get; set; }

        [DataMember]
        public string productCode { get; set; }

        [DataMember]
        public string transactionId { get; set; }

        [DataMember]
        public int gracePeriodMinutes { get; set; }

        [DataMember]
        public double price { get; set; }
    }
}
