using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVinciShared;
using System.Xml;
using System.Data;
using DAL;
using ApiObjects;
using ApiObjects.Response;

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

        public UserBasicData()
        {
            m_sUserName = "";
            m_sFacebookImage = "";
            m_sFacebookID = "";
            m_bIsFacebookImagePermitted = false;
            m_sPassword = "";
            m_sSalt = "";
            m_sFirstName = "";
            m_sLastName = "";
            m_sEmail = "";
            m_sAddress = "";
            m_sCity = "";
            m_State = null;
            m_Country = null;
            m_sZip = "";
            m_sPhone = "";
            m_sAffiliateCode = "";
            m_sFacebookToken = string.Empty;
            m_sTwitterToken = string.Empty;
            m_sTwitterTokenSecret = string.Empty;
            RoleIds = new List<long>();
            CreateDate = DateTime.MinValue;
            UpdateDate = DateTime.MinValue;
        }

        public bool Initialize(Int32 nUserID, Int32 nGroupID)
        {
            bool res = false;
            DataTable dtUserBasicData = UsersDal.GetUserBasicData(nUserID, nGroupID);

            if (dtUserBasicData == null || dtUserBasicData.DefaultView.Count == 0)
            {
                return false;
            }

            string sUserName = dtUserBasicData.DefaultView[0].Row["USERNAME"].ToString();
            string sPass = dtUserBasicData.DefaultView[0].Row["PASSWORD"].ToString();
            string sSalt = dtUserBasicData.DefaultView[0].Row["SALT"].ToString();
            string sFirstName = dtUserBasicData.DefaultView[0].Row["FIRST_NAME"].ToString();
            string sLastName = dtUserBasicData.DefaultView[0].Row["LAST_NAME"].ToString();
            string sEmail = dtUserBasicData.DefaultView[0].Row["EMAIL_ADD"].ToString();
            string sAddress = dtUserBasicData.DefaultView[0].Row["ADDRESS"].ToString();
            object oAffiliates = dtUserBasicData.DefaultView[0].Row["REG_AFF"];
            string sCity = dtUserBasicData.DefaultView[0].Row["CITY"].ToString();
            Int32 nStateID = int.Parse(dtUserBasicData.DefaultView[0].Row["STATE_ID"].ToString());
            Int32 nCountryID = int.Parse(dtUserBasicData.DefaultView[0].Row["COUNTRY_ID"].ToString());
            string sZip = dtUserBasicData.DefaultView[0].Row["ZIP"].ToString();
            string sPhone = dtUserBasicData.DefaultView[0].Row["PHONE"].ToString();
            object oFacebookID = dtUserBasicData.DefaultView[0].Row["FACEBOOK_ID"];
            object oFacebookImage = dtUserBasicData.DefaultView[0].Row["FACEBOOK_IMAGE"];
            Int32 nFacebookImagePermitted = int.Parse(dtUserBasicData.DefaultView[0].Row["FACEBOOK_IMAGE_PERMITTED"].ToString());
            string sCoGuid = dtUserBasicData.DefaultView[0].Row["CoGuid"].ToString();
            string sExternalToken = dtUserBasicData.DefaultView[0].Row["ExternalToken"].ToString();
            string sFacebookToken = ODBCWrapper.Utils.GetSafeStr(dtUserBasicData.DefaultView[0].Row["fb_token"]);
            string sUserType = ODBCWrapper.Utils.GetSafeStr(dtUserBasicData.DefaultView[0].Row["user_type_desc"]);
            bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(dtUserBasicData.DefaultView[0].Row["is_default"]));
            DateTime createDate = ODBCWrapper.Utils.GetDateSafeVal(dtUserBasicData.DefaultView[0].Row["CREATE_DATE"]);
            DateTime updateDate = ODBCWrapper.Utils.GetDateSafeVal(dtUserBasicData.DefaultView[0].Row["UPDATE_DATE"]);

            bool bFacebookImagePermitted = (nFacebookImagePermitted == 1);

            string sAffiliate = "";
            if (oAffiliates != null && oAffiliates != DBNull.Value)
                sAffiliate = oAffiliates.ToString();
            
            string sFacebookImage = "";
            if (oFacebookImage != null && oFacebookImage != DBNull.Value)
                sFacebookImage = oFacebookImage.ToString();

            string sFacebookID = "";
            if (oFacebookID != null && oFacebookID != DBNull.Value)
                sFacebookID = oFacebookID.ToString();
            
            int? nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(dtUserBasicData.DefaultView[0].Row["user_type_id"]);
            if (nUserTypeID == 0)
            {
                nUserTypeID = null;
            }
            
            UserType userType = new UserType(nUserTypeID, sUserType, isDefault);

            res = Initialize(sUserName, sPass, sSalt, sFirstName, sLastName, sEmail, sAddress, sCity, nStateID, nCountryID, sZip, sPhone, sFacebookID, 
                             bFacebookImagePermitted, sFacebookImage, sAffiliate, sFacebookToken, sCoGuid, sExternalToken, userType, nUserID, nGroupID,
                             createDate, updateDate);

            return res;
        }

        // TODO SHIR - DELETE IF DONT NEED
        //public void Initialize(string sUserName, string sPassword, string sSalt, string sFirstName, string sLastName, string sEmail, string sAddress, string sCity, 
        //                       Int32 nStateID, Int32 nCountryID, string sZip, string sPhone, string sFacebookID, bool bIsFacebookImagePermitted, string sFacebookImageURL, 
        //                       string sAffiliate, UserType userType, Int32 userId, Int32 groupId)
        //{
        //    Initialize(sUserName, sPassword, sSalt, sFirstName, sLastName, sEmail, sAddress, sCity, nStateID, nCountryID, sZip, sPhone, sFacebookID, 
        //               bIsFacebookImagePermitted, sFacebookImageURL, sAffiliate, string.Empty, userType, userId, groupId);
        //}

        //public void Initialize(string sUserName, string sPassword, string sSalt, string sFirstName, string sLastName, string sEmail, string sAddress, string sCity,
        //                       Int32 nStateID, Int32 nCountryID, string sZip, string sPhone, string sFacebookID, bool bIsFacebookImagePermitted, string sFacebookImageURL, 
        //                       string sAffiliate, string sFacebookToken, UserType userType, Int32 userId, Int32 groupId)
        //{
        //    Initialize(sUserName, sPassword, sSalt, sFirstName, sLastName, sEmail, sAddress, sCity, nStateID, nCountryID, sZip, sPhone, sFacebookID, 
        //               bIsFacebookImagePermitted, sFacebookImageURL, sAffiliate, m_sFacebookToken, string.Empty, string.Empty, userType, userId, groupId, DateTime.MinValue, DateTime.MinValue);
        //}

        public bool Initialize(string sUserName, string sPassword, string sSalt, string sFirstName, string sLastName, string sEmail, string sAddress, string sCity, 
                               int nStateID, Int32 nCountryID, string sZip, string sPhone, string sFacebookID, bool bIsFacebookImagePermitted, string sFacebookImageURL, 
                               string sAffiliate, string sFacebookToken, string sCoGuid, string sExternalToken, UserType userType, Int32 userId, Int32 groupId, 
                               DateTime createDate, DateTime updateDate)
        {
            bool res = true;

            m_sUserName = sUserName;
            m_sFacebookID = sFacebookID;
            m_sFacebookImage = sFacebookImageURL;
            m_bIsFacebookImagePermitted = bIsFacebookImagePermitted;
            m_sPassword = sPassword;
            m_sSalt = sSalt;
            m_sFirstName = sFirstName;
            m_sLastName = sLastName;
            m_sEmail = sEmail;
            m_sAddress = sAddress;
            m_sCity = sCity;
            m_sZip = sZip;
            m_sPhone = sPhone;
            m_sFacebookToken = sFacebookToken;
            m_CoGuid = sCoGuid;
            m_ExternalToken = sExternalToken;
            m_UserType = userType;
            m_sAffiliateCode = String.IsNullOrEmpty(sAffiliate) ? "" : sAffiliate;
            CreateDate = createDate;
            UpdateDate = updateDate;

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
            // TODO SHIR - ASK LIOR IF TO ADD CREATE_DATE AND UPDATE_DATE HERE.. (OLD METHOD)
            
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
        
        public bool Save(Int32 nUserID)
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

            // TODO SHIR - update roleIds
            bool saved = DAL.UsersDal.SaveBasicData(nUserID,
                                                    m_sPassword,
                                                    m_sSalt,
                                                    m_sFacebookID,
                                                    m_sFacebookImage,
                                                    m_bIsFacebookImagePermitted,
                                                    m_sFacebookToken,
                                                    m_sUserName,
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
                                                    m_CoGuid);
            
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
                //TODO SHIR - CHECK IN THE FUTURE IF OVERRIDE CURRENT ROLES
                RoleIds = userIdsResponse.Ids;
                isRoleIdsSet = true;
            }

            return isRoleIdsSet;
        }

        public void Copy(UserBasicData other)
        {
            if (!string.IsNullOrEmpty(other.m_sUserName))
            {
                this.m_sUserName = other.m_sUserName;
            }

            this.m_sEmail = other.m_sEmail;
            this.m_sFirstName = other.m_sFirstName;
            this.m_sLastName = other.m_sLastName;
            this.m_Country = other.m_Country;
            this.m_sAddress = other.m_sAddress;
            this.m_sCity = other.m_sCity;
            this.m_sPhone = other.m_sPhone;
            this.m_State = other.m_State;
            this.m_sZip = other.m_sZip;
            this.m_sFacebookID = other.m_sFacebookID;
            this.m_sFacebookImage = other.m_sFacebookImage;
            this.m_bIsFacebookImagePermitted = other.m_bIsFacebookImagePermitted;
            this.m_sFacebookToken = other.m_sFacebookToken;
            this.m_UserType = other.m_UserType;
            this.m_CoGuid = other.m_CoGuid;

            if (other.RoleIds != null && other.RoleIds.Count > 0)
            {
                this.RoleIds = other.RoleIds;
            }
            UpdateDate = DateTime.MinValue;
        }
    }
}
