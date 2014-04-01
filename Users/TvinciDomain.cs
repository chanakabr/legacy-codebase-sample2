using DAL;
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
            // Create new response
            DomainResponseObject oDomainResponseObject;

            int nDomainID = GetDomainIDByCoGuid(coGuid);

            if (nDomainID <= 0)
            {
                oDomainResponseObject = new DomainResponseObject(null, DomainResponseStatus.DomainNotExists);

                return oDomainResponseObject;
            }

            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

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

            // Create new response
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            //Init The Domain

            if ((domain != null) && (domain.m_DomainStatus != DomainStatus.OK))
            {
                eDomainResponseStatus = domain.TryRemove();
            }
            else
            {
                //Remove the domain
                eDomainResponseStatus = domain.Remove();
            }

            //Re-Init domain to return updated data

            return eDomainResponseStatus;
        }

        public override DomainResponseObject SetDomainInfo(int nDomainID, string sDomainName, int nGroupID, string sDomainDescription)
        {
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //New domain
            Domain domain = DomainFactory.GetDomain(sDomainName, sDomainDescription, nGroupID, nDomainID);

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
            
            DomainResponseObject oDomainResponseObject;
            
            //Init The Domain

            Device device = new Device(sUDID, nBrandID, m_nGroupID, sDeviceName, nDomainID);
            bool init = device.Initialize(sUDID, sDeviceName);

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

            //// If given username and user adding the device is not Master
            //if ((domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0) &&
            //    (!domain.m_masterGUIDs.Contains(nUserID) && !domain.m_masterGUIDs.Contains(nUserID)))
            //{
            //    oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.ActionUserNotMaster);
            //    return oDomainResponseObject;
            //}

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


            int nDomainDeviceID = 0;
            int nTokenDeviceID = DAL.DomainDal.GetDeviceIDByDomainActivationToken(m_nGroupID, sToken, ref nDomainDeviceID);

            if (nDeviceID != nTokenDeviceID)
            {
                resp.m_oDomain = null;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DeviceNotConfirmed;
                return resp;
            }

            string sNewGuid = Guid.NewGuid().ToString();
            int rows = DAL.DomainDal.UpdateDeviceDomainActivationToken(m_nGroupID, nDomainDeviceID, nDeviceID, sToken, sNewGuid);

            bool isActivated = rows > 0;

            Domain domain = DomainFactory.GetDomain(m_nGroupID, masterUser.m_domianID);

            bInit = bInit && (domain != null); //.Initialize(m_nGroupID, masterUser.m_domianID);

            if (!bInit)
            {
                resp.m_oDomain = domain;
                resp.m_oDomainResponseStatus = DomainResponseStatus.DomainNotInitialized;
                return resp;
            }

            int nActivationStatus = DAL.DomainDal.GetDomainDeviceActivateStatus(m_nGroupID, nDeviceID);

            resp.m_oDomain = (nActivationStatus == 1) ? domain : null;
            resp.m_oDomainResponseStatus = (nActivationStatus == 1) ? DomainResponseStatus.OK : DomainResponseStatus.DeviceNotConfirmed;

            return resp;
        }

        public override DomainResponseObject RemoveDeviceFromDomain(int nDomainID, string deviceUDID)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);
            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Remove Device from Domain
            DomainResponseStatus eDomainResponseStatus = domain.RemoveDeviceFromDomain(deviceUDID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        public override DomainResponseObject ChangeDeviceDomainStatus(int nDomainID, string deviceUDID, bool isEnable)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

            // Create new response
            DomainResponseObject oDomainResponseObject;

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
            domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

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
            DomainResponseObject oDomainResponseObject;

            if (nDomainID <= 0 || nUserGUID <= 0)
            {
                return (new DomainResponseObject(null, DomainResponseStatus.UnKnown));
            }

            //Init the Domain
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

            if (domain == null)
            {
                return (new DomainResponseObject(null, DomainResponseStatus.DomainNotInitialized));
            }


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

            //Init the Domain

            return domain;
        }

        public override List<Domain> GetDeviceDomains(string udid)
        {
            List<Domain> retVal = null;

            if (string.IsNullOrEmpty(udid))
            {
                return null;
            }

            int deviceID = Device.GetDeviceIDByUDID(udid, m_nGroupID);
            if (deviceID > 0)
            {
                retVal = Domain.GetDeviceDomains(deviceID, m_nGroupID);
            }
            return retVal;
        }

        public override DeviceResponseObject RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName)
        {
            Domain domain = DomainFactory.GetDomain(nGroupID, nDomainID);

            DeviceResponseStatus eRetVal = DeviceResponseStatus.UnKnown;
            Device device = domain.RegisterDeviceToDomainWithPIN(nGroupID, sPIN, nDomainID, sDeviceName, ref eRetVal);
            return new DeviceResponseObject(device, eRetVal);
        }

        //public override DomainResponseObject ResetDomain(int nDomainID)
        //{
        //    return ResetDomain(nDomainID, 0);

            //New domain
            //Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

            //// Create new response
            //DomainResponseObject oDomainResponseObject;

            ////Reset the domain
            //DomainResponseStatus eDomainResponseStatus = domain.ResetDomain();

            ////Re-Init domain to return updated data
            //domain.Initialize(m_nGroupID, nDomainID);
            //oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            //return oDomainResponseObject;
        //}

        public override DomainResponseObject ResetDomain(int nDomainID, int nFrequencyType = 0)
        {
            //New domain
            Domain domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

            // Create new response
            DomainResponseObject oDomainResponseObject;

            //Reset the domain
            DomainResponseStatus eDomainResponseStatus = domain.ResetDomain(nFrequencyType);

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
            domain = DomainFactory.GetDomain(m_nGroupID, nDomainID);

            // No change required, return OK 
            if (nNewMasterID == nCurrentMasterID)
            {
                oDomainResponseObject = new DomainResponseObject(domain, DomainResponseStatus.OK);
            }


            DomainResponseStatus eDomainResponseStatus = domain.ChangeDomainMaster(m_nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);
            oDomainResponseObject = new DomainResponseObject(domain, eDomainResponseStatus);

            return oDomainResponseObject;
        }

        protected override NetworkResponseObject RemoveDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks,
            int numOfActiveNetworks, int frequency, HomeNetwork candidate,
            HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res)
        {
            if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency) || !existingNetwork.IsActive)
            {
                // we can remove it if the home network either satisfies the frequency constraint or the request
                // is to remove a de-activated home network
                if (DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, existingNetwork.UID, m_nGroupID, existingNetwork.Name, existingNetwork.Description, false))
                {
                    res.eReason = NetworkResponseStatus.OK;
                    res.bSuccess = true;
                }
                else
                {
                    // failed to update db. log and return err

                    Logger.Logger.Log("RemoveDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("Failed to delete in DB. ", lDomainID, existingNetwork, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                    res.eReason = NetworkResponseStatus.Error;
                    res.bSuccess = false;
                }
            }
            else
            {
                // does not satisfy the frequency constraint. return frequency err
                res.eReason = NetworkResponseStatus.FrequencyLimitation;
                res.bSuccess = false;
            }

            return res;
        }

        protected override NetworkResponseObject UpdateDomainHomeNetworkInner(long lDomainID, int numOfAllowedNetworks, int numOfActiveNetworks,
            int frequency, HomeNetwork candidate, HomeNetwork existingNetwork, DateTime dtLastDeactivationDate, ref NetworkResponseObject res)
        {
            if (candidate.IsActive == existingNetwork.IsActive)
            {
                // no changes of network activeness. just update in DB name and desc
                if (DomainDal.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive))
                {
                    res.eReason = NetworkResponseStatus.OK;
                    res.bSuccess = true;
                }
                else
                {
                    // failed to update in db. log and return error
                    Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update. In if candidate.IsActive == existingHomeNetwork.IsActive", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                    res.eReason = NetworkResponseStatus.Error;
                    res.bSuccess = false;
                }
            }
            else
            {
                if (candidate.IsActive)
                {
                    // a request to activate the home network.
                    // check if violates the quantity constraint
                    if (IsSatisfiesQuantityConstraint(numOfAllowedNetworks, numOfActiveNetworks))
                    {
                        // we can activate the home network. update data in db.
                        if (DomainDal.Update_HomeNetworkWithoutDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, candidate.IsActive))
                        {
                            res.eReason = NetworkResponseStatus.OK;
                            res.bSuccess = true;
                        }
                        else
                        {
                            // failed to update db. log and return error
                            Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                            res.eReason = NetworkResponseStatus.Error;
                            res.bSuccess = false;
                        }

                    }
                    else
                    {
                        // we cannot activate the home network. return quantity error

                        res.eReason = NetworkResponseStatus.QuantityLimitation;
                        res.bSuccess = false;
                    }
                }
                else
                {
                    // the request is to de-activate the home network
                    // check if violates the frequency constraint
                    if (IsSatisfiesFrequencyConstraint(dtLastDeactivationDate, frequency))
                    {
                        // satsfies the frequency constraint. update data in db.
                        if (DomainDal.Update_HomeNetworkWithDeactivationDate(lDomainID, candidate.UID, m_nGroupID, candidate.Name, candidate.Description, true))
                        {
                            res.eReason = NetworkResponseStatus.OK;
                            res.bSuccess = true;
                        }
                        else
                        {
                            // failed to update data in db. log and return error
                            Logger.Logger.Log("UpdateDomainHomeNetworkInner", GetUpdateHomeNetworkErrMsg("DB failed to update", lDomainID, candidate, frequency, numOfAllowedNetworks, numOfActiveNetworks), "TvinciDomain");

                            res.eReason = NetworkResponseStatus.Error;
                            res.bSuccess = false;
                        }
                    }
                    else
                    {
                        // does not satisfy the frequency constraint. return frequency error
                        res.eReason = NetworkResponseStatus.FrequencyLimitation;
                        res.bSuccess = false;
                    }
                }
            }

            return res;
        }
    }
}
