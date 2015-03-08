using System;
using System.Collections.Generic;
using System.Globalization;
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

                    //if ((key.Equals("birthdate", StringComparison.OrdinalIgnoreCase)) &&
                    //    (!string.IsNullOrEmpty(dUserInfo[key])))
                    //{
                    //    DateTime birthdate = new DateTime(2000, 1, 1);
                    //    if (DateTime.TryParse(dUserInfo[key], out birthdate))
                    //    {
                    //        dUserInfo["BirthYear"] = birthdate.Year.ToString();
                    //        dUserInfo["BirthMonth"] = birthdate.Month.ToString();
                    //        dUserInfo["BirthDay"] = birthdate.Day.ToString();
                    //    }
                    //}
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
                        // User has domain but with different cougid 
                        else if (userObj.m_RespStatus == ResponseStatus.OK)
                        {
                            if (userObj.m_user.m_domianID != domain.m_oDomain.m_nDomainID)
                            {
                                bool changedCoguid = DAL.DomainDal.UpdateDomainCoGuid(userObj.m_user.m_domianID, m_nGroupID, sDomainCoGuid);
                                if (!changedCoguid)
                                {
                                    userObj.m_RespStatus = ResponseStatus.InternalError;
                                    Logger.Logger.Log("Error updating domain CoGuid", string.Format("newDomainCoGuid:{0} domainID:{1}", sDomainCoGuid, userObj.m_user.m_domianID), "MC - SSO");
                                }
                            }
                        }
                    }
                    else
                    {
                        // User has no domain
                        if ((domainImpl != null) && domain != null && (domain.m_oDomainResponseStatus != DomainResponseStatus.OK) &&
                            userObj.m_RespStatus == ResponseStatus.UserWithNoDomain)
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
            else if (userInfo.m_user != null && userInfo.m_user.m_oBasicData != null)
            {
                try
                {
                    //handle basic data
                    UserBasicData userBasicData = GetUserBasicData(dUserInfo);
                    UpdateUserBasicData(userInfo, userBasicData);

                    //get the dynamic data from and update it in DB
                    UserDynamicData userDynamic = GetUserDynamicData(dUserInfo);
                    Logger.Logger.Log("GetUserProfile", "Existing user: Dynamic data - " + userDynamic.ToJSON(), "Users");

                    // fill the user data with values already exsits 
                    List<ApiObjects.KeyValuePair> lKeyValue = new List<ApiObjects.KeyValuePair>();
                    foreach (UserDynamicDataContainer container in userDynamic.m_sUserData)
                    {
                        if (container == null)
                        {
                            continue;
                        }

                        ApiObjects.KeyValuePair kvp = new ApiObjects.KeyValuePair(container.m_sDataType, (container.m_sValue != null ? container.m_sValue : string.Empty));

                        Logger.Logger.Log("GetUserProfile_Key_Value", string.Format("key={0} value={1}", kvp.key, kvp.value), "Users_Test");

                        if (!string.IsNullOrEmpty(kvp.value))
                        {
                            lKeyValue.Add(kvp);
                        }
                        Logger.Logger.Log("GetUserProfile_Key_Value", string.Format("key={0} value={1}", kvp.key, kvp.value), "Users_Test");
                    }

                    // Then save only updated dynamic data
                    SetUserDynamicData(userInfo.m_user.m_sSiteGUID, lKeyValue, userInfo);
                }
                catch (Exception)
                {
                    Logger.Logger.Log("GetUserProfile", "Setting dynamic data: " + userInfo.m_user.m_sSiteGUID, "Users");
                    throw;
                }
            }

            uo = userInfo;
            return uo;
        }

        private UserBasicData GetUserBasicData(Dictionary<string, string> dUserData)
        {
            UserBasicData ubd = new UserBasicData();

            Utils.GetContentInfo(ref ubd.m_sUserName, "UserName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sPassword, "UserName", dUserData);

            Utils.GetContentInfo(ref ubd.m_sEmail, "UserName", dUserData);
            Utils.GetContentInfo(ref ubd.m_CoGuid, "Id", dUserData);

            Utils.GetContentInfo(ref ubd.m_sFirstName, "FirstName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sLastName, "LastName", dUserData);
            Utils.GetContentInfo(ref ubd.m_sPhone, "HomePhone", dUserData);

            Utils.GetContentInfo(ref ubd.m_sCity, "City", dUserData);
            Utils.GetContentInfo(ref ubd.m_sZip, "PostalCode", dUserData);

            return ubd;
        }

        private List<string> _dynamicParams = new List<string>()
        {
            "Nationality", "IdentificationNumber", "BirthYear", "BirthDate", "Occupation", "Gender", "Ethnicity", "MaritalStatus", "MobilePhone", "Income", 
            "AccountStatus", "Xinmsn_English", "Xinmsn_Chinese", "MEClub", "MERadio", "OktoAsia", "CNA_MailPref", "CreatedDate", "VizPro", "SingaporeMediaAcademy", 
            "Avatar"
        };

        private UserDynamicData GetUserDynamicData(Dictionary<string, string> dUserData)
        {
            UserDynamicData udd = new UserDynamicData();

            List<string> lDynamicParams = _dynamicParams;
            string sDynamicKeys = TCMClient.Settings.Instance.GetValue<string>("MCAuthDynamicKeys");

            if (string.IsNullOrEmpty(sDynamicKeys))
            {
                return udd;
            }
            else
            {
                lDynamicParams = sDynamicKeys.Trim().Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            List<UserDynamicDataContainer> uddc = new List<UserDynamicDataContainer>();

            foreach (string key in lDynamicParams)
            {
                switch (key)
                {
                    case "Unit": // The format are “#xx-yy” or “xx-yy” or “zz”
                        {
                            string unit = GetContentInfo(key, dUserData);
                            if (!string.IsNullOrEmpty(unit))
                            {
                                string[] splitUnit = unit.Replace("#", "").Split('-');
                                int UnitEndNumberIndex = 0;
                                if (splitUnit.Length == 2)
                                {
                                    AddDDToList("UnitStartNumber", splitUnit[0], ref uddc);
                                    UnitEndNumberIndex = 1;
                                }

                                AddDDToList("UnitEndNumber", splitUnit[UnitEndNumberIndex], ref uddc);
                            }
                            break;
                        }

                    case "BirthDate": // format of BirthDate is "yyyy-MM-ddTHH:mm:ss”
                        {
                            string BirthDate = GetContentInfo(key, dUserData);
                            try
                            {
                                DateTime birthDate = DateTime.ParseExact(BirthDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

                                AddDDToList("BirthYear", birthDate.Year.ToString(), ref uddc);
                                AddDDToList("BirthMonth", birthDate.Month.ToString(), ref uddc);
                                AddDDToList("BirthDay", birthDate.Day.ToString(), ref uddc);
                            }
                            catch (Exception ex)
                            {

                            }

                            break;
                        }

                    case "FirstName":
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList("NickName", val, ref uddc);
                            break;
                        }

                    case "IdentificationNumber":
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList("NricFin", val, ref uddc);
                            break;
                        }

                    case "Block":
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList("BlockHouseNumber", val, ref uddc);
                            break;
                        }
                    
                    case "Building":
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList("BuildingName", val, ref uddc);
                            break;
                        }

                    case "Street":
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList("StreetName", val, ref uddc);
                            break;
                        }

                    default:
                        {
                            string val = GetContentInfo(key, dUserData);
                            AddDDToList(key, val, ref uddc);
                            break;
                        }
                }
            }

            if (uddc.Count > 0)
            {
                udd.m_sUserData = uddc.ToArray<UserDynamicDataContainer>();
            }

            return udd;
        }

        private void UpdateUserBasicData(UserResponseObject userInfo, UserBasicData newUserData)
        {
            UserBasicData oldUserData = userInfo.m_user.m_oBasicData;

            bool isBasicChanged = false;
            if (!oldUserData.m_sUserName.Equals(newUserData.m_sUserName))
            {
                oldUserData.m_sUserName = newUserData.m_sUserName;
                oldUserData.m_sPassword = newUserData.m_sUserName.ToLower();
                oldUserData.m_sEmail = newUserData.m_sUserName;
                isBasicChanged = true;
            }

            if (!string.IsNullOrEmpty(newUserData.m_sFirstName) && !oldUserData.m_sFirstName.Equals(newUserData.m_sFirstName))
            {
                oldUserData.m_sFirstName = newUserData.m_sFirstName;
                isBasicChanged = true;
            }

            if (!string.IsNullOrEmpty(newUserData.m_sLastName) && !oldUserData.m_sLastName.Equals(newUserData.m_sLastName))
            {
                oldUserData.m_sLastName = newUserData.m_sLastName;
                isBasicChanged = true;
            }

            if (!string.IsNullOrEmpty(newUserData.m_sPhone) && !oldUserData.m_sPhone.Equals(newUserData.m_sPhone))
            {
                oldUserData.m_sPhone = newUserData.m_sPhone;
                isBasicChanged = true;
            }

            if (!string.IsNullOrEmpty(newUserData.m_sCity) && !oldUserData.m_sCity.Equals(newUserData.m_sCity))
            {
                oldUserData.m_sCity = newUserData.m_sCity;
                isBasicChanged = true;
            }

            if (!string.IsNullOrEmpty(newUserData.m_sZip) && !oldUserData.m_sZip.Equals(newUserData.m_sZip))
            {
                oldUserData.m_sZip = newUserData.m_sZip;
                isBasicChanged = true;
            }

            if (isBasicChanged)
            {
                oldUserData.Save(int.Parse(userInfo.m_user.m_sSiteGUID));
            }
        }

        private string GetContentInfo(string key, Dictionary<string, string> info)
        {
            if (info.ContainsKey(key))
            {
                return info[key];
            }

            return string.Empty;
        }

        private void AddDDToList(string key, string val, ref List<UserDynamicDataContainer> uddc)
        {
            if (!string.IsNullOrEmpty(val))
            {
                UserDynamicDataContainer udd = new UserDynamicDataContainer();
                udd.m_sValue = val;
                udd.m_sDataType = key;
                uddc.Add(udd);
            }
        }
    }

}
