using ApiLogic.Users.Managers;
using ApiLogic.Users.Security;
using ApiObjects;
using ApiObjects.Response;
using AuthenticationGrpcClientWrapper;
using CachingProvider.LayeredCache;
using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using Phx.Lib.Log;
using ApiObjects.CanaryDeployment.Microservices;
using CanaryDeploymentManager;
using TVinciShared;

namespace Core.Users
{
    public class UserBasicData : ICloneable
    {
        [System.Xml.Serialization.XmlIgnore]
        public string m_sPassword;
        [System.Xml.Serialization.XmlIgnore]
        public string m_sSalt;

        public string m_sUserName;
        public string m_sFirstName;
        public string m_sLastName;
        public string m_sEmail;
        public string m_sAddress;
        public string m_sCity;
        public State m_State;
        public Country m_Country;
        public string m_sZip;
        public string m_sPhone;
        public string m_sFacebookID;
        public string m_sFacebookImage;
        public bool m_bIsFacebookImagePermitted;
        public string m_sAffiliateCode;
        public string m_CoGuid;
        public string m_ExternalToken;
        public string m_sFacebookToken;
        public string m_sTwitterToken;
        public string m_sTwitterTokenSecret;
        public UserType m_UserType;
        public List<long> RoleIds;
        public DateTime CreateDate;
        public DateTime UpdateDate;
        public DateTime LastLoginDate;
        public int FailedLoginCount;
        
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public UserBasicData()
        {
            m_sPassword = string.Empty;
            m_sSalt = string.Empty;
            m_sUserName = string.Empty;
            m_sFirstName = string.Empty;
            m_sLastName = string.Empty;
            m_sEmail = string.Empty;
            m_sAddress = string.Empty;
            m_sCity = string.Empty;
            m_State = null;
            m_Country = null;
            m_sZip = string.Empty;
            m_sPhone = string.Empty;
            m_sFacebookID = string.Empty;
            m_sFacebookImage = string.Empty;
            m_bIsFacebookImagePermitted = false;
            m_sAffiliateCode = string.Empty;
            m_CoGuid = string.Empty;
            m_ExternalToken = string.Empty;
            m_sFacebookToken = string.Empty;
            m_sTwitterToken = string.Empty;
            m_sTwitterTokenSecret = string.Empty;
            //m_UserType = null;
            RoleIds = new List<long>();
            CreateDate = DateTime.MinValue;
            UpdateDate = DateTime.MinValue;
            LastLoginDate = DateTime.MinValue;
            FailedLoginCount = 0;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID)
        {
            bool res = false;
            DataTable dtUserBasicData = UsersDal.GetUserBasicData(nUserID, nGroupID);

            if (dtUserBasicData != null && dtUserBasicData.Rows != null && dtUserBasicData.Rows.Count == 0)
            {
                return false;
            }

            DataRow drUserBasicData = dtUserBasicData.Rows[0];

            string sUserName = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "USERNAME");
            sUserName = UserDataEncryptor.Instance().DecryptUsername(nGroupID, sUserName);

            string sPass = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "PASSWORD");
            string sSalt = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "SALT");
            string sFirstName = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "FIRST_NAME");
            string sLastName = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "LAST_NAME");
            string sEmail = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "EMAIL_ADD");
            string sAddress = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "ADDRESS");
            string sAffiliate = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "REG_AFF");
            string sCity = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "CITY");
            Int32 nStateID = ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "STATE_ID");
            Int32 nCountryID = ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "COUNTRY_ID");
            string sZip = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "ZIP");
            string sPhone = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "PHONE");
            string sFacebookID = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "FACEBOOK_ID");
            string sFacebookImage = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "FACEBOOK_IMAGE");
            bool bFacebookImagePermitted = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "FACEBOOK_IMAGE_PERMITTED"));
            string sCoGuid = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "CoGuid");
            string sExternalToken = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "externaltoken");
            string sFacebookToken = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "fb_token");
            string sUserType = ODBCWrapper.Utils.GetSafeStr(drUserBasicData, "user_type_desc");
            bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "is_default"));
            DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(drUserBasicData, "CREATE_DATE");
            DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(drUserBasicData, "UPDATE_DATE");

            // will be populated by MS or from db data below
            int failedLoginCount = 0;
            DateTime lastLoginDate = DateTime.UtcNow;

            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(nGroupID, CanaryDeploymentDataOwnershipEnum.AuthenticationUserLoginHistory))
            {
                try
                {
                    var authClient = AuthenticationClient.GetClientFromTCM();
                    var failHistory = authClient.GetUserLoginHistory(nGroupID, nUserID);
                    
                    if (failHistory != null)
                    {
                        failedLoginCount = failHistory.ConsecutiveFailedLoginCount;
                        lastLoginDate = DateUtils.UtcUnixTimestampSecondsToDateTime(failHistory.LastLoginSuccessDate);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"Failed to GetUserLoginHistory from auth client. UserID: {nUserID}, GroupID : {nGroupID}",e);
                }
            }
            else
            {
                failedLoginCount = ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "FAIL_COUNT");
                lastLoginDate = ODBCWrapper.Utils.GetDateSafeVal(drUserBasicData, "LAST_LOGIN_DATE");
            }


            int? nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(drUserBasicData, "user_type_id");
            if (nUserTypeID.HasValue && nUserTypeID.Value == 0)
            {
                nUserTypeID = null;
            }

            UserType userType = new UserType(nUserTypeID, sUserType, isDefault);

            res = Initialize(sUserName, sPass, sSalt, sFirstName, sLastName, sEmail, sAddress, sCity, nStateID, nCountryID, sZip, sPhone, sFacebookID,
                             bFacebookImagePermitted, sFacebookImage, sAffiliate, sFacebookToken, sCoGuid, sExternalToken, userType, nUserID, nGroupID,
                             createDate, updateDate, lastLoginDate, failedLoginCount);

            return res;
        }

        public bool Initialize(string sUserName, string sPassword, string sSalt, string sFirstName, string sLastName, string sEmail, string sAddress, string sCity,
                               int nStateID, Int32 nCountryID, string sZip, string sPhone, string sFacebookID, bool bIsFacebookImagePermitted, string sFacebookImageURL,
                               string sAffiliate, string sFacebookToken, string sCoGuid, string sExternalToken, UserType userType, Int32 userId, Int32 groupId,
                               DateTime createDate, DateTime updateDate, DateTime lastLoginDate, int failedLoginCount)
        {
            bool res = true;

            m_sPassword = sPassword;
            m_sSalt = sSalt;
            m_sUserName = sUserName;
            m_sFirstName = sFirstName;
            m_sLastName = sLastName;
            m_sEmail = sEmail;
            m_sAddress = sAddress;
            m_sCity = sCity;
            m_sZip = sZip;
            m_sPhone = sPhone;
            m_sFacebookID = sFacebookID;
            m_sFacebookImage = sFacebookImageURL;
            m_bIsFacebookImagePermitted = bIsFacebookImagePermitted;
            m_sAffiliateCode = String.IsNullOrEmpty(sAffiliate) ? "" : sAffiliate;
            m_CoGuid = sCoGuid;
            m_ExternalToken = sExternalToken;
            m_sFacebookToken = sFacebookToken;
            //m_sTwitterToken = string.Empty;
            //m_sTwitterTokenSecret = string.Empty;
            m_UserType = userType;
            CreateDate = createDate;
            UpdateDate = updateDate;
            LastLoginDate = lastLoginDate;
            FailedLoginCount = failedLoginCount;

            if (nStateID != 0)
            {
                m_State = new State();
                res = m_State.Initialize(nStateID);
            }

            if (nCountryID != 0)
            {
                m_Country = new Country();
                bool res2 = m_Country.Initialize(nCountryID);
                res = res && res2;
            }

            res &= SetRoleIds(userId.ToString(), groupId);

            return res;
        }

        // TODO remove me - NOT USED
        public bool Initialize(string sXML, string userId, int groupId)
        {
            XmlDocument theDoc = new XmlDocument();

            try
            {
                theDoc.LoadXml(sXML);
            }
            catch
            {
                return false;
            }

            bool res = false;
            XmlNode t = theDoc.FirstChild;

            m_sFirstName = WS_Utils.GetNodeValue(ref t, "firstname");
            m_sAffiliateCode = WS_Utils.GetNodeValue(ref t, "affiliate");
            m_sLastName = WS_Utils.GetNodeValue(ref t, "lastname");
            m_sUserName = WS_Utils.GetNodeValue(ref t, "username");
            m_sFacebookID = WS_Utils.GetNodeValue(ref t, "facebookid");
            m_sZip = WS_Utils.GetNodeValue(ref t, "zip");
            m_sPhone = WS_Utils.GetNodeValue(ref t, "phone");
            m_sFacebookImage = WS_Utils.GetNodeValue(ref t, "facebookimage");
            m_sPassword = WS_Utils.GetNodeValue(ref t, "password");
            m_sSalt = WS_Utils.GetNodeValue(ref t, "salt");
            m_sEmail = WS_Utils.GetNodeValue(ref t, "email");
            m_sAddress = WS_Utils.GetNodeValue(ref t, "address");
            m_sCity = WS_Utils.GetNodeValue(ref t, "city");

            string sbIsFacebookImagePermitted = WS_Utils.GetNodeValue(ref t, "facebookimagepermitted").ToLower().Trim();
            m_bIsFacebookImagePermitted = (sbIsFacebookImagePermitted == "true");

            string sStateCode = WS_Utils.GetNodeValue(ref t, "state");
            string sCountryCode = WS_Utils.GetNodeValue(ref t, "country");

            if (sStateCode != "")
            {
                m_Country = new Country();
                res = m_Country.InitializeByCode(sCountryCode);

                if (sStateCode != "")
                {
                    m_State = new State();
                    bool res2 = m_State.InitializeByCode(sStateCode, m_Country.m_nObjecrtID);
                    res = res && res2;
                }
            }

            res &= SetRoleIds(userId, groupId);

            return res;
        }

        public bool Save(Int32 nUserID, int groupId)
        {
            return Save(nUserID, groupId, false, false);
        }

        public bool Save(Int32 nUserID, int groupId, bool resetFailCount = false, bool updateUserPassword = false)
        {
            int nCountryID = (-1);
            int nStateID = (-1);

            if (m_Country != null)
            {
                nCountryID = m_Country.m_nObjecrtID;
            }

            if (m_State != null && m_Country != null && m_Country.m_nObjecrtID != 0)
            {
                nStateID = m_State.m_nObjecrtID;
            }

            this.UpdateDate = DateTime.UtcNow;

            var userDataEncryptor = UserDataEncryptor.Instance();
            var encryptionType = userDataEncryptor.GetUsernameEncryptionType(groupId);
            m_sUserName = userDataEncryptor.CorrectUsernameCase(encryptionType, m_sUserName);
            var encryptedUsername = userDataEncryptor.EncryptUsername(groupId, encryptionType, m_sUserName);

            bool saved = UsersDal.SaveBasicData(groupId, nUserID,
                                                    m_sPassword,
                                                    m_sSalt,
                                                    m_sFacebookID,
                                                    m_sFacebookImage,
                                                    m_bIsFacebookImagePermitted,
                                                    m_sFacebookToken,
                                                    encryptedUsername,
                                                    m_sFirstName,
                                                    m_sLastName,
                                                    m_sEmail,
                                                    m_sAddress,
                                                    m_sCity,
                                                    nCountryID,
                                                    nStateID,
                                                    m_sZip,
                                                    m_sPhone,
                                                    m_sAffiliateCode,
                                                    m_sTwitterToken,
                                                    m_sTwitterTokenSecret,
                                                    UpdateDate,
                                                    m_CoGuid,
                                                    m_ExternalToken,
                                                    resetFailCount,
                                                    updateUserPassword,
                                                    encryptionType.HasValue);

            if (UsersDal.UpsertUserRoleIds(groupId, nUserID, this.RoleIds))
            {
                // add invalidation key for user roles cache
                string invalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(groupId, nUserID.ToString());
                LayeredCache.Instance.SetInvalidationKey(invalidationKey);
            }

            return saved;
        }

        public object Clone()
        {
            return CloneImpl();
        }

        protected virtual UserBasicData CloneImpl()
        {
            var copy = (UserBasicData)MemberwiseClone();

            return copy;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("UserBasicData: ");
            sb.Append(String.Concat(" Username: ", m_sUserName));
            sb.Append(String.Concat(" First Name: ", m_sFirstName));
            sb.Append(String.Concat(" Last Name: ", m_sLastName));

            return sb.ToString();
        }

        private bool SetRoleIds(string userId, Int32 groupId)
        {
            bool isRoleIdsSet = false;

            LongIdsResponse userIdsResponse = Module.GetUserRoleIds(groupId, userId);
            if (userIdsResponse != null && userIdsResponse.Status != null && userIdsResponse.Status.Code == (int)eResponseStatus.OK && userIdsResponse.Ids != null)
            {
                RoleIds = userIdsResponse.Ids;
                isRoleIdsSet = true;
            }

            return isRoleIdsSet;
        }

        /// <summary>
        /// copy all relevent properites from other UserBasicData to currenct object befor update
        /// </summary>
        /// <param name="other"></param>
        /// <param name="isSSOMCImplementation"></param>
        /// <returns></returns>
        public bool CopyForUpdate(UserBasicData other, bool isSSOMCImplementation)
        {
            bool isBasicChanged = false;

            if (!isSSOMCImplementation && !string.IsNullOrEmpty(other.m_sUserName))
            {
                this.m_sUserName = other.m_sUserName;
            }
            else if (isSSOMCImplementation && !this.m_sUserName.Equals(other.m_sUserName))
            {
                this.m_sUserName = other.m_sUserName;
                this.m_sPassword = other.m_sUserName.ToLower();
                this.m_sEmail = other.m_sUserName;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation || (!string.IsNullOrEmpty(other.m_sFirstName) && !this.m_sFirstName.Equals(other.m_sFirstName)))
            {
                this.m_sFirstName = other.m_sFirstName == null ? this.m_sFirstName : other.m_sFirstName;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation || (!string.IsNullOrEmpty(other.m_sLastName) && !this.m_sLastName.Equals(other.m_sLastName)))
            {
                this.m_sLastName = other.m_sLastName == null ? this.m_sLastName : other.m_sLastName;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation || (!string.IsNullOrEmpty(other.m_sCity) && !this.m_sCity.Equals(other.m_sCity)))
            {
                this.m_sCity = other.m_sCity == null ? this.m_sCity : other.m_sCity;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation || (!string.IsNullOrEmpty(other.m_sPhone) && !this.m_sPhone.Equals(other.m_sPhone)))
            {
                this.m_sPhone = other.m_sPhone == null ? this.m_sPhone : other.m_sPhone;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation || (!string.IsNullOrEmpty(other.m_sZip) && !this.m_sZip.Equals(other.m_sZip)))
            {
                this.m_sZip = other.m_sZip == null ? this.m_sZip : other.m_sZip;
                isBasicChanged = true;
            }

            if (!isSSOMCImplementation)
            {
                this.m_sEmail = other.m_sEmail == null ? this.m_sEmail : other.m_sEmail;
                this.m_Country = other.m_Country == null ? this.m_Country : other.m_Country;
                this.m_sAddress = other.m_sAddress == null ? this.m_sAddress : other.m_sAddress;
                this.m_State = other.m_State == null ? this.m_State : other.m_State;
                this.m_sFacebookID = other.m_sFacebookID == null ? this.m_sFacebookID : other.m_sFacebookID;
                this.m_sFacebookImage = other.m_sFacebookImage == null ? this.m_sFacebookImage : other.m_sFacebookImage;
                this.m_bIsFacebookImagePermitted = other.m_bIsFacebookImagePermitted;
                this.m_sFacebookToken = other.m_sFacebookToken == null ? this.m_sFacebookToken : other.m_sFacebookToken;
                this.m_UserType.Description = other.m_UserType.Description == null ? this.m_UserType.Description : other.m_UserType.Description;
                this.m_CoGuid = other.m_CoGuid == null ? this.m_CoGuid : other.m_CoGuid;
                this.m_sZip = other.m_sZip == null ? this.m_sZip : other.m_sZip;
                this.m_sPhone = other.m_sPhone == null ? this.m_sPhone : other.m_sPhone;
                this.m_sCity = other.m_sCity == null ? this.m_sCity : other.m_sCity;
            }

            if (other.RoleIds != null && other.RoleIds.Count > 0)
            {
                this.RoleIds = other.RoleIds;
                isBasicChanged = true;
            }

            return isBasicChanged;
        }

        public bool SetPassword(string password, int nGroupID)
        {
            if (password.Length == 0)
            {
                return false;
            }

            // check if we need to encrypt the password
            BaseEncrypter encrypter = Utils.GetBaseImpl(nGroupID);

            // if encrypter is null the group does not have an encrypter support
            if (encrypter != null)
            {
                string sEncryptedPassword = string.Empty;
                string sSalt = string.Empty;

                encrypter.GenerateEncryptPassword(password, ref sEncryptedPassword, ref sSalt);

                this.m_sPassword = sEncryptedPassword;
                this.m_sSalt = sSalt;
            }
            else
            {
                this.m_sPassword = password;
                this.m_sSalt = string.Empty;
            }

            return true;
        }
    }
}
