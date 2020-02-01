using System.Runtime.Serialization;

namespace OSSAdapter.Models
{
    [DataContract]
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