using ApiObjects;
using ApiObjects.DRM;
using ApiObjects.Response;
using ApiObjects.Roles;
using Core.Users.Cache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Users
{
    public abstract class BaseDomain
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string DEFAULT_SUSPENDED_ROLE = "DefaultSuspendedRole";

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

                domain.shouldUpdateInfo = true;

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
            switch (domainStatus)
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

        public virtual DomainResponseStatus RemoveDomain(int domainId, bool purge)
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            try
            {
                if (domainId < 1)
                    return DomainResponseStatus.DomainNotExists;

                Domain domain = DomainInitializer(m_nGroupID, domainId, false);

                if (domain == null || domain.m_DomainStatus == DomainStatus.DomainNotExists)
                    return DomainResponseStatus.DomainNotExists;

                //cjeck if  send mail = true
                bool sendMail = DomainDal.GetCloseAccountMailTrigger(m_nGroupID);
                log.DebugFormat("RemoveDomain - Close Account mail settings groupId = {0}, sendMail = {1}", m_nGroupID, sendMail);
                User masterUser = new User();
                if (sendMail)
                {
                    // get domain master user details                    
                    int userId = domain.m_masterGUIDs.FirstOrDefault();
                    masterUser = new User(m_nGroupID, userId);

                    if (masterUser == null || !int.TryParse(masterUser.m_sSiteGUID, out userId) || userId == 0)
                    {
                        log.DebugFormat("RemoveDomain - Can't send mail to domain = {0} due to master user {1} is not valid", domainId, userId);
                        sendMail = false;
                    }
                }

                domain.shouldPurge = purge;
                res = domain.Remove();

                if (res != DomainResponseStatus.OK)
                {
                    return res;
                }

                if (sendMail)
                {
                    RemoveDomianMailRequest mailRequest = MailFactory.GetRemoveDomainMailRequest(masterUser, m_nGroupID);
                    if (mailRequest != null)
                    {
                        bool sendingMailResult = Utils.SendMail(m_nGroupID, mailRequest);
                        log.DebugFormat("RemoveDomain - sending mail to domain : {0}, result : {1}", domainId, sendingMailResult);
                    }
                }

            }

            catch (Exception ex)
            {
                res = DomainResponseStatus.Error;
                StringBuilder sb = new StringBuilder("Exception at RemoveDomain. ");
                sb.Append(String.Concat(" D ID: ", domainId));
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
            
            eDomainResponseStatus = domain.ChangeDeviceDomainStatus(m_nGroupID, nDomainID, sDeviceUDID, bIsEnable);
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
            int masterUserID = DAL.UsersDal.GetUserIDByUsername(sMasterUN, m_nGroupID);

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

        public virtual ValidationResponseObject ValidateLimitationModule(string udid, int deviceBrandID, long userId, long domainID, ValidationType validationType,
                                                                         List<int> mediaRuleIds, List<long> assetRuleIds, int mediaID = 0)
        {
            ValidationResponseObject response = new ValidationResponseObject();

            Domain domain = GetDomainForValidation(userId, domainID);
            if (domain != null && domain.m_DomainStatus != DomainStatus.Error)
            {
                int deviceFamilyId = 0;
                //to add here isDevicePlayValid
                bool bisDevicePlayValid = IsDevicePlayValid(userId.ToString(), udid, domain, out deviceFamilyId);

                response.m_lDomainID = domain.m_nDomainID;
                if (!bisDevicePlayValid)
                {
                    response.m_eStatus = DomainResponseStatus.DeviceNotInDomain;
                    return response;
                }

                switch (validationType)
                {
                    case ValidationType.Concurrency:
                        {
                            response.m_eStatus = ConcurrencyManager.Validate(mediaRuleIds, assetRuleIds, domain, mediaID, udid, m_nGroupID, deviceBrandID, deviceFamilyId);
                            break;
                        }
                    case ValidationType.Frequency:
                        {
                            response.m_eStatus = domain.ValidateFrequency(udid, deviceBrandID);
                            break;
                        }
                    default:
                        {
                            // Quantity
                            response.m_eStatus = domain.ValidateQuantity(udid, deviceBrandID);
                            break;
                        }
                }
            }

            return response;
        }

        /*This method return status via ValidationResponseObject object if there is a limitation to play this npvrID */
        public virtual ValidationResponseObject ValidateLimitationNpvr(string sUDID, int nDeviceBrandID, long lSiteGuid, long lDomainID, ValidationType eValidationType,
            int nNpvrConcurrencyLimit = 0, string sNpvrID = default(string))
        {
            ValidationResponseObject res = new ValidationResponseObject();

            Domain domain = GetDomainForValidation(lSiteGuid, lDomainID);
            if (domain != null && domain.m_DomainStatus != DomainStatus.Error)
            {
                int deviceFamilyId = 0;
                //to add here isDevicePlayValid
                bool bisDevicePlayValid = IsDevicePlayValid(lSiteGuid.ToString(), sUDID, domain, out deviceFamilyId);

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

        public virtual ApiObjects.Response.Status SuspendDomain(int nDomainID, int? roleId = null)
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

            Role suspendDefaultRole = null;

            // validate domain is not suspended
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended)
            {
                if (!roleId.HasValue)
                {
                    //get default sespend role id 
                    suspendDefaultRole = GetDefaultSuspendedRole();
                    if (suspendDefaultRole != null)
                    {
                        // if domain == the default role id return "Domain already suspended"
                        if (domain.roleId.HasValue && domain.roleId.Value == (int)suspendDefaultRole.Id)
                        {
                            result.Code = (int)eResponseStatus.DomainAlreadySuspended;
                            result.Message = "Domain already suspended";
                            return result;
                        }
                    }
                }
                else if (domain.roleId.HasValue && domain.roleId.Value == roleId.Value)
                {
                    result.Code = (int)eResponseStatus.DomainAlreadySuspended;
                    result.Message = "Domain already suspended";
                    return result;
                }
            }

            domain.shouldUpdateSuspendStatus = true;
            domain.nextSuspensionStatus = DomainSuspentionStatus.Suspended;

            // get current domain.roleId ==> to update the users 
            int? currentRoleId = domain.roleId;

            if (roleId.HasValue)
            {
                domain.roleId = roleId.Value;
            }
            else // get default roleId
            {
                // get default role 
                if (suspendDefaultRole == null)
                {
                    suspendDefaultRole = GetDefaultSuspendedRole();
                }
                if (suspendDefaultRole != null)
                {
                    domain.roleId = (int)suspendDefaultRole.Id;
                }
            }

            // suspend domain
            bool suspendSucceed = domain.Update();

            // remove from cache
            if (suspendSucceed)
            {
                if (domain.roleId.HasValue)
                {
                    bool resultSuspendedUsers = UpdateSuspendedUserRoles(domain, m_nGroupID, currentRoleId.HasValue ? currentRoleId.Value : 0, domain.roleId.Value);

                    // invalidate user roles 
                    domain.InvalidateDomainUsersRoles();

                    // check if this new roleId excluded renew subscription if so- insert messages to queue
                    if (APILogic.Api.Managers.RolesPermissionsManager.IsPermittedPermission(m_nGroupID, domain.m_masterGUIDs[0].ToString(), RolePermissions.RENEW_SUBSCRIPTION))
                    {
                        ResumeDomainSubscriptions(nDomainID);
                    }
                }

                // Remove Domain
                oDomainCache.RemoveDomain(nDomainID);
                UsersCache usersCache = UsersCache.Instance();
                foreach (int userID in domain.m_UsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);
                }

                foreach (int userID in domain.m_DefaultUsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);
                }

                domain.InvalidateDomain();

            }
            // update result
            if (suspendSucceed)
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

        private Role GetDefaultSuspendedRole()
        {
            try
            {
                List<Role> roles = ApiDAL.GetRolesByNames(m_nGroupID, new List<string>() { DEFAULT_SUSPENDED_ROLE });
                if (roles != null && roles.Count() > 0)
                {
                    return roles[0];
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail to get ApiDAL.GetRolesByNames ex :{0}, m_nGroupID : {1}, DefaultSuspendedRoleName : {2}", ex, m_nGroupID, DEFAULT_SUSPENDED_ROLE);
            }
            return null;
        }

        private bool UpdateSuspendedUserRoles(Domain domain, int groupId, int? currentRoleId, int? newRoleId)
        {
            int rowCount = 0;
            try
            {
                // add roleId to all Users
                List<int> usersInDomain = domain.m_DefaultUsersIDs;
                usersInDomain.AddRange(domain.m_UsersIDs);

                rowCount = UsersDal.Upsert_SuspendedUsersRole(groupId, usersInDomain, currentRoleId.HasValue ? currentRoleId.Value : 0, newRoleId.HasValue ? newRoleId.Value : 0);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail Upsert_SuspendedUsersRole in DB m_nGroupID={0}, domainId={1}, currentRoleId={2}, newRoleId={3}, ex={4}", groupId, domain.m_nDomainID,
                    currentRoleId.HasValue ? currentRoleId.Value : 0, newRoleId.HasValue ? newRoleId.Value : 0, ex);
            }
            return rowCount > 0 ? true : false;
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

            domain.shouldUpdateSuspendStatus = true;
            domain.nextSuspensionStatus = DomainSuspentionStatus.OK;

            int? currentRoleId = domain.roleId;
            domain.roleId = null;

            // resume domain
            bool resumeSucceed = domain.Update();

            // remove from cache
            if (resumeSucceed)
            {
                // update all users to suspended roleId = null
                if (currentRoleId.HasValue)
                {
                    bool resultSuspendedUsers = UpdateSuspendedUserRoles(domain, m_nGroupID, currentRoleId.HasValue ? currentRoleId.Value : 0, null);
                }
                // Remove Domain
                oDomainCache.RemoveDomain(nDomainID);
                UsersCache usersCache = UsersCache.Instance();
                foreach (int userID in domain.m_UsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);
                }

                foreach (int userID in domain.m_DefaultUsersIDs)
                {
                    // Remove Users
                    usersCache.RemoveUser(userID, m_nGroupID);

                }
                domain.InvalidateDomain();
                domain.InvalidateDomainUsersRoles();

                result.Code = (int)eResponseStatus.OK;
                // get all subscription in status suspended 
                ResumeDomainSubscriptions(nDomainID);
            }
            else
            {
                result.Code = (int)eResponseStatus.Error;
                result.Message = "Failed to resume domain";
            }

            return result;
        }
        private void ResumeDomainSubscriptions(int domainId)
        {
            try
            {
                DataTable dt = ConditionalAccessDAL.GetSubscriptionPurchase(m_nGroupID, domainId);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    //create messages to queue as needed
                    // get all regular subscription 
                    List<DataRow> drs = dt.AsEnumerable().Where(x => x.Field<long>("unified_process_id") == 0 && x.Field<DateTime>("end_date") < DateTime.UtcNow).ToList();
                    if (drs != null && drs.Count > 0)
                    {
                        foreach (DataRow dr in drs)
                        {
                            string siteguid = ODBCWrapper.Utils.GetSafeStr(dr, "SITE_USER_GUID");
                            long purchaseId = ODBCWrapper.Utils.GetLongSafeVal(dr, "id");
                            string billingGuid = ODBCWrapper.Utils.GetSafeStr(dr, "billing_guid");
                            DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "end_date");
                            if (!Core.ConditionalAccess.RenewManager.HandleResumeDomainSubscription(m_nGroupID, domainId, siteguid, purchaseId, billingGuid, endDate))
                            {
                                log.ErrorFormat("fail to add new message to queue groupID:{0}, domainId:{1}, siteguid:{2}, purchaseId:{3}, billingGuid:{4}, endDate:{5}",
                                    m_nGroupID, domainId, siteguid, purchaseId, billingGuid, endDate);
                            }
                        }
                    }
                    // get all unified billing renews 
                    drs = dt.AsEnumerable().Where(x => x.Field<long>("unified_process_id") > 0 && x.Field<DateTime>("unified_end_date") < DateTime.UtcNow).ToList();
                    Dictionary<string, List<DataRow>> renewUnifiedDict = drs.GroupBy(x => string.Format("{0}_{1}", x.Field<long>("unified_process_id"), x.Field<DateTime>("unified_end_date"))).ToDictionary(g => g.Key, g => g.ToList());
                    // enqueue unified renew transaction
                    foreach (KeyValuePair<string, List<DataRow>> dr in renewUnifiedDict)
                    {
                        if (dr.Value != null && dr.Value.Count > 0)
                        {
                            long processID = ODBCWrapper.Utils.GetLongSafeVal(dr.Value[0], "unified_process_id");
                            DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dr.Value[0], "unified_end_date");
                            if (!Core.ConditionalAccess.RenewManager.HandleRenewUnifiedSubscriptionPending(m_nGroupID, domainId, endDate, 0, processID))
                            {
                                log.ErrorFormat("fail to add new message to unified billing queue groupID:{0}, domainId:{1}, processID:{2}, endDate:{3}",
                                    m_nGroupID, domainId, processID, endDate);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("fail ResumeDomainSubscriptions groupId:{0}, domainId:{1}, ex:{2}", m_nGroupID, domainId, ex);
            }
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

        /// <summary>
        /// return True if device recognize in Domain false another case (assumption : user is valid !)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="udid"></param>
        /// <param name="userDomain"></param>
        /// <param name="deviceFamilyId"></param>
        /// <returns></returns>
        protected bool IsDevicePlayValid(string userId, string udid, Domain userDomain, out int deviceFamilyId)
        {
            bool isDeviceRecognized = false;
            deviceFamilyId = 0;

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
                                        if (string.Compare(device.m_deviceUDID.Trim(), udid.Trim()) == 0)
                                        {
                                            isDeviceRecognized = true;
                                            deviceFamilyId = device.m_deviceFamilyID;
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
                                        if (string.Compare(device.m_deviceUDID.Trim(), udid.Trim()) == 0)
                                        {
                                            isDeviceRecognized = true;
                                            deviceFamilyId = device.m_deviceFamilyID;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    //Patch!!
                                    if (container.m_deviceFamilyID == 5 && (string.IsNullOrEmpty(udid) || udid.ToLower().Equals("web site")))
                                    {
                                        isDeviceRecognized = true;
                                        deviceFamilyId = container.m_deviceFamilyID;
                                    }
                                }
                                if (isDeviceRecognized)
                                {
                                    break;
                                }
                            }
                        }
                        if (!familyIDs.Contains(5) && string.IsNullOrEmpty(udid) || (familyIDs.Contains(5) && familyIDs.Count == 0) || (!familyIDs.Contains(5) && udid.ToLower().Equals("web site")))
                        {
                            isDeviceRecognized = true;
                            deviceFamilyId = 5;
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
                log.Error("IsDevicePlayValid - " + string.Format("faild ex={0} siteGuid ={1} deviceName={2} domainID={3}", ex.Message, userId, udid, userDomain != null ? userDomain.m_nDomainID : 0),
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

        private Domain GetDomainForValidation(long userId, long domainId)
        {
            DomainsCache oDomainCache = DomainsCache.Instance();
            Domain res = null;
            if (domainId > 0)
            {
                res = oDomainCache.GetDomain((int)domainId, m_nGroupID);
            }

            if (res == null && userId > 0)
            {
                bool tempIsMaster = false;
                int tempOperatorID = 0;
                DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;
                int newDomainId = 0;
                try
                {
                    // try getting user from cache
                    User user = null;
                    UsersCache usersCache = UsersCache.Instance();
                    user = usersCache.GetUser(Convert.ToInt32(userId), m_nGroupID);
                    if (user != null)
                    {
                        newDomainId = user.m_domianID;
                    }
                }

                catch (Exception ex)
                {
                    log.Error("Failed getting user from cache", ex);
                }

                if (newDomainId == 0)
                {
                    newDomainId = DomainDal.GetDomainIDBySiteGuid(m_nGroupID, (int)userId, ref tempOperatorID, ref tempIsMaster, ref eSuspendStat);
                }

                // TODO SHIR - CHECK ==
                if (newDomainId < 1 || newDomainId == (int)domainId)
                    return null;
                res = oDomainCache.GetDomain(newDomainId, m_nGroupID);
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

        public virtual DeviceResponse GetDevice(string udid, int domainId, string userId, string ip)
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

                            // get drmAdapterId if configured and call adapter
                            int drmAdapterId = CachingHelpers.DrmAdapterCache.Instance.GetGroupDrmAdapterId(m_nGroupID);
                            if (drmAdapterId > 0)
                            {
                                var adapterResponse = Api.api.GetCustomDrmDeviceLicenseData(m_nGroupID, drmAdapterId, userId, udid, device.m_deviceFamily, device.m_deviceBrandID, ip);
                                if (adapterResponse.Status.Code != (int)eResponseStatus.OK)
                                {
                                    response.Status = adapterResponse.Status;
                                    return response;
                                }
                                response.Device.m_oDevice.LicenseData = adapterResponse.Value;
                            }

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
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.DomainSuspended, "Domain suspended");
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

        /*
         VerifyDRMDevice => verify that the DRM ID is bound to the specific UDID by specific policy
         *  get 4 params : 
         *  groupId , 
         *  userId , 
         *  udid (is not necessarily sent), 
         *  drmId (is a unique ID per device - will check the uniqueness)
         *  1. In this scenario, a UDID is not necessarily sent in the verification request for a specific device brand. Therefore, the DRM ID shall be set on a random free device on the household.
         *  2. In this scenario, a UDID is assumed to always be sent => need to act by policy : free in specific family device/specific deviceUdid / free in household devices
         */
        public virtual bool VerifyDRMDevice(int groupId, string userId, string udid, string drmId)
        {
            try
            {
                #region params
                DeviceContainer deviceContainer = new DeviceContainer();
                Device device = new Device();
                List<int> deviceIds = new List<int>();
                Dictionary<int, string> domainDrmId = new Dictionary<int, string>();
                #endregion

                // get configuration of drm policy
                DrmPolicy drmPolicy = Utils.GetDrmPolicy(m_nGroupID);
                if (drmPolicy == null)
                {
                    log.Error("fail to get drm policy at VerifyDRMDevice ");
                    return false; // error 
                }

                // when deviceUdid is empty drmPolicy.Policy can't be DeviceLevel              
                if (string.IsNullOrEmpty(udid) && drmPolicy.Policy == DrmSecurityPolicy.DeviceLevel)
                {
                    return false;
                }

                // get domain by userId
                Domain domain = GetDomainByUser(m_nGroupID, userId);
                if (domain == null || domain.m_deviceFamilies == null || domain.m_deviceFamilies.Count == 0)
                {
                    log.ErrorFormat("fail to get GetDomainByUser or m_deviceFamilies empty VerifyDRMDevice groupId={0}, userId={1}", m_nGroupID, userId);
                    return false; // error 
                }
                //check the uniqueness of drmID
                KeyValuePair<int, string> drmValue = new KeyValuePair<int, string>();
                bool isDrmIdUnique = Utils.IsDrmIdUnique(drmId, domain.m_nDomainID, udid, m_nGroupID, ref drmValue);
                if (!isDrmIdUnique)
                {
                    return false; // drmid exsits in ANOTHER domain
                }
                else if (drmValue.Key == domain.m_nDomainID && drmValue.Value == udid && !string.IsNullOrEmpty(udid))
                {
                    return true;
                }// else - drmId unique in domain or not exsits at all continue 


                if (!string.IsNullOrEmpty(udid))
                {
                    // check that udid exsits in doimain device list
                    deviceContainer = domain.m_deviceFamilies.FirstOrDefault(x => x.DeviceInstances != null && x.DeviceInstances.Find(u => u.m_deviceUDID == udid) != null ? true : false);
                    if (deviceContainer == null || deviceContainer.DeviceInstances == null || deviceContainer.DeviceInstances.Count == 0)
                    {
                        log.ErrorFormat("udid not exsits in Domain devices list groupId={0}, userId={1}, udid ={2}", m_nGroupID, userId, udid);
                        return false; // error 
                    }

                    device = deviceContainer.DeviceInstances.Where(x => x.m_deviceUDID == udid).First(); // get specific device by udid

                    // check that device family in the Family policy roles
                    if (drmPolicy.FamilyLimitation.Contains(deviceContainer.m_deviceFamilyID))
                    {
                        // get domainDrmId by deviceIds list 
                        deviceIds = deviceContainer.DeviceInstances.Select(d => int.Parse(d.m_id)).ToList<int>();
                        domainDrmId = Utils.GetDomainDrmId(m_nGroupID, domain.m_nDomainID, deviceIds);
                        if (domainDrmId.Count == 0)
                        {
                            log.ErrorFormat("fail GetDomainDrmId groupId={0}, domainId={1}", m_nGroupID, domain.m_nDomainID);
                            return false; // error 
                        }
                        if (domainDrmId.Where(x => x.Value == drmId).Count() > 0)
                        {
                            if (drmValue.Key == 0 || string.IsNullOrEmpty(drmValue.Value))
                            {
                                return Utils.SetDrmId(drmId, domain.m_nDomainID, udid, groupId);
                            }
                            return true;
                        }
                        else if (drmPolicy.Policy == DrmSecurityPolicy.HouseholdLevel && drmValue.Key > 0 && !string.IsNullOrEmpty(drmValue.Value))
                        {
                            return false;
                        }
                        // get all devices with empty drmId 
                        domainDrmId = domainDrmId.Where(x => string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
                        if (domainDrmId != null && domainDrmId.Count > 0)
                        {
                            // update device table + return true/false by success of update table and remove it from CB

                            if (DomainDal.UpdateDeviceDrmID(m_nGroupID, domainDrmId.First().Key.ToString(), drmId, domain.m_nDomainID))
                            {
                                return Utils.SetDrmId(drmId, domain.m_nDomainID, udid, groupId);
                            }
                        }
                        return false;
                    }
                }

                switch (drmPolicy.Policy)
                {
                    case DrmSecurityPolicy.DeviceLevel:
                        if (drmValue.Key == domain.m_nDomainID && drmValue.Value != udid && !string.IsNullOrEmpty(udid) && !string.IsNullOrEmpty(drmValue.Value))
                        {
                            return false;
                        }
                        deviceIds = new List<int>() { int.Parse(device.m_id) };
                        if (CheckDrmSecurity(drmId, deviceIds, domainDrmId, domain, drmPolicy.Policy, drmValue))
                        {
                            return Utils.SetDrmId(drmId, domain.m_nDomainID, udid, m_nGroupID);
                        }
                        return false;

                    case DrmSecurityPolicy.HouseholdLevel:

                        deviceIds = (domain.m_deviceFamilies.SelectMany(x => x.DeviceInstances).ToList<Device>()).Where(f => drmPolicy.FamilyLimitation.Count == 0 ||
                            !drmPolicy.FamilyLimitation.Contains(f.m_deviceFamilyID)).Select(y => int.Parse(y.m_id)).ToList<int>();
                        if (CheckDrmSecurity(drmId, deviceIds, domainDrmId, domain, drmPolicy.Policy, drmValue))
                        {
                            return Utils.SetDrmId(drmId, domain.m_nDomainID, udid, m_nGroupID);
                        }
                        return false;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("VerifyDRMDevice fail groupId={0}, userId={1}, deviceUdid={2}, drmId={3}, ex={4}", groupId, userId, udid, drmId, ex.Message);
            }
            return false;
        }

        private bool CheckDrmSecurity(string drmId, List<int> deviceIds, Dictionary<int, string> domainDrmId, Domain domain, DrmSecurityPolicy drmPolicy, KeyValuePair<int, string> drmValue)
        {
            domainDrmId = Utils.GetDomainDrmId(m_nGroupID, domain.m_nDomainID, deviceIds);
            // drmid exsits (in the houshold)
            if (domainDrmId != null && domainDrmId.Count > 0)
            {
                if (domainDrmId.Where(x => x.Value == drmId).Count() > 0)
                {
                    if (drmPolicy == DrmSecurityPolicy.DeviceLevel)
                    {
                        if (domainDrmId.Where(x => deviceIds.Contains(x.Key)).Count() > 0)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    return true;
                }
                else if (drmPolicy == DrmSecurityPolicy.HouseholdLevel && drmValue.Key > 0 && !string.IsNullOrEmpty(drmValue.Value))
                {
                    return false;
                }
                // get an empty slot and set it with drmId
                domainDrmId = domainDrmId.Where(x => string.IsNullOrEmpty(x.Value)).ToDictionary(x => x.Key, x => x.Value);
                if (domainDrmId != null && domainDrmId.Count > 0)
                {
                    return DomainDal.UpdateDeviceDrmID(m_nGroupID, domainDrmId.First().Key.ToString(), drmId, domain.m_nDomainID);
                }
            }
            return false;
        }

        internal UserResponse LoginWithDevicePIN(int groupId, string pin, string sessionID, string ip, string udid, bool preventDoubleLogins, List<KeyValuePair> keyValueList)
        {
            UserResponse response = new UserResponse();

            // get device
            Device device = new Device(groupId);
            device.Initialize(udid);
            if (device == null || string.IsNullOrEmpty(device.m_deviceUDID))
            {
                log.ErrorFormat("Device does not exist, UDID = {0}", udid);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotExists, "Device does not exist");
                return response;
            }

            // validations
            if (string.IsNullOrEmpty(device.m_pin))
            {
                log.ErrorFormat("Device pin does not exists, UDID = {0}, pin = {1}", udid, pin);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.PinNotExists, "Device pin does not exists");
                return response;
            }

            if (device.m_domainID == 0)
            {
                log.ErrorFormat("Device is not in a domain, UDID = {0}", udid);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotInDomain, "Device is not in a domain");
                return response;
            }

            if (pin != device.m_pin)
            {
                log.ErrorFormat("Device pin does not match the supplied pin, UDID = {0}, pin = {1}, device pin = {3}", udid, pin, device.m_pin);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.NoValidPin, "Supplied pin is not valid");
                return response;
            }

            // login with master user
            DomainsCache domainCache = DomainsCache.Instance();
            Domain domain = domainCache.GetDomain(device.m_domainID, groupId);
            if (domain == null || domain.m_DomainStatus == DomainStatus.Error || domain.m_DomainStatus == DomainStatus.DomainNotExists)
            {
                log.ErrorFormat("Domain does not exist, UDID = {0}, domainId", udid, device.m_domainID);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.DomainNotExists, "Domain does not exist");
                return response;
            }

            if (domain.m_masterGUIDs == null || domain.m_masterGUIDs.Count == 0)
            {
                log.ErrorFormat("Domain master users do not exist, UDID = {0}, domainId", udid, device.m_domainID);
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.MasterUserNotFound, "Master user was not found");
                return response;
            }

            int masterUserId = domain.m_masterGUIDs[0];

            response.user = Core.Users.Module.SignInWithUserId(groupId, masterUserId, sessionID, ip, udid, preventDoubleLogins, keyValueList);
            if (response.user == null)
            {
                log.ErrorFormat("Failed to login with master user ID = {0}", masterUserId);
                return response;
            }

            response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);

            if (response.resp.Code != (int)eResponseStatus.OK)
            {
                log.ErrorFormat("Failed to login with master user ID = {0}, code = {1}, message = {2}", masterUserId, response.resp.Code, response.resp.Message);
                return response;
            }

            // delete pin from DB - for single login 
            if (!DomainDal.SetDevicePinToNull(groupId, udid, pin))
            {
                log.ErrorFormat("Failed to delete pin for device after successful login. udid = {0}, pin = {1}", udid, pin);
            }

            return response;
        }

        internal ApiObjects.Response.Status DeleteDevice(int groupId, string udid)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            int deviceId = Device.GetDeviceIDByUDID(udid, groupId);
            if (deviceId == 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.DeviceNotExists, "Device does not exist");
            }
            else
            {
                List<Domain> deviceDomains = Domain.GetDeviceDomains(deviceId, groupId);

                if (deviceDomains == null || deviceDomains.Count == 0)
                {
                    DomainDal.UpdateDeviceStatus(deviceId, 0, 2);
                }
                else
                {
                    DomainResponseStatus domainResponseStatus = deviceDomains[0].RemoveDeviceFromDomain(udid, true);
                    response = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
                    if (response.Code != (int)eResponseStatus.OK)
                    {
                        log.ErrorFormat("Failed to remove device with UDID = {0} and deviceId = {1} from domainId = {2}", udid, deviceId, deviceDomains[0].m_nDomainID);
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
            }
            return response;
        }

        public virtual ApiObjects.Response.Status ShouldPurgeDomain(int domainId, out bool shouldPurge)
        {
            shouldPurge = false;
            DataTable dt = DomainDal.GetDomainDbObject(this.m_nGroupID, domainId);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                //check if domain exist and belong to partner
                DataRow dr = dt.Rows[0];
                int domianStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "status");
                int purge = ODBCWrapper.Utils.GetIntSafeVal(dr, "PURGE");

                // if Purge column value true - error already purge
                if (purge == 1)
                {
                    log.ErrorFormat("PurgeDomain failed: Household {0}, GroupId: {1} already purged", domainId, this.m_nGroupID);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Household already purged" };
                }

                if (domianStatus != 2)
                {
                    shouldPurge = false;
                    log.ErrorFormat("PurgeDomain failed: Household {0}, GroupId: {1} need to be deleted before purged", domainId, this.m_nGroupID);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
                else
                {
                    shouldPurge = true;
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
            }
            else
            {
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.DomainNotExists, Message = eResponseStatus.DomainNotExists.ToString() };
            }
        }

        public virtual ApiObjects.Response.Status PurgeDomain(int domainId)
        {
            DataTable dt = DomainDal.GetDomainDbObject(this.m_nGroupID, domainId);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                //check if domain exist and belong to partner
                DataRow dr = dt.Rows[0];
                int domianStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "status");
                int purge = ODBCWrapper.Utils.GetIntSafeVal(dr, "PURGE");

                // if Purge column value true - error already purge
                if (purge == 1)
                {
                    log.ErrorFormat("PurgeDomain failed: Household {0}, GroupId: {1} already purged", domainId, this.m_nGroupID);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = "Household already purged" };
                }

                if (domianStatus != 2)
                {
                    log.ErrorFormat("PurgeDomain failed: Household {0}, GroupId: {1} need to be deleted before purged", domainId, this.m_nGroupID);
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
                }

                //else purge domain and users
                bool success = DomainDal.PurgeDomain(this.m_nGroupID, domainId);
                if (success)
                {
                    return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.OK, Message = eResponseStatus.OK.ToString() };
                }
            }
            else
            {
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.DomainNotExists, Message = eResponseStatus.DomainNotExists.ToString() };
            }

            return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() };
        }
    }
}
