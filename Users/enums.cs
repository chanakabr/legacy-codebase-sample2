using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Users
{
    public enum ResponseStatus
    {
        OK = 0,
        UserExists = 1,
        UserDoesNotExist = 2,
        WrongPasswordOrUserName = 3,
        InsideLockTime = 4,
        NotImplementedYet = 5,
        UserNotActivated = 6,
        UserAllreadyLoggedIn = 7,
        UserDoubleLogIn = 8,
        SessionLoggedOut = 9,
        DeviceNotRegistered = 10,
        ErrorOnSendingMail = 11,
        UserEmailAlreadyExists = 12,
        ErrorOnUpdatingUserType = 13,
        UserTypeNotExist = 14,
        UserNotMasterApproved = 15,
        ErrorOnInitUser = 16,
        ErrorOnSaveUser = 17,
        UserNotIndDomain = 18,
        TokenNotFound = 19,
        UserAlreadyMasterApproved = 20,
        UserWithNoDomain = 21,  
        InternalError = 999,
        LoginServerDown = 22,
        UserSuspended = 23,
        UserCreatedWithNoRole = 24
    }

    public enum UserAction
    {
        SignUp = 0,
        SignIn = 1,
        SignOut = 2
    }

    public enum UserState
    {
        Unknown = 0,
        Activated = 1,
        SingleSignIn = 2,
        DoubleSignIn = 3,
        LoggedOut = 4        
    }

    public enum UserActivationState
    {
        Error = -2,
        UserDoesNotExist = -1,
        Activated = 0,
        NotActivated = 1,
        NotActivatedByMaster = 2,
        UserRemovedFromDomain = 3,
        UserWIthNoDomain  = 4,
        UserSuspended = 5
    }

    [Serializable]
    public enum DeviceState
    {
        [EnumMember]
        UnKnown = 0,     // Initial device status
        [EnumMember]
        Error = 1,       // Error
        [EnumMember]
        NotExists = 2,   // Device not in devices table or is in table but status<>1
        [EnumMember]
        Pending = 3,
        [EnumMember]
        Activated = 4,   // Device in devices table and status=1 & active=1 (domains_devcies status not known);
        [EnumMember]
        UnActivated = 5, // Device in devices table and status=1 but active<>1
    }


    public enum DomainStatus
    {
        OK = 1,
        DomainAlreadyExists = 2,
        ExceededLimit = 3,
        DeviceTypeNotAllowed = 7,
        UnKnown = 4,
        Error = 5,
        DeviceNotInDomin = 6,
        MasterEmailAlreadyExists = 8,
        UserNotInDomain = 9,
        DomainNotExists = 10,
        HouseholdUserFailed = 11,
        DomainCreatedWithoutNPVRAccount = 12,
        DomainSuspended = 13,
        NoUsersInDomain = 14,
        UserExistsInOtherDomains = 15
    }

    [Serializable]
    public enum DomainRestriction
    {
        [EnumMember]
        Unrestricted = 0,
         [EnumMember]
        UserMasterRestricted = 1,
         [EnumMember]
        DeviceMasterRestricted = 2,
         [EnumMember]
        DeviceUserMasterRestricted = 3
    }


    public enum DomainResponseStatus
    {
        LimitationPeriod = 0,
        UnKnown = 1,
        Error = 2,
        DomainAlreadyExists = 3,
        ExceededLimit = 4,
        DeviceTypeNotAllowed = 5,
        DeviceNotInDomain = 6,
        DeviceNotExists = 7,
        DeviceAlreadyExists = 8,
        UserNotExistsInDomain = 9,
        OK = 10,
        ActionUserNotMaster = 11,
        UserNotAllowed = 12,
        ExceededUserLimit = 13,
        NoUsersInDomain = 14,
        UserExistsInOtherDomains = 15,
        DomainNotExists = 16,
        HouseholdUserFailed = 17,
        DeviceExistsInOtherDomains = 18,
        DomainNotInitialized = 19,
        RequestSent = 20,
        DeviceNotConfirmed = 21,
        RequestFailed = 22,
        InvalidUser = 23,
        ConcurrencyLimitation = 24,
        MediaConcurrencyLimitation = 25,
        DomainSuspended = 26,
        UserAlreadyInDomain = 27
    }

    public enum DeviceResponseStatus
    {
        UnKnown,
        Error,
        DuplicatePin,
        DeviceNotExists,
        OK,
        ExceededLimit
    }

    public enum UserGroupRuleResponseStatus
    {
        OK = 1,
        TokenExpired = 2,
        TokenNotExist = 3,
        Error = 4
    }

    public enum ListType
    {
        All = 0,
        Watch = 1,
        Purchase = 2,
        Library = 3
    }

    public enum ItemType
    {
        All = 0,
        Media = 2
    }

    public enum UserDomainType
    {
        Regular = 0,
        Master = 1,
        Household = 2
    }

    public enum ImplementationsModules
    {
        Users = 1,
        Domains,
    }

    public enum NetworkResponseStatus : byte
    {
        OK = 0,
        QuantityLimitation = 1,
        FrequencyLimitation = 2,
        NetworkExists = 3,
        NetworkDoesNotExist = 4,
        InvalidInput = 5,
        Error = 6
    }

    public enum ValidationType : byte
    {
        Concurrency = 0,
        Quantity = 1,
        Frequency = 2
    }

    public enum Code
    {
        Success = 0,
        Failure = 1
    }

    public enum DeviceRegistrationStatus
    {
        Unknown = 0,
        NotRegistered = 1,
        Registered = 2,
        RegisteredToAnotherDomain = 3
    }

    public enum FavoriteOrderBy
    {
        CreateDateAsc = 0,
        CreateDateDesc = 1
    }
}
