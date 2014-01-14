using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class UserResponseObject
    {
        public ResponseStatus m_RespStatus { get; set; }

        public User m_user { get; set; }

        public string m_userInstanceID { get; set; }
    }

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

    public class User
    {
        public UserBasicData m_oBasicData { get; set; }

        public UserDynamicData m_oDynamicData { get; set; }

        public string m_sSiteGUID { get; set; }

        public int m_domianID { get; set; }

        public bool m_isDomainMaster { get; set; }

        public UserState m_eUserState { get; set; }

        public int m_nSSOOperatorID { get; set; }
    }

    public class UserBasicData
    {
        public string m_sUserName { get; set; }

        public string m_sFirstName { get; set; }

        public string m_sLastName { get; set; }

        public string m_sEmail { get; set; }

        public string m_sAddress { get; set; }

        public string m_sCity { get; set; }

        public State m_State { get; set; }

        public Country m_Country { get; set; }

        public string m_sZip { get; set; }

        public string m_sPhone { get; set; }

        public string m_sFacebookID { get; set; }

        public string m_sFacebookImage { get; set; }

        public bool m_bIsFacebookImagePermitted { get; set; }

        public string m_sAffiliateCode { get; set; }

        public string m_CoGuid { get; set; }

        public string m_ExternalToken { get; set; }

        public string m_sFacebookToken { get; set; }

        public UserType m_UserType { get; set; }
    }

    public class UserDynamicData
    {
        public UserDynamicDataContainer[] m_sUserData { get; set; }
    }

    public class UserDynamicDataContainer
    {
        public string m_sDataType { get; set; }

        public string m_sValue { get; set; }
    }

    public class State
    {
        public int m_nObjecrtID { get; set; }

        public string m_sStateName { get; set; }

        public string m_sStateCode { get; set; }

        public Country m_Country { get; set; }
    }

    public class Country
    {
        public int m_nObjecrtID { get; set; }

        public string m_sCountryName { get; set; }

        public string m_sCountryCode { get; set; }
    }

    public enum UserState
    {

        /// <remarks/>
        Unknown,

        /// <remarks/>
        Activated,

        /// <remarks/>
        SingleSignIn,

        /// <remarks/>
        DoubleSignIn,

        /// <remarks/>
        LoggedOut,
    }

    public class UserType
    {
        public int? ID { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }
    }
}
