using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TVinciShared;

namespace Users
{
    public class SSOMCImplementation : SSOUsers, ISSOProvider
    {

        public SSOMCImplementation(int groupId, int operatorId)
            : base(groupId, operatorId)
        {
        }

        #region ISSOProviderImplementation

        public override UserResponseObject SignIn(string username, string pass, int nOperatorID, int nMaxFailCount,
            int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retResponseObject = new UserResponseObject()
            {
                m_RespStatus = ResponseStatus.WrongPasswordOrUserName
            };

            try
            {
                // username = MediaCorp SSO username
                // password = SSO Token
                UserResponseObject userProfile = GetMCUserProfile(pass, sIP);

                if (userProfile != null && userProfile.m_RespStatus == ResponseStatus.OK)
                {
                    string sUsername = userProfile.m_user.m_oBasicData.m_sUserName;
                    string sPass = userProfile.m_user.m_oBasicData.m_sPassword;
                    retResponseObject = base.SignIn(sUsername, sPass, nMaxFailCount, nLockMinutes, m_nGroupID, sSessionID, sIP, sDeviceID, bPreventDoubleLogins);
                }
            }
            catch (Exception ex)
            {
                retResponseObject.m_RespStatus = ResponseStatus.ErrorOnSaveUser;
                Logger.Logger.Log("MC-SSO", string.Format("Error Signing in. ex:{0} UN|PASS={1}|{2}", ex.ToString(), username, pass), "MC-SSO");
            }

            return retResponseObject;
        }

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            User u = new User();
            UserResponseObject uRepsObj = new UserResponseObject();
            int nSiteGuid = u.InitializeByUsername(sUserName, m_nGroupID);

            if (nSiteGuid == 0 || u.m_sSiteGUID.Length == 0)
            {
                uRepsObj.Initialize(ResponseStatus.UserDoesNotExist, u);
            }
            else
            {
                uRepsObj.Initialize(ResponseStatus.OK, u);
            }

            return uRepsObj;
        }

        #endregion

        private UserResponseObject GetMCUserProfile(string pass, string clientIP)
        {
            UserResponseObject userObj = null;

            try
            {
                Logger.Logger.Log("GetMCUserProfile", string.Format("password:{0}, clientIp:{1}", pass, clientIP), "MC-SSO");
                //MCUserProfile kdgLoginRespObj = new KdgLoginResp() { Status = eKdgStatus.Unknown };
                
                string ssoMcUrl = TCMClient.Settings.Instance.GetValue<string>("MCAuthURL");

                if (string.IsNullOrEmpty(ssoMcUrl))
                {
                    Logger.Logger.Log("MC-SSO", "Error getting MC URL From TCM", string.Format("SSO-MC-{0}", Utils.DateToFilename(DateTime.UtcNow.Date)));
                    return userObj;
                }

                Uri ssoMcUri = new Uri(ssoMcUrl);
                if ((!Uri.TryCreate(ssoMcUrl, UriKind.Absolute, out ssoMcUri)) || ((ssoMcUri.Scheme != Uri.UriSchemeHttp) && (ssoMcUri.Scheme != Uri.UriSchemeHttps)))
                {
                    Logger.Logger.Log("MC-SSO", "Error in MC URL format - [" + ssoMcUrl + "]", string.Format("SSO-MC-{0}", Utils.DateToFilename(DateTime.UtcNow.Date)));
                    return userObj;
                }

                Dictionary<string, string> dHeaders = new Dictionary<string, string>() 
                {
                    //{ "Host", "beta-login.mediacorp.sg" },
                    { "Authorization", "Bearer " + pass }
                };

                DateTime dNow = DateTime.UtcNow;
                string profJson = TVinciShared.WS_Utils.SendXMLHttpReqWithHeaders(ssoMcUrl, "", dHeaders, "application/x-www-form-urlencoded", "", "", "", "", "get");
                double dTime = DateTime.UtcNow.Subtract(dNow).TotalMilliseconds;

                Logger.Logger.Log("MC-SSO", string.Format("MC Json: {0}", profJson), "MC-SSO");

                if (string.IsNullOrEmpty(profJson))
                {
                    Logger.Logger.Log("MC-SSO", "No data received from MCAuth API", string.Format("SSO-MC-{0}", Utils.DateToFilename(DateTime.UtcNow.Date)));
                    return userObj;
                }

                Dictionary<string, string> dUserInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(profJson);

                if (profJson.ToLower().Contains("api failed") && dUserInfo.Count < 3)
                {
                    Logger.Logger.Log("MC-SSO", "Failed to retrieve user profile from MCAuth", string.Format("SSO-MC-{0}", Utils.DateToFilename(DateTime.UtcNow.Date)));
                    return userObj;                       
                }

                if (profJson.ToLower().Contains("invalid token") && dUserInfo.Count < 3)
                {
                    Logger.Logger.Log("MC-SSO", "Invalid Token", string.Format("SSO-MC-{0}", Utils.DateToFilename(DateTime.UtcNow.Date)));
                    return userObj;                       
                }

                //Logger.Logger.Log("MC-SSO", string.Format("User info dict count: {0}", dUserInfo.Count), "MC-SSO");


                foreach (string key in dUserInfo.Keys.ToList())
                {
                    if (string.IsNullOrEmpty(dUserInfo[key]))
                    {
                        dUserInfo[key] = string.Empty;
                    }

                    if ((key.Equals("birthdate", StringComparison.OrdinalIgnoreCase)) &&
                        (!string.IsNullOrEmpty(dUserInfo[key])))
                    {
                        DateTime birthdate = new DateTime(2000, 1, 1);
                        if (DateTime.TryParse(dUserInfo[key], out birthdate))
                        {
                            dUserInfo["BirthYear"] = birthdate.Year.ToString();
                            dUserInfo["BirthMonth"] = birthdate.Month.ToString();
                            dUserInfo["BirthDay"] = birthdate.Day.ToString();
                        }
                    }
                }

                // If does not exist, new user will be created
                userObj = GetUserProfile(dUserInfo);
                //Logger.Logger.Log("MC-SSO", string.Format("User object: {0}", userObj.ToJSON()), "MC-SSO");

                // Check the user CoGuid (= Id)
                string sDomainCoGuid = string.Empty;
                Utils.GetContentInfo(ref sDomainCoGuid, "Id", dUserInfo);

                if ((userObj.m_user == null) || (string.IsNullOrEmpty(userObj.m_user.m_sSiteGUID)))
                {
                    Logger.Logger.Log("MC-SSO", "User init error", "MC-SSO");
                    userObj.m_RespStatus = ResponseStatus.ErrorOnInitUser;
                    return userObj;                       
                }

                int userID = int.Parse(userObj.m_user.m_sSiteGUID);

                BaseDomain domainImpl = null;
                Utils.GetBaseDomainsImpl(ref domainImpl, m_nGroupID);

                if (domainImpl != null)
                {
                    DomainResponseObject domain = domainImpl.GetDomainByCoGuid(sDomainCoGuid, m_nGroupID);

                    // domain by coguid found
                    if (domain != null && domain.m_oDomainResponseStatus == DomainResponseStatus.OK)
                    {
                        if (userObj.m_RespStatus == ResponseStatus.UserWithNoDomain)
                        {
                            //Add user to domain
                            DomainResponseObject domainResponse = domainImpl.AddUserToDomain(m_nGroupID, domain.m_oDomain.m_nDomainID, userID, domain.m_oDomain.m_masterGUIDs[0]);
                            if (domainResponse.m_oDomainResponseStatus == DomainResponseStatus.OK)
                            {
                                userObj.m_user.m_domianID = domainResponse.m_oDomain.m_nDomainID;
                                userObj.m_RespStatus = ResponseStatus.OK;
                            }
                            else
                            {
                                Logger.Logger.Log("Error adding user to domain", string.Format("domainCoGuid:{0}", sDomainCoGuid), "MC - SSO");
                            }
                        }
                    }
                    else
                    {
                        // User has domain but with different cougid 
                        if (userObj.m_RespStatus == ResponseStatus.OK)
                        {
                            bool changedCoguid = DAL.DomainDal.UpdateDomainCoGuid(userObj.m_user.m_domianID, m_nGroupID, sDomainCoGuid);
                            if (!changedCoguid)
                            {
                                userObj.m_RespStatus = ResponseStatus.InternalError;
                                Logger.Logger.Log("Error updating domain CoGuid", string.Format("newDomainCoGuid:{0} domainID:{1}", sDomainCoGuid, userObj.m_user.m_domianID), "MC - SSO");
                            }
                        }
                            // User has no domain
                        else if ((domainImpl != null) && (userObj.m_RespStatus == ResponseStatus.UserWithNoDomain))
                        {
                            DomainResponseObject domainResponseObject =
                                domainImpl.AddDomain(sDomainCoGuid, "", int.Parse(userObj.m_user.m_sSiteGUID), m_nGroupID, sDomainCoGuid);

                            if (domainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK)
                            {
                                userObj.m_user.m_domianID = domainResponseObject.m_oDomain.m_nDomainID;
                                userObj.m_user.m_isDomainMaster = true;
                                userObj.m_RespStatus = ResponseStatus.OK;
                            }
                            else
                            {
                                Logger.Logger.Log("Error creating domain", string.Format("domainCoGuid:{0}", sDomainCoGuid), "MC-SSO");
                            }   
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("MC-SSO", string.Format("Error validating credentials with MC: ex:{0} Pass:{1}", ex.ToString(), pass), "MC-SSO");
            }

            return userObj;
            
        }

        private UserResponseObject GetUserProfile(Dictionary<string, string> dUserInfo)
        {
            UserResponseObject uo = null;

            if (dUserInfo == null || dUserInfo.Count == 0)
            {
                return uo;
            }

            // Check the user CoGuid (= Id) 
            string sUserCoGuid = string.Empty;
            Utils.GetContentInfo(ref sUserCoGuid, "Id", dUserInfo);

            // Check if user exists in the DB
            UserResponseObject userInfo = base.GetUserByCoGuid(sUserCoGuid, -1);
            //Logger.Logger.Log("GetUserProfile", "userInfo: " + userInfo.ToJSON(), "Users");

            if (userInfo.m_RespStatus == ResponseStatus.UserDoesNotExist)
            {
                // Create Tvinci user - set password = username
                UserBasicData userBasic = GetUserBasicData(dUserInfo);
                UserDynamicData userDynamic = GetUserDynamicData(dUserInfo);

                //Logger.Logger.Log("GetUserProfile", "New user: Basic - " + userBasic.ToJSON() + " dynamic data - " + userDynamic.ToJSON(), "Users");

                string sPass = userBasic.m_sUserName.ToLower();
                bool userCreated = false;

                try
                {
                    userInfo = base.AddNewUser(userBasic, userDynamic, sPass);

                    userCreated = ((userInfo.m_RespStatus == ResponseStatus.OK) && 
                                   (userInfo.m_user != null) &&
                                   (!string.IsNullOrEmpty(userInfo.m_user.m_sSiteGUID)));
                }
                catch
                {
                }

                // In case of existing username and problem with password
                if (!userCreated || userInfo.m_RespStatus == ResponseStatus.UserExists)
                {
                    int nUserID = GetUserIDByUserName(userBasic.m_sUserName);

                    if (nUserID > 0)
                    {
                        Utils.SetPassword(sPass, ref userBasic, m_nGroupID);
                        userBasic.Save(nUserID);
                        userDynamic.Save(nUserID);
                        
                        userInfo = base.GetUserByCoGuid(sUserCoGuid, -1);
                    }
                }

                if (userInfo.m_RespStatus != ResponseStatus.OK)
                {
                    Logger.Logger.Log("Creating User Error", "sUsername = " + userBasic.m_sUserName + " Response = " + userInfo.m_RespStatus.ToString(), "Users");
                    uo = new UserResponseObject();
                    uo.m_RespStatus = ResponseStatus.WrongPasswordOrUserName;
                }
            }
            else
            {
                //get the dynamic data from yes and update it in DB
                UserDynamicData userDynamic = GetUserDynamicData(dUserInfo);
                //Logger.Logger.Log("GetUserProfile", "Existing user: Dynamic data - " + userDynamic.ToJSON(), "Users");

                List<ApiObjects.KeyValuePair> lKeyValue = new List<ApiObjects.KeyValuePair>();
                foreach (UserDynamicDataContainer container in userDynamic.m_sUserData)
                {
                    if (container == null)
                    {
                        continue;
                    }

                    ApiObjects.KeyValuePair kvp = new ApiObjects.KeyValuePair(container.m_sDataType, (container.m_sValue != null ? container.m_sValue : string.Empty));
                    lKeyValue.Add(kvp);
                }

                //Logger.Logger.Log("GetUserProfile", "Setting dynamic data: " + userInfo.m_user.m_sSiteGUID, "Users");

                // First remove existing dynamic data
                UserDynamicData emptyDynamic = new UserDynamicData();
                emptyDynamic.m_sUserData = new UserDynamicDataContainer[] { };
                userInfo.m_user.UpdateDynamicData(emptyDynamic, m_nGroupID);

                // Then save updated dynamic data
                SetUserDynamicData(userInfo.m_user.m_sSiteGUID, lKeyValue, userInfo);   
            }

            uo = userInfo;
            return uo;
        }

        private UserBasicData GetUserBasicData(Dictionary<string, string> dUserData)
        {
            UserBasicData ubd = new UserBasicData();

            Utils.GetContentInfo(ref ubd.m_sUserName, "UserName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sPassword, "UserName", dUserData);

            Utils.GetContentInfo(ref ubd.m_sEmail, "OriginatingEmailId", dUserData);
            Utils.GetContentInfo(ref ubd.m_CoGuid, "Id", dUserData);

            Utils.GetContentInfo(ref ubd.m_sFirstName, "FirstName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sLastName, "LastName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sPhone, "HomePhone", dUserData);
            

            Utils.GetContentInfo(ref ubd.m_sCity, "City", dUserData);
            Utils.GetContentInfo(ref ubd.m_sZip, "PostalCode", dUserData);

            string sCountry = string.Empty;
            Utils.GetContentInfo(ref sCountry, "Country", dUserData);
            ubd.m_Country = new Country();
            if (!string.IsNullOrEmpty(sCountry))
            {
                ubd.m_Country.InitializeByName(sCountry);
            }

            string state = string.Empty;
            Utils.GetContentInfo(ref state, "State", dUserData);
            ubd.m_State = new State();
            if (!string.IsNullOrEmpty(state) && ubd.m_Country.m_nObjecrtID > 0)
            {
                ubd.m_State.InitializeByName(state, ubd.m_Country.m_nObjecrtID);
            }

            string sBlock = string.Empty;
            string street = string.Empty;
            string sBuilding = string.Empty;
            string sUnit = string.Empty;
            Utils.GetContentInfo(ref sBlock, "Block", dUserData);
            Utils.GetContentInfo(ref street, "Street", dUserData);
            Utils.GetContentInfo(ref sBuilding, "Building", dUserData);
            Utils.GetContentInfo(ref sUnit, "Unit", dUserData);

            if (!string.IsNullOrEmpty(sBlock))
            {
                ubd.m_sAddress = string.Format("Blk {0} ", sBlock);
            }
            if (!string.IsNullOrEmpty(street))
            {
                ubd.m_sAddress += street + " ";
            }
            if (!string.IsNullOrEmpty(sUnit))
            {
                ubd.m_sAddress += "# " + sUnit + " ";
            }
            if (!string.IsNullOrEmpty(sBuilding))
            {
                ubd.m_sAddress += sBuilding;
            }

            return ubd;
        }

        private List<string> _dynamicParams = new List<string>()
        {
            "Nationality", "IdentificationNumber", "BirthYear", "BirthDate", "Occupation", "Gender", "Ethnicity", "MaritalStatus", "MobilePhone", "Income", "AccountStatus", "Xinmsn_English", "Xinmsn_Chinese",
            "MEClub", "MERadio", "OktoAsia", "CNA_MailPref", "CreatedDate", "VizPro", "SingaporeMediaAcademy", "Avatar"
        };

        private UserDynamicData GetUserDynamicData(Dictionary<string, string> dUserData)
        {
            UserDynamicData udd = new UserDynamicData();

            List<string> lDynamicParams = _dynamicParams;
            string sDynamicKeys = TCMClient.Settings.Instance.GetValue<string>("MCAuthDynamicKeys");

            if (!string.IsNullOrEmpty(sDynamicKeys))
            {
                lDynamicParams = sDynamicKeys.Trim().Split(new char[] {',', ';', '|'}, StringSplitOptions.RemoveEmptyEntries).ToList();

                if (lDynamicParams == null || lDynamicParams.Count == 0)
                {
                    lDynamicParams = _dynamicParams;
                }

                lDynamicParams = lDynamicParams.Select(k => k.Trim()).ToList();
            }

            udd.m_sUserData = new UserDynamicDataContainer[lDynamicParams.Count];

            for (int i = 0; i < lDynamicParams.Count; ++i)
            {
                UserDynamicDataContainer dynamicData = new UserDynamicDataContainer();

                if (dUserData.ContainsKey(lDynamicParams[i]))
                {
                    dynamicData.m_sDataType = lDynamicParams[i];
                    dynamicData.m_sValue = dUserData[lDynamicParams[i]];
                    udd.m_sUserData[i] = dynamicData;
                }
            }

            return udd;
        }
    }



    //internal class MCUserProfile
    //{
    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public string Nationality { get; set; }
    //    public string IdentificationNumber { get; set; }
    //    public int BirthYear { get; set; }
    //    public DateTime BirthDate { get; set; }
    //    public string Occupation { get; set; }
    //    public string Gender { get; set; }
    //    public string Ethnicity { get; set; }
    //    public string MaritalStatus { get; set; }
    //    public string Block { get; set; }
    //    public string Street { get; set; }
    //    public string Building { get; set; }
    //    public string Unit { get; set; }
    //    public string City { get; set; }
    //    public object State { get; set; }
    //    public string Country { get; set; }
    //    public string PostalCode { get; set; }
    //    public string HomePhone { get; set; }
    //    public string MobilePhone { get; set; }
    //    public string Income { get; set; }
    //    public string OriginatingEmailId { get; set; }
    //    public string Id { get; set; }
    //    public string UserName { get; set; }
    //    public string AccountStatus { get; set; }
    //    public bool Xinmsn_English { get; set; }
    //    public bool Xinmsn_Chinese { get; set; }
    //    //public bool TV_Channel5 { get; set; }
    //    //public bool TV_Channel8 { get; set; }
    //    //public bool TV_ChannelU { get; set; }
    //    //public bool TV_Suria { get; set; }
    //    //public bool TV_Vasantham { get; set; }
    //    //public bool TV_Okto { get; set; }
    //    //public bool RDO_933 { get; set; }
    //    //public bool RDO_938 { get; set; }
    //    //public bool RDO_938_Slice { get; set; }
    //    //public bool RDO_938_Work { get; set; }
    //    //public bool RDO_938_Soul { get; set; }
    //    //public bool RDO_924 { get; set; }
    //    //public bool RDO_958 { get; set; }
    //    //public bool RDO_987 { get; set; }
    //    //public bool RDO_968 { get; set; }
    //    //public bool RDO_Warna { get; set; }
    //    //public bool CNA_AsiaPacific { get; set; }
    //    //public bool CNA_World { get; set; }
    //    //public bool CNA_Business { get; set; }
    //    //public bool CNA_Singapore { get; set; }
    //    //public bool CNA_Sports { get; set; }
    //    //public bool CNA_Morning { get; set; }
    //    //public bool CNA_Evening { get; set; }
    //    //public bool CNA_Weekdays { get; set; }
    //    //public bool CNA_Weekends { get; set; }
    //    //public bool CNA_Partners { get; set; }
    //    //public bool TDY_Singapore { get; set; }
    //    //public bool TDY_World { get; set; }
    //    //public bool TDY_Entertainment { get; set; }
    //    //public bool TDY_Sports { get; set; }
    //    //public bool TDY_Business { get; set; }
    //    //public bool TDY_Tech { get; set; }
    //    //public bool TDY_Voices { get; set; }
    //    //public bool TDY_Commentary { get; set; }
    //    //public bool TDY_Focus { get; set; }
    //    //public bool TDY_ChinaIndia { get; set; }
    //    //public bool TDY_Photos { get; set; }
    //    //public bool TDY_Videos { get; set; }
    //    //public bool TDY_Partners { get; set; }
    //    //public string TDY_MailPref { get; set; }
    //    public bool MEClub { get; set; }
    //    public bool MERadio { get; set; }
    //    public bool OktoAsia { get; set; }
    //    public string CNA_MailPref { get; set; }
    //    public DateTime CreatedDate { get; set; }
    //    public bool VizPro { get; set; }
    //    public bool SingaporeMediaAcademy { get; set; }
    //    public string Avatar { get; set; }
    //}


}
