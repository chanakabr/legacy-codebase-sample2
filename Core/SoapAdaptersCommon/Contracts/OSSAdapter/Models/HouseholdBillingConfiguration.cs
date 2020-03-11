using System.Runtime.Serialization;

namespace OSSAdapter.Models
{
    [DataContract]
    public class HouseholdBillingConfiguration
    {
        [DataMember]
        public int StateCode { get; set; }

        [DataMember]
        public string ChargeId { get; set; }

        [DataMember]
        public string PaymentGatewayId { get; set; }

    }
}
