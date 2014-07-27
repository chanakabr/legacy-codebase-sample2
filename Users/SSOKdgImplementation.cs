using System;

namespace Users
{
    public class SSOKdgImplementation : SSOUsers, ISSOProvider
    {
        public SSOKdgImplementation(int groupId)
            : base(groupId)
        {

        }

        #region ISSOProvider implementation

        public UserResponseObject SignIn(string wsUsername, string wsPass, string username, string pass, int nOperatorID, int nMaxFailCount, int nLockMinutes, string sSessionID, string sIP, string sDeviceID, bool bPreventDoubleLogins)
        {
            UserResponseObject retResponseObject = new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };
            try
            {
                KdgLoginResp kdgLoginResp = ValidateCredentials(username, pass, sIP);
                if (kdgLoginResp != null)
                {
                    if (kdgLoginResp.Status == eKdgStatus.CredsOkInNetwork || kdgLoginResp.Status == eKdgStatus.CredsOkOutOfNetwork)
                    {
                        retResponseObject.m_RespStatus = HandleLoginRequest(wsUsername, wsPass, username, pass, kdgLoginResp);
                    }
                }
            }
            catch (Exception)
            {
                retResponseObject.m_RespStatus = ResponseStatus.ErrorOnSaveUser;
            }
            return retResponseObject;
        }

        public UserResponseObject CheckLogin(string sUserName, int nOperatorID)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Private methods

        private ResponseStatus HandleLoginRequest(string wsUsername, string wsPass, string username, string pass, KdgLoginResp kdgLoginResp)
        {
            ResponseStatus retResponseStatus = ResponseStatus.ErrorOnSaveUser;

            BaseDomain domainImplementation = null;
            Utils.GetGroupID(wsUsername, wsPass, "SSOSignIn", ref domainImplementation);
            if (domainImplementation != null)
            {

                UserResponseObject user = base.GetUserByUsername(username, m_nGroupID);
                DomainResponseObject domain = domainImplementation.GetDomainByCoGuid(kdgLoginResp.CustomerAccountNumber, m_nGroupID);

                //Domain co_guid exists
                if (domain.m_oDomainResponseStatus == DomainResponseStatus.OK)
                {
                    //User doesn't exist
                    if (user.m_RespStatus != ResponseStatus.OK)
                    {
                        //Add new user
                        user = AddNewKdgUser(wsUsername, wsPass, username, pass, kdgLoginResp.Status);
                        if (user.m_RespStatus == ResponseStatus.OK)
                        {
                            //Add user to domain
                            DomainResponseObject domainResponse = domainImplementation.AddUserToDomain(m_nGroupID, domain.m_oDomain.m_nDomainID, int.Parse(user.m_user.m_sSiteGUID), domain.m_oDomain.m_masterGUIDs[0]);
                            retResponseStatus = domainResponse.m_oDomainResponseStatus == DomainResponseStatus.OK ? user.m_RespStatus : ResponseStatus.ErrorOnSaveUser;
                        }
                        else retResponseStatus = ResponseStatus.ErrorOnSaveUser;
                    }
                    //User and domain already exist: return OK
                    else retResponseStatus = ResponseStatus.OK;
                }
                //Domain co_guid doesn't exists
                else
                {
                    //User exists: change domain co_guid
                    if (user.m_RespStatus == ResponseStatus.OK)
                    {
                        bool isChangeCoGuidSuccess = DAL.DomainDal.UpdateDomainCoGuid(user.m_user.m_domianID, m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                        retResponseStatus = isChangeCoGuidSuccess ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                    }
                    //User doesn't exist: Add new user and domain
                    else
                    {
                        user = AddNewKdgUser(wsUsername, wsPass, username, pass, kdgLoginResp.Status);
                        if (user.m_RespStatus == ResponseStatus.OK)
                        {
                            DomainResponseObject domainResponseObject = domainImplementation.AddDomain("", "", int.Parse(user.m_user.m_sSiteGUID), m_nGroupID);
                            retResponseStatus = domainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                        }
                        else retResponseStatus = ResponseStatus.ErrorOnSaveUser;
                    }
                }
            }
            return retResponseStatus;
        }

        private UserResponseObject AddNewKdgUser(string wsUsername, string wsPass, string username, string password, eKdgStatus ipType)
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
