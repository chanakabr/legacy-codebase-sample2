using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public enum eHouseholdSuspensionState
    {
        [EnumMember]
        NOT_SUSPENDED = 0,
        [EnumMember]
        SUSPENDED = 1
    }
}