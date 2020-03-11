using System.Runtime.Serialization;
using AdapaterCommon.Models;

namespace OSSAdapter.Models
{
    [DataContract]
    public class HouseholdPaymentGatewayResponse
    {
        [DataMember]
        public HouseholdBillingConfiguration Configuration { get; set; }

        [DataMember]
        public AdapterStatus Status { get; set; }

    }
}
