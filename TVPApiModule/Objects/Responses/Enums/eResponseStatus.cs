using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public enum eResponseStatus
    {
        /// <remarks/>
        [EnumMember]
        OK,

        /// <remarks/>
        [EnumMember]
        UserExists,

        /// <remarks/>
        [EnumMember]
        UserDoesNotExist,

        /// <remarks/>
        [EnumMember]
        WrongPasswordOrUserName,

        /// <remarks/>
        [EnumMember]
        InsideLockTime,

        /// <remarks/>
        [EnumMember]
        NotImplementedYet,

        /// <remarks/>
        [EnumMember]
        UserNotActivated,

        /// <remarks/>
        [EnumMember]
        UserAllreadyLoggedIn,

        /// <remarks/>
        [EnumMember]
        UserDoubleLogIn,

        /// <remarks/>
        [EnumMember]
        SessionLoggedOut,

        /// <remarks/>
        [EnumMember]
        DeviceNotRegistered,

        /// <remarks/>
        [EnumMember]
        ErrorOnSendingMail,

        /// <remarks/>
        [EnumMember]
        UserEmailAlreadyExists,

        /// <remarks/>
        [EnumMember]
        ErrorOnUpdatingUserType,

        /// <remarks/>
        [EnumMember]
        UserTypeNotExist,

        /// <remarks/>
        [EnumMember]
        UserNotMasterApproved,

        /// <remarks/>
        [EnumMember]
        ErrorOnInitUser,

        /// <remarks/>
        [EnumMember]
        ErrorOnSaveUser,

        /// <remarks/>
        [EnumMember]
        UserNotIndDomain,

        /// <remarks/>
        [EnumMember]
        TokenNotFound,

        /// <remarks/>
        [EnumMember]
        UserAlreadyMasterApproved,

        /// <remarks/>
        [EnumMember]
        UserWithNoDomain,
    }

}
