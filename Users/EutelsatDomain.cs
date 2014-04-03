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

        public override Domain GetDomainInfo(int nDomainID, int nGroupID)
        {
            //New domain
            Domain domain = InitializeDomain(nGroupID, nDomainID);
            //Domain domain = new Domain();

            //Init the Domain
            //bool bInit = InitializeDomain(nGroupID, domainID, ref domain);

            return domain;
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
            int nDomainID = DAL.DomainDal.GetDomainIDByCoGuid(sCoGuid);

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

                //domain = (EutelsatDomain)domain.CreateNewDomain(sDomainName, sDomainDescription, nGroupID, nMasterUserGuid, sCoGuid);

                //Send Wellcome Email
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

                    nDomainLimitID = DAL.DomainDal.GetDomainDefaultLimitsID(nOperatorSubGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit);
                }
            }


            bool bInserRes = DAL.DomainDal.InsertNewDomain(sName, sDescription, nGroupID, dDateTime, nDomainLimitID, sCoGuid, nOperatorID);

            if (bInserRes)
            {
                //string sName = string.Empty;
                //string sDescription = string.Empty;
                int nDomainID = -1;
                int nIsActive = 0;
                int nStatus = 0;

                //Domain domainDbObj = this;
                bool resDbObj = DAL.DomainDal.GetDomainDbObject(nGroupID, dDateTime, //nDeviceLimit, nUserLimit, nConcurrentLimit, nDomainLimitID,
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
                        d.m_deviceFamilies = d.InitializeDeviceFamilies(nDomainLimitID, nOperatorSubGroupID);
                    }
                    else
                    {
                        d.m_deviceFamilies = d.InitializeDeviceFamilies(nDomainLimitID, nGroupID);
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
            //DomainResponseObject oDomainResponseObject;
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            //Init The Domain
            //bool bInit = InitializeDomain(m_nGroupID, nDomainID, ref domain);

            //if (bInit)
            //{
                eDomainResponseStatus = domain.Remove();
            //}
            //else
            //{
            //    eDomainResponseStatus = domain.TryRemove();
            //}

            //Re-Init domain to return updated data
            //bInit = InitializeDomain(m_nGroupID, nDomainID, ref domain);
            //oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return eDomainResponseStatus;
        }

        public override DomainResponseObject AddUserToDomain(int nGroupID, int domainID, int userGuid, int nMasterUserGuid, bool bIsMaster)
        {
            //New domain
            Domain domain = new Domain();

            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Check if UserGuid is valid
            if (!User.IsUserValid(nGroupID, userGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            domain = InitializeDomain(nGroupID, domainID);
            //bool bInit = InitializeDomain(nGroupID, domainID, nMasterUserGuid, ref domain);

            //Add new User to Domain
            UserDomainType userDomainType = bIsMaster ? UserDomainType.Master : UserDomainType.Regular;
            DomainResponseStatus eDomainResponseStatus = domain.AddUserToDomain(nGroupID, domainID, userGuid, nMasterUserGuid, userDomainType);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
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
            if ((!User.IsUserValid(nGroupID, nUserGuid)) || (nMasterID <= 0) || (masterUser.m_domianID <= 0))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            domain = InitializeDomain(nGroupID, masterUser.m_domianID);
            //bool bInit = InitializeDomain(nGroupID, masterUser.m_domianID, nMasterID, ref domain);

            //Add new User to Domain
            //DomainResponseStatus eDomainResponseStatus = domain.AddUserToDomainByMasterEmail(nGroupID, nDomainID, nUserGuid, sMasterUserEmail);

            oDomainResponseObject = domain.SubmitAddUserToDomainRequest(nGroupID, nUserGuid, sMasterUsername);  //new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string sDeviceName, int nBrandID)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = InitializeDomain(nGroupID, nDomainID);

            Device device = new Device(sUDID, nBrandID, m_nGroupID, sDeviceName, nDomainID);
            bool devInit = device.Initialize(sUDID);
            device.m_deviceName = sDeviceName;

            //Add new Device to Domain
            DomainResponseStatus eDomainResponseStatus = domain.AddDeviceToDomain(m_nGroupID, nDomainID, sUDID, sDeviceName, nBrandID, ref device);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject SubmitAddDeviceToDomainRequest(int nGroupID, int nDomainID, int nUserID, string sUDID, string sDeviceName, int nBrandID)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0)
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.Error);
            }

            // Initialize domain
            Domain domain = InitializeDomain(nGroupID, nDomainID);

            Device device = new Device(sUDID, nBrandID, m_nGroupID, sDeviceName, nDomainID);

            // If domain is restricted and action user is the master, just add the device
            if ((domain.m_DomainRestriction == DomainRestriction.Unrestricted) ||
                (domain.m_DomainRestriction == DomainRestriction.UserMasterRestricted) ||
                ((domain.m_DomainRestriction == DomainRestriction.DeviceMasterRestricted || domain.m_DomainRestriction == DomainRestriction.DeviceUserMasterRestricted) &&
                (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0) && (domain.m_masterGUIDs.Contains(nUserID))))
            {
                DomainResponseStatus eDomainResponseStatus = domain.AddDeviceToDomain(nGroupID, domain.m_nDomainID, sUDID, sDeviceName, nBrandID, ref device);
                oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

                return oDomainResponseObject;
            }

            DomainResponseStatus eDomainResponseStatus1 = domain.SubmitAddDeviceToDomainRequest(nGroupID, sUDID, sDeviceName, ref device);

            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus1);

            return oDomainResponseObject;
        }

        public override DomainResponseObject RemoveDeviceFromDomain(int nDomainID, string sDeviceUDID)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID);

            //Remove Device from Domain
            DomainResponseStatus eDomainResponseStatus = domain.RemoveDeviceFromDomain(sDeviceUDID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject ChangeDeviceDomainStatus(int nDomainID, string sDeviceUDID, bool bIsEnable)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID); //new Domain();

            // Change DeVice data
            DomainResponseStatus eDomainResponseStatus = domain.ChangeDeviceDomainStatus(m_nGroupID, nDomainID, sDeviceUDID, bIsEnable);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject RemoveUserFromDomain(int nGroupID, int nDomainID, int nUserID)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0 || nUserID <= 0)
            {
                return (new DomainResponseObject(null, DomainResponseStatus.UnKnown));
            }

            //Init the Domain
            Domain domain = InitializeDomain(nGroupID, nDomainID);

            if (domain == null)
            {
                //domain.m_DomainStatus = DomainStatus.Error;
                return (new DomainResponseObject(null, DomainResponseStatus.DomainNotInitialized));
            }
                     

            //Delete the User from Domain
            DomainResponseStatus eDomainResponseStatus = domain.RemoveUserFromDomain(nGroupID, nDomainID, nUserID);
            
            domain = InitializeDomain(nGroupID, nDomainID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DeviceResponseObject RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName)
        {
            Domain domain = InitializeDomain(nGroupID, nDomainID);  //new Domain();
            
            // Init Domain
            //bool bInit = InitializeDomain(nGroupID, nDomainID, ref domain);

            DeviceResponseStatus eRetVal = DeviceResponseStatus.UnKnown;
            Device device = domain.RegisterDeviceToDomainWithPIN(nGroupID, sPIN, nDomainID, sDeviceName, ref eRetVal);
            
            return new DeviceResponseObject(device, eRetVal);
        }

        public override DomainResponseObject ResetDomain(int nDomainID, int nFrequencyType = 0)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID); //new Domain();

            //Reset the domain
            DomainResponseStatus eDomainResponseStatus = domain.ResetDomain(nFrequencyType);

            //Re-Init domain to return updated data
            domain = InitializeDomain(m_nGroupID, nDomainID);
            //bInit = InitializeDomain(m_nGroupID, nDomainID, ref domain); //domain.Initialize(m_nGroupID, nDomainID);

            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription)
        {
            //New domain
            //Domain domain = new Domain();

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

        public override bool SetDomainRestriction(int nDomainID, DomainRestriction rest)
        {
            //New domain
            Domain domain = InitializeDomain(m_nGroupID, nDomainID);

            domain.m_DomainRestriction = rest;
            bool res = domain.Update();

            return res;
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

            // No change required, return OK 
            if (nNewMasterID == nCurrentMasterID)
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
            }


            DomainResponseStatus eDomainResponseStatus = domain.ChangeDomainMaster(m_nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);

            domain = InitializeDomain(m_nGroupID, nDomainID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }


        protected static Domain InitializeDomain(int nGroupID, int nDomainID)
        {
            string sDomainCoGuid = DAL.DomainDal.GetDomainCoGuid(nDomainID, null);

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
                Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID, nIpnoGroupID);    //domain.Initialize(nGroupID, nDomainID, nIpnoGroupID);
                domain.m_nSSOOperatorID = nOperatorID;

                return domain;
            }
            //else //Init The Domain by Parent Group Settings
            //{
            //    domain = DomainFactory.GetDomain(nGroupID, nDomainID);    //domain.Initialize(nGroupID, nDomainID);
            //}

            return null;
        }


        protected UserResponseObject ValidateMasterUser(int nGroupID, int nUserID)
        {
            //Check if UserGuid is valid
            //User user = new User();

            User masterUser = new User(nGroupID, nUserID);      //bool initRes = user.Initialize(userGuid, nGroupID);

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
