using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class TvinciDomain : BaseDomain
    {

        protected TvinciDomain()
        {
        }

        public TvinciDomain(int groupID)
            : base(groupID)
        {
        }

        //Override Methods
        public override int GetDomainIDByCoGuid(string coGuid)
        {
            int domainID = DAL.DomainDal.GetDomainIDByCoGuid(coGuid);

            return domainID;
        }

        public override DomainResponseObject GetDomainByCoGuid(string coGuid, int nGroupID)
        {
            int nDomainID = GetDomainIDByCoGuid(coGuid);

            //Create new domain
            Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0)
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);

                return oDomainResponseObject;
            }

            domain = DomainFactory.GetDomain(nGroupID, nDomainID);  //domain = GetDomainInfo(nDomainID, nGroupID);
            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        public override int[] GetDomainIDsByOperatorCoGuid(string coGuid)
        {
            if (string.IsNullOrEmpty(coGuid))
            {
                return new int[] { };
            }

            List<int> lDomainIDs = DAL.DomainDal.GetDomainIDsByOperatorCoGuid(coGuid);

            return lDomainIDs.ToArray();
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid)
        {
            //DomainResponseObject oDomainResponseObject;

            //Create new domain
            Domain domain = DomainFactory.CreateDomain(sDomainName.Trim(), sDomainDescription.Trim(), nMasterUserGuid, nGroupID, sCoGuid);

            DomainResponseObject oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.UnKnown);

            if (domain.m_DomainStatus != DomainStatus.OK)
            {
                if (domain.m_DomainStatus == DomainStatus.DomainAlreadyExists)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainAlreadyExists);
                }
                else if (domain.m_DomainStatus == DomainStatus.HouseholdUserFailed)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.HouseholdUserFailed);
                }
                else if (domain.m_DomainStatus == DomainStatus.Error)
                {
                    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
                }

                return oDomainResponseObject;
            }

            #region Commented
            //Domain domain = new Domain();
            // Create new response
            //DomainResponseObject oDomainResponseObject;

            //Check if UserGuid is valid 
            //if (!User.IsUserValid(nGroupID, nMasterUserGuid))
            //{
            //    domain.m_DomainStatus = DomainStatus.Error;
            //    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            //
            //    return oDomainResponseObject;
            //}

            //if (!string.IsNullOrEmpty(sCoGuid))
            //{
            //
            //    //Check if CoGuid already exists
            //    int nDomainID = DAL.DomainDal.GetDomainIDByCoGuid(sCoGuid);
            //
            //    if (nDomainID > 0)
            //    {
            //        domain.m_DomainStatus = DomainStatus.DomainAlreadyExists;
            //        oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainAlreadyExists);
            //
            //        return oDomainResponseObject;
            //    }
            //}

            // Create new domain
            //Domain oNewDomain = domain.CreateNewDomain(sDomainName, sDomainDescription, nGroupID, nMasterUserGuid, sCoGuid);


            // Add Monkey User for Household Devices (STB, ConnectedTV)
            //User user = new User();
            //bool initRes = user.Initialize(nMasterUserGuid, nGroupID);

            //if (initRes)
            //{
            //    User monkeyUser = new User();
            //    UserBasicData basicOfMonkey = (UserBasicData)user.m_oBasicData.Clone();
            //    UserDynamicData dynamicOfMonkey = (UserDynamicData)user.m_oDynamicData.Clone();

            //    basicOfMonkey.m_sUserName = (oNewDomain.m_nDomainID + Guid.NewGuid().ToString());
                
            //    bool monkeyCreated = monkeyUser.Initialize(basicOfMonkey, dynamicOfMonkey, nGroupID, basicOfMonkey.m_sPassword);
            //    DomainResponseStatus addedMonkey = oNewDomain.AddUserToDomain(nGroupID, oNewDomain.m_nDomainID, int.Parse(monkeyUser.m_sSiteGUID), nMasterUserGuid, UserDomainType.Monkey);

            //    if ((!monkeyCreated) || (string.IsNullOrEmpty(monkeyUser.m_sSiteGUID)) || (addedMonkey != DomainResponseStatus.OK))
            //    {
            //        oDomainResponseObject = new DomainResponseObject(oNewDomain, DomainResponseStatus.HouseholdUserFailed);
            //        return oDomainResponseObject;
            //    }
            //}
            #endregion

            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        public override DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID)
        {
            return AddDomain(sDomainName, sDomainDescription, nMasterUserGuid, nGroupID, "");
        }

        public override DomainResponseStatus RemoveDomain(int nDomainID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);
            //domain.m_nDomainID = nDomainID;
            //domain.m_nGroupID = m_nGroupID;

            // Create new response
            //DomainResponseObject oDomainResponseObject;
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            //Init The Domain
            //bool init = domain.Initialize(m_nGroupID, nDomainID);

            //if (!init && domain.m_DomainStatus == DomainStatus.DomainNotExists)

            if ((domain != null) && (domain.m_DomainStatus != DomainStatus.OK)) // || domain.m_DomainStatus == DomainStatus.DomainNotExists)
            {
                eDomainResponseStatus = domain.TryRemove();

                //eDomainResponseStatus = DomainResponseStatus.DomainNotExists;
                //oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

                //return oDomainResponseObject;
            }
            else
            {
                //Remove the domain
                eDomainResponseStatus = domain.Remove();
            }

            //Re-Init domain to return updated data
            //init = domain.Initialize(m_nGroupID, nDomainID);
            //oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return eDomainResponseStatus;
        }

        public override DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = DomainFactory.GetDomain(sDomainName, sDomainDescription, nGroupID, nDomainID);

            //Domain domain = new Domain();
            //Init the domain according to domainId
            //domain.Initialize(sDomainName, sDomainDescription, nGroupID, nDomainID);

            //Update the domain fields
            bool updated = domain.Update();

            if (updated && (domain.m_DomainStatus == DomainStatus.OK))
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
            }
            else
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            return oDomainResponseObject;
        }

        public override DomainResponseObject AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string sDeviceName, int nBrandID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);
            
            //Domain domain = new Domain();
            // Create new response
            
            DomainResponseObject oDomainResponseObject;
            
            //Init The Domain
            //domain.Initialize(m_nGroupID, nDomainID);

            Device device = new Device(sUDID, nBrandID, m_nGroupID, sDeviceName, nDomainID);
            bool init = device.Initialize(sUDID, sDeviceName);
            //device.m_deviceName = sDeviceName;

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

            //New domain
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

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

        public override DomainResponseObject ConfirmDeviceByDomainMaster(string sMasterUN, string sUDID, string sToken)
        {
            DomainResponseObject resp = new DomainResponseObject();

            // Check the Master User
            List<int> lGroupIDs = DAL.UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int masterUserID = DAL.UsersDal.GetUserIDByUsername(sMasterUN, arrGroupIDs);

            User masterUser = new User();
            bool bInit = masterUser.Initialize(masterUserID, m_nGroupID);

            if (masterUserID <= 0 || !bInit || !masterUser.m_isDomainMaster)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.ActionUserNotMaster;
                return resp;
            }

            // Check if such device exists
            Device device = new Device(m_nGroupID);
            bInit = device.Initialize(sUDID);
            int nDeviceID = int.Parse(device.m_id);


            //New domain
            Domain domain = new Domain();
            bInit = bInit && domain.Initialize(m_nGroupID, masterUser.m_domianID);

            if (!bInit)
            {
                resp.m_oDomain = domain;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DeviceNotExists;
                return resp;
            }


            int nDomainDeviceID = 0;
            int nTokenDeviceID = DAL.DomainDal.GetDeviceIDByDomainActivationToken(m_nGroupID, sToken, ref nDomainDeviceID);

            if (nDeviceID != nTokenDeviceID)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DeviceNotConfirmed;
                return resp;
            }


            string sNewGuid = Guid.NewGuid().ToString();
            bool isActivated = DAL.DomainDal.UpdateDeviceDomainActivationToken(m_nGroupID, nDomainDeviceID, sToken, sNewGuid);

            int nActivationStatus = DAL.DomainDal.GetDomainDeviceActivateStatus(m_nGroupID, nDeviceID);

            resp.m_oDomain = (nActivationStatus == 1) ? domain : null;
            resp.m_oDomainResponseStatus = (nActivationStatus == 1) ? DomainResponseStatus.OK : DomainResponseStatus.DeviceNotConfirmed;

            return resp;
        }

        public override DomainResponseObject RemoveDeviceFromDomain(int nDomainID, string deviceUDID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);     //Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Init The Domain
            //domain.Initialize(m_nGroupID, domainID);

            //Remove Device from Domain
            DomainResponseStatus eDomainResponseStatus = domain.RemoveDeviceFromDomain(deviceUDID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject ChangeDeviceDomainStatus(int nDomainID, string deviceUDID, bool isEnable)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);     //Domain domain = new Domain();

            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Init The Domain
            //domain.Initialize(m_nGroupID, domainID);

            // Change DeVice data
            DomainResponseStatus eDomainResponseStatus = domain.ChangeDeviceDomainStatus(m_nGroupID, nDomainID, deviceUDID, isEnable);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override List<string> GetDomainUserList(int domainID, int groupID)
        {
            return Domain.GetFullUserList(domainID, groupID);
        }

        /// <summary>
        /// AddUserToDomain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="domainID"></param>
        /// <param name="userGuid"></param>
        /// <param name="nMasterUserGuid"></param>
        /// <param name="bIsMaster"></param>
        /// <returns></returns>
        public override DomainResponseObject AddUserToDomain(int nGroupID, int nDomainID, int userGuid, int nMasterUserGuid, bool bIsMaster)
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

            //Init The Domain
            domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);    //domain.Initialize(nGroupID, domainID);

            //Add new User to Domain
            UserDomainType userType = bIsMaster ? UserDomainType.Master : UserDomainType.Regular;
            DomainResponseStatus eDomainResponseStatus = domain.AddUserToDomain(nGroupID, nDomainID, userGuid, nMasterUserGuid, userType);   //bIsMaster);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        /// <summary>
        /// SubmitAddUserToDomainRequest
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nUserGuid"></param>
        /// <param name="sMasterUsername"></param>
        /// <returns></returns>
        public override DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername)
        {
            //New domain
            Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Check if UserGuid is valid
            if (!User.IsUserValid(nGroupID, nUserGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }


            //Init The Domain
            //domain.Initialize(nGroupID, nDomainID);

            //Add new User to Domain
            //DomainResponseStatus eDomainResponseStatus = domain.AddUserToDomainByMasterEmail(nGroupID, nDomainID, nUserGuid, sMasterUserEmail);

            oDomainResponseObject = domain.SubmitAddUserToDomainRequest(nGroupID, nUserGuid, sMasterUsername);

            return oDomainResponseObject;
        }

        /// <summary>
        /// RemoveUserFromDomain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="domainID"></param>
        /// <param name="userGUID"></param>
        /// <returns></returns>
        public override DomainResponseObject RemoveUserFromDomain(int nGroupID, int nDomainID, int nUserGUID)
        {
            //New domain
            //Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Init the Domain
            //New domain
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

            //bool init = domain.Initialize(nGroupID, domainID);

            //Delete the User from Domain
            
            DomainResponseStatus eDomainResponseStatus = domain.RemoveUserFromDomain(nGroupID, nDomainID, nUserGUID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        /// <summary>
        /// GetDomainInfo
        /// </summary>
        /// <param name="domainID"></param>
        /// <param name="nGroupID"></param>
        /// <returns></returns>
        public override Domain GetDomainInfo(int nDomainID, int nGroupID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);
            //Domain domain = new Domain();

            //Init the Domain
            //domain.Initialize(nGroupID, domainID);

            return domain;
        }

        public override List<Domain> GetDeviceDomains(string udid)
        {
            List<Domain> retVal = null;
            int deviceID = Device.GetDeviceIDByUDID(udid, m_nGroupID);
            if (deviceID > 0)
            {
                retVal = Domain.GetDeviceDomains(deviceID, m_nGroupID);
            }
            return retVal;
        }

        //protected bool IsUserValid(int nGroupID, int userGuid)
        //{
        //    //Check if UserGuid is valid
        //    User user = new User();

        //    user.Initialize(userGuid, nGroupID);

        //    UserResponseObject resp = new UserResponseObject();

        //    if (user.m_oBasicData.m_sUserName == "")
        //        resp.Initialize(ResponseStatus.UserDoesNotExist, user);
        //    else
        //        resp.Initialize(ResponseStatus.OK, user);

        //    return (resp.m_RespStatus == ResponseStatus.OK);
        //}

        public override DeviceResponseObject RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName)
        {
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);
            //Domain domain = new Domain();
            //domain.Initialize(nGroupID, nDomainID);

            DeviceResponseStatus eRetVal = DeviceResponseStatus.UnKnown;
            Device device = domain.RegisterDeviceToDomainWithPIN(nGroupID, sPIN, nDomainID, sDeviceName, ref eRetVal);
            return new DeviceResponseObject(device, eRetVal);
        }

        public override DomainResponseObject ResetDomain(int nDomainID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);
            //Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Init The Domain
            //domain.Initialize(m_nGroupID, nDomainID);

            //Reset the domain
            DomainResponseStatus eDomainResponseStatus = domain.ResetDomain();

            //Re-Init domain to return updated data
            domain.Initialize(m_nGroupID, nDomainID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override bool SetDomainRestriction(int nDomainID, DomainRestriction rest)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

            domain.m_DomainRestriction = rest;
            bool res = domain.Update();

            return res;
        }

        //public override DomainResponseObject SetDomainActive(int nDomainID, bool bActive)
        //{
        //    //New domain
        //    Domain domain = new Domain();
        //    // Create new response
        //    DomainResponseObject oDomainResponseObject;

        //    //Init The Domain
        //    domain.Initialize(m_nGroupID, nDomainID);

        //    //Reset the domain
        //    DomainResponseStatus eDomainResponseStatus = domain.SetDomainActive(bActive);

        //    //Re-Init domain to return updated data
        //    domain.Initialize(m_nGroupID, nDomainID);
        //    oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

        //    return oDomainResponseObject;
        //}
    }
}
