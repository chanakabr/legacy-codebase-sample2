using System;
using System.Collections.Generic;
using ApiObjects;

namespace Users
{
    public class SSOKdgImplementation : SSOUsers, ISSOProvider
    {
        public SSOKdgImplementation(int groupId)
            : base(groupId)
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
                    if (kdgLoginResp.Status == eKdgStatus.CredsOkInNetwork || kdgLoginResp.Status == eKdgStatus.CredsOkOutOfNetwork)
                    {
                        retResponseObject = HandleLoginRequest(username, pass, kdgLoginResp);
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

        private UserResponseObject HandleLoginRequest(string username, string pass, KdgLoginResp kdgLoginResp)
        {
            UserResponseObject retResponse = new UserResponseObject();

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
                            retResponse = usersImplementation.GetUserByUsername(username, m_nGroupID);
                            retResponse.m_RespStatus = domainResponse.m_oDomainResponseStatus == DomainResponseStatus.OK ? user.m_RespStatus : ResponseStatus.ErrorOnSaveDomain;
                        }
                        else retResponse = user;
                    }
                        //User and domain already exist: return OK
                    else
                    {
                        bool updateUserSuccess = usersImplementation.SetUserDynamicData(user.m_user.m_sSiteGUID, new List<KeyValuePair>() {new KeyValuePair("IPType", kdgLoginResp.Status.ToString())}, user);
                        retResponse = usersImplementation.GetUserByUsername(username, m_nGroupID);
                        retResponse.m_RespStatus = updateUserSuccess ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                    }
                }
                //Domain co_guid doesn't exists
                else
                {
                    //User exists: change domain co_guid
                    if (user.m_RespStatus == ResponseStatus.OK)
                    {
                        bool isChangeCoGuidSuccess = DAL.DomainDal.UpdateDomainCoGuid(user.m_user.m_domianID, m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                        retResponse = usersImplementation.GetUserByUsername(username, m_nGroupID);
                        retResponse.m_RespStatus = isChangeCoGuidSuccess ? ResponseStatus.OK : ResponseStatus.ErrorOnSaveUser;
                    }
                    //User doesn't exist: Add new user and domain
                    else
                    {
                        user = AddNewKdgUser(username, pass, kdgLoginResp.Status, usersImplementation);
                        if (user.m_RespStatus == ResponseStatus.UserWithNoDomain)
                        {
                            DomainResponseObject domainResponseObject = domainImplementation.AddDomain(kdgLoginResp.CustomerAccountNumber, "", int.Parse(user.m_user.m_sSiteGUID), m_nGroupID, kdgLoginResp.CustomerAccountNumber);
                            retResponse = usersImplementation.GetUserByUsername(username, m_nGroupID);
                            retResponse.m_RespStatus = domainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK ? ResponseStatus.OK : ResponseStatus.ErrorCreatingDomain;
                        }
                        else retResponse = user;
                    }
                }
            }
            return retResponse;
        }

        private UserResponseObject AddNewKdgUser(string username, string password, eKdgStatus ipType, BaseUsers usersImplementation)
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
                return new KdgLoginResp(){ Status = eKdgStatus.Unknown };
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
            Unknown = -1,
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
