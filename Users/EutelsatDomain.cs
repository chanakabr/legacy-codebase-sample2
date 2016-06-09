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
        protected override Domain DomainInitializer(int nGroupID, int nDomainID, bool bCache)
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
                return oDomainResponseObject;
            }                

            //Check if CoGuid already exists
            int nDomainID = DomainDal.GetDomainIDByCoGuid(sCoGuid, m_nGroupID);

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

            DateTime dDateTime = DateTime.UtcNow;

            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nConcurrentLimit = 0;
            int nGroupConcurrentLimit = 0;
            int nDomainLimitID = 0;
            int nOperatorSubGroupID = 0;
            int nOperatorID = (-1);
            int nDeviceFreqLimit = 0;

            // Get domain's default limits identifier from parent group
            if (string.IsNullOrEmpty(sCoGuid))
            {
                nDomainLimitID = DomainDal.GetDomainDefaultLimitsID(nGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit, ref nDeviceFreqLimit);
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

                    nDomainLimitID = DomainDal.GetDomainDefaultLimitsID(nOperatorSubGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit, ref nDeviceFreqLimit);
                }
            }


            bool bInserRes = DomainDal.InsertNewDomain(sName, sDescription, nGroupID, dDateTime, nDomainLimitID, sCoGuid, nOperatorID);

            if (bInserRes)
            {
                int nDomainID = -1;
                int nIsActive = 0;
                int nStatus = 0;
                int regionId = 0;

                bool resDbObj = DomainDal.GetDomainDbObject(nGroupID, dDateTime,
                                                                ref sName, ref sDescription, ref nDomainID, ref nIsActive, ref nStatus, ref sCoGuid, ref regionId);

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
                    d.InitializeLimitationsManager(nConcurrentLimit, nGroupConcurrentLimit, nDeviceLimit, nDeviceFreqLimit, Utils.FICTIVE_DATE);

                    d.m_DomainStatus = DomainStatus.OK;

                    if (nOperatorSubGroupID > 0)
                    {
                        d.DeviceFamiliesInitializer(nDomainLimitID, nOperatorSubGroupID);
                    }
                    else
                    {
                        d.DeviceFamiliesInitializer(nDomainLimitID, nGroupID);
                    }

                    DomainResponseStatus res = d.AddUserToDomain(m_nGroupID, d.m_nDomainID, nMasterGuID, nMasterGuID, UserDomainType.Master);

                    d.m_UsersIDs = new List<int>();
                    d.m_UsersIDs.Add(nMasterGuID);
                }
            }

            return d;
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


        #endregion

    }
}
