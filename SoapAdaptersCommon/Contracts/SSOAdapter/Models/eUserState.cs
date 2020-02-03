using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public enum eUserState
    {
        [EnumMember]
        ok,
        [EnumMember]
        user_with_no_household,
        [EnumMember]
        user_created_with_no_role,
        [EnumMember]
        user_not_activated
    }
}