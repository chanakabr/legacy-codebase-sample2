using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ApiObjects;
using KLogMonitor;
using Newtonsoft.Json;
using Core.Users;

namespace SSOKdgImplementation
{
    public class User : KalturaSSOUsers, ISSOProvider
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public User(int groupId, int operatorId)
            : base(groupId, operatorId)
        {

        }

        #region ISSOProviderImplementation

        public override UserResponseObject PreSignIn(ref Int32 siteGuid, ref string userName, ref string password, ref int maxFailCount, ref int lockMin, ref int groupId, ref string sessionId, ref string ip, ref string deviceId, ref bool preventDoubleLogin, ref List<KeyValuePair> keyValueList)
        {
            UserResponseObject retResponseObject = new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };
            try
            {
                KdgLoginResp kdgLoginResp = ValidateCredentials(userName, password, ip);
                if (kdgLoginResp != null)
                {
                    log.Debug("KDG-SSO - " + string.Format("ValidateCredentials res:{0}", kdgLoginResp.Status));
                    if (kdgLoginResp.Status != eKdgStatus.BadCredsOutOfNetwork && kdgLoginResp.Status != eKdgStatus.BadCredsInNetwork && kdgLoginResp.Status != eKdgStatus.Unknown)
                    {
                        retResponseObject = HandleLoginRequest(userName, password, kdgLoginResp);
                    }
                }
            }
            catch (Exception ex)
            {
                retResponseObject.m_RespStatus = ResponseStatus.ErrorOnSaveUser;
                log.Error("KDG-SSO - " + string.Format("Error Signing in. ex:{0} UN|PASS={1}|{2}", ex.Message, userName, password), ex);
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
                DomainInfo domainInfo = new DomainInfo(GroupId);
                BaseDomain domainImplementation = null;
                Utils.GetBaseImpl(ref domainImplementation, GroupId);

                BaseUsers usersImplementation = null;
                Utils.GetBaseImpl(ref usersImplementation, GroupId);
                if (domainImplementation != null && usersImplementation != null)
                {
                    UserResponseObject user = usersImplementation.GetUserByUsername(username, GroupId);
                    DomainResponseObject domain = domainImplementation.GetDomainByCoGuid(kdgLoginResp.CustomerAccountNumber, GroupId);

                    if (domain.m_oDomainResponseStatus == DomainResponseStatus.OK)
                    {
                        // domain co_guid exists
                        if (user.m_RespStatus != ResponseStatus.OK)
                        {
                            // user doesn't exist - add new user to an existing domain
                            domainInfo.AddDomainType = DomainInfo.eAddToDomainType.AddToExistingDomain;
                            domainInfo.DomainMasterId = domain.m_oDomain.m_masterGUIDs[0];
                            domainInfo.DomainId = domain.m_oDomain.m_nDomainID;
                            user = AddNewKdgUser(username, pass, kdgLoginResp.Status, usersImplementation, domainInfo);
                        }
                        else
                        {
                            // user and domain already exist: return OK
                            if (user.m_user.m_oDynamicData.GetValByKey("ext_status") != ((int)kdgLoginResp.Status).ToString())
                            {
                                bool updateUserSuccess = usersImplementation.SetUserDynamicData(user.m_user.m_sSiteGUID, new List<KeyValuePair>() { new KeyValuePair("ext_status", ((int)kdgLoginResp.Status).ToString()) }, user);
                                if (!updateUserSuccess)
                                    log.Error("Error updating user dynamic data - " + string.Format("UN:{0} Pass:{1}", username, pass));
                            }
                        }
                    }
                    else
                    {
                        // domain co_guid doesn't exists    
                        if (user.m_RespStatus == ResponseStatus.OK)
                        {
                            // user exists - update domain co_guid
                            bool isChangeCoGuidSuccess = DAL.DomainDal.UpdateDomainCoGuid(user.m_user.m_domianID, GroupId, kdgLoginResp.CustomerAccountNumber);
                            if (!isChangeCoGuidSuccess)
                            {
                                user.m_RespStatus = ResponseStatus.InternalError;
                                log.Error("Error updating domainCoGuid - " + string.Format("username:{0} newDomainCoGuid:{1} domainID:{2}", username, kdgLoginResp.CustomerAccountNumber, user.m_user.m_domianID));
                            }
                        }

                        else
                        {
                            // user doesn't exist - Add a new user to a new domain
                            domainInfo.AddDomainType = DomainInfo.eAddToDomainType.CreateNewDomain;
                            domainInfo.DomainCoGuid = kdgLoginResp.CustomerAccountNumber;
                            user = AddNewKdgUser(username, pass, kdgLoginResp.Status, usersImplementation, domainInfo);
                        }
                    }
                    retResponse = user;
                    if (user.m_RespStatus == ResponseStatus.OK)
                        DAL.UsersDal.UpdateHitDate(int.Parse(user.m_user.m_sSiteGUID));
                }
            }
            catch (Exception ex)
            {
                log.Error("KDG-SSO - " + string.Format("Error Handing SignIn: ex:{0} UN|PASS={1}|{2}", ex.Message, username, pass), ex);
            }

            return retResponse;
        }

        private UserResponseObject AddNewKdgUser(string username, string password, eKdgStatus ext_status, BaseUsers usersImplementation, DomainInfo domainInfo)
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

                return FlowManager.AddNewUser(this, userBasicData, userDynamicData, Guid.NewGuid().ToString(), new List<KeyValuePair>(), domainInfo);
            }
            catch (Exception ex)
            {
                log.Error("KDG-SSO - " + string.Format("Error adding new user - ex:{0} UN|PASS={1}|{2}", ex.Message, username, password), ex);
                return new UserResponseObject() { m_RespStatus = ResponseStatus.ErrorOnSaveUser };
            }
        }

        private KdgLoginResp ValidateCredentials(string username, string password, string clientIp)
        {
            try
            {
                log.Debug("ValidateCredentials - " + string.Format("username:{0}, password:{1}, clientIp:{3}", username, password, clientIp));
                KdgLoginResp kdgLoginRespObj = new KdgLoginResp() { Status = eKdgStatus.Unknown };
                string kdgUrl = TCMClient.Settings.Instance.GetValue<string>("KDG-AuthURL");
                if (!string.IsNullOrEmpty(kdgUrl))
                {
                    KdgLoginReq loginReq = new KdgLoginReq(username, password, clientIp);
                    using (WebClient client = new WebClient())
                    {
                        string reqStr = JsonConvert.SerializeObject(loginReq);
                        string kdgRespStr = client.UploadString(kdgUrl, reqStr);

                        log.Debug("ValidateCredentials - " + string.Format("reqStr:{0}, kdgRespStr:{1}", reqStr, kdgRespStr));

                        kdgLoginRespObj = JsonConvert.DeserializeObject<KdgLoginResp>(kdgRespStr);
                    }
                }
                else
                    log.Error("KDG-SSO - Error getting KDG URL From TCM " + string.Format("{0}-KDG-SSO", DateTime.UtcNow.Date));
                return kdgLoginRespObj;
            }
            catch (Exception ex)
            {
                log.Error("KDG-SSO - " + string.Format("Error validating credentials with KDG: ex:{0} UN:{1} Pass:{2}", ex.Message, username, password), ex);
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
            CredsOkOutOfNetwork = 5
        }

        #endregion
    }
}
