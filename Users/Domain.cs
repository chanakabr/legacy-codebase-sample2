using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using ApiObjects.MediaMarks;
using Tvinci.Core.DAL;
using DAL;
using System.Xml.Serialization;
using NPVR;

namespace Users
{
    /// <summary>
    /// This Class Represents Domain Object
    /// </summary>
    public class Domain
    {
        #region Private Fields

        //Name of the Domain
        public string m_sName;

        //Description of the Domain
        public string m_sDescription;

        //CoGuid of the Domain
        public string m_sCoGuid;

        //Domain ID in Domains table
        public int m_nDomainID;

        //Domain group_id
        public int m_nGroupID;

        //Domain Max_Limit [Obsolete]
        public int m_nLimit;

        //Domain Device Max_Limit
        public int m_nDeviceLimit;

        //Domain User Max_Limit
        public int m_nUserLimit;

        //Domain User Max_Limit
        public int m_nConcurrentLimit;

        //Domain Status
        public int m_nStatus;

        //Domain IsActive
        public int m_nIsActive;

        //List of Users
        public List<int> m_UsersIDs;

        //List of Master Users
        public List<int> m_masterGUIDs;

        //List of Master-approval Pending Users
        public List<int> m_PendingUsersIDs;

        //List of Household Devices (STB/ConnectedTV) Users
        public List<int> m_DefaultUsersIDs;

        //List of device brands
        public List<DeviceContainer> m_deviceFamilies;

        public DomainStatus m_DomainStatus;

        public int m_frequencyFlag;

        public DateTime m_NextActionFreq;

        public DateTime m_NextUserActionFreq;

        // Domain's Operator ID
        public int m_nSSOOperatorID;

        public DomainRestriction m_DomainRestriction;

        protected int m_deviceLimitationModule;

        protected int m_totalNumOfDevices;

        protected int m_totalNumOfUsers;

        protected int m_minPeriodId;

        protected int m_minUserPeriodId;

        public List<HomeNetwork> m_homeNetworks;

        [XmlIgnore]
        protected LimitationsManager m_oLimitationsManager;

        [XmlIgnore]
        protected Dictionary<string, int> m_oUDIDToDeviceFamilyMapping;

        [XmlIgnore]
        protected Dictionary<int, DeviceContainer> m_oDeviceFamiliesMapping;

        #endregion


        #region Public Methods

        public Domain()
        {
            m_sName = string.Empty;
            m_sDescription = string.Empty;
            m_nGroupID = 0;
            m_nDeviceLimit = m_nLimit = 0;
            m_nUserLimit = 0;
            m_nDomainID = 0;
            m_nIsActive = 0;
            m_nStatus = 0;

            m_UsersIDs = new List<int>();
            m_masterGUIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();

            m_DomainStatus = DomainStatus.UnKnown;
            m_DomainRestriction = DomainRestriction.DeviceMasterRestricted;

            m_homeNetworks = new List<HomeNetwork>();

            m_oLimitationsManager = new LimitationsManager();

            m_oUDIDToDeviceFamilyMapping = new Dictionary<string, int>();

        }

        public Domain(int nDomainID)
            : this()
        {
            m_nDomainID = nDomainID;
        }

        /// <summary>
        /// Create new Domain record in DB
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nGroupID"></param>
        /// <param name="nLimit"></param>
        /// <returns>the new record ID</returns>
        public virtual Domain CreateNewDomain(string sName, string sDescription, int nGroupID, int nMasterGuID, string sCoGuid = null)
        {
            DateTime dDateTime = DateTime.UtcNow;

            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nConcurrentLimit = 0;
            int nGroupConcurrentLimit = 0;
            int nDeviceFreqLimit = 0;
            long npvrQuotaInSecs = 0;
            int nDomainLimitID = DomainDal.GetDomainDefaultLimitsID(nGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit, ref nDeviceFreqLimit, ref npvrQuotaInSecs);

            bool bInserRes = DomainDal.InsertNewDomain(sName, sDescription, nGroupID, dDateTime, nDomainLimitID, sCoGuid);

            if (!bInserRes)
            {
                m_DomainStatus = DomainStatus.Error;
                return this;
            }


            int nDomainID = -1;
            int nIsActive = 0;
            int nStatus = 0;

            Domain domainDbObj = this;
            bool resDbObj = DomainDal.GetDomainDbObject(nGroupID, dDateTime, ref sName, ref sDescription, ref nDomainID, ref nIsActive, ref nStatus, ref sCoGuid);

            m_sName = sName;
            m_sDescription = sDescription;
            m_nDomainID = nDomainID;
            m_nIsActive = nIsActive;
            m_nStatus = nStatus;
            m_sCoGuid = sCoGuid;
            m_nGroupID = nGroupID;

            m_nDeviceLimit = m_nLimit = nDeviceLimit;
            m_nUserLimit = nUserLimit;
            m_nConcurrentLimit = nConcurrentLimit;

            // initialize device limitations manager. user limitations are not managed through this object.
            InitializeLimitationsManager(nConcurrentLimit, nGroupConcurrentLimit, nDeviceLimit, nDeviceFreqLimit, Utils.FICTIVE_DATE);


            m_DomainStatus = DomainStatus.OK;

            DeviceFamiliesInitializer(nDomainLimitID, nGroupID);

            m_UsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();

            DomainResponseStatus res = AddUserToDomain(m_nGroupID, m_nDomainID, nMasterGuID, nMasterGuID, UserDomainType.Master);

            if (res == DomainResponseStatus.OK)
            {
                m_UsersIDs = new List<int>();
                m_UsersIDs.Add(nMasterGuID);
            }

            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID))
            {
                INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                if (npvr != null)
                {
                    NPVRUserActionResponse resp = npvr.CreateAccount(new NPVRParamsObj() { EntityID = m_nDomainID.ToString(), Quota = npvrQuotaInSecs });
                    if (resp != null)
                    {
                        if (resp.isOK)
                        {
                            m_DomainStatus = DomainStatus.OK;
                        }
                        else
                        {
                            m_DomainStatus = DomainStatus.Error;
                            Logger.Logger.Log("Error", string.Format("CreateNewDomain. NPVR Provider returned null from Factory. G ID: {0} , D ID: {1} , NPVR Err Msg: {2}", m_nGroupID, m_nDomainID, resp.msg), "Domain");
                        }
                    }
                    else
                    {
                        m_DomainStatus = DomainStatus.Error;
                        Logger.Logger.Log("Error", string.Format("CreateNewDomain. NPVR Provider CreateAccount response is null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID), "Domain");
                    }

                }
                else
                {
                    Logger.Logger.Log("Error", string.Format("CreateNewDomain. NPVR Provider returned null from Factory. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID), "Domain");
                }

            }

            return this;
        }

        protected internal void InitializeLimitationsManager(int nDomainLevelConcurrentLimit, int nGroupLevelConcurrentLimit, int nDeviceQuantityLimit, int nDeviceFrequencyLimit, DateTime dtLastActionDate)
        {
            if (m_oLimitationsManager == null)
                m_oLimitationsManager = new LimitationsManager();
            m_oLimitationsManager.SetConcurrency(nDomainLevelConcurrentLimit, nGroupLevelConcurrentLimit);
            m_oLimitationsManager.Frequency = nDeviceFrequencyLimit;
            m_oLimitationsManager.Quantity = nDeviceQuantityLimit;
            if (dtLastActionDate == null || dtLastActionDate.Equals(Utils.FICTIVE_DATE) || dtLastActionDate.Equals(DateTime.MinValue) || nDeviceFrequencyLimit == 0)
                m_oLimitationsManager.NextActionFreqDate = DateTime.MinValue;
            else
                m_oLimitationsManager.NextActionFreqDate = Utils.GetEndDateTime(dtLastActionDate, nDeviceFrequencyLimit);
        }

        public DomainResponseStatus Remove()
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            int isActive = 2;   // Inactive
            int status = 2;     // Removed

            int statusRes = DomainDal.SetDomainStatus(m_nGroupID, m_nDomainID, isActive, status);

            //return statusRes == 2 ? DomainResponseStatus.OK : DomainResponseStatus.Error;
            if (IsDomainRemovedSuccessfully(statusRes))
            {
                if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID))
                {
                    INPVRProvider npvr = NPVRProviderFactory.Instance().GetProvider(m_nGroupID);
                    if (npvr != null)
                    {
                        NPVRUserActionResponse response = npvr.DeleteAccount(new NPVRParamsObj() { EntityID = m_nDomainID.ToString() });

                        if (response != null)
                        {
                            if (response.isOK)
                            {
                                res = DomainResponseStatus.OK;
                            }
                            else
                            {
                                res = DomainResponseStatus.Error;
                                Logger.Logger.Log("Error", string.Format("Remove. NPVR DeleteAccount response status is not ok. G ID: {0} , D ID: {1} , Err Msg: {2}", m_nGroupID, m_nDomainID, response.msg), "Domain");
                            }
                        }
                        else
                        {
                            res = DomainResponseStatus.Error;
                            Logger.Logger.Log("Error", string.Format("Remove. DeleteAccount returned response null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID), "Domain");
                        }
                    }
                    else
                    {
                        res = DomainResponseStatus.Error;
                        Logger.Logger.Log("Error", string.Format("Remove. NPVR Provider is null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID), "Domain");
                    }
                }
            }
            else
            {
                res = DomainResponseStatus.Error;
            }

            return res;
        }

        private bool IsDomainRemovedSuccessfully(int statusRes)
        {
            return statusRes == 2;
        }

        /// <summary>
        /// Init new Domain Object according to GroupId and DomainId
        /// </summary>
        /// <param name="sName">Name of the Domain</param>
        /// <param name="sDescription">Description of the Domain</param>
        /// <param name="nGroupID">the GroupId</param>
        /// <param name="nDomainId">the DomainId</param>
        public bool Initialize(string sName, string sDescription, int nGroupID, int nDomainID)
        {
            m_nGroupID = nGroupID;
            m_nDomainID = nDomainID;

            if (!GetDomainSettings(nDomainID, nGroupID))
            {
                m_DomainStatus = DomainStatus.Error;
                return false;
            }
            
            if (!string.IsNullOrEmpty(sName))
            {
                m_sName = sName;
            }
            if (!string.IsNullOrEmpty(sDescription))
            {
                m_sDescription = sDescription;
            }
            
            DomainResponseStatus dStatus = GetUserList(nDomainID, nGroupID);   // OK or NoUsersInDomain

            int numOfDevices = GetDeviceList();

            m_homeNetworks = Utils.GetHomeNetworksOfDomain(nDomainID, nGroupID);

            m_DomainStatus = dStatus == DomainResponseStatus.OK ? DomainStatus.OK : DomainStatus.Error;

            return m_DomainStatus == DomainStatus.OK;
        }

        /// <summary>
        /// Init New Domain Object according to GroupId and DomainId
        /// </summary>
        /// <param name="nGroupID">The GroupId</param>
        /// <param name="nDomainId">The DomainId</param>
        public bool Initialize(int nGroupID, int nDomainID)
        {
            return Initialize(string.Empty, string.Empty, nGroupID, nDomainID);
        }

        /// <summary>
        /// Init New Domain Object according to GroupId, SubGroupID (sub-account) and DomainId
        /// </summary>
        /// <param name="nGroupID">The GroupId</param>
        /// <param name="nDomainId">The DomainId</param>
        public bool Initialize(int nGroupID, int nDomainID, int nSubGroupID)
        {
            if (nSubGroupID <= 0)
            {
                nSubGroupID = nGroupID;
            }

            m_nGroupID = nGroupID;
            m_nDomainID = nDomainID;

            if (!GetDomainSettings(nDomainID, nGroupID))
            {
                m_DomainStatus = DomainStatus.Error;
                return false;
            }

            // Users are stored on parent (nGroupID) accound 
            DomainResponseStatus domainRes = GetUserList(nDomainID, nGroupID);

            // Device families (limits) are per sub-account
            //m_deviceFamilies = InitializeDeviceFamilies(m_deviceLimitationModule, nSubGroupID);
            DeviceFamiliesInitializer(m_deviceLimitationModule, nSubGroupID);
            GetDeviceList(false);

            m_homeNetworks = Utils.GetHomeNetworksOfDomain(nDomainID, nGroupID);

            m_DomainStatus = DomainStatus.OK;

            return true;

        }

        /// <summary>
        /// Remove User from the Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        public DomainResponseStatus RemoveUserFromDomain(int nGroupID, int nDomainID, int nUserID)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;

            // if next allowed action is in future, return LimitationPeriod status
            if (m_NextUserActionFreq >= DateTime.UtcNow)
            {
                eRetVal = DomainResponseStatus.LimitationPeriod;
                return eRetVal;
            }

            int nUserDomainID = DAL.DomainDal.DoesUserExistInDomain(nGroupID, nDomainID, nUserID, false);
            if (nUserDomainID <= 0)
            {
                eRetVal = DomainResponseStatus.UserNotExistsInDomain;
                return eRetVal;
            }


            //Check if UserID is valid
            if ((!User.IsUserValid(nGroupID, nUserID)))
            {
                eRetVal = DomainResponseStatus.InvalidUser;
                return eRetVal;
            }

            Dictionary<int, int> dTypedUserIDs = DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);

            // User validations
            if (dTypedUserIDs == null || dTypedUserIDs.Count == 0)
            {
                // Try to remove anyway (maybe user is inactive or pending)

                eRetVal = DomainResponseStatus.NoUsersInDomain;
                return eRetVal;

            }

            // Check master and default users
            KeyValuePair<int, int> masterUserKV = dTypedUserIDs.FirstOrDefault(ut => ut.Value == (int)UserDomainType.Master);
            KeyValuePair<int, int> defaultUserKV = dTypedUserIDs.FirstOrDefault(ut => ut.Value == (int)UserDomainType.Household);

            if (masterUserKV.Equals(default(KeyValuePair<int, int>)) || masterUserKV.Key <= 0 ||
                (nUserID == masterUserKV.Key || nUserID == defaultUserKV.Key))
            {
                eRetVal = DomainResponseStatus.UserNotAllowed;
                return eRetVal;

            }


            try
            {
                int nStatus = 2;
                int nIsActive = 2;
                int rowsAffected = DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, nUserDomainID, nStatus, nIsActive);

                if (rowsAffected > 0)
                {
                    SetDomainFlag(nDomainID, 1, false);
                    eRetVal = RemoveUserFromList(nUserID);
                }
                else
                {
                    eRetVal = DomainResponseStatus.Error;
                }
            }
            catch
            {
                eRetVal = DomainResponseStatus.Error;
            }

            return eRetVal;

        }

        public DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, ref Device device)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;
            int isDevActive = 0;
            int status = 0;
            int tempDeviceID = 0;
            int nDbDomainDeviceID = 0;
            int domainID = DomainDal.GetDeviceDomainData(nGroupID, sUDID, ref tempDeviceID, ref isDevActive, ref status, ref nDbDomainDeviceID);

            //Very Patchy - change the group check to be configurable!!
            if (domainID != 0 && m_nGroupID != 147)
            {
                if (status == 3 && isDevActive == 3)    // Pending master approval
                {
                    bool updated = DomainDal.UpdateDomainsDevicesStatus(nDbDomainDeviceID, 1, 1);
                    if (updated)
                    {
                        eRetVal = DomainResponseStatus.OK;

                        device.m_domainID = nDomainID;
                        device.m_state = DeviceState.Activated;
                        int deviceID = device.Save(1, 1, tempDeviceID);
                        GetDeviceList();
                        return eRetVal;
                    }
                }

                eRetVal = DomainResponseStatus.DeviceAlreadyExists;
                return eRetVal;
            }

            DeviceContainer container = GetDeviceContainer(device.m_deviceFamilyID);

            //Check if exceeded limit for the device type
            DomainResponseStatus responseStatus = ValidateQuantity(sUDID, brandID, container, device);

            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed || responseStatus == DomainResponseStatus.DeviceAlreadyExists)
            {
                eRetVal = responseStatus;
                return eRetVal;
            }

            int isActive = 0;
            int nDeviceID = 0;
            // Get row id from domains_devices
            int nDomainsDevicesID = DomainDal.DoesDeviceExistInDomain(m_nDomainID, m_nGroupID, sUDID, ref isActive, ref nDeviceID);

            //New Device Domain Connection
            if (nDomainsDevicesID == 0)
            {
                // Get row id from devices table (not udid)
                device.m_domainID = nDomainID;
                int deviceID = device.Save(1);

                int domainDeviceRecordID = DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 1, 1);


                if (domainDeviceRecordID > 0)
                {
                    device.m_state = DeviceState.Activated;
                    container.AddDeviceInstance(device);

                    m_totalNumOfDevices++;

                    eRetVal = DomainResponseStatus.OK;
                }
                else
                {
                    eRetVal = DomainResponseStatus.Error;
                }
            }
            else
            {
                //Update device status if exists
                if (isActive != 1)               // should be status != 1 ?
                {
                    // Set is_active = 1 and status = 1
                    bool updated = DomainDal.UpdateDomainsDevicesStatus(nDomainsDevicesID, 1, 1);

                    if (updated)
                    {
                        eRetVal = DomainResponseStatus.OK;
                        device.m_domainID = nDomainID;
                        int deviceID = device.Save(1);
                    }
                }
                else
                {
                    eRetVal = DomainResponseStatus.DeviceAlreadyExists;
                }
            }

            GetDeviceList();

            return eRetVal;
        }

        public DomainResponseStatus RemoveDeviceFromDomain(string sUDID)
        {
            DomainResponseStatus bRes = DomainResponseStatus.UnKnown;

            // if next allowed action is in future, return LimitationPeriod status
            // Since frequency is defined at domain level, and not in device family level, we can pass a fictive (0)
            // device brand id to ValidateFrequency method
            if (ValidateFrequency(sUDID, 0) == DomainResponseStatus.LimitationPeriod)
            {
                bRes = DomainResponseStatus.LimitationPeriod;
                return bRes;
            }


            int isActive = 0;
            int nDeviceID = 0;

            int nDomainDeviceID = DomainDal.DoesDeviceExistInDomain(m_nDomainID, m_nGroupID, sUDID, ref isActive, ref nDeviceID);   //DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            if (nDomainDeviceID > 0)
            {
                // set is_Active = 2; status = 2
                bool bUpdate = DomainDal.UpdateDomainsDevicesStatus(nDomainDeviceID, 2, 2);

                if (!bUpdate)
                {
                    Logger.Logger.Log("RemoveDeviceFromDomain", String.Concat("Failed to update domains_device table. Status=2, Is_Active=2, ID in domains_devices: ", nDomainDeviceID, " UDID: ", sUDID), "Domain");
                    return DomainResponseStatus.Error;
                }


                int nDomainsDevicesCount = DomainDal.GetDomainsDevicesCount(m_nGroupID, nDeviceID);
                bool bDeleteDevice = nDomainsDevicesCount == 0;   // No other domains attached to this device

                if (bDeleteDevice)
                {
                    // set is_Active = 2; status = 2
                    bUpdate = DomainDal.UpdateDeviceStatus(nDeviceID, 2, 2);

                    if (!bUpdate)
                    {
                        return DomainResponseStatus.Error;
                    }
                }

                DeviceContainer container = null;
                Device device = GetDomainDevice(sUDID, ref container);
                if (container != null && device != null)
                {
                    if (container.RemoveDeviceInstance(sUDID))
                    {
                        bRes = DomainResponseStatus.OK;

                        if (m_minPeriodId != 0)
                        {
                            SetDomainFlag(m_nDomainID, 1);
                        }
                    }
                    else
                    {
                        bRes = DomainResponseStatus.Error;
                    }
                }
                else
                {
                    bRes = DomainResponseStatus.DeviceNotInDomain;
                }
            }
            else
            {
                bRes = DomainResponseStatus.DeviceNotInDomain;
            }

            return bRes;
        }



        /// <summary>
        /// Activate/Deactivate device in Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="sUDID"></param>
        /// <param name="deviceName"></param>
        /// <param name="brandID"></param>
        public DomainResponseStatus ChangeDeviceDomainStatus(int nGroupID, int nDomainID, string sUDID, bool bIsEnable)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            /*
             * 1. Since frequency is defined at domain level and not in device family level we can pass a fictive (0)
             * device brand id to ValidateFrequency method
             */
            if (!bIsEnable && ValidateFrequency(sUDID, 0) == DomainResponseStatus.LimitationPeriod)
            {
                eDomainResponseStatus = DomainResponseStatus.LimitationPeriod;
                return eDomainResponseStatus;
            }


            DeviceContainer container = null;
            Device device = GetDomainDevice(sUDID, ref container);

            int enableInt = 1;
            DeviceState eNewDeviceState = DeviceState.Activated;
            if (!bIsEnable)
            {
                enableInt = 0;
                eNewDeviceState = DeviceState.UnActivated;
            }
            else
            {
                eDomainResponseStatus = ValidateQuantity(sUDID, device.m_deviceBrandID, container, device);
                eNewDeviceState = DeviceState.Activated;
            }

            int isActive = 0;
            int nDeviceID = 0;
            int nDomainDeviceID = DomainDal.DoesDeviceExistInDomain(m_nDomainID, nGroupID, sUDID, ref isActive, ref nDeviceID);     //DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            if (nDomainDeviceID > 0 && eDomainResponseStatus != DomainResponseStatus.ExceededLimit)
            {
                bool resUpdated = DomainDal.UpdateDomainsDevicesIsActive(nDomainDeviceID, enableInt, bIsEnable);

                if (resUpdated)
                {
                    container.ChangeDeviceInstanceState(sUDID, eNewDeviceState);
                    if (eNewDeviceState == DeviceState.UnActivated)
                    {
                        SetDomainFlag(m_nDomainID, 1);
                    }

                    eDomainResponseStatus = DomainResponseStatus.OK;
                }
                else
                {
                    Logger.Logger.Log("ChangeDeviceDomainStatus", String.Concat("Failed to update is_active in domains_devices. domains devices id: ", nDomainDeviceID, " enableInt: ", enableInt, " UDID: ", sUDID), "Domain");
                    eDomainResponseStatus = DomainResponseStatus.Error;
                }
            }
            else
            {
                if (nDomainDeviceID == 0)
                {
                    eDomainResponseStatus = DomainResponseStatus.DeviceNotInDomain;
                }
                else
                {
                    eDomainResponseStatus = DomainResponseStatus.ExceededLimit;
                }
            }

            return eDomainResponseStatus;
        }

        /// <summary>
        /// Add User to the Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        /// <param name="nMasterUserGuid"></param>
        public DomainResponseStatus AddUserToDomain(int nGroupID, int nDomainID, int nUserID, int nMasterUserGuid, UserDomainType userType)  //bool bIsMaster)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            int numOfUsers = m_UsersIDs.Count;

            Dictionary<int, int> dbTypedUserIDs = DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);


            // If domain has no users, insert new Master user
            //
            int status = 1;
            int isActive = 1;

            if ((dbTypedUserIDs == null || dbTypedUserIDs.Count == 0) &&
                (nUserID == nMasterUserGuid))
            {
                int inserted = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int)userType, status, isActive, nMasterUserGuid);

                if (inserted > 0)
                {
                    m_UsersIDs.Add(nUserID);
                    m_masterGUIDs.Add(nUserID);

                    m_totalNumOfUsers = m_UsersIDs.Count - m_DefaultUsersIDs.Count;
                    eDomainResponseStatus = DomainResponseStatus.OK;

                    return DomainResponseStatus.OK;
                }

                return DomainResponseStatus.Error;
            }

            // No users, but 
            if ((dbTypedUserIDs == null || dbTypedUserIDs.Count == 0) && (nUserID != nMasterUserGuid))
            {
                return DomainResponseStatus.ActionUserNotMaster;
            }

            // Domain has users, but action user is NOT Master
            List<int> masterUserIDs = dbTypedUserIDs.Where(ut => ut.Value == (int)UserDomainType.Master).Select(ut => ut.Key).ToList();

            if (masterUserIDs != null && masterUserIDs.Count > 0 && !masterUserIDs.Contains(nMasterUserGuid))
            {
                return DomainResponseStatus.ActionUserNotMaster;
            }


            // Check if user already exists in domain (active or pending)
            int nUserDomainID = DomainDal.DoesUserExistInDomain(nGroupID, nDomainID, nUserID, false);

            if (nUserDomainID > 0)  // If user exists, update its status to active
            {
                int rowsAffected = DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, nUserDomainID);

                if (rowsAffected < 1)
                {
                    eDomainResponseStatus = DomainResponseStatus.Error;
                }
                else
                {
                    eDomainResponseStatus = GetUserList(nDomainID, nGroupID);
                }

                return eDomainResponseStatus;
            }


            // Process New User

            //Check if exceeded limit for users
            if (userType != UserDomainType.Household)
            {
                DomainResponseStatus responseStatus = CheckUserLimit(nDomainID, nUserID);
                if (responseStatus == DomainResponseStatus.ExceededUserLimit || responseStatus == DomainResponseStatus.UserNotAllowed)
                {
                    eDomainResponseStatus = responseStatus;
                    return eDomainResponseStatus;
                }
            }

            int inserted1 = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int)userType, status, isActive, nMasterUserGuid);

            if (inserted1 > 0)
            {
                switch (userType)
                {
                    case UserDomainType.Regular:
                        m_UsersIDs.Add(nUserID);
                        break;
                    case UserDomainType.Master:
                        m_UsersIDs.Add(nUserID);
                        m_masterGUIDs.Add(nUserID);
                        break;
                    case UserDomainType.Household:
                        m_DefaultUsersIDs.Add(nUserID);
                        break;
                    default:
                        break;
                }

                m_totalNumOfUsers = m_UsersIDs.Count;
                eDomainResponseStatus = DomainResponseStatus.OK;
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.Error;
            }

            return eDomainResponseStatus;
        }

        public DomainResponseStatus AddUserToDomain(int nGroupID, int nDomainID, int nUserID, bool bIsMaster)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            int numOfUsers = m_UsersIDs.Count;

            List<int> lDomainIDs = UsersDal.GetUserDomainIDs(nGroupID, nUserID);

            if (lDomainIDs != null && lDomainIDs.Count > 0)
            {
                if (lDomainIDs.Count == 1 && lDomainIDs[0] == nDomainID)
                {
                    int rowsAffected = DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, null, 0, 0);
                    lDomainIDs = UsersDal.GetUserDomainIDs(nGroupID, nUserID);
                }
                else if (!lDomainIDs.Contains<int>(nDomainID) || lDomainIDs.Count > 1)
                {
                    // The user belongs to other domain(s), maybe pending activation
                    return DomainResponseStatus.UserExistsInOtherDomains;
                }
            }

            int inserted = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, Convert.ToInt32(bIsMaster), 1, 1, 43);

            if (inserted > 0)
            {
                m_UsersIDs.Add(nUserID);
                eDomainResponseStatus = DomainResponseStatus.OK;
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.Error;
            }

            return eDomainResponseStatus;

        }

        public DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserID, string sMasterUsername)
        {
            User masterUser = new User();
            int nMasterID = masterUser.InitializeByUsername(sMasterUsername, nGroupID);

            if (nMasterID <= 0)
            {
                return new DomainResponseObject(this, DomainResponseStatus.UserNotAllowed);
            }

            // Let's try to find the domain of this master
            int nDomainID = masterUser.m_domianID;

            // No domain - no luck :(
            if (nDomainID <= 0)
            {
                return new DomainResponseObject(this, DomainResponseStatus.UnKnown);
            }

            // Found domain, but username is not a master 
            if (!masterUser.m_isDomainMaster)
            {
                return new DomainResponseObject(this, DomainResponseStatus.ActionUserNotMaster);
            }

            // Domain found, let's initialize it (users, device families, ...)
            bool init = Initialize(nGroupID, nDomainID);

            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(nDomainID, nGroupID);

                if (m_UsersIDs == null || m_UsersIDs.Count == 0)
                {
                    return new DomainResponseObject(this, DomainResponseStatus.NoUsersInDomain);
                }
            }

            if (m_masterGUIDs == null || m_masterGUIDs.Count == 0)
            {
                return new DomainResponseObject(this, DomainResponseStatus.Error);
            }


            // Now let's see which domains the user belons to
            List<int> lDomainIDs = UsersDal.GetUserDomainIDs(nGroupID, nUserID);

            if (lDomainIDs != null && lDomainIDs.Count > 0)
            {
                if (lDomainIDs.Count == 1 && lDomainIDs[0] == nDomainID)
                {
                    int rowsAffected = DAL.DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, null, 0, 0);
                    lDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nUserID);
                }
                else if ((!lDomainIDs.Contains<int>(nDomainID)) || (lDomainIDs.Count > 1))
                {
                    // The user belongs to other domain(s), maybe pending activation
                    return new DomainResponseObject(this, DomainResponseStatus.UserExistsInOtherDomains);
                }
            }


            // Check if user is somehow still in the domain
            if (lDomainIDs != null && lDomainIDs.Count > 0)
            {
                return (new DomainResponseObject(this, DomainResponseStatus.Error));
            }

            //Check if exceeded limit for users
            DomainResponseStatus responseStatus = CheckUserLimit(nDomainID, nUserID);
            if (responseStatus == DomainResponseStatus.ExceededUserLimit || responseStatus == DomainResponseStatus.UserNotAllowed)
            {
                return new DomainResponseObject(this, responseStatus);
            }

            // Let's fetch the new user's Username and First name (for e-mail)
            string sNewUsername = string.Empty;
            string sNewFirstName = string.Empty;
            string sNewEmail = string.Empty;

            using (DataTable dtUserBasicData = UsersDal.GetUserBasicData(nUserID))
            {
                if (dtUserBasicData != null)
                {
                    int nCount = dtUserBasicData.DefaultView.Count;
                    if (nCount > 0)
                    {
                        sNewUsername = dtUserBasicData.DefaultView[0].Row["USERNAME"].ToString();
                        sNewFirstName = dtUserBasicData.DefaultView[0].Row["FIRST_NAME"].ToString();
                        sNewEmail = dtUserBasicData.DefaultView[0].Row["EMAIL_ADD"].ToString();
                    }
                }
            }

            // Add the new user to the domain
            int isMaster = 0;
            int status = 3;     // Pending
            int isActive = 0;
            string sActivationToken = Guid.NewGuid().ToString();

            int inserted = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, isMaster, status, isActive, nMasterID, sActivationToken);

            if (inserted > 0)
            {
                m_UsersIDs.Add(nUserID);
                m_PendingUsersIDs.Add(nUserID);

                // Update user's email to its Master's if empty
                //
                if (string.IsNullOrEmpty(sNewEmail))
                {
                    bool saved = false;

                    UserBasicData uBasic = new UserBasicData();
                    bool initBasic = uBasic.Initialize(nUserID, nGroupID);
                    uBasic.m_sEmail = masterUser.m_oBasicData.m_sEmail;
                    saved = uBasic.Save(nUserID);

                    if (!saved)
                    {
                        return (new DomainResponseObject(this, DomainResponseStatus.RequestFailed));
                    }
                }

                // Now we can send the activation mail to the Master
                if (!string.IsNullOrEmpty(sActivationToken))
                {
                    TvinciAPI.AddUserMailRequest sMailRequest = MailFactory.GetAddUserMailRequest(nGroupID,
                                                                                                masterUser.m_oBasicData.m_sFirstName,
                                                                                                masterUser.m_oBasicData.m_sUserName,
                                                                                                masterUser.m_oBasicData.m_sEmail,
                                                                                                sNewUsername,
                                                                                                sNewFirstName,
                                                                                                sActivationToken);

                    if (sMailRequest != null)
                    {
                        bool sendingMailResult = Utils.SendMail(nGroupID, sMailRequest);
                        return (new DomainResponseObject(this, DomainResponseStatus.RequestSent));
                    }

                    return (new DomainResponseObject(this, DomainResponseStatus.RequestFailed));
                }
            }

            return (new DomainResponseObject(this, DomainResponseStatus.UnKnown));
        }

        /// <summary>
        /// Update the Domain name, description and max_limit
        /// according to GroupID and DomainID
        /// </summary>
        public bool Update()
        {
            bool dbRes = DomainDal.UpdateDomain(m_sName, m_sDescription, m_nDomainID, m_nGroupID, (int)m_DomainRestriction);

            if (!dbRes)
            {
                m_DomainStatus = DomainStatus.Error;
            }

            return dbRes;
        }

        public static List<Domain> GetDeviceDomains(int deviceID, int groupID)
        {
            List<Domain> retVal = null;
            List<int> dbDomains = DAL.DomainDal.GetDeviceDomains(deviceID, groupID);

            if (dbDomains != null && dbDomains.Count > 0)
            {
                retVal = new List<Domain>(dbDomains.Count);

                for (int i = 0; i < dbDomains.Count; i++)
                {
                    Domain domain = new Domain();
                    domain.Initialize(groupID, dbDomains[i]);

                    retVal.Add(domain);
                }
            }

            return retVal;
        }

        /// <summary>
        /// update m_Users list
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        public static List<string> GetFullUserList(int nDomainID, int nGroupID)
        {
            return DomainDal.Get_FullUserListOfDomain(nGroupID, nDomainID);
        }

        public Device RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName, ref DeviceResponseStatus eRetVal)
        {
            string sUDID = string.Empty;
            int nBrandID = 0;

            bool res = DomainDal.GetDeviceIdAndBrandByPin(sPIN, nGroupID, ref sUDID, ref nBrandID);

            Device device = null;

            // If devices to register was found in devices table, register it to domain
            if (!string.IsNullOrEmpty(sUDID))
            {
                //Add new Debice to Domain
                device = new Device(sUDID, nBrandID, nGroupID);
                device.Initialize(sUDID);
                device.m_deviceName = sDeviceName;
                DomainResponseStatus eStatus = AddDeviceToDomain(nGroupID, nDomainID, sUDID, sDeviceName, nBrandID, ref device);

                if (eStatus == DomainResponseStatus.OK)
                {
                    device.m_state = DeviceState.Activated;
                    eRetVal = DeviceResponseStatus.OK;
                }
                else if (eStatus == DomainResponseStatus.DeviceAlreadyExists)
                {
                    eRetVal = DeviceResponseStatus.DuplicatePin;
                }
                else
                {
                    eRetVal = DeviceResponseStatus.DuplicatePin;
                }
            }
            else
            {
                eRetVal = DeviceResponseStatus.DeviceNotExists;

                device = new Device(m_nGroupID);
                device.m_state = DeviceState.NotExists;
            }

            return device;
        }

        public DomainResponseStatus ResetDomain(int nFreqencyType = 0)
        {
            bool res = DomainDal.ResetDomain(m_nDomainID, m_nGroupID, nFreqencyType);

            if (!res)
            {
                return DomainResponseStatus.Error;
            }

            return DomainResponseStatus.OK;
        }

        /// <summary>
        /// Domain has to be initialized before entering this method
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="sMasterUsername"></param>
        /// <param name="sDeviceUdid"></param>
        /// <param name="deviceName"></param>
        /// <param name="nBrandID"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public DomainResponseStatus SubmitAddDeviceToDomainRequest(int nGroupID, string sDeviceUdid, string sDeviceName, ref Device device)
        {
            #region Validations

            if (this.m_nDomainID <= 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }

            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(this.m_nDomainID, nGroupID);

                if (m_UsersIDs == null || m_UsersIDs.Count == 0)
                {
                    return DomainResponseStatus.NoUsersInDomain;
                }
            }

            if (m_masterGUIDs == null || m_masterGUIDs.Count == 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }


            int isActive = 0;
            int status = 0;
            int deviceID = 0;
            int nDeviceDomainRecordID = 0;

            // Now let's see which domain the device belongs to
            int nDeviceDomainID = DomainDal.GetDeviceDomainData(nGroupID, sDeviceUdid, ref deviceID, ref isActive, ref status, ref nDeviceDomainRecordID);

            if (isActive == 1)
            {
                if (nDeviceDomainID > 0 && nDeviceDomainID != this.m_nDomainID)
                {
                    return DomainResponseStatus.DeviceExistsInOtherDomains;
                }

                if (nDeviceDomainID == this.m_nDomainID)
                {
                    return DomainResponseStatus.DeviceAlreadyExists;
                }
            }

            #endregion

            if (isActive == 3)  // device pending activation in this or other domain; reset this association
            {
                int rowsAffected = DomainDal.SetDeviceStatusInDomain(deviceID, this.m_nDomainID, nGroupID, nDeviceDomainRecordID, 2, 2);
            }

            //Check if exceeded limit for users
            DeviceContainer container = GetDeviceContainer(device.m_deviceFamilyID);

            //Check if exceeded limit for the device type
            DomainResponseStatus responseStatus = ValidateQuantity(sDeviceUdid, device.m_deviceBrandID, container, device);
            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed || responseStatus == DomainResponseStatus.DeviceAlreadyExists)
            {
                return responseStatus;
            }

            // Get row id from devices table (not udid)
            device.m_domainID = this.m_nDomainID;
            deviceID = device.Save(0, 3);

            string sActivationToken = Guid.NewGuid().ToString();
            nDeviceDomainRecordID = DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 3, 3, sActivationToken);

            if (nDeviceDomainRecordID > 0)
            {
                device.m_state = DeviceState.Pending;
                container.AddDeviceInstance(device);

                m_totalNumOfDevices++;
                //m_deviceFamilies.Add(container); - no need. it is reference type

                User masterUser = new User(nGroupID, m_masterGUIDs[0]);

                TvinciAPI.AddDeviceMailRequest sMailRequest = null;

                if (masterUser != null)
                {
                    sMailRequest = MailFactory.GetAddDeviceMailRequest(nGroupID,
                                                                    masterUser.m_oBasicData.m_sFirstName,
                                                                    masterUser.m_oBasicData.m_sUserName,
                                                                    masterUser.m_oBasicData.m_sEmail,
                                                                    sDeviceUdid,
                                                                    device.m_deviceName,
                                                                    sActivationToken);
                }

                if (sMailRequest != null)
                {
                    bool sendingMailResult = Utils.SendMail(nGroupID, sMailRequest);

                    return sendingMailResult ? DomainResponseStatus.RequestSent : DomainResponseStatus.RequestFailed;
                }
            }

            return DomainResponseStatus.Error;
        }

        public DomainResponseStatus ChangeDomainMaster(int nGroupID, int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            #region Validations

            if (m_nDomainID <= 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }

            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(this.m_nDomainID, nGroupID);

                if (m_UsersIDs == null || m_UsersIDs.Count == 0)
                {
                    return DomainResponseStatus.NoUsersInDomain;
                }
            }

            if (m_masterGUIDs == null || m_masterGUIDs.Count == 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }

            User curMasterUser = new User(nGroupID, nCurrentMasterID);

            // Found domain, but username is not a master 
            if (!curMasterUser.m_isDomainMaster)
            {
                return DomainResponseStatus.ActionUserNotMaster;
            }

            // Now let's see which domains the user belons to
            List<int> lCurMasterDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nCurrentMasterID);
            List<int> lNewMasterDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nNewMasterID);

            if (((lCurMasterDomainIDs != null && lCurMasterDomainIDs.Count > 0) && (!lCurMasterDomainIDs.Contains(nDomainID))) ||
                ((lNewMasterDomainIDs != null && lNewMasterDomainIDs.Count > 0) && (!lNewMasterDomainIDs.Contains(nDomainID))))
            {
                return DomainResponseStatus.UserExistsInOtherDomains;
            }

            #endregion

            int rowsAffected = DomainDal.SwitchDomainMaster(nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);

            return rowsAffected > 0 ? DomainResponseStatus.OK : DomainResponseStatus.Error;
        }

        #endregion


        #region Protected Methods

        protected internal void DeviceFamiliesInitializer(int nDomainLimitationModuleID, int nGroupID)
        {
            Dictionary<int, int> concurrencyOverride = new Dictionary<int, int>();
            Dictionary<int, int> quantityOverride = new Dictionary<int, int>();
            List<string[]> dbDeviceFamilies = DomainDal.Get_DeviceFamiliesLimits(nGroupID, nDomainLimitationModuleID, ref concurrencyOverride, ref quantityOverride);

            m_deviceFamilies = new List<DeviceContainer>(dbDeviceFamilies.Count);
            m_oDeviceFamiliesMapping = new Dictionary<int, DeviceContainer>(dbDeviceFamilies.Count);

            for (int i = 0; i < dbDeviceFamilies.Count; i++)
            {
                string[] currentDeviceFamily = dbDeviceFamilies[i];

                int nFamilyID = string.IsNullOrEmpty(currentDeviceFamily[0]) ? 0 : Int32.Parse(currentDeviceFamily[0]);
                string sFamilyName = currentDeviceFamily[1];
                int nOverrideQuantityLimit = -1;
                int nOverrideConcurrencyLimit = -1;
                if (concurrencyOverride != null && concurrencyOverride.Count > 0 && concurrencyOverride.ContainsKey(nFamilyID))
                {
                    nOverrideConcurrencyLimit = concurrencyOverride[nFamilyID];
                }
                if (quantityOverride != null && quantityOverride.Count > 0 && quantityOverride.ContainsKey(nFamilyID))
                {
                    nOverrideQuantityLimit = quantityOverride[nFamilyID];
                }
                DeviceContainer dc = new DeviceContainer(nFamilyID, sFamilyName, nOverrideQuantityLimit > -1 ? nOverrideQuantityLimit : m_oLimitationsManager.Quantity, nOverrideConcurrencyLimit > -1 ? nOverrideConcurrencyLimit : m_oLimitationsManager.Concurrency);
                if (!m_oDeviceFamiliesMapping.ContainsKey(nFamilyID))
                {
                    m_deviceFamilies.Add(dc);
                    m_oDeviceFamiliesMapping.Add(nFamilyID, dc);
                }
                else
                {
                    Logger.Logger.Log("DeviceFamiliesInitializer Error", String.Concat("DeviceContainer duplicate: ", dc.ToString()) , "Domain");
                }
            }
        }

        protected int GetDeviceList()
        {
            return GetDeviceList(true);
        }

        /// <summary>
        /// update Device Family list
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        protected int GetDeviceList(bool isInitializeFamilies)
        {
            if (isInitializeFamilies)
            {
                DeviceFamiliesInitializer(m_deviceLimitationModule, m_nGroupID);
            }

            DataTable dt = DomainDal.Get_DomainDevices(m_nGroupID, m_nDomainID);
            InitializeDomainDevicesData(dt);

            return m_totalNumOfDevices;
        }

        private void InitializeDomainDevicesData(DataTable dt)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string sUDID = string.Empty;
                int nDeviceBrandID = 0;
                string sDeviceName = string.Empty;
                int nDeviceFamilyID = 0;
                string sPin = string.Empty;
                DateTime dtActivationDate = Utils.FICTIVE_DATE;
                DeviceState eState = DeviceState.UnKnown;
                int nDeviceID = 0;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    sUDID = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["UDID"]);
                    nDeviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["device_brand_id"]);
                    sDeviceName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["Name"]);
                    nDeviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["device_family_id"]);
                    sPin = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["pin"]);
                    bool bIsActiveInDevices = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE_IN_DEVICES"]) == 1;
                    bool bIsActiveInDomainsDevices = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["IS_ACTIVE_IN_DD"]) == 1;
                    eState = !bIsActiveInDevices ? DeviceState.Pending : bIsActiveInDomainsDevices ? DeviceState.Activated : DeviceState.UnActivated;
                    dtActivationDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["last_activation_date"]);
                    nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["device_id"]);

                    Device device = new Device(sUDID, nDeviceBrandID, m_nGroupID, sDeviceName, m_nDomainID, nDeviceID, nDeviceFamilyID, string.Empty, sPin,
                        dtActivationDate, eState);

                    if (AddDeviceToContainer(device))
                    {
                        MapDeviceToFamily(device);
                        IncrementDeviceCount(device);
                    }
                    else
                    {
                        Logger.Logger.Log("InitializeDomainDevicesData", String.Concat("No device container was found. ", device.ToString()), "Domain");
                    }
                }
            }
            else
            {
                Logger.Logger.Log("InitializeDomainDevicesData", String.Concat("No devices were extracted from DB. Domain ID: ", m_nDomainID), "Domain");
            }
        }

        private void IncrementDeviceCount(Device device)
        {
            if (device.IsActivated())
                m_totalNumOfDevices++;
        }

        private void MapDeviceToFamily(Device device)
        {
            if (!m_oUDIDToDeviceFamilyMapping.ContainsKey(device.m_deviceUDID))
            {
                m_oUDIDToDeviceFamilyMapping.Add(device.m_deviceUDID, device.m_deviceFamilyID);
            }
        }

        private bool AddDeviceToContainer(Device device)
        {
            bool res = false;
            DeviceContainer dc = GetDeviceContainer(device.m_deviceFamilyID);
            if (dc != null)
            {
                dc.AddDeviceInstance(device);
                res = true;
            }

            return res;
        }

        /// <summary>
        /// update m_Users list
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        protected DomainResponseStatus GetUserList(int nDomainID, int nGroupID)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            m_UsersIDs = new List<int>();
            m_masterGUIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();

            int status = 1;
            int isActive = 1;


            // Get Domain users from DB; Master user is first
            Dictionary<int, int> dbTypedUserIDs = DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);

            if (dbTypedUserIDs != null && dbTypedUserIDs.Count > 0)
            {
                m_UsersIDs = dbTypedUserIDs.Where(ut => ut.Value != (int)UserDomainType.Household).Select(ut => ut.Key).ToList();
                m_masterGUIDs = dbTypedUserIDs.Where(ut => ut.Value == (int)UserDomainType.Master).Select(ut => ut.Key).ToList();
                m_DefaultUsersIDs = dbTypedUserIDs.Where(ut => ut.Value == (int)UserDomainType.Household).Select(ut => ut.Key).ToList();

                eDomainResponseStatus = DomainResponseStatus.OK;
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.NoUsersInDomain;
                return eDomainResponseStatus;
            }

            // Now get only pending users
            isActive = 0;
            status = 3;
            Dictionary<int, int> dbPendingUserIDs = DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);

            if (dbPendingUserIDs != null && dbPendingUserIDs.Count > 0)
            {
                m_PendingUsersIDs = dbPendingUserIDs.Select(ut => ut.Key).ToList();
            }

            return eDomainResponseStatus;
        }

        protected bool SetDomainFlag(int domainId, int val, bool deviceFlag = true)
        {
            DateTime dt = DateTime.UtcNow;

            bool res = DomainDal.SetDomainFlag(domainId, val, dt, Convert.ToInt32(deviceFlag));

            if (res)
            {
                if (deviceFlag)
                {
                    m_NextActionFreq = dt;
                }
                else
                {
                    m_NextUserActionFreq = dt;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        protected DomainResponseStatus RemoveUserFromList(int nUserGuid)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;
            string sUserGuid = nUserGuid.ToString();
            for (int i = 0; i < m_UsersIDs.Count; i++)
            {
                int nGuID = m_UsersIDs[i];
                if (nGuID == nUserGuid)
                {
                    m_UsersIDs.RemoveAt(i);
                    eRetVal = DomainResponseStatus.OK;
                    break;
                }
            }
            return eRetVal;
        }


        protected DeviceContainer GetDeviceContainer(int deviceFamilyID)
        {
            if (m_oDeviceFamiliesMapping != null && m_oDeviceFamiliesMapping.Count > 0 && m_oDeviceFamiliesMapping.ContainsKey(deviceFamilyID))
                return m_oDeviceFamiliesMapping[deviceFamilyID];
            return null;
        }


        /// <summary>
        /// set Domain Object name, description and limit,
        /// according to DomainId and GroupID
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        /// <returns>true if Query is valid</returns>
        protected bool GetDomainSettings(int nDomainID, int nGroupID)
        {
            DateTime dDeviceFrequencyLastAction = Utils.FICTIVE_DATE;
            DateTime dUserFrequencyLastAction = Utils.FICTIVE_DATE;

            string sName = string.Empty;
            string sDescription = string.Empty;
            int nDeviceLimitationModule = 0;
            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nConcurrentLimit = 0;
            int nStatus = 0;
            int nIsActive = 0;
            int nFrequencyFlag = 0;
            int nDeviceMinPeriodId = 0;
            int nUserMinPeriodId = 0;
            string sCoGuid = string.Empty;
            int nDeviceRestriction = 0;
            int nGroupConcurrentLimit = 0;

            bool res = DomainDal.GetDomainSettings(nDomainID, nGroupID, ref sName, ref sDescription, ref nDeviceLimitationModule, ref nDeviceLimit,
                ref nUserLimit, ref nConcurrentLimit, ref nStatus, ref nIsActive, ref nFrequencyFlag, ref nDeviceMinPeriodId, ref nUserMinPeriodId,
                ref dDeviceFrequencyLastAction, ref dUserFrequencyLastAction, ref sCoGuid, ref nDeviceRestriction, ref nGroupConcurrentLimit);

            if (res)
            {
                m_sName = sName;
                m_sDescription = sDescription;
                m_deviceLimitationModule = nDeviceLimitationModule;
                m_nDeviceLimit = m_nLimit = nDeviceLimit;
                m_nUserLimit = nUserLimit;
                m_nConcurrentLimit = nConcurrentLimit;
                m_nStatus = nStatus;
                m_nIsActive = nIsActive;
                m_frequencyFlag = nFrequencyFlag;
                m_minPeriodId = nDeviceMinPeriodId;
                m_minUserPeriodId = nUserMinPeriodId;
                m_sCoGuid = sCoGuid;
                m_DomainRestriction = (DomainRestriction)nDeviceRestriction;

                InitializeLimitationsManager(nConcurrentLimit, nGroupConcurrentLimit, nDeviceLimit, nDeviceMinPeriodId, dDeviceFrequencyLastAction);

                if (m_minPeriodId != 0)
                {
                    m_NextActionFreq = Utils.GetEndDateTime(dDeviceFrequencyLastAction, m_minPeriodId);
                }

                if (m_minUserPeriodId != 0)
                {
                    m_NextUserActionFreq = Utils.GetEndDateTime(dUserFrequencyLastAction, m_minUserPeriodId);
                }
            }

            return res;
        }

        private DomainResponseStatus CheckUserLimit(int nDomainID, int nUserGuid)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;

            if (nUserGuid <= 0)
            {
                eRetVal = DomainResponseStatus.UserNotAllowed;

            }
            else if (m_nUserLimit > 0 && m_totalNumOfUsers >= m_nUserLimit)
            {
                eRetVal = DomainResponseStatus.ExceededUserLimit;
            }
            else
            {
                eRetVal = DomainResponseStatus.OK;
            }

            return eRetVal;
        }

        private Device GetDomainDevice(string udid, ref DeviceContainer cont)
        {
            #region New part
            Device retVal = null;
            if (m_oUDIDToDeviceFamilyMapping.ContainsKey(udid) && m_oDeviceFamiliesMapping.ContainsKey(m_oUDIDToDeviceFamilyMapping[udid]))
            {
                DeviceContainer dc = m_oDeviceFamiliesMapping[m_oUDIDToDeviceFamilyMapping[udid]];
                foreach (Device device in dc.DeviceInstances)
                {
                    if (device.m_deviceUDID.Equals(udid))
                    {
                        retVal = device;
                        cont = dc;
                        return device;
                    }
                }
            }

            #endregion
            if (m_deviceFamilies != null)
            {
                foreach (DeviceContainer container in m_deviceFamilies)
                {
                    if (container.DeviceInstances != null)
                    {
                        foreach (Device device in container.DeviceInstances)
                        {
                            if (device.m_deviceUDID.Equals(udid))
                            {
                                retVal = device;
                                cont = container;
                                break;
                            }
                        }
                    }
                }
            }

            return retVal;
        }

        private bool IsAgnosticToDeviceLimitation(ValidationType vt, int deviceFamilyID)
        {
            bool res = false;
            switch (vt)
            {
                case ValidationType.Concurrency:
                    {
                        res = m_oDeviceFamiliesMapping != null && m_oDeviceFamiliesMapping.ContainsKey(deviceFamilyID)
                            && m_oDeviceFamiliesMapping[deviceFamilyID].IsUnlimitedConcurrency();
                        break;
                    }
                case ValidationType.Quantity:
                    {
                        res = m_oDeviceFamiliesMapping != null && m_oDeviceFamiliesMapping.ContainsKey(deviceFamilyID) 
                            && m_oDeviceFamiliesMapping[deviceFamilyID].IsUnlimitedQuantity();
                        break;
                    }
                default:
                    break;
            }

            return res;
        }

        public DomainResponseStatus ValidateConcurrency(string sUDID, int nDeviceBrandID, long lDomainID)
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            if (!string.IsNullOrEmpty(sUDID))
            {
                Device device = new Device(sUDID, nDeviceBrandID, m_nGroupID, string.Empty, (int)lDomainID);
                DeviceContainer dc = GetDeviceContainer(device.m_deviceFamilyID);
                if (dc != null)
                {
                    if (m_oLimitationsManager.Concurrency <= 0 || IsAgnosticToDeviceLimitation(ValidationType.Concurrency, device.m_deviceFamilyID))
                    {
                        // there are no concurrency limitations at all.
                        res = DomainResponseStatus.OK;
                    }
                    else
                    {
                        int nTotalStreams = 0;
                        Dictionary<int, int> deviceFamiliesStreams = GetConcurrentCount(lDomainID, sUDID, ref nTotalStreams);
                        if (deviceFamiliesStreams == null)
                        {
                            // no active streams at all
                            res = DomainResponseStatus.OK;
                        }
                        else
                        {
                            if (nTotalStreams >= m_oLimitationsManager.Concurrency)
                            {
                                // Cannot allow a new stream. Domain reached its max limitation
                                res = DomainResponseStatus.ConcurrencyLimitation;
                            }
                            else
                            {
                                if (deviceFamiliesStreams.ContainsKey(device.m_deviceFamilyID))
                                {
                                    if (deviceFamiliesStreams[device.m_deviceFamilyID] >= dc.m_oLimitationsManager.Concurrency)
                                    {
                                        // device family reached its max limit. Cannot allow a new stream
                                        res = DomainResponseStatus.ConcurrencyLimitation;
                                    }
                                    else
                                    {
                                        // User is able to watch through this device. Hasn't reach the device family max limitation
                                        res = DomainResponseStatus.OK;
                                    }
                                }
                                else
                                {
                                    // no active streams at the device's family.
                                    res = DomainResponseStatus.OK;
                                }
                            }
                        }
                    }
                }
                else
                {
                    res = DomainResponseStatus.DeviceTypeNotAllowed;
                }
            }
            else
            {
                res = DomainResponseStatus.OK;
            }

            return res;
        }

        private Dictionary<int, int> GetConcurrentCount(long lDomainID, string sUDID, ref int nTotalConcurrentStreamsWithoutGivenDevice)
        {
            Dictionary<int, int> res = null;
            List<UserMediaMark> positions = CatalogDAL.GetDomainLastPositions((int)lDomainID, Utils.CONCURRENCY_MILLISEC_THRESHOLD);
            if (positions != null)
            {
                res = new Dictionary<int, int>();
                nTotalConcurrentStreamsWithoutGivenDevice = positions.Where(x => !x.UDID.Equals(sUDID) && x.CreatedAt.AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow).Count();
                var filteredPositions = positions.Where(x => !x.UDID.Equals(sUDID));
                if (filteredPositions != null)
                {
                    foreach (UserMediaMark umm in filteredPositions)
                    {
                        int nDeviceFamilyID = 0;
                        if (!m_oUDIDToDeviceFamilyMapping.TryGetValue(umm.UDID, out nDeviceFamilyID) || nDeviceFamilyID == 0)
                        {
                            // the device family id does not exist in Domain cache. Grab it from DB.
                            int nDeviceBrandID = 0;
                            nDeviceFamilyID = DeviceDal.GetDeviceFamilyID(m_nGroupID, umm.UDID, ref nDeviceBrandID);
                            if(nDeviceFamilyID > 0)
                                m_oUDIDToDeviceFamilyMapping.Add(umm.UDID, nDeviceFamilyID);
                        }
                        if (nDeviceFamilyID > 0)
                        {
                            if (!IsAgnosticToDeviceLimitation(ValidationType.Concurrency, nDeviceFamilyID))
                            {
                                // we have device family id and its not agnostic to limitation. increment its value in the result dictionary.
                                if (res.ContainsKey(nDeviceFamilyID))
                                {
                                    // increment by one
                                    res[nDeviceFamilyID]++;
                                }
                                else
                                {
                                    // add the device family id to dictionary
                                    res.Add(nDeviceFamilyID, 1);
                                }
                            }
                            else
                            {
                                // agnostic to device limitation, decrement total streams count by one.
                                nTotalConcurrentStreamsWithoutGivenDevice--;
                            }
                        }
                    } // end foreach
                }
            }

            return res;
        }

        public DomainResponseStatus ValidateFrequency(string sUDID, int nDeviceBrandID)
        {
            if (m_oLimitationsManager.NextActionFreqDate > DateTime.UtcNow)
                return DomainResponseStatus.LimitationPeriod;
            return DomainResponseStatus.OK;
        }

        public DomainResponseStatus ValidateQuantity(string sUDID, int nDeviceBrandID, DeviceContainer dc = null, Device device = null)
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            if (device == null)
                device = new Device(sUDID, nDeviceBrandID, m_nGroupID);
            if (dc == null)
                dc = GetDeviceContainer(device.m_deviceFamilyID);
            if (dc == null)
            {
                // device type not allowed for this domain
                res = DomainResponseStatus.DeviceTypeNotAllowed;
            }
            else
            {
                bool bIsDeviceActivated = false;
                if (dc.IsContainingDevice(device, ref bIsDeviceActivated))
                {
                    if (bIsDeviceActivated)
                    {
                        // device is associated to this domain and activated
                        res = DomainResponseStatus.DeviceAlreadyExists;
                    }
                    else
                    {
                        // device is associated to domain but not activated.
                        res = CanAddToDeviceContainer(dc);
                    }
                }
                else
                {
                    // the device is not associated to this domain. we need to validate it is not associated to different
                    // domain in the same group
                    if (Device.GetDeviceIDByUDID(device.m_deviceUDID, m_nGroupID) > 0)
                    {
                        // the device is associated to a different domain.
                        res = DomainResponseStatus.DeviceExistsInOtherDomains;
                    }
                    else
                    {
                        res = CanAddToDeviceContainer(dc);
                    }
                }
            }

            return res;
        }

        private DomainResponseStatus CanAddToDeviceContainer(DeviceContainer dc)
        {
            DomainResponseStatus res = DomainResponseStatus.ExceededLimit;

            int activatedDevices = dc.GetActivatedDeviceCount();
            if (m_totalNumOfDevices >= m_oLimitationsManager.Quantity || activatedDevices >= dc.m_oLimitationsManager.Quantity)
            {
                res = DomainResponseStatus.ExceededLimit;
            }
            else
            {
                res = DomainResponseStatus.OK;
            }


            return res;
        }


        #endregion

        /***************************************************************************************************************
         * This method get MediaConcurrencyLimit (int) , domain and mediaID 
         * Get from CB all media play at the last 
         ************************************************************************************************************* */
        internal DomainResponseStatus ValidateMediaConcurrency(int nRuleID, int nMediaConcurrencyLimit, long lDomainID, int nMediaID)
        {
            DomainResponseStatus res = DomainResponseStatus.OK;
            if (nMediaConcurrencyLimit == 0)
            {
                // get limitation from DB
               DataTable dt = ApiDAL.GetMCRulesByID(nRuleID);
               if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
               {
                   nMediaConcurrencyLimit = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "media_concurrency_limit");
               }
            }

            if (nMediaConcurrencyLimit > 0) // check concurrency only if limitation  > 0 
            {  
                List<UserMediaMark> lUserMediaMark = CatalogDAL.GetDomainLastPositions((int)lDomainID, Utils.CONCURRENCY_MILLISEC_THRESHOLD);
                if (lUserMediaMark != null)
                {
                    List<UserMediaMark> lMediaConcurrency = lUserMediaMark.Where(c => c.MediaID == nMediaID && c.CreatedAt.AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow).ToList();
                    if (lMediaConcurrency != null && lMediaConcurrency.Count >= nMediaConcurrencyLimit)
                    {
                        res = DomainResponseStatus.MediaConcurrencyLimitation;
                    }
                }
            }
            return res;
        }
    }
}
