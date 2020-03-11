using System.Runtime.Serialization;

namespace SSOAdapter.Models
{
    [DataContract]
    public enum eSSOUserResponseStatus
    {
        [EnumMember]
        OK = 0,
        [EnumMember]
        UserExists = 1,
        [EnumMember]
        UserDoesNotExist = 2,
        [EnumMember]
        WrongPasswordOrUserName = 3,
        [EnumMember]
        InsideLockTime = 4,
        [EnumMember]
        NotImplementedYet = 5,
        [EnumMember]
        UserNotActivated = 6,
        [EnumMember]
        UserAlreadyLoggedIn = 7,
        [EnumMember]
        UserDoubleLogIn = 8,
        [EnumMember]
        SessionLoggedOut = 9,
        [EnumMember]
        DeviceNotRegistered = 10,
        [EnumMember]
        ErrorOnSendingMail = 11,
        [EnumMember]
        UserEmailAlreadyExists = 12,
        [EnumMember]
        ErrorOnUpdatingUserType = 13,
        [EnumMember]
        UserTypeNotExist = 14,
        [EnumMember]
        UserNotMasterApproved = 15,
        [EnumMember]
        ErrorOnInitUser = 16,
        [EnumMember]
        ErrorOnSaveUser = 17,
        [EnumMember]
        UserNotIndDomain = 18,
        [EnumMember]
        TokenNotFound = 19,
        [EnumMember]
        UserAlreadyMasterApproved = 20,
        [EnumMember]
        UserWithNoDomain = 21,
        [EnumMember]
        LoginServerDown = 22,
        [EnumMember]
        UserSuspended = 23,
        [EnumMember]
        UserCreatedWithNoRole = 24,
        [EnumMember]
        ExternalIdAlreadyExists = 25,
        [EnumMember]
        ExternalError = 26,
        [EnumMember]
        InternalError = 999

    }
}