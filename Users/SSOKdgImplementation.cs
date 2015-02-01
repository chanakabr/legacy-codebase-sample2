using System;
using System.Collections.Generic;
using System.Net;
using ApiObjects;
using Newtonsoft.Json;

namespace Users
{
    public class SSOKdgImplementation : SSOUsers, ISSOProvider
    {
        public SSOKdgImplementation(int groupId, int operatorId)
            : base(groupId, operatorId)
        {

        }

        #region ISSOProviderImplementation

        public override UserResponseObject SignIn(string username, string pass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retResponseObject = new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };
            try
            {
                KdgLoginResp kdgLoginResp = ValidateCredentials(username, pass, sIP);
                if (kdgLoginResp != null)
                {
                    Logger.Logger.Log("KDG-SSO", string.Format("ValidateCredentials res:{0}", kdgLoginResp.Status), "KDG-SSO");

                    if (kdgLoginResp.Status == eKdgStatus.ServerDown)
                    {
                        retResponseObject.m_RespStatus = ResponseStatus.LoginServerDown;
                        Logger.Logger.Log("KDG-SSO", string.Format("No response from KDG Login Server down: UN:{0} Pass:{1}", username, pass), "KDG-SSO");
                    }
                    else if (kdgLoginResp.Status != eKdgStatus.BadCredsOutOfNetwork &&
                            kdgLoginResp.Status != eKdgStatus.BadCredsInNetwork &&
                            kdgLoginResp.Status != eKdgStatus.Unknown)
                    {
                        retResponseObject = HandleLoginRequest(username, pass, kdgLoginResp);
                    }
                }
            }
            catch (Exception ex)
            {
                retResponseObject.m_RespStatus = ResponseStatus.ErrorOnSaveUser;
                Logger.Logger.Log("KDG-SSO", string.Format("Error Signing in. ex:{0} UN|PASS={1}|{2}", ex.ToString(), username, pass), "KDG-SSO");
            }

            return retResponseObject;
        }


        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private UserResponseObject HandleLoginRequest(string username, string pass, KdgLoginResp kdgLoginResp)
        {
            UserResponseObject retResponse = new UserResponseObject();
            try
            {
                BaseDomain domainImplementation = null;
                Utils.GetBaseDomainsImpl(ref domainImplementation, m_nGroupID);

                BaseUsers usersImplementation = null;
                Utils.GetBaseUsersImpl(ref usersImplementation, m_nGroupID);
                if (domainImplementation != null && usersImplementation != null)
                {

                    UserResponseObject user = usersImplementation.GetUserByUsername(username, m_nGroupID);
                    DomainResponseObject domain = domainImplementation.GetDomainByCoGuid(kdgLoginResp.CustomerAccountNumber, m_nGroupID);

                    //Domain co_guid exists
                    if (domain.m_oDomainResponseStatus == DomainResponseStatus.OK)
                    {
                        //User doesn't exist
                        if (user.m_RespStatus != ResponseStatus.OK)
                        {
                            //Add new user
                            user = AddNewKdgUser(username, pass, kdgLoginResp.Status, usersImplementation);
                            if (user.m_RespStatus == ResponseStatus.UserWithNoDomain)
                            {
                                //Add user to domain
                                DomainResponseObject domainResponse = domainImplementation.AddUserToDomain(m_nGroupID, domain.m_oDomain.m_nDomainID, int.Parse(user.m_user.m_sSiteGUID), domain.m_oDomain.m_masterGUIDs[0]);
                                if (domainResponse.m_oDomainResponseStatus == DomainResponseStatus.OK)
                                {
                                    user.m_user.m_domianID = domainResponse.m_oDomain.m_nDomainID;
                                    user.m_RespStatus = ResponseStatus.OK;
                                }
                                else
                                {
                                    Logger.Logger.Log("Error adding user to domain", string.Format("username:{0} domainCoGuid:{1}", username, kdgLoginResp.CustomerAccountNumber), "KDG - SSO");
                                }
                            }
                        }
                        //User and domain already exist: return OK
                        else
                        {
                            if (user.m_user.m_oDynamicData.GetValByKey("ext_status") != ((int)kdgLoginResp.Status).ToString())
                            {
                                bool updateUserSuccess = usersImplementation.SetUserDynamicData(user.m_user.m_sSiteGUID, new List<KeyValuePair>() { new KeyValuePair("ext_status", ((int)kdgLoginResp.Status).ToString()) }, user);
                                if (!updateUserSuccess)
                                {
                                    Logger.Logger.Log("Error updating user dynamic data", string.Format("UN:{0} Pass:{1}", username, pass), "KDG - SSO");
                                }
                            }
                        }
                    }
                    //Domain co_guid doesn't exists
                    else
                    {
                        //User exists: change domain co_guid
                        if (user.m_RespStatus == ResponseStatus.OK)
                        {
                            bool isChangeCoGuidSuccess = DAL.DomainDal.UpdateDomainCoGuid(user.m_user.m_domianID, m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                            if (!isChangeCoGuidSuccess)
                            {
                                user.m_RespStatus = ResponseStatus.InternalError;
                                Logger.Logger.Log("Error updating domainCoGuid", string.Format("username:{0} newDomainCoGuid:{1} domainID:{2}", username, kdgLoginResp.CustomerAccountNumber, user.m_user.m_domianID), "KDG - SSO");
                            }
                        }
                        //User doesn't exist: Add new user and domain
                        else
                        {
                            user = AddNewKdgUser(username, pass, kdgLoginResp.Status, usersImplementation);
                            if (user.m_RespStatus == ResponseStatus.UserWithNoDomain)
                            {
                                DomainResponseObject domainResponseObject = domainImplementation.AddDomain(kdgLoginResp.CustomerAccountNumber, "", int.Parse(user.m_user.m_sSiteGUID), m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                                if (domainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK)
                                {
                                    user.m_user.m_domianID = domainResponseObject.m_oDomain.m_nDomainID;
                                    user.m_user.m_isDomainMaster = true;
                                    user.m_RespStatus = ResponseStatus.OK;
                                }
                                else
                                {
                                    Logger.Logger.Log("Error creating domain", string.Format("username:{0} domainCoGuid:{1}", username, kdgLoginResp.CustomerAccountNumber), "KDG-SSO");
                                }
                            }
                        }
                    }
                    retResponse = user;

                    if (user.m_RespStatus == ResponseStatus.OK)
                    {
                        DAL.UsersDal.UpdateHitDate(int.Parse(user.m_user.m_sSiteGUID));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("KDG-SSO", string.Format("Error Handing SignIn: ex:{0} UN|PASS={1}|{2}", ex.Message, username, pass), "KDG-SSO");
            }

            return retResponse;
        }

        private UserResponseObject AddNewKdgUser(string username, string password, eKdgStatus ext_status, BaseUsers usersImplementation)
        {
            try
            {

                UserBasicData userBasicData = new UserBasicData()
                {
                    m_sUserName = username,
                };
                UserDynamicData userDynamicData = new UserDynamicData()
                {
                    m_sUserData = new[]
                    {
                        new UserDynamicDataContainer()
                        {
                            m_sDataType = "ext_status",
                            m_sValue = ((int) ext_status).ToString()
                        }
                    }
                };
                return usersImplementation.AddNewUser(userBasicData, userDynamicData, Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("KDG-SSO", string.Format("Error adding new user - ex:{0} UN|PASS={1}|{2}", ex.Message, username, password), "KDG-SSO");
                return new UserResponseObject() { m_RespStatus = ResponseStatus.ErrorOnSaveUser };
            }

        }

        private KdgLoginResp ValidateCredentials(string username, string password, string clientIp)
        {
            try
            {
                Logger.Logger.Log("ValidateCredentials", string.Format("username:{0}, password:{1}, clientIp:{3}", username, password, clientIp), "KDG-SSO");
                KdgLoginResp kdgLoginRespObj = new KdgLoginResp() { Status = eKdgStatus.Unknown };
                string kdgUrl = TCMClient.Settings.Instance.GetValue<string>("KDG-AuthURL");

                if (!string.IsNullOrEmpty(kdgUrl))
                {
                    KdgLoginReq loginReq = new KdgLoginReq(username, password, clientIp);
                    using (WebClient client = new WebClient())
                    {
                        string reqStr = JsonConvert.SerializeObject(loginReq);
                        string kdgRespStr = client.UploadString(kdgUrl, reqStr);

                        Logger.Logger.Log("ValidateCredentials", string.Format("reqStr:{0}, kdgRespStr:{1}", reqStr, kdgRespStr), "KDG-SSO");

                        kdgLoginRespObj = JsonConvert.DeserializeObject<KdgLoginResp>(kdgRespStr);
                    }
                }
                else
                {
                    Logger.Logger.Log("KDG-SSO", "Error getting KDG URL From TCM", string.Format("{0}-KDG-SSO", DateTime.UtcNow.Date));
                }

                return kdgLoginRespObj;
            }
            catch (WebException we)
            {
                //Logger.Logger.Log("KDG-SSO", string.Format("No response from KDG Login Server down: ex:{0} UN:{1} Pass:{2}", we.ToString(), username, password), "KDG-SSO");
                return new KdgLoginResp() { Status = eKdgStatus.ServerDown };
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("KDG-SSO", string.Format("Error validating credentials with KDG: ex:{0} UN:{1} Pass:{2}", ex.ToString(), username, password), "KDG-SSO");
                return new KdgLoginResp() { Status = eKdgStatus.Unknown };
            }
        }



        #endregion

        #region KDG objects

        private class KdgLoginReq
        {
            public KdgLoginReq(string username, string password, string ip)
            {
                this.Username = username;
                this.Password = password;
                this.IP = ip;
            }

            [JsonProperty("username")]
            public string Username { get; set; }
            [JsonProperty("password")]
            public string Password { get; set; }
            [JsonProperty("clientIp")]
            public string IP { get; set; }
        }

        private class KdgLoginResp
        {
            [JsonProperty("accountnr")]
            public string CustomerAccountNumber { get; set; }
            [JsonProperty("result")]
            public eKdgStatus Status { get; set; }
        }

        public enum eKdgStatus
        {
            Unknown = 99,
            CredsOkInNetwork = 0,
            BadCredsOutOfNetwork = 1,
            BadCredsInNetwork = 2,
            CredsOkOutOfNetworkPackageDoesntExist = 3,
            CredsOkInNetworkPackageDoesntExist = 4,
            CredsOkOutOfNetwork = 5,
            ServerDown = 6
        }

        #endregion
    }
}
