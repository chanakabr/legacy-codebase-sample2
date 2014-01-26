using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public enum ResponseStatus
    {

        /// <remarks/>
        OK,

        /// <remarks/>
        UserExists,

        /// <remarks/>
        UserDoesNotExist,

        /// <remarks/>
        WrongPasswordOrUserName,

        /// <remarks/>
        InsideLockTime,

        /// <remarks/>
        NotImplementedYet,

        /// <remarks/>
        UserNotActivated,

        /// <remarks/>
        UserAllreadyLoggedIn,

        /// <remarks/>
        UserDoubleLogIn,

        /// <remarks/>
        SessionLoggedOut,

        /// <remarks/>
        DeviceNotRegistered,

        /// <remarks/>
        ErrorOnSendingMail,

        /// <remarks/>
        UserEmailAlreadyExists,

        /// <remarks/>
        ErrorOnUpdatingUserType,

        /// <remarks/>
        UserTypeNotExist,
    }

}
