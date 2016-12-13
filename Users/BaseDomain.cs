using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Users.Cache;
using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System.Reflection;

namespace Users
{
    public abstract class BaseDomain
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected int m_nGroupID;

        protected BaseDomain() { }
        public BaseDomain(int nGroupID)
        {
            m_nGroupID = nGroupID;

        }

        #region Public Abstract
        public abstract DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid);

        public abstract DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID);

        public abstract DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername);

        #endregion

        #region Public Virtual

        public virtual DomainResponseObject ChangeDomainMaster(int nDomainID, int nCurrentMasterID, int nNewMasterID)
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
            domain = DomainInitializer(m_nGroupID, nDomainID, false);
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
            else if (domain != null && domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.DomainSuspended;
            }
            else
            {
                oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;
            }

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription)
        {
            DomainResponseObject res = null;

            Domain domain = DomainInitializer(m_nGroupID, nDomainID, false);
            if (domain != null)
            {
                domain.m_sName = sDomainName;
                domain.m_sDescription = sDomainDescription;

                if (domain.Update() && domain.m_DomainStatus == DomainStatus.OK)
                {
                    res = new DomainResponseObject(domain, DomainResponseStatus.OK);
                }
                else
                {
                    res = new DomainResponseObject(domain, DomainResponseStatus.Error);
                }
            }
            else
            {
                res = new DomainResponseObject(domain, DomainResponseStatus.DomainNotExists);
            }

            return res;

        }

        private DomainResponseStatus ConvertDomainStatusToDomainResponseStatus(DomainStatus domainStatus)
        {
            switch(domainStatus)
            {
                case DomainStatus.OK:
                    return DomainResponseStatus.OK;

                case DomainStatus.DomainAlreadyExists:
                    return DomainResponseStatus.DomainAlreadyExists;

                case DomainStatus.ExceededLimit:
                    return DomainResponseStatus.ExceededLimit;

                case DomainStatus.DeviceTypeNotAllowed:
                    return DomainResponseStatus.DeviceTypeNotAllowed;

                case DomainStatus.UnKnown:
                    return DomainResponseStatus.UnKnown;

                case DomainStatus.DeviceNotInDomin:
                    return DomainResponseStatus.DeviceNotInDomain;

                case DomainStatus.UserNotInDomain:
                    return DomainResponseStatus.UserNotExistsInDomain;

                case DomainStatus.DomainNotExists:
                    return DomainResponseStatus.DomainNotExists;

                case DomainStatus.HouseholdUserFailed:
                    return DomainResponseStatus.HouseholdUserFailed;

                case DomainStatus.DomainSuspended:
                    return DomainResponseStatus.DomainSuspended;

                case DomainStatus.NoUsersInDomain:
                    return DomainResponseStatus.NoUsersInDomain;

                case DomainStatus.UserExistsInOtherDomains:
                    return DomainResponseStatus.UserExistsInOtherDomains;
            }
            return DomainResponseStatus.Error;
        }
		
        public virtual DomainResponseObject ResetDomain(int nDomainID, int nFrequencyType)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID, false); // build the domain - without insert it to cache 

            DomainResponseStatus eDomainResponseStatus;
            if (domain.m_DomainStatus != DomainStatus.OK)
            {
                eDomainResponseStatus = ConvertDomainStatusToDomainResponseStatus(domain.m_DomainStatus);
                return new DomainResponseObject(null, eDomainResponseStatus);
            }
			
            eDomainResponseStatus = domain.ResetDomain(nFrequencyType);

            domain = DomainInitializer(m_nGroupID, nDomainID, true); // Build the domain after the Reset and insert it to cache

            return new DomainResponseObject(domain, eDomainResponseStatus);
        }

        public virtual DomainResponseStatus RemoveDomain(int nDomainID)
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            try
            {
                if (nDomainID < 1)
                    return DomainResponseStatus.DomainNotExists;

                Domain domain = DomainInitializer(m_nGroupID, nDomainID, false);

                if (domain == null)
                    return DomainResponseStatus.DomainNotExists;
 
                //cjeck if  send mail = true
                bool sendMail = DomainDal.GetCloseAccountMailTrigger(m_nGroupID);
                log.DebugFormat("Close Account mail settings m_nGroupID = {0}, sendMail = {1}", m_nGroupID, sendMail);
                User masterUser = new User();
                bool isUserValid = false;
                if (sendMail)
                {
                    // get domain master user details                    
                    int userGuid = domain.m_masterGUIDs.FirstOrDefault();
                    isUserValid = User.IsUserValid(m_nGroupID, userGuid, ref masterUser);
                }

                res = domain.Remove();

                if (res == DomainResponseStatus.OK && isUserValid)
                {
                    RemoveDomianMailRequest mailRequest = MailFactory.GetRemoveDomainMailRequest(masterUser, m_nGroupID);
                    if (mailRequest != null)
                    {
                        bool sendingMailResult = Utils.SendMail(m_nGroupID, mailRequest);
                    }
                }
                else if (!isUserValid)
                {
                    log.DebugFormat("can't send mail to DomainID = {0} due to master user is not valid", nDomainID);
                }
            }

            catch (Exception ex)
            {
                res = DomainResponseStatus.Error;
                StringBuilder sb = new StringBuilder("Exception at RemoveDomain. ");
                sb.Append(String.Concat(" D ID: ", nDomainID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" this is: ", this.GetType().Name));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                throw;
            }

            return res;
        }

        public virtual DomainResponseObject AddDeviceToDomain(int groupId, int domainId, string udid, string deviceName, int brandId)
        {
            DomainResponseObject oDomainResponseObject = new DomainResponseObject();
            oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;

            // validate UDID is not empty
            if (string.IsNullOrEmpty(udid))
                return oDomainResponseObject;

            // get domain data
            oDomainResponseObject.m_oDomain = DomainInitializer(groupId, domainId, false);
            if (oDomainResponseObject.m_oDomain == null ||
                oDomainResponseObject.m_oDomain.m_DomainStatus == DomainStatus.Error)
            {
                // error getting domain
                log.ErrorFormat("Domain doesn't exists. nGroupID: {0}, nDomainID: {1}, sUDID: {2}, sDeviceName: {3}, nBrandID: {4}", groupId, domainId, udid, deviceName, brandId);
                oDomainResponseObject.m_oDomain = null;
                oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.DomainNotExists;
            }
            else if (oDomainResponseObject.m_oDomain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                // domain is suspended
                oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.DomainSuspended;
            }
            else
            {
                // create new device
                Device device = new Device(udid, brandId, m_nGroupID, deviceName, domainId);
                bool res = device.Initialize(udid, deviceName);

                // add device to domain
                oDomainResponseObject.m_oDomainResponseStatus = oDomainResponseObject.m_oDomain.AddDeviceToDomain(m_nGroupID, domainId, udid, deviceName, brandId, ref device);

                if (oDomainResponseObject.m_oDomainResponseStatus == DomainResponseStatus.OK)
                {
                    // update domain info (to include new device)
                    oDomainResponseObject.m_oDomain = DomainInitializer(groupId, domainId, false);
                }
            }

            return oDomainResponseObject;
        }

        public virtual DeviceResponse AddDevice(int groupId, int domainId, string udid, string deviceName, int brandId)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // validate UDID is not empty
            if (string.IsNullOrEmpty(udid))
                return response;

            DomainResponseStatus domainResponseStatus = DomainResponseStatus.OK;

            // get domain data
            Domain domain = DomainInitializer(groupId, domainId, false);
            if (domain == null || domain.m_DomainStatus == DomainStatus.Error)
            {
                // error getting domain
                log.ErrorFormat("Domain doesn't exists. nGroupID: {0}, nDomainID: {1}, sUDID: {2}, sDeviceName: {3}, nBrandID: {4}", groupId, domainId, udid, deviceName, brandId);
                domainResponseStatus = DomainResponseStatus.DomainNotExists;
            }
            else if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                // domain is suspended
                domainResponseStatus = DomainResponseStatus.DomainSuspended;
            }
            else
            {
                // create new device
                Device device = new Device(udid, brandId, m_nGroupID, deviceName, domainId);
                device.Initialize(udid, deviceName);

                // add device to domain
                domainResponseStatus = domain.AddDeviceToDomain(m_nGroupID, domainId, udid, deviceName, brandId, ref device);
                if (domainResponseStatus == DomainResponseStatus.OK)
                {
                    // update domain info (to include new device)
                    response.Device = new DeviceResponseObject();
                    response.Device.m_oDevice = device;
                }
            }

            response.Status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
            return response;
        }

        public virtual DomainResponseObject AddUserToDomain(int nGroupID, int nDomainID, int userGuid, int nMasterUserGuid, bool bIsMaster = false)
        {
            //New domain
            Domain domain = new Domain();
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Check if UserGuid is valid
            if (!User.IsUserValid(nGroupID, userGuid))
            {
                domain.m_DomainStatus = DomainStatus.Error;
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.InvalidUser);
                return oDomainResponseObject;
            }

            //Init The Domain
            domain = DomainInitializer(nGroupID, nDomainID, false);

            //Add new User to Domain
            UserDomainType userType = bIsMaster ? UserDomainType.Master : UserDomainType.Regular;
            DomainResponseStatus eDomainResponseStatus = domain.AddUserToDomain(nGroupID, nDomainID, userGuid, nMasterUserGuid, userType);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject RemoveUserFromDomain(int nGroupID, int nDomainID, int nUserGUID)
        {
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0 || nUserGUID <= 0)
            {
                return new DomainResponseObject(null, DomainResponseStatus.UnKnown);
            }

            //Init the Domain
            Domain domain = DomainInitializer(nGroupID, nDomainID, false);

            if (domain == null)
            {
                return (new DomainResponseObject(null, DomainResponseStatus.DomainNotExists));
            }

            //Delete the User from Domain
            DomainResponseStatus eDomainResponseStatus = domain.RemoveUserFromDomain(nGroupID, nDomainID, nUserGUID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual Domain GetDomainInfo(int nDomainID, int nGroupID)
        {
            Domain domain = DomainInitializer(nGroupID, nDomainID, true);

            return domain;
        }

        public virtual DomainResponseObject ChangeDeviceDomainStatus(int nDomainID, string sDeviceUDID, bool bIsEnable)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID, false);

            DomainResponseObject oDomainResponseObject;
            DomainResponseStatus eDomainResponseStatus;
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                eDomainResponseStatus = DomainResponseStatus.DomainSuspended;
            }
            else
            {
                eDomainResponseStatus = domain.ChangeDeviceDomainStatus(m_nGroupID, nDomainID, sDeviceUDID, bIsEnable);
            }

            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject RemoveDeviceFromDomain(int nDomainID, string sDeviceUDID)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID, false);
            DomainResponseObject oDomainResponseObject;

            DomainResponseStatus eDomainResponseStatus = domain.RemoveDeviceFromDomain(sDeviceUDID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject SubmitAddDeviceToDomainRequest(int nGroupID, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID)
        {
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0 || string.IsNullOrEmpty(sDeviceUdid))
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.Error);
            }

            Domain domain = DomainInitializer(nGroupID, nDomainID, false);

            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.DomainSuspended);
                return oDomainResponseObject;
            }

            Device device = new Device(sDeviceUdid, nBrandID, m_nGroupID, sDeviceName, nDomainID);

            // If domain is restricted and action user is the master, just add the device
            if ((domain.m_DomainRestriction == DomainRestriction.Unrestricted) ||
                (domain.m_DomainRestriction == DomainRestriction.UserMasterRestricted) ||
                ((domain.m_DomainRestriction == DomainRestriction.DeviceMasterRestricted || domain.m_DomainRestriction == DomainRestriction.DeviceUserMasterRestricted) &&
                (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0) && (domain.m_masterGUIDs.Contains(nUserID))))
            {
                DomainResponseStatus eDomainResponseStatus = domain.AddDeviceToDomain(nGroupID, domain.m_nDomainID, sDeviceUdid, sDeviceName, nBrandID, ref device);
                oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

                return oDomainResponseObject;
            }

            DomainResponseStatus eDomainResponseStatus1 = domain.SubmitAddDeviceToDomainRequest(nGroupID, sDeviceUdid, sDeviceName, ref device);

            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus1);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject ConfirmDeviceByDomainMaster(string sMasterUN, string sUDID, string sToken)
        {
            DomainResponseObject resp = new DomainResponseObject();

            // Check the Master User
            List<int> lGroupIDs = UtilsDal.GetAllRelatedGroups(m_nGroupID);
            string[] arrGroupIDs = lGroupIDs.Select(g => g.ToString()).ToArray();

            int masterUserID = DAL.UsersDal.GetUserIDByUsername(sMasterUN, arrGroupIDs);

            User masterUser = new User();
            bool bInit = masterUser.Initialize(masterUserID, m_nGroupID, false);

            if (masterUserID <= 0 || !bInit || !masterUser.m_isDomainMaster)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.ActionUserNotMaster;
                return resp;
            }

            if (masterUser.m_eSuspendState == DomainSuspentionStatus.Suspended)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DomainSuspended;
                return resp;
            }

            // Check if such device exists
            Device device = new Device(m_nGroupID);
            bInit = device.Initialize(sUDID);
            int nDeviceID = int.Parse(device.m_id);

            int nDomainDeviceID = 0;
            int nTokenDeviceID = DomainDal.GetDeviceIDByDomainActivationToken(m_nGroupID, sToken, ref nDomainDeviceID);

            if (nDeviceID != nTokenDeviceID)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DeviceNotConfirmed;
                return resp;
            }

            string sNewGuid = Guid.NewGuid().ToString();
            int rows = DomainDal.UpdateDeviceDomainActivationToken(m_nGroupID, nDomainDeviceID, nDeviceID, sToken, sNewGuid);

            bool isActivated = rows > 0;

            Domain domain = DomainFactory.GetDomain(m_nGroupID, masterUser.m_domianID);

            bInit &= (domain != null);

            if (!bInit)
            {
                resp.m_oDomain = domain;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DomainNotInitialized;
                return resp;
            }

            // insert the new domain to cache 
            DomainsCache oDomainCache = DomainsCache.Instance();
            bool bInsert = oDomainCache.InsertDomain(domain);

            int nActivationStatus = DomainDal.GetDomainDeviceActivateStatus(m_nGroupID, nDeviceID);

            resp.m_oDomain = nActivationStatus == 1 ? domain : null;
            resp.m_oDomainResponseStatus = nActivationStatus == 1 ? DomainResponseStatus.OK : DomainResponseStatus.DeviceNotConfirmed;

            return resp;
        }

        public virtual List<string> GetDomainUserList(int nDomainID, int nGroupID)
        {
            List<string> luser = new List<string>();
            List<int> ltempUsers = new List<int>();
            Domain domain = null;
            DomainsCache oDomainCache = DomainsCache.Instance();
            domain = oDomainCache.GetDomain(nDomainID, nGroupID);
            bool bUsers = UsersFullListFromDomain(nDomainID, domain, ltempUsers);

            if (bUsers && ltempUsers != null && ltempUsers.Count > 0)
            {
                luser = ltempUsers.ConvertAll<string>(x => x.ToString());
                return luser;
            }
            return new List<string>(0);
        }

        public virtual List<Domain> GetDeviceDomains(string sUDID)
        {
            try
            {
                List<Domain> retVal = null;
                int deviceID = Device.GetDeviceIDByUDID(sUDID, m_nGroupID);
                if (deviceID > 0)
                {
                    retVal = Domain.GetDeviceDomains(deviceID, m_nGroupID);
                }
                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("GetDeviceDomains - " + string.Format("Failed ex={0}, sUDID={1}", ex.Message, sUDID), ex);
                return null;
            }
        }

        public virtual DeviceResponseObject RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName)
        {
            Domain domain = DomainInitializer(nGroupID, nDomainID, false);

            DeviceResponseStatus eRetVal = DeviceResponseStatus.UnKnown;
            Device device = domain.RegisterDeviceToDomainWithPIN(nGroupID, sPIN, nDomainID, sDeviceName, ref eRetVal);
            return new DeviceResponseObject(device, eRetVal);
        }

        public virtual int GetDomainIDByCoGuid(string coGuid)
        {
            return DomainDal.GetDomainIDByCoGuid(coGuid, m_nGroupID);
        }

        public virtual DomainResponseObject GetDomainByCoGuid(string coGuid, int nGroupID)
        {
            DomainResponseObject oDomainResponseObject;

            int nDomainID = GetDomainIDByCoGuid(coGuid);

            if (nDomainID <= 0)
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.DomainNotExists);

                return oDomainResponseObject;
            }

            Domain domain = DomainInitializer(m_nGroupID, nDomainID, true);

            oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);

            return oDomainResponseObject;
        }

        public virtual int[] GetDomainIDsByOperatorCoGuid(string sOperatorCoGuid)
        {
            if (string.IsNullOrEmpty(sOperatorCoGuid))
            {
                return new int[] { };
            }

            List<int> lDomainIDs = DomainDal.GetDomainIDsByOperatorCoGuid(sOperatorCoGuid);

            return lDomainIDs.ToArray();
        }

        public virtual bool SetDomainRestriction(int nDomainID, DomainRestriction rest)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID, false);
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                return false;
            }
            domain.m_DomainRestriction = rest;
            bool res = domain.Update();

            return res;
        }

        public virtual List<HomeNetwork> GetDomainHomeNetworks(long lDomainID)
        {
            return Utils.GetHomeNetworksOfDomain(lDomainID, m_nGroupID, true);
        }

        public virtual NetworkResponseObject AddHomeNetworkToDomain(long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc)
        {
            NetworkResponseObject res = new NetworkResponseObject(false, NetworkResponseStatus.Error);
            List<HomeNetwork> lstOfHomeNetworks = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;

            // check network id is valid
            if (IsHomeNetworkInputInvalid(lDomainID, sNetworkID))
            {
                res.eReason = NetworkResponseStatus.InvalidInput;
                res.bSuccess = false;
                return res;
            }

            HomeNetwork candidate = new HomeNetwork(sNetworkName, sNetworkID, sNetworkDesc, DateTime.UtcNow, true);
            DataTable dt = null;
            if (!DomainDal.Get_ProximityDetectionDataForInsertion(m_nGroupID, lDomainID, ref numOfAllowedNetworks, ref dt))
            {
                // failed to extract data from DB.
                // log and return err

                log.Debug("AddHomeNetworkToDomain - " + GetUpdateHomeNetworkErrMsg("Failed to extract data from DB", lDomainID, candidate, 0, numOfAllowedNetworks, numOfActiveNetworks));
                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;

                return res;
            }

            GetListOfExistingHomeNetworksForInsertion(dt, out lstOfHomeNetworks, out numOfActiveNetworks);


            // check if network already exists
            if (lstOfHomeNetworks.Contains(candidate))
            {
                res.eReason = NetworkResponseStatus.NetworkExists;
                res.bSuccess = false;

                return res;
            }

            // check quantity limitation
            if (!IsSatisfiesQuantityConstraint(numOfAllowedNetworks, numOfActiveNetworks))
            {
                res.eReason = NetworkResponseStatus.QuantityLimitation;
                res.bSuccess = false;

                return res;
            }

            // all validations pass, add new home network to domain
            if (DomainDal.Insert_NewHomeNetwork(m_nGroupID, candidate.UID, lDomainID, candidate.Name, candidate.Description, candidate.IsActive, candidate.CreateDate) == null)
            {
                // failed to insert
                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;

                // log
                log.Debug("AddHomeNetworkToDomain - " + String.Concat("Failed to add to domain: ", lDomainID, " the home network: ", candidate.ToString()));

            }
            else
            {
                // remove domain from cache 
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain((int)lDomainID);

                res.eReason = NetworkResponseStatus.OK;
                res.bSuccess = true;
            }

            return res;
        }

        public virtual HomeNetworkResponse UpdateDomainHomeNetwork(long domainID, string networkID, string networkName, string networkDesc, bool isActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            ApiObjects.Response.Status res = null;
            HomeNetwork candidate = null;
            HomeNetwork existingNetwork = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;
            int frequency = 0;
            DateTime dtLastDeactivationDate = DateTime.MinValue;
            try
            {
                if (!UpdateRemoveHomeNetworkCommon(domainID, networkID, networkName, networkDesc, isActive, out res,
                    ref candidate, ref existingNetwork, ref numOfAllowedNetworks, ref numOfActiveNetworks, ref frequency, ref dtLastDeactivationDate))
                {
                    response.Status = res;
                    return response;
                }
                response.HomeNetwork = UpdateDomainHomeNetworkInner(domainID, numOfAllowedNetworks, numOfActiveNetworks, frequency,
                    candidate, existingNetwork, dtLastDeactivationDate, ref res);
                response.Status = res;

                if (res != null && res.Code == (int)eResponseStatus.OK)
                {
                    DomainsCache oDomainCache = DomainsCache.Instance();
                    oDomainCache.RemoveDomain((int)domainID);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error while updating domain home network. domainId = {0}, networkId = {1}", domainID, networkID), ex);
            }
            return response;
        }

        public virtual ApiObjects.Response.Status RemoveDomainHomeNetwork(long lDomainID, string sNetworkID)
        {
            ApiObjects.Response.Status res = null;
            HomeNetwork candidate = null;
            HomeNetwork existingNetwork = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;
            int frequency = 0;
            DateTime dtLastDeactivationDate = DateTime.MinValue;
            if (!UpdateRemoveHomeNetworkCommon(lDomainID, sNetworkID, string.Empty, string.Empty, false, out res,
                ref candidate, ref existingNetwork, ref numOfAllowedNetworks, ref numOfActiveNetworks, ref frequency, ref dtLastDeactivationDate))
            {
                return res;
            }

            res = RemoveDomainHomeNetworkInner(lDomainID, numOfAllowedNetworks, numOfActiveNetworks, frequency, candidate, existingNetwork, dtLastDeactivationDate, ref res);
            if (res != null && res.Code == (int)ApiObjects.Response.eResponseStatus.OK)
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain((int)lDomainID);
            }
            return res;
        }

        public virtual ValidationResponseObject ValidateLimitationModule(string udid, int deviceBrandID, long siteGuid, long domainID, ValidationType validationType,
            int ruleID = 0, int mediaConcurrencyLimit = 0, int mediaID = 0)
        {
            ValidationResponseObject res = new ValidationResponseObject();

            Domain domain = GetDomainForValidation(siteGuid, domainID);
            if (domain != null && domain.m_DomainStatus != DomainStatus.Error)
            {
                //to add here isDevicePlayValid
                bool bisDevicePlayValid = IsDevicePlayValid(siteGuid.ToString(), udid, domain);

                res.m_lDomainID = domainID > 0 ? domainID : domain.m_nDomainID;
                if (!bisDevicePlayValid)
                {
                    res.m_eStatus = DomainResponseStatus.DeviceNotInDomain;
                    return res;
                }

                switch (validationType)
                {
                    case ValidationType.Concurrency:
                        {
                            if (ruleID > 0)
                            {
                                res.m_eStatus = domain.ValidateAssetConcurrency(ruleID, mediaConcurrencyLimit, res.m_lDomainID, mediaID, udid);
                            }
                            if (res.m_eStatus == DomainResponseStatus.OK || res.m_eStatus == DomainResponseStatus.UnKnown) // if it's MediaConcurrencyLimitation no need to check this one 
                            {
                                res.m_eStatus = domain.ValidateConcurrency(udid, deviceBrandID, res.m_lDomainID);
                            }
                            break;
                        }
                    case ValidationType.Frequency:
                        {
                            res.m_eStatus = domain.ValidateFrequency(udid, deviceBrandID);
                            break;
                        }
                    default:
                        {
                            // Quantity
                            res.m_eStatus = domain.ValidateQuantity(udid, deviceBrandID);
                            break;
                        }
                }
            } // end if

            return res;
        }

        /*This method return status via ValidationResponseObject object if there is a limitation to play this npvrID */
        public virtual ValidationResponseObject ValidateLimitationNpvr(string sUDID, int nDeviceBrandID, long lSiteGuid, long lDomainID, ValidationType eValidationType,
            int nNpvrConcurrencyLimit = 0, string sNpvrID = default(string))
        {
            ValidationResponseObject res = new ValidationResponseObject();

            Domain domain = GetDomainForValidation(lSiteGuid, lDomainID);
            if (domain != null && domain.m_DomainStatus != DomainStatus.Error)
            {
                //to add here isDevicePlayValid
                bool bisDevicePlayValid = IsDevicePlayValid(lSiteGuid.ToString(), sUDID, domain);

                res.m_lDomainID = lDomainID > 0 ? lDomainID : domain.m_nDomainID;
                if (!bisDevicePlayValid)
                {
                    res.m_eStatus = DomainResponseStatus.DeviceNotInDomain;
                    return res;
                }

                switch (eValidationType)
                {
                    case ValidationType.Concurrency:
                        {
                            res.m_eStatus = domain.ValidateNpvrConcurrency(nNpvrConcurrencyLimit, res.m_lDomainID, sNpvrID);
                            break;
                        }
                    case ValidationType.Frequency:
                        {
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            } // end if

            return res;
        }

        public virtual ApiObjects.Response.Status SuspendDomain(int nDomainID)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();
            DomainsCache oDomainCache = DomainsCache.Instance();

            // validate domain
            var domain = oDomainCache.GetDomain(nDomainID, m_nGroupID, false);
            if (domain == null || domain.m_DomainStatus == DomainStatus.Error || domain.m_DomainStatus == DomainStatus.DomainNotExists)
            {
                result.Code = (int)eResponseStatus.Error;
                result.Message = "Domain doesn't exist";
                return result;
            }

            // validate domain is not suspended
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                result.Code = (int)eResponseStatus.DomainAlreadySuspended;
                result.Message = "Domain already suspended";
                return result;
            }

            // suspend domain
            bool SuspendSucceed = DAL.DomainDal.ChangeSuspendDomainStatus(nDomainID, m_nGroupID, DomainSuspentionStatus.Suspended);

            // remove from cache
            if (SuspendSucceed)
            {
                // Remove Domain
                oDomainCache.RemoveDomain(nDomainID);
                UsersCache usersCache = UsersCache.Instance();
                foreach (int userID in domain.m_UsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);
                }
            }

            // update result
            if (SuspendSucceed)
            {
                result.Code = (int)eResponseStatus.OK;
            }
            else
            {
                result.Code = (int)eResponseStatus.Error;
                result.Message = "Failed to suspend domain";
            }

            return result;
        }

        public virtual ApiObjects.Response.Status ResumeDomain(int nDomainID)
        {
            ApiObjects.Response.Status result = new ApiObjects.Response.Status();
            DomainsCache oDomainCache = DomainsCache.Instance();

            // validate domain
            var domain = oDomainCache.GetDomain(nDomainID, m_nGroupID, false);
            if (domain == null || domain.m_DomainStatus == DomainStatus.Error || domain.m_DomainStatus == DomainStatus.DomainNotExists)
            {
                result.Code = (int)eResponseStatus.Error;
                result.Message = "Domain doesn't exist";
                return result;
            }

            // validate domain is not active
            if (domain.m_DomainStatus == DomainStatus.OK)
            {
                result.Code = (int)eResponseStatus.DomainAlreadyActive;
                result.Message = "Domain already active";
                return result;
            }

            // resume domain
            bool ResumeSucceed = DAL.DomainDal.ChangeSuspendDomainStatus(nDomainID, m_nGroupID, DomainSuspentionStatus.OK);

            // remove from cache
            if (ResumeSucceed)
            {
                // Remove Domain
                oDomainCache.RemoveDomain(nDomainID);
                UsersCache usersCache = UsersCache.Instance();
                foreach (int userID in domain.m_UsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);
                }
            }

            // update result
            if (ResumeSucceed)
                result.Code = (int)eResponseStatus.OK;
            else
            {
                result.Code = (int)eResponseStatus.Error;
                result.Message = "Failed to suspend domain";
            }

            return result;
        }

        #endregion

        #region Protected abstract

        protected abstract ApiObjects.Response.Status RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref ApiObjects.Response.Status res);

        protected abstract HomeNetwork UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref ApiObjects.Response.Status res);

        /*
         * 07/04/2014
         * Initializing a domain in Eutelsat is different than in other customers.
         * 
         */
        // protected abstract Domain DomainInitializer(int nGroupID, int nDomainID);
        protected abstract Domain DomainInitializer(int nGroupID, int nDomainID, bool bCache = true);

        #endregion

        #region Protected implemented

        // return True if device recognize in Domain false another case (assumption : user is valid !)
        protected bool IsDevicePlayValid(string sSiteGUID, string sDEVICE_NAME, Domain userDomain)
        {
            bool isDeviceRecognized = false;
            try
            {
                if (userDomain != null)
                {
                    List<DeviceContainer> deviceContainers = userDomain.m_deviceFamilies;
                    if (deviceContainers != null && deviceContainers.Count() > 0)
                    {
                        List<int> familyIDs = new List<int>();
                        for (int i = 0; i < deviceContainers.Count(); i++)
                        {
                            DeviceContainer container = deviceContainers[i];

                            if (container != null)
                            {
                                if (!familyIDs.Contains(container.m_deviceFamilyID))
                                {
                                    familyIDs.Add(container.m_deviceFamilyID);
                                }

                                if (container.DeviceInstances != null && container.DeviceInstances.Count() > 0)
                                {
                                    for (int j = 0; j < container.DeviceInstances.Count(); j++)
                                    {
                                        Device device = container.DeviceInstances[j];
                                        if (string.Compare(device.m_deviceUDID.Trim(), sDEVICE_NAME.Trim()) == 0)
                                        {
                                            isDeviceRecognized = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    familyIDs.Add(container.m_deviceFamilyID);
                                }

                                if (container.DeviceInstances != null && container.DeviceInstances.Count() > 0)
                                {
                                    for (int j = 0; j < container.DeviceInstances.Count(); j++)
                                    {
                                        Device device = container.DeviceInstances[j];
                                        if (string.Compare(device.m_deviceUDID.Trim(), sDEVICE_NAME.Trim()) == 0)
                                        {
                                            isDeviceRecognized = true;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //Patch!!
                                    if (container.m_deviceFamilyID == 5 && (string.IsNullOrEmpty(sDEVICE_NAME) || sDEVICE_NAME.ToLower().Equals("web site")))
                                    {
                                        isDeviceRecognized = true;
                                    }
                                }
                                if (isDeviceRecognized)
                                {
                                    break;
                                }

                            }
                        }
                        if (!familyIDs.Contains(5) && string.IsNullOrEmpty(sDEVICE_NAME) || (familyIDs.Contains(5) && familyIDs.Count == 0) || (!familyIDs.Contains(5) && sDEVICE_NAME.ToLower().Equals("web site")))
                        {
                            isDeviceRecognized = true;
                        }
                    }
                    else
                    {
                        // No Domain - No device check!!
                        isDeviceRecognized = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("IsDevicePlayValid - " + string.Format("faild ex={0} siteGuid ={1} deviceName={2} domainID={3}", ex.Message, sSiteGUID, sDEVICE_NAME, userDomain != null ? userDomain.m_nDomainID : 0),
                    ex);
                isDeviceRecognized = false;
            }

            return isDeviceRecognized;
        }

        protected string GetLogFilename()
        {
            return String.Concat("BaseDomain_", m_nGroupID);
        }

        protected UserResponseObject ValidateMasterUser(int nGroupID, int nUserID)
        {
            User masterUser = new User(nGroupID, nUserID);

            UserResponseObject resp = new UserResponseObject();

            if (masterUser == null || string.IsNullOrEmpty(masterUser.m_oBasicData.m_sUserName))
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

        protected bool IsSatisfiesFrequencyConstraint(DateTime dtLastDeactivationDate, int frequency)
        {
            DateTime dt = Utils.GetEndDateTime(dtLastDeactivationDate, frequency);

            return dt < DateTime.UtcNow;
        }

        protected bool IsSatisfiesQuantityConstraint(int numOfAllowedNetworks, int numOfActiveNetworks)
        {
            return numOfAllowedNetworks > numOfActiveNetworks;
        }

        protected string GetUpdateHomeNetworkErrMsg(string errMsg, long lDomainID, HomeNetwork candidate, int frequency, int numOfAllowedHomeNetworks, int numOfActiveHomeNetworks)
        {
            StringBuilder sb = new StringBuilder(errMsg);
            sb.Append(String.Concat(" Domain ID: ", lDomainID));
            sb.Append(String.Concat(". Frequency: ", frequency));
            sb.Append(String.Concat(". Num of allowed home networks: ", numOfAllowedHomeNetworks));
            sb.Append(String.Concat(". Num of activate home networks: ", numOfActiveHomeNetworks));
            sb.Append(String.Concat(". Home network candidate: ", candidate.ToString()));

            return sb.ToString();
        }

        protected HomeNetwork GetHomeNetworkFromList(List<HomeNetwork> lstOfHomeNetworks, HomeNetwork hn)
        {
            foreach (HomeNetwork iter in lstOfHomeNetworks)
            {
                if (iter.Equals(hn))
                    return iter;
            }
            return null;
        }

        protected void GetListOfExistingHomeNetworksForUpdating(DataTable dt, out List<HomeNetwork> lst, out int numOfActiveNetworks)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                numOfActiveNetworks = 0;

                lst = new List<HomeNetwork>(length);

                for (int i = 0; i < length; i++)
                {
                    HomeNetwork hn = new HomeNetwork();
                    hn.UID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NETWORK_ID"]);
                    hn.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE"]) != 0;
                    if (hn.IsActive)
                        numOfActiveNetworks++;
                    hn.Name = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NAME"]);
                    hn.Description = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["DESCRIPTION"]);

                    lst.Add(hn);

                }

            }
            else
            {
                lst = new List<HomeNetwork>(0);
                numOfActiveNetworks = 0;
            }

        }

        protected bool IsHomeNetworkInputInvalid(long lDomainID, string sNetworkID)
        {
            return lDomainID < 1 || string.IsNullOrEmpty(sNetworkID);
        }

        protected void GetListOfExistingHomeNetworksForInsertion(DataTable dt, out List<HomeNetwork> lst, out int numOfActiveNetworks)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                int length = dt.Rows.Count;
                lst = new List<HomeNetwork>(length);

                numOfActiveNetworks = 0;

                for (int i = 0; i < length; i++)
                {
                    HomeNetwork hn = new HomeNetwork();
                    hn.UID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["NETWORK_ID"]);
                    if (hn.UID.Length == 0)
                        continue;
                    hn.IsActive = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE"]) != 0;
                    if (hn.IsActive)
                        numOfActiveNetworks++;
                    lst.Add(hn);
                }
            }
            else
            {
                lst = new List<HomeNetwork>(0);
                numOfActiveNetworks = 0;
            }
        }

        #endregion

        #region Private methods

        private Domain GetDomainForValidation(long lSiteGuid, long lDomainID)
        {
            DomainsCache oDomainCache = DomainsCache.Instance();
            Domain res = null;
            if (lDomainID > 0)
            {
                res = oDomainCache.GetDomain((int)lDomainID, m_nGroupID);
            }

            if (res == null && lSiteGuid > 0)
            {
                bool tempIsMaster = false;
                int tempOperatorID = 0;
                DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
                int domainID = 0;
                try
                {
                    // try getting user from cache
                    User user = null;
                    UsersCache usersCache = UsersCache.Instance();
                    user = usersCache.GetUser(Convert.ToInt32(lSiteGuid), m_nGroupID);
                    if (user != null)
                    {
                        domainID = user.m_domianID;
                    }
                }

                catch (Exception ex)
                {
                    log.Error("Failed getting user from cache", ex);
                }

                if (domainID == 0)
                {
                    domainID = DomainDal.GetDomainIDBySiteGuid(m_nGroupID, (int)lSiteGuid, ref tempOperatorID, ref tempIsMaster, ref eSuspendStat);
                }

                if (domainID < 1 || domainID == (int)lDomainID)
                    return null;
                res = oDomainCache.GetDomain(domainID, m_nGroupID);
            }

            return res;
        }

        private bool UpdateRemoveHomeNetworkCommon(long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive,
            out ApiObjects.Response.Status res, ref HomeNetwork nullifiedCandidate, ref HomeNetwork nullifiedExistingNetwork,
            ref int numOfAllowedNetworks, ref int numOfActiveNetworks, ref int frequency, ref DateTime dtLastDeactivationDate)
        {
            bool retVal = true;
            res = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            List<HomeNetwork> lstOfHomeNetworks = null;
            DataTable dt = null;
            if (string.IsNullOrEmpty(sNetworkID) || lDomainID == 0)
            {
                res = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, "External identifier is required");
                retVal = false;
                return retVal;
            }

            nullifiedCandidate = new HomeNetwork(sNetworkName, sNetworkID, sNetworkDesc, DateTime.UtcNow, bIsActive);

            //get domain dlm
            DomainsCache oDomainsCache = DomainsCache.Instance();
            Domain domain = oDomainsCache.GetDomain((int)lDomainID, m_nGroupID);
            long dlmId = domain.m_nLimit;

            if (!DomainDal.Get_ProximityDetectionDataForUpdating(m_nGroupID, lDomainID, sNetworkID, ref numOfAllowedNetworks, ref frequency, ref dtLastDeactivationDate, ref dt, dlmId))
            {
                // failed to extract data from db. log and return err
                log.Debug("UpdateRemoveHomeNetworkCommon - " + GetUpdateHomeNetworkErrMsg("DomainDal.Get_ProximityDetectionDataForUpdating failed.", lDomainID, nullifiedCandidate, frequency, numOfAllowedNetworks, numOfActiveNetworks));

                retVal = false;
                return retVal;
            }

            GetListOfExistingHomeNetworksForUpdating(dt, out lstOfHomeNetworks, out numOfActiveNetworks);

            nullifiedExistingNetwork = GetHomeNetworkFromList(lstOfHomeNetworks, nullifiedCandidate);

            if (nullifiedExistingNetwork == null)
            {
                res = new ApiObjects.Response.Status((int)eResponseStatus.HomeNetworkDoesNotExist, "Home network does not exist");
                retVal = false;

                return retVal;
            }

            return retVal;
        }


        private static bool UsersFullListFromDomain(int nDomainID, Domain oDomain, List<int> users)
        {
            try
            {
                if (oDomain != null && oDomain.m_nDomainID == nDomainID)
                {
                    if (oDomain.m_PendingUsersIDs != null)
                    {
                        foreach (int pendingUser in oDomain.m_PendingUsersIDs)
                        {
                            users.Add(pendingUser * (-1));
                        }
                    }

                    if (oDomain.m_DefaultUsersIDs != null)
                    {
                        foreach (int user in oDomain.m_DefaultUsersIDs)
                        {
                            users.Add(user);
                        }
                    }

                    if (oDomain.m_UsersIDs != null)
                    {
                        users.AddRange(oDomain.m_UsersIDs);
                    }

                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                log.Error("UsersFullListFromDomain - " + string.Format("Couldn't get domain {0}, ex = {1}", nDomainID, ex.Message), ex);
                return false;
            }
        }


        #endregion
        public ApiObjects.Response.Status RemoveDLM(int nDlmID)
        {
            ApiObjects.Response.Status resp = new ApiObjects.Response.Status();
            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                bool bRes = oDomainCache.RemoveDLM(nDlmID);
                if (bRes)
                    resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                else
                    resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, "Dlm Not Exist");

                return resp;
            }
            catch (Exception ex)
            {
                log.Error("RemoveDLM - " + string.Format("Couldn't get nDlmID {0}, ex = {1}", nDlmID, ex.Message), ex);
                resp.Code = (int)eResponseStatus.Error;
                return resp;
            }

        }

        public ChangeDLMObj ChangeDLM(int domainID, int dlmID, int nGroupID)
        {
            ChangeDLMObj oChangeDLMObj = new ChangeDLMObj();
            try
            {
                LimitationsManager oLimitationsManager = null;
                // get Domain (with it current DLM) by domain ID 
                DomainsCache oDomainsCache = DomainsCache.Instance();
                Domain domain = oDomainsCache.GetDomain(domainID, nGroupID);
                if (domain != null)
                {
                    if (domain.m_nLimit == dlmID) // noo need to change anything
                    {
                        oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        // get the new DLM from cache 
                        bool bDLM = oDomainsCache.GetDLM(dlmID, nGroupID, out oLimitationsManager);
                        if (!bDLM || oLimitationsManager == null)
                        {
                            oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, "Domain Not Exists");
                        }
                        else // start compare between two DLMs
                        {
                            bool bSuccess = domain.CompareDLM(oLimitationsManager, ref oChangeDLMObj);
                        }
                    }
                    oDomainsCache.RemoveDomain(domainID);
                }
                else
                {
                    oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DomainNotExists, "Domain Not Exists");
                }

                return oChangeDLMObj;
            }
            catch (Exception ex)
            {
                log.Error("ChangeDLM - " + string.Format("failed to ChangeDLM DlmID = {0}, DomainID = {1}, nGroupID = {2}, ex = {3}", dlmID, domainID, nGroupID, ex.Message), ex);
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
                return oChangeDLMObj;
            }
        }

        public DLMResponse GetDLM(int nDlmID, int nGroupID)
        {
            DLMResponse oDLMResponse = new DLMResponse();
            try
            {
                LimitationsManager dlmObj;
                DomainsCache oDomainsCache = DomainsCache.Instance();
                // get the DLM from cache 
                bool bDLM = oDomainsCache.GetDLM(nDlmID, nGroupID, out dlmObj);
                if (bDLM && dlmObj != null)
                {
                    oDLMResponse.dlm = dlmObj;
                    oDLMResponse.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    oDLMResponse.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, "Dlm Not Exist");
                }

                return oDLMResponse;
            }
            catch (Exception ex)
            {
                log.Error("GetDLM - " + string.Format("failed to GetDLM DlmID = {0}, nGroupID = {1}, ex = {2}", nDlmID, nGroupID, ex.Message), ex);
                oDLMResponse.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return oDLMResponse;
            }
        }

        public ApiObjects.Response.Status SetDomainRegion(int groupId, int domainId, string extRegionId, string lookupKey)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                DomainsCache domainsCache = DomainsCache.Instance();
                Domain domain = domainsCache.GetDomain(domainId, groupId);
                if (domain == null)
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.DomainNotExists, "Domain Not Exists");
                    return status;
                }

                if (DomainDal.UpdateDomainRegion(domainId, groupId, extRegionId, lookupKey))
                {
                    DomainsCache.Instance().RemoveDomain(domainId);
                    status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }

            }
            catch (Exception ex)
            {
                log.Error("SetDomainRegion - " + string.Format("failed to SetDomainRegion domainId = {0}, extRegionId = {1}, lookupKey = {2}, ex = {3}", domainId, extRegionId, lookupKey, ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        public Domain GetDomainByUser(int groupId, string siteGuid)
        {
            Domain domain = null;
            try
            {
                // try getting user from cache
                User user = null;
                UsersCache usersCache = UsersCache.Instance();
                user = usersCache.GetUser(int.Parse(siteGuid), groupId);
                int domainId;
                if (user != null)
                {
                    domainId = user.m_domianID;
                }
                else
                {
                    domainId = DomainDal.GetDomainIDBySiteGuid(groupId, siteGuid);
                }                

                if (domainId == 0)
                    return null;

                DomainsCache domainsCache = DomainsCache.Instance();
                domain = domainsCache.GetDomain(domainId, groupId);
            }
            catch (Exception ex)
            {
                log.Error("GetDomainByUser - " + string.Format("failed to GetDomainByUser siteGuid = {0}, ex = {1}", siteGuid, ex.Message), ex);
                return null;
            }
            return domain;
        }

        public virtual DeviceRegistrationStatusResponse GetDeviceRegistrationStatus(string udid, int domainId)
        {
            DeviceRegistrationStatusResponse response = new DeviceRegistrationStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            try
            {
                // get device
                int deviceID = Device.GetDeviceIDByUDID(udid, m_nGroupID);

                // device not found - device not registered
                if (deviceID == 0)
                {
                    response.DeviceRegistrationStatus = DeviceRegistrationStatus.NotRegistered;
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                // device found
                else
                {
                    // get device domains
                    var domains = Domain.GetDeviceDomains(deviceID, m_nGroupID);

                    // no domains found for device - device not registered
                    if (domains == null || domains.Count == 0)
                    {
                        response.DeviceRegistrationStatus = DeviceRegistrationStatus.NotRegistered;
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    // domains found
                    else
                    {
                        // look for the supplied domain
                        var domain = domains.Where(d => d.m_nDomainID == domainId).FirstOrDefault();

                        // domain found - device registered
                        if (domain != null)
                        {
                            response.DeviceRegistrationStatus = DeviceRegistrationStatus.Registered;
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                        // domain not found - device registered to another domain
                        else
                        {
                            response.DeviceRegistrationStatus = DeviceRegistrationStatus.RegisteredToAnotherDomain;
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetDeviceRegistrationStatus - " + string.Format("Failed ex = {0}, udid = {1}, domainId = {2}", ex.Message, udid, domainId), ex);
            }
            return response;
        }

        public virtual DeviceResponse GetDevice(string udid, int domainId)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                // get device
                int deviceID = Device.GetDeviceIDByUDID(udid, m_nGroupID);

                // device not found - device not registered
                if (deviceID == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotExists, eResponseStatus.DeviceNotExists.ToString());
                }
                // device found
                else
                {
                    // get device domains
                    var domains = Domain.GetDeviceDomains(deviceID, m_nGroupID);

                    // no domains found for device - device not registered
                    if (domains == null || domains.Count == 0)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, eResponseStatus.DeviceNotInDomain.ToString());
                    }
                    // domains found
                    else
                    {
                        // look for the supplied domain
                        var domain = domains.Where(d => d.m_nDomainID == domainId).FirstOrDefault();

                        // domain found - device registered
                        if (domain != null)
                        {
                            Device device = new Device(m_nGroupID);
                            device.Initialize(udid);

                            response.Device = new DeviceResponseObject();
                            response.Device.m_oDevice = device;

                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                        // domain not found - device registered to another domain
                        else
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DeviceExistsInOtherDomains, eResponseStatus.DeviceExistsInOtherDomains.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetDevice - " + string.Format("Failed ex = {0}, udid = {1}, domainId = {2}", ex.Message, udid, domainId), ex);
            }
            return response;
        }

        public virtual HomeNetworkResponse AddDomainHomeNetwork(long domainId, string externalId, string name, string description, bool isActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            List<HomeNetwork> lstOfHomeNetworks = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;

            // check network id is valid
            if (string.IsNullOrEmpty(externalId))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ExternalIdentifierRequired, "External identifier is required");
                return response;
            }

            HomeNetwork candidate = new HomeNetwork(name, externalId, description, DateTime.UtcNow, isActive);
            DomainsCache oDomainsCache = DomainsCache.Instance();
            Domain domain = oDomainsCache.GetDomain((int)domainId, m_nGroupID);
            long dlmId = domain.m_nLimit;
            DataTable dt = null;
            if (!DomainDal.Get_ProximityDetectionDataForInsertion(m_nGroupID, domainId, ref numOfAllowedNetworks, ref dt, dlmId))
            {
                // failed to extract data from DB.
                // log and return err

                log.Debug("AddHomeNetworkToDomain - " + GetUpdateHomeNetworkErrMsg("Failed to extract data from DB", domainId, candidate, 0, numOfAllowedNetworks, numOfActiveNetworks));
                return response;
            }

            GetListOfExistingHomeNetworksForInsertion(dt, out lstOfHomeNetworks, out numOfActiveNetworks);

            // check if network already exists
            if (lstOfHomeNetworks.Contains(candidate))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.HomeNetworkAlreadyExists, "Home network already exists for household");
                return response;
            }

            // check quantity limitation
            if (!IsSatisfiesQuantityConstraint(numOfAllowedNetworks, numOfActiveNetworks))
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.HomeNetworkLimitation, "Home networks exceeded limit");
                return response;
            }

            // all validations pass, add new home network to domain
            DataTable dtInsertedNetwork = DomainDal.Insert_NewHomeNetwork(m_nGroupID, candidate.UID, domainId, candidate.Name, candidate.Description, candidate.IsActive, candidate.CreateDate);
            if (dtInsertedNetwork == null)
            {
                // failed to insert
                // log
                log.Debug("AddHomeNetworkToDomain - " + String.Concat("Failed to add to domain: ", domainId, " the home network: ", candidate.ToString()));
                return response;
            }
            else
            {
                response.HomeNetwork = new HomeNetwork()
                {
                    UID = ODBCWrapper.Utils.GetSafeStr(dtInsertedNetwork.Rows[0]["NETWORK_ID"]),
                    Name = ODBCWrapper.Utils.GetSafeStr(dtInsertedNetwork.Rows[0]["NAME"]),
                    Description = ODBCWrapper.Utils.GetSafeStr(dtInsertedNetwork.Rows[0]["DESCRIPTION"]),
                    IsActive = ODBCWrapper.Utils.GetIntSafeVal(dtInsertedNetwork.Rows[0]["IS_ACTIVE"]) != 0,
                    CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dtInsertedNetwork.Rows[0]["CREATE_DATE"])
                };

                // remove domain from cache 
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain((int)domainId);

                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public virtual DeviceResponse SubmitAddDeviceToDomain(int groupID, int domainID, string userID, string deviceUdid, string deviceName, int brandID)
        {
            DeviceResponse response = new DeviceResponse() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            if (domainID <= 0 || string.IsNullOrEmpty(deviceUdid))
            {
                return response;
            }

            Domain domain = DomainInitializer(groupID, domainID, false);

            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DomainSuspended, "Domain suspended") ;
                return response;
            }

            Device device = new Device(deviceUdid, brandID, m_nGroupID, deviceName, domainID);
            
            DomainResponseStatus domainResponseStatus;
            int userId = 0;
            if (!int.TryParse(userID, out userId))
            {
                return response;
            }


            // If domain is restricted and action user is the master, just add the device
            if ((domain.m_DomainRestriction == DomainRestriction.Unrestricted) ||
                (domain.m_DomainRestriction == DomainRestriction.UserMasterRestricted) ||
                ((domain.m_DomainRestriction == DomainRestriction.DeviceMasterRestricted || domain.m_DomainRestriction == DomainRestriction.DeviceUserMasterRestricted) &&
                (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0) && (domain.m_masterGUIDs.Contains(userId))))
            {
                domainResponseStatus = domain.AddDeviceToDomain(groupID, domain.m_nDomainID, deviceUdid, deviceName, brandID, ref device);
                response.Device = new DeviceResponseObject() { m_oDevice = device, m_oDeviceResponseStatus = DeviceResponseStatus.OK };
                response.Status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
                return response;
            }

            domainResponseStatus = domain.SubmitAddDeviceToDomainRequest(groupID, deviceUdid, deviceName, ref device);
            response.Device = new DeviceResponseObject() { m_oDevice = device, m_oDeviceResponseStatus = DeviceResponseStatus.OK };
            response.Status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
            return response;
        }
    }
}
