using System;

namespace Users
{
    public class SSOKdgImplementation : SSOUsers, ISSOProviderImplementation
    {
        public SSOKdgImplementation(int groupId)
            : base(groupId)
        {

        }

        #region ISSOProviderImplementation

        public UserResponseObject SignIn(string wsUsername, string wsPass, string username, string pass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retResponseObject = new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };

            KdgLoginResp kdgLoginResp = ValidateCredentials(username, pass, sIP);
            if (kdgLoginResp != null)
            {
                if (kdgLoginResp.Status == eKdgStatus.CredsOkInNetwork || kdgLoginResp.Status == eKdgStatus.CredsOkOutOfNetwork)
                {
                    BaseDomain domainImplementation = null;
                    Utils.GetGroupID(wsUsername, wsPass, "SSOSignIn", ref domainImplementation);
                    if (domainImplementation != null)
                    {

                        UserResponseObject user = base.GetUserByUsername(username, m_nGroupID);
                        int domainId = domainImplementation.GetDomainIDByCoGuid(kdgLoginResp.CustomerAccountNumber);

                        //Domain co_guid exists
                        if (domainId != 0)
                        {
                            //User doesn't exist
                            if (user.m_RespStatus != ResponseStatus.OK)
                            {
                                user = AddNewUser(wsUsername, wsPass, username, pass, kdgLoginResp.Status);
                                retResponseObject.m_RespStatus = user.m_RespStatus;
                            }
                            //User and domain already exist: return OK
                            else retResponseObject.m_RespStatus = ResponseStatus.OK;
                        }
                        //Domain co_guid doesn't exists
                        else
                        {
                            //User exists: change domain co_guid
                            if (user.m_RespStatus == ResponseStatus.OK)
                            {
                                bool IsChangeCoGuidSuccess = DAL.DomainDal.UpdateDomainCoGuid(user.m_user.m_domianID, m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                                retResponseObject.m_RespStatus = IsChangeCoGuidSuccess ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                            }
                            //User doesn't exist: Add new user and domain
                            else
                            {
                                user = AddNewUser(wsUsername, wsPass, username, pass, kdgLoginResp.Status);
                                if (user.m_RespStatus == ResponseStatus.OK)
                                {
                                    DomainResponseObject domainResponseObject = domainImplementation.AddDomain("", "", int.Parse(user.m_user.m_sSiteGUID), m_nGroupID);
                                    retResponseObject.m_RespStatus = domainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                                }
                                else retResponseObject.m_RespStatus = ResponseStatus.ErrorOnSaveUser;
                            }
                        }
                    }
                }
            }
            return retResponseObject;
        }


        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private UserResponseObject AddNewUser(string wsUsername, string wsPass, string username, string password, eKdgStatus ipType)
        {
            BaseUsers usersImplementation = null;
            Utils.GetGroupID(wsUsername, wsPass, "SSOSignIn", ref usersImplementation);
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
                        m_sDataType = "IPType",
                        m_sValue = ipType.ToString()
                    }
                }
            };
            return usersImplementation.AddNewUser(userBasicData, userDynamicData, password);
        }

        private KdgLoginResp ValidateCredentials(string username, string password, string clientIp)
        {
            try
            {
                using (KDGApi.KDGSubscriberAuthPortTypeClient client = new KDGApi.KDGSubscriberAuthPortTypeClient())
                {
                    string accountIdentifier;
                    eKdgStatus status = (eKdgStatus)client.authenticate(username, password, clientIp, out accountIdentifier);
                    return new KdgLoginResp()
                    {
                        CustomerAccountNumber = accountIdentifier,
                        Status = status
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region KDG objects

        private class KdgLoginResp
        {
            public string CustomerAccountNumber { get; set; }
            public eKdgStatus Status { get; set; }
        }

        public enum eKdgStatus
        {
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
