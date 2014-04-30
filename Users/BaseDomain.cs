using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Users
{
    public abstract class BaseDomain
    {
        protected int m_nGroupID;

        protected BaseDomain() { }
        public BaseDomain(int nGroupID)
        {
            m_nGroupID = nGroupID;

        }

        #region Public Abstract
        public abstract DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID, string sCoGuid);

        public abstract DomainResponseObject AddDomain(string sDomainName, string sDomainDescription, int nMasterUserGuid, int nGroupID);

        public abstract DomainResponseStatus RemoveDomain(int nDomainID);

        public abstract DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription);

        public abstract DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserGuid, string sMasterUsername);

        public abstract DomainResponseObject ResetDomain(int nDomainID, int nFrequencyType);

        public abstract DomainResponseObject ChangeDomainMaster(int nDomainID, int nCurrentMasterID, int nNewMasterID);
        #endregion

        #region Public Virtual

        public virtual DomainResponseObject AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string sDeviceName, int nBrandID)
        {
            DomainResponseObject oDomainResponseObject = new DomainResponseObject();
            oDomainResponseObject.m_oDomainResponseStatus = DomainResponseStatus.Error;

            Domain domain = DomainInitializer(nGroupID, nDomainID);
            if (domain == null)
            {
                oDomainResponseObject.m_oDomain = null;
            }
            else
            {
                oDomainResponseObject.m_oDomain = domain;
                Device device = new Device(sUDID, nBrandID, m_nGroupID, sDeviceName, nDomainID);
                device.Initialize(sUDID, sDeviceName);
                oDomainResponseObject.m_oDomainResponseStatus = domain.AddDeviceToDomain(m_nGroupID, nDomainID, sUDID, sDeviceName, nBrandID, ref device);
            }

            return oDomainResponseObject;
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
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.Error);
            }

            //Init The Domain
            domain = DomainInitializer(nGroupID, nDomainID);

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
                return (new DomainResponseObject(null, DomainResponseStatus.UnKnown));
            }

            //Init the Domain
            Domain domain = DomainInitializer(nGroupID, nDomainID);

            if (domain == null)
            {
                return (new DomainResponseObject(null, DomainResponseStatus.DomainNotInitialized));
            }


            //Delete the User from Domain

            DomainResponseStatus eDomainResponseStatus = domain.RemoveUserFromDomain(nGroupID, nDomainID, nUserGUID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual Domain GetDomainInfo(int nDomainID, int nGroupID)
        {
            Domain domain = DomainInitializer(nGroupID, nDomainID);

            return domain;
        }

        public virtual DomainResponseObject ChangeDeviceDomainStatus(int nDomainID, string sDeviceUDID, bool bIsEnable)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID);

            DomainResponseObject oDomainResponseObject;

            DomainResponseStatus eDomainResponseStatus = domain.ChangeDeviceDomainStatus(m_nGroupID, nDomainID, sDeviceUDID, bIsEnable);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject RemoveDeviceFromDomain(int nDomainID, string sDeviceUDID)
        {
            Domain domain = DomainInitializer(m_nGroupID, nDomainID);
            DomainResponseObject oDomainResponseObject;

            DomainResponseStatus eDomainResponseStatus = domain.RemoveDeviceFromDomain(sDeviceUDID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public virtual DomainResponseObject SubmitAddDeviceToDomainRequest(int nGroupID, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID)
        {
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0)
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.Error);
            }

            Domain domain = DomainInitializer(nGroupID, nDomainID);

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

            int nActivationStatus = DomainDal.GetDomainDeviceActivateStatus(m_nGroupID, nDeviceID);

            resp.m_oDomain = nActivationStatus == 1 ? domain : null;
            resp.m_oDomainResponseStatus = nActivationStatus == 1 ? DomainResponseStatus.OK : DomainResponseStatus.DeviceNotConfirmed;

            return resp;
        }

        public virtual List<string> GetDomainUserList(int nDomainID, int nGroupID)
        {
            return Domain.GetFullUserList(nDomainID, nGroupID);
        }

        public virtual List<Domain> GetDeviceDomains(string sUDID)
        {
            List<Domain> retVal = null;
            int deviceID = Device.GetDeviceIDByUDID(sUDID, m_nGroupID);
            if (deviceID > 0)
            {
                retVal = Domain.GetDeviceDomains(deviceID, m_nGroupID);
            }
            return retVal;
        }

        public virtual DeviceResponseObject RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName)
        {
            Domain domain = DomainInitializer(nGroupID, nDomainID);

            DeviceResponseStatus eRetVal = DeviceResponseStatus.UnKnown;
            Device device = domain.RegisterDeviceToDomainWithPIN(nGroupID, sPIN, nDomainID, sDeviceName, ref eRetVal);
            return new DeviceResponseObject(device, eRetVal);
        }

        public virtual int GetDomainIDByCoGuid(string coGuid)
        {
            return DomainDal.GetDomainIDByCoGuid(coGuid);
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

            Domain domain = DomainInitializer(nGroupID, nDomainID);

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
            Domain domain = DomainInitializer(m_nGroupID, nDomainID);

            domain.m_DomainRestriction = rest;
            bool res = domain.Update();

            return res;
        }

        public virtual List<HomeNetwork> GetDomainHomeNetworks(long lDomainID)
        {
            return Utils.GetHomeNetworksOfDomain(lDomainID, m_nGroupID);
        }

        public virtual NetworkResponseObject AddHomeNetworkToDomain(long lDomainID, string sNetworkID,
            string sNetworkName, string sNetworkDesc)
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

                Logger.Logger.Log("AddHomeNetworkToDomain", GetUpdateHomeNetworkErrMsg("Failed to extract data from DB", lDomainID, candidate, 0, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");
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
            if (!DomainDal.Insert_NewHomeNetwork(m_nGroupID, candidate.UID, lDomainID, candidate.Name, candidate.Description, candidate.IsActive, candidate.CreateDate))
            {
                // failed to insert
                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;

                // log
                Logger.Logger.Log("AddHomeNetworkToDomain", String.Concat("Failed to add to domain: ", lDomainID, " the home network: ", candidate.ToString()), "TvinciDomains");

            }
            else
            {
                res.eReason = NetworkResponseStatus.OK;
                res.bSuccess = true;
            }

            return res;
        }

        public virtual NetworkResponseObject UpdateDomainHomeNetwork(long lDomainID, string sNetworkID,
            string sNetworkName, string sNetworkDesc, bool bIsActive)
        {
            NetworkResponseObject res = null;
            HomeNetwork candidate = null;
            HomeNetwork existingNetwork = null;
            int numOfAllowedNetworks = 0;
            int numOfActiveNetworks = 0;
            int frequency = 0;
            DateTime dtLastDeactivationDate = DateTime.MinValue;
            if (!UpdateRemoveHomeNetworkCommon(lDomainID, sNetworkID, sNetworkName, sNetworkDesc, bIsActive, out res,
                ref candidate, ref existingNetwork, ref numOfAllowedNetworks, ref numOfActiveNetworks, ref frequency, ref dtLastDeactivationDate))
            {
                return res;
            }
            return UpdateDomainHomeNetworkInner(lDomainID, numOfAllowedNetworks, numOfActiveNetworks, frequency,
                candidate, existingNetwork, dtLastDeactivationDate, ref res);
        }


        public virtual NetworkResponseObject RemoveDomainHomeNetwork(long lDomainID, string sNetworkID)
        {
            NetworkResponseObject res = null;
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

            return RemoveDomainHomeNetworkInner(lDomainID, numOfAllowedNetworks, numOfActiveNetworks, frequency,
                candidate, existingNetwork, dtLastDeactivationDate, ref res);
        }

        public virtual ValidationResponseObject ValidateLimitationModule(string sUDID, int nDeviceBrandID, long lSiteGuid, long lDomainID, ValidationType eValidationType, Domain domain = null)
        {
            ValidationResponseObject res = new ValidationResponseObject();
            if (domain == null)
                domain = GetDomainForValidation(lSiteGuid, lDomainID);
            if (domain != null && domain.m_DomainStatus != DomainStatus.Error)
            {
                res.m_lDomainID = lDomainID > 0 ? lDomainID : domain.m_nDomainID;
                switch (eValidationType)
                {
                    case ValidationType.Concurrency:
                        {
                            res.m_eStatus = domain.ValidateConcurrency(sUDID, nDeviceBrandID, res.m_lDomainID);
                            break;
                        }
                    case ValidationType.Frequency:
                        {
                            res.m_eStatus = domain.ValidateFrequency(sUDID, nDeviceBrandID);
                            break;
                        }
                    default:
                        {
                            // Quantity
                            res.m_eStatus = domain.ValidateQuantity(sUDID, nDeviceBrandID);
                            break;
                        }
                }
            } // end if

            return res;
        }

        private Domain GetDomainForValidation(long lSiteGuid, long lDomainID)
        {
            Domain res = null;
            if (lDomainID > 0)
            {
                res = DomainInitializer(m_nGroupID, (int)lDomainID);
            }
            if (res == null && lSiteGuid > 0)
            {
                bool tempIsMaster = false;
                int tempOperatorID = 0;
                int domainID = DomainDal.GetDomainIDBySiteGuid(m_nGroupID, (int)lSiteGuid, ref tempOperatorID, ref tempIsMaster);
                if (domainID < 1 || domainID == (int)lDomainID)
                    return null;
                res = DomainInitializer(m_nGroupID, domainID);
            }

            return res;
        }

        #endregion

        #region Protected abstract

        protected abstract NetworkResponseObject RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res);

        protected abstract NetworkResponseObject UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res);

        /*
         * 07/04/2014
         * Initializing a domain in Eutelsat is different than in other customers.
         * 
         */
        protected abstract Domain DomainInitializer(int nGroupID, int nDomainID);

        #endregion

        #region Protected implemented

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
        private bool UpdateRemoveHomeNetworkCommon(long lDomainID, string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive,
            out NetworkResponseObject res, ref HomeNetwork nullifiedCandidate, ref HomeNetwork nullifiedExistingNetwork,
            ref int numOfAllowedNetworks, ref int numOfActiveNetworks, ref int frequency, ref DateTime dtLastDeactivationDate)
        {
            bool retVal = true;
            res = new NetworkResponseObject(false, NetworkResponseStatus.Error);
            List<HomeNetwork> lstOfHomeNetworks = null;
            DataTable dt = null;
            if (IsHomeNetworkInputInvalid(lDomainID, sNetworkID))
            {
                res.eReason = NetworkResponseStatus.InvalidInput;
                res.bSuccess = false;
                retVal = false;
                return retVal;
            }

            nullifiedCandidate = new HomeNetwork(sNetworkName, sNetworkID, sNetworkDesc, DateTime.UtcNow, bIsActive);

            if (!DomainDal.Get_ProximityDetectionDataForUpdating(m_nGroupID, lDomainID, sNetworkID, ref numOfAllowedNetworks, ref frequency, ref dtLastDeactivationDate, ref dt))
            {
                // failed to extract data from db. log and return err
                Logger.Logger.Log("UpdateRemoveHomeNetworkCommon", GetUpdateHomeNetworkErrMsg("DomainDal.Get_ProximityDetectionDataForUpdating failed.", lDomainID, nullifiedCandidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "BaseDomain");

                res.eReason = NetworkResponseStatus.Error;
                res.bSuccess = false;
                retVal = false;
                return retVal;
            }

            GetListOfExistingHomeNetworksForUpdating(dt, out lstOfHomeNetworks, out numOfActiveNetworks);

            nullifiedExistingNetwork = GetHomeNetworkFromList(lstOfHomeNetworks, nullifiedCandidate);

            if (nullifiedExistingNetwork == null)
            {
                res.eReason = NetworkResponseStatus.NetworkDoesNotExist;
                res.bSuccess = false;
                retVal = false;

                return retVal;
            }

            return retVal;
        }

        #endregion

    }
}
