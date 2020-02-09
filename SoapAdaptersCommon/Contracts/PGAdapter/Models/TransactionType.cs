using System.Runtime.Serialization;

namespace PGAdapter.Models
{
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/PGAdapterCommon.Models")]
    public enum eTransactionType
    {
        [EnumMember]
        PPV,

        [EnumMember]
        Subscription,

        [EnumMember]
        Collection
    }
}