using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class EutelsatDomain : TvinciDomain
    {

        protected EutelsatDomain()
        {
        }

        public EutelsatDomain(int nGroupID)
            : base(nGroupID)
        {
        }


        #region Overriden Methods
        protected override Domain DomainInitializer(int nGroupID, int nDomainID)
        {
            return InitializeDomain(nGroupID, nDomainID);
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid)
        {
            //Create new domain
            Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;


            if (string.IsNullOrEmpty(sCoGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }


            //Check if CoGuid already exists
            int nDomainID = DomainDal.GetDomainIDByCoGuid(sCoGuid);

            if (nDomainID > 0)
            {
                domain.m_DomainStatus = DomainStatus.DomainAlreadyExists;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainAlreadyExists);

                return oDomainResponseObject;
            }


            // CoGuid not found - create new domain

            UserResponseObject userResp = ValidateMasterUser(nGroupID, nMasterUserGuid);

            //Check if UserGuid is valid 
            if (userResp == null)
            {
                domain.m_DomainStatus = DomainStatus.Error;
                return (new DomainResponseObject(domain, DomainResponseStatus.Error));
            }

            if (userResp.m_RespStatus != ResponseStatus.OK)
            {
                if (userResp.m_RespStatus == ResponseStatus.UserEmailAlreadyExists)
                {
                    domain.m_DomainStatus = DomainStatus.MasterEmailAlreadyExists;
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.UserExistsInOtherDomains);
                }
                else
                {
                    domain.m_DomainStatus = DomainStatus.Error;
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
                }

                return oDomainResponseObject;
            }

            // Create new domain
            domain = CreateNewDomain(sDomainName, sDomainDescription, nGroupID, nMasterUserGuid, sCoGuid);

            if (domain != null)
            {
                domain = DomainFactory.CheckAddMonkey(domain);
                domain.m_DomainRestriction = DomainRestriction.DeviceMasterRestricted;
                Users.EutelsatUsers eUsers = new EutelsatUsers(m_nGroupID);
                bool sent = eUsers.SendWelcomePasswordMail(userResp);

                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
                return oDomainResponseObject;
            }

            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);

            return oDomainResponseObject;
        }

        /// <summary>
        /// Create new Domain record in DB
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nGroupID"></param>
        /// <param name="nLimit"></param>
        /// <returns>the new record ID</returns>
        public Domain CreateNewDomain(string sName, string sDescription, int nGroupID, int nMasterGuID, string sCoGuid)
        {
            Domain d = null;

            DateTime dDateTime = DateTime.Now;

            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nConcurrentLimit = 0;
            int nGroupConcurrentLimit = 0;
            int nDomainLimitID = 0;
            int nOperatorSubGroupID = 0;
            int nOperatorID = (-1);

            // Get domain's default limits identifier from parent group
            if (string.IsNullOrEmpty(sCoGuid))
            {
                nDomainLimitID = DAL.DomainDal.GetDomainDefaultLimitsID(nGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit);
            }
            else // Get domain's default limits identifier from sub-account group
            {

                string sOperatorCoGuid = string.Empty;
                int nHouseholdID = 0;
                if (Utils.GetUserOperatorAndHouseholdIDs(nGroupID, sCoGuid, ref nOperatorID, ref sOperatorCoGuid, ref nOperatorSubGroupID, ref nHouseholdID))
                {
                    if (nOperatorSubGroupID <= 0)
                    {
                        nOperatorSubGroupID = nGroupID;
                    }

                    nDomainLimitID = DomainDal.GetDomainDefaultLimitsID(nOperatorSubGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit);
                }
            }


            bool bInserRes = DomainDal.InsertNewDomain(sName, sDescription, nGroupID, dDateTime, nDomainLimitID, sCoGuid, nOperatorID);

            if (bInserRes)
            {
                int nDomainID = -1;
                int nIsActive = 0;
                int nStatus = 0;

                bool resDbObj = DomainDal.GetDomainDbObject(nGroupID, dDateTime,
                                                                ref sName, ref sDescription, ref nDomainID, ref nIsActive, ref nStatus, ref sCoGuid);

                if (resDbObj)
                {
                    d = new Domain();

                    d.m_sName = sName;
                    d.m_sDescription = sDescription;
                    d.m_nDomainID = nDomainID;
                    d.m_nIsActive = nIsActive;
                    d.m_nStatus = nStatus;
                    d.m_sCoGuid = sCoGuid;
                    d.m_nGroupID = nGroupID;

                    d.m_nDeviceLimit = d.m_nLimit = nDeviceLimit;
                    d.m_nUserLimit = nUserLimit;
                    d.m_nConcurrentLimit = nConcurrentLimit;

                    d.m_DomainStatus = DomainStatus.OK;

                    if (nOperatorSubGroupID > 0)
                    {
                        //d.m_deviceFamilies = d.InitializeDeviceFamilies(nDomainLimitID, nOperatorSubGroupID);
                        d.DeviceFamiliesInitializer(nDomainLimitID, nOperatorSubGroupID);
                    }
                    else
                    {
                        //d.m_deviceFamilies = d.InitializeDeviceFamilies(nDomainLimitID, nGroupID);
                        d.DeviceFamiliesInitializer(nDomainLimitID, nGroupID);
                    }

                    DomainResponseStatus res = d.AddUserToDomain(m_nGroupID, d.m_nDomainID, nMasterGuID, nMasterGuID, UserDomainType.Master);    //AddUserToDomain(m_nGroupID, m_nDomainID, nMasterGuID, true);

                    d.m_UsersIDs = new List<int>();
                    d.m_UsersIDs.Add(nMasterGuID);
                }
            }

            return d;
        }

        public override DomainResponseStatus RemoveDomain(int nDomainID)
        {
            if (nDomainID <= 0)
            {
                return DomainResponseStatus.DomainNotExists;
            }

            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID);

            if (domain == null)
            {
                return DomainResponseStatus.DomainNotExists;
            }


            // Create new response
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            //Init The Domain
            eDomainResponseStatus = domain.Remove();


            return eDomainResponseStatus;
        }


        public override DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername)
        {
            //New domain
            Domain domain = new Domain();

            // Create new response
            DomainResponseObject oDomainResponseObject;

            User masterUser = new User();
            int nMasterID = masterUser.InitializeByUsername(sMasterUsername, nGroupID);

            //Check if UserGuid is valid
            if (nMasterID <= 0 || masterUser.m_domianID <= 0 || !User.IsUserValid(nGroupID, nUserGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            domain = InitializeDomain(nGroupID, masterUser.m_domianID);

            oDomainResponseObject = domain.SubmitAddUserToDomainRequest(nGroupID, nUserGuid, sMasterUsername);

            return oDomainResponseObject;
        }

        public override DomainResponseObject ResetDomain(int nDomainID, int nFrequencyType = 0)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID);

            //Reset the domain
            DomainResponseStatus eDomainResponseStatus = domain.ResetDomain(nFrequencyType);

            //Re-Init domain to return updated data
            domain = InitializeDomain(m_nGroupID, nDomainID);

            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription)
        {

            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Init the domain according to domainId
            // Init Domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID);
            domain.m_sName = sDomainName;
            domain.m_sDescription = sDomainDescription;

            //Update the domain fields
            domain.Update();

            if (domain.m_DomainStatus == DomainStatus.OK)
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
            }
            else
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            return oDomainResponseObject;
        }

        public override DomainResponseObject ChangeDomainMaster(int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            //New domain
            Domain domain = new Domain();

            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Check if user IDs are valid
            if (!User.IsUserValid(m_nGroupID, nCurrentMasterID) || !User.IsUserValid(m_nGroupID, nNewMasterID))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            //Init The Domain
            domain = InitializeDomain(m_nGroupID, nDomainID);
            oDomainResponseObject = new DomainResponseObject() { m_oDomain = domain };

            if (domain != null && domain.m_DomainStatus == DomainStatus.OK)
            {
                //cannot set domain default user as master
                if (domain.m_DefaultUsersIDs != null && domain.m_DefaultUsersIDs.Contains(nNewMasterID))
                {
                    oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;
                }
                //cannot change master to user that's not in domain
                else if (domain.m_UsersIDs == null || !domain.m_UsersIDs.Contains(nNewMasterID))
                {
                    oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;
                }
                // No change required, return OK 
                else if (nNewMasterID == nCurrentMasterID)
                {
                    oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.OK;
                }
                else
                {
                    oDomainResponseObject.m_oDomainResponseStatus = domain.ChangeDomainMaster(m_nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);
                }
            }
            else
            {
                oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;
            }

            return oDomainResponseObject;
        }


        protected static Domain InitializeDomain(int nGroupID, int nDomainID)
        {
            string sDomainCoGuid = DomainDal.GetDomainCoGuid(nDomainID, null);

            if (string.IsNullOrEmpty(sDomainCoGuid))
            {
                return null;
            }

            string sOperatorCoGuid = string.Empty;
            int nOperatorID = 0;
            int nHouseholdID = 0;
            int nIpnoGroupID = 0;

            if (Utils.GetUserOperatorAndHouseholdIDs(nGroupID, sDomainCoGuid, ref nOperatorID, ref sOperatorCoGuid, ref nIpnoGroupID, ref nHouseholdID))
            {
                //Init The Domain by IPNO Sub Group Settings
                Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID, nIpnoGroupID);
                domain.m_nSSOOperatorID = nOperatorID;

                return domain;
            }

            return null;
        }


        protected UserResponseObject ValidateMasterUser(int nGroupID, int nUserID)
        {

            User masterUser = new User(nGroupID, nUserID);

            UserResponseObject resp = new UserResponseObject();

            if ((masterUser == null) || (string.IsNullOrEmpty(masterUser.m_oBasicData.m_sUserName)))
            {
                resp.Initialize(ResponseStatus.UserDoesNotExist, masterUser);
                return resp;
            }

            if (!string.IsNullOrEmpty(masterUser.m_oBasicData.m_sEmail))
            {
                List<int> lDomainsByMail = DAL.DomainDal.GetDomainIDsByEmail(nGroupID, masterUser.m_oBasicData.m_sEmail);

                if (lDomainsByMail != null && lDomainsByMail.Count > 0)
                {
                    resp.Initialize(ResponseStatus.UserEmailAlreadyExists, masterUser);
                    return resp;
                }
            }

            resp.Initialize(ResponseStatus.OK, masterUser);
            return resp;

        }


        #endregion

    }
}
