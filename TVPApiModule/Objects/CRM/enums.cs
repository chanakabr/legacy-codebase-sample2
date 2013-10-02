using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TVPApiModule.Objects.CRM
{
    public enum CRMResponseStatus
    {
        OK = 0,
        UnexpectedError = 1000,
        MethodNotFound = 1001,
        MethodNotIncludedInQueryString = 1002,
        CredentialsNotAuthorized = 1003,
        OnlySecureConnectionAllowed = 1004,
        UnknownGroup = 1005,
        SerializationError = 1006
    }

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
        ErrorOnSendingMail = 11
    }

    public enum UserState
    {
        Unknown = 0,
        Activated = 1,
        SingleSignIn = 2,
        DoubleSignIn = 3,
        LoggedOut = 4
    }
}
