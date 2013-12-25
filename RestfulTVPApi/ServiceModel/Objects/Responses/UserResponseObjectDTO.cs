using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.ServiceModel
{
    public class UserResponseObjectDTO
    {
        public ResponseStatusDTO m_RespStatus { get; set; }

        public UserDTO m_user { get; set; }

        public string m_userInstanceID { get; set; }
    }

    public enum ResponseStatusDTO
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

    public class UserDTO
    {
        public UserBasicDataDTO m_oBasicData { get; set; }

        public UserDynamicDataDTO m_oDynamicData { get; set; }

        public string m_sSiteGUID { get; set; }

        public int m_domianID { get; set; }

        public bool m_isDomainMaster { get; set; }

        public UserStateDTO m_eUserState { get; set; }

        public int m_nSSOOperatorID { get; set; }
    }

    public class UserBasicDataDTO
    {
        public string m_sUserName { get; set; }

        public string m_sFirstName { get; set; }

        public string m_sLastName { get; set; }

        public string m_sEmail { get; set; }

        public string m_sAddress { get; set; }

        public string m_sCity { get; set; }

        public StateDTO m_State { get; set; }

        public CountryDTO m_Country { get; set; }

        public string m_sZip { get; set; }

        public string m_sPhone { get; set; }

        public string m_sFacebookID { get; set; }

        public string m_sFacebookImage { get; set; }

        public bool m_bIsFacebookImagePermitted { get; set; }

        public string m_sAffiliateCode { get; set; }

        public string m_CoGuid { get; set; }

        public string m_ExternalToken { get; set; }

        public string m_sFacebookToken { get; set; }

        public UserTypeDTO m_UserType { get; set; }
    }

    public class UserDynamicDataDTO
    {
        public UserDynamicDataContainerDTO[] m_sUserData { get; set; }
    }

    public class UserDynamicDataContainerDTO
    {
        public string m_sDataType { get; set; }

        public string m_sValue { get; set; }
    }

    public class StateDTO
    {
        public int m_nObjecrtID { get; set; }

        public string m_sStateName { get; set; }

        public string m_sStateCode { get; set; }

        public CountryDTO m_Country { get; set; }
    }

    public class CountryDTO
    {
        public int m_nObjecrtID { get; set; }

        public string m_sCountryName { get; set; }

        public string m_sCountryCode { get; set; }
    }

    public enum UserStateDTO
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

    public class UserTypeDTO
    {
        public int? ID { get; set; }

        public string Description { get; set; }

        public bool IsDefault { get; set; }
    }
}