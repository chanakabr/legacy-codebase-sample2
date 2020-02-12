using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public enum eSSOMethods
    {
        [EnumMember]
        PerSignIn = 0,
        [EnumMember]
        PostSignIn = 1,
        [EnumMember]
        PreGetUserData = 2,
        [EnumMember]
        PostGetUserData = 3,
        [EnumMember]
        PreSignOut = 4,
        [EnumMember]
        PostSignOut = 5
    }
}