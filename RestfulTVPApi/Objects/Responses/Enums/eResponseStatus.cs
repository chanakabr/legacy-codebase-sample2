using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum eResponseStatus
    {
        OK,

        UserExists,

        UserDoesNotExist,

        WrongPasswordOrUserName,

        InsideLockTime,

        NotImplementedYet,

        UserNotActivated,

        UserAllreadyLoggedIn,

        UserDoubleLogIn,

        SessionLoggedOut,

        DeviceNotRegistered,

        ErrorOnSendingMail,

        UserEmailAlreadyExists,

        ErrorOnUpdatingUserType,

        UserTypeNotExist,

        UserNotMasterApproved,

        ErrorOnInitUser,

        ErrorOnSaveUser,
        
        UserNotIndDomain,

        TokenNotFound,
        
        UserAlreadyMasterApproved,

        UserWithNoDomain,
    }

}
