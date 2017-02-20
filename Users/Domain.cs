using ApiObjects;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using NPVR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using Users.Cache;

namespace Users
{
    /// <summary>
    /// This Class Represents Domain Object
    /// </summary>
    [Serializable]
    [JsonObject(Id = "Domain")]
    public class Domain : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_USER_TASK\\{0}";


        //Name of the Domain       
        public string m_sName;

        //Description of the Domain
        public string m_sDescription;

        //CoGuid of the Domain        
        public string m_sCoGuid;

        //Domain ID in Domains table        
        public int m_nDomainID;

        //Domain group_id        
        [JsonProperty()]
        public int m_nGroupID
        {
            get
            {
                return this.GroupId;
            }
            set
            {
                this.GroupId = value;
            }
        }

        //Domain Max_Limit  = module_group_limit_id     
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
        [JsonProperty()]
        public List<DeviceContainer> m_deviceFamilies;

        [JsonProperty()]
        public DomainStatus m_DomainStatus;

        public int m_frequencyFlag;

        public DateTime m_NextActionFreq;
        public DateTime m_NextUserActionFreq;

        // Domain's Operator ID
        public int m_nSSOOperatorID;

        [JsonProperty()]
        public DomainRestriction m_DomainRestriction;

        [JsonProperty()]
        protected int m_totalNumOfDevices;

        [JsonProperty()]
        protected int m_totalNumOfUsers;

        [JsonProperty()]
        protected int m_minPeriodId;

        [JsonProperty()]
        protected int m_minUserPeriodId;

        [JsonProperty()]
        public List<HomeNetwork> m_homeNetworks;

        [XmlIgnore]
        [JsonProperty()]
        protected LimitationsManager m_oLimitationsManager;

        [XmlIgnore]
        protected Dictionary<string, int> m_oUDIDToDeviceFamilyMapping;

        [XmlIgnore]
        [JsonProperty()]
        protected Dictionary<int, DeviceContainer> m_oDeviceFamiliesMapping;

        [JsonProperty()]
        public int m_nRegion;

        [XmlIgnore]
        protected int m_nMasterGuID;

        [XmlIgnore]
        [JsonIgnore()]
        private DomainResponseStatus removeResponse;


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
            m_sName = sName;
            m_sDescription = sDescription;
            m_nGroupID = nGroupID;
            m_sCoGuid = sCoGuid;
            m_nMasterGuID = nMasterGuID;

            this.Insert();

            return this;
        }

        private long InitializeDLM(long npvrQuotaInSecs, int nDomainLimitID, int nGroupID, DateTime nextAction)
        {
            LimitationsManager oLimitationsManager = GetDLM(nDomainLimitID, nGroupID);
            bool bInitialize = Initialize(out npvrQuotaInSecs, oLimitationsManager, nextAction);
            return npvrQuotaInSecs;
        }

        public void InitializeDLM()
        {
            long npvrQuotaInSecs = 0;
            npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, this.m_nLimit, this.m_nGroupID, this.m_NextActionFreq);
        }

        private bool Initialize(out long npvrQuotaInSecs, LimitationsManager oLimitationsManager, DateTime nextAction)
        {
            npvrQuotaInSecs = 0;
            if (oLimitationsManager != null) // initialize all fileds 
            {
                m_nConcurrentLimit = oLimitationsManager.Concurrency;
                m_nDeviceLimit = oLimitationsManager.Quantity;
                m_nUserLimit = oLimitationsManager.nUserLimit;

                m_oLimitationsManager = new LimitationsManager();
                m_oLimitationsManager.Concurrency = oLimitationsManager.Concurrency;
                m_oLimitationsManager.domianLimitID = oLimitationsManager.domianLimitID;
                m_oLimitationsManager.Frequency = oLimitationsManager.Frequency;
                m_oLimitationsManager.npvrQuotaInSecs = oLimitationsManager.npvrQuotaInSecs;
                npvrQuotaInSecs = oLimitationsManager.npvrQuotaInSecs;
                m_oLimitationsManager.Quantity = oLimitationsManager.Quantity;
                m_oLimitationsManager.NextActionFreqDate = nextAction;

                if (m_oDeviceFamiliesMapping == null)
                {
                    m_oDeviceFamiliesMapping = new Dictionary<int, DeviceContainer>();
                }
                if (m_deviceFamilies == null)
                {
                    m_deviceFamilies = new List<DeviceContainer>();
                }
                if (oLimitationsManager.lDeviceFamilyLimitations != null)
                {
                    m_oLimitationsManager.lDeviceFamilyLimitations = oLimitationsManager.lDeviceFamilyLimitations;

                    foreach (DeviceFamilyLimitations item in oLimitationsManager.lDeviceFamilyLimitations)
                    {
                        DeviceContainer dc = new DeviceContainer(item.deviceFamily, item.deviceFamilyName, item.quantity, item.concurrency, item.Frequency);
                        if (!m_oDeviceFamiliesMapping.ContainsKey(item.deviceFamily))
                        {
                            m_deviceFamilies.Add(dc);
                            m_oDeviceFamiliesMapping.Add(item.deviceFamily, dc);
                        }
                        else
                        {
                            for (int i = 0; i < m_deviceFamilies.Count; i++)
                            {
                                if (m_deviceFamilies[i].m_deviceFamilyID == item.deviceFamily)
                                {
                                    m_deviceFamilies[i].m_oLimitationsManager = dc.m_oLimitationsManager;
                                    m_deviceFamilies[i].m_deviceLimit = dc.m_deviceLimit;
                                    m_deviceFamilies[i].m_deviceConcurrentLimit = dc.m_deviceConcurrentLimit;
                                    break;
                                }
                            }
                            m_oDeviceFamiliesMapping[item.deviceFamily].m_oLimitationsManager = dc.m_oLimitationsManager;
                            m_oDeviceFamiliesMapping[item.deviceFamily].m_deviceLimit = dc.m_deviceLimit;
                            m_oDeviceFamiliesMapping[item.deviceFamily].m_deviceConcurrentLimit = dc.m_deviceConcurrentLimit;
                        }
                    }
                }
            }
            return true;
        }

        private LimitationsManager GetDLM(int nDomainLimitID, int m_nGroupID)
        {
            LimitationsManager oLimitationsManager = null;
            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                bool bGet = oDomainCache.GetDLM(nDomainLimitID, m_nGroupID, out oLimitationsManager);

                return oLimitationsManager;
            }
            catch (Exception ex)
            {
                log.Error(string.Empty, ex);
                return null;
            }
        }


        public DomainResponseStatus Remove()
        {
            this.removeResponse = DomainResponseStatus.UnKnown;

            this.Delete();

            return this.removeResponse;
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

            // If we found out that the domain doesn't exist, get out now
            if (this.m_DomainStatus == DomainStatus.DomainNotExists)
            {
                return (true);
            }

            if (!string.IsNullOrEmpty(sName))
            {
                m_sName = sName;
            }
            if (!string.IsNullOrEmpty(sDescription))
            {
                m_sDescription = sDescription;
            }

            DomainResponseStatus dStatus = GetUserList(nDomainID, nGroupID, false);   // OK or NoUsersInDomain --  get user list from DB (not from cache)

            if (m_UsersIDs != null)
                m_totalNumOfUsers = m_UsersIDs.Count();

            int numOfDevices = GetDeviceList(false);

            m_homeNetworks = Utils.GetHomeNetworksOfDomain(nDomainID, nGroupID, false);

            if (m_DomainStatus != DomainStatus.DomainSuspended)
            {
                m_DomainStatus = dStatus == DomainResponseStatus.OK ? DomainStatus.OK : DomainStatus.Error;
            }

            return (m_DomainStatus == DomainStatus.OK || m_DomainStatus == DomainStatus.DomainSuspended);
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

        private bool Initialize(Domain domain)
        {
            try
            {
                this.m_DefaultUsersIDs = domain.m_DefaultUsersIDs;
                this.m_deviceFamilies = domain.m_deviceFamilies;
                this.m_DomainRestriction = domain.m_DomainRestriction;
                this.m_DomainStatus = domain.m_DomainStatus;
                this.m_frequencyFlag = domain.m_frequencyFlag;
                this.m_homeNetworks = domain.m_homeNetworks;
                this.m_masterGUIDs = domain.m_masterGUIDs;
                this.m_minPeriodId = domain.m_minPeriodId;
                this.m_minUserPeriodId = domain.m_minUserPeriodId;
                this.m_nConcurrentLimit = domain.m_nConcurrentLimit;
                this.m_nDeviceLimit = domain.m_nDeviceLimit;
                this.m_nDomainID = domain.m_nDomainID;
                this.m_NextActionFreq = domain.m_NextActionFreq;
                this.m_NextUserActionFreq = domain.m_NextUserActionFreq;
                this.m_nGroupID = domain.m_nGroupID;
                this.m_nIsActive = domain.m_nIsActive;
                this.m_nLimit = domain.m_nLimit;
                this.m_nSSOOperatorID = domain.m_nSSOOperatorID;
                this.m_nStatus = domain.m_nStatus;
                this.m_nUserLimit = domain.m_nUserLimit;
                this.m_oDeviceFamiliesMapping = domain.m_oDeviceFamiliesMapping;
                this.m_oLimitationsManager = domain.m_oLimitationsManager;
                this.m_oUDIDToDeviceFamilyMapping = domain.m_oUDIDToDeviceFamilyMapping;
                this.m_PendingUsersIDs = domain.m_PendingUsersIDs;
                this.m_sCoGuid = domain.m_sCoGuid;
                this.m_sDescription = domain.m_sDescription;
                this.m_sName = domain.m_sName;
                this.m_totalNumOfDevices = domain.m_totalNumOfDevices;
                this.m_UsersIDs = domain.m_UsersIDs;
                if (m_UsersIDs != null)
                    this.m_totalNumOfUsers = this.m_UsersIDs.Count();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
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
            DomainResponseStatus domainRes = GetUserList(nDomainID, nGroupID, false);

            // Device families (limits) are per sub-account            
            //DeviceFamiliesInitializer(m_deviceLimitationModule, nSubGroupID);
            GetDeviceList(false);

            m_homeNetworks = Utils.GetHomeNetworksOfDomain(nDomainID, nGroupID, false);

            if (m_DomainStatus != DomainStatus.DomainSuspended)
            {
                m_DomainStatus = DomainStatus.OK;
            }
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

            int nUserDomainID;
            nUserDomainID = DAL.DomainDal.DoesUserExistInDomain(nGroupID, nDomainID, nUserID, false);

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

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                eRetVal = DomainResponseStatus.DomainSuspended;
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
            List<int> masterUserKV = dTypedUserIDs.Where(x => x.Value == (int)UserDomainType.Master).Select(y => y.Key).ToList();
            List<int> defaultUserKV = dTypedUserIDs.Where(x => x.Value == (int)UserDomainType.Household).Select(y => y.Key).ToList();

            //User can be removed in case there is more than 1 master in domain
            if ((masterUserKV.Contains(nUserID) && masterUserKV.Count == 1) || defaultUserKV.Contains(nUserID))
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
                    // send message to update data of the user in the domain (like series recordings, entitlements)
                    var queue = new QueueWrapper.GenericCeleryQueue();
                    ApiObjects.QueueObjects.UserTaskData message = new ApiObjects.QueueObjects.UserTaskData(nGroupID, UserTaskType.Delete, nUserID.ToString(), nDomainID);
                    queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, nGroupID));

                    SetDomainFlag(nDomainID, 1, false);
                    RemoveUserFromList(nUserID);

                    // MHUB-169: return value shouldn't depend on inner-list removal, but on successful operation in DB instead.
                    eRetVal = DomainResponseStatus.OK;

                    // remove domain from cache 
                    DomainsCache oDomainCache = DomainsCache.Instance();
                    oDomainCache.RemoveDomain(nDomainID);

                    // remove user from cache
                    UsersCache usersCache = UsersCache.Instance();
                    usersCache.RemoveUser(nUserID, nGroupID);

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
            bool bRemove = false;
            eRetVal = AddDeviceToDomain(nGroupID, nDomainID, sUDID, deviceName, brandID, ref device, out bRemove);

            if (bRemove) // changes made on the domain - remove it from Cache
            {
                //Remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(nDomainID);
            }
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

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                bRes = DomainResponseStatus.DomainSuspended;
                return bRes;
            }

            // try to get device from cache 
            bool bDeviceExist = IsDeviceExistInDomain(this, sUDID, ref isActive, ref nDeviceID);

            //int nDomainDeviceID = DomainDal.DoesDeviceExistInDomain(m_nDomainID, m_nGroupID, sUDID, ref isActive, ref nDeviceID);
            //if (nDomainDeviceID > 0)
            if (bDeviceExist)
            {
                // set is_Active = 2; status = 2
                bool bUpdate = DomainDal.UpdateDomainsDevicesStatus(m_nDomainID, m_nGroupID, sUDID, 2, 2);

                if (!bUpdate)
                {
                    log.Debug("RemoveDeviceFromDomain - " + String.Format("Failed to update domains_device table. Status=2, Is_Active=2, ID in m_nDomainID={0}, sUDID={1}", m_nDomainID, sUDID));
                    return DomainResponseStatus.Error;
                }

                // if the first update done successfully - remove domain from cache
                try
                {
                    DomainsCache oDomainCache = DomainsCache.Instance();
                    oDomainCache.RemoveDomain(m_nDomainID);
                }
                catch (Exception ex)
                {
                    log.Error("RemoveDeviceFromDomain - " + String.Format("Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", m_nDomainID, sUDID, ex), ex);
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

                        if (m_minPeriodId != 0 && GetDeviceFrequency(sUDID) != 0)
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

        private bool IsDeviceExistInDomain(Domain domain, string sUDID, ref int isActive, ref int nDeviceID)
        {
            try
            {
                bool bContinue = true;
                if (domain != null && domain.m_deviceFamilies != null && domain.m_deviceFamilies.Count > 0)
                {
                    foreach (DeviceContainer dc in domain.m_deviceFamilies.TakeWhile(x => bContinue))
                    {
                        List<Device> lDevice = dc.DeviceInstances;
                        foreach (Device device in lDevice.TakeWhile(x => bContinue))
                        {
                            if (device.m_deviceUDID.Equals(sUDID))
                            {
                                isActive = device.IsActivated() ? 1 : 0;
                                nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(device.m_id);
                                bContinue = false;
                            }
                        }
                    }

                    if (!bContinue)
                    {
                        return true;
                    }
                }

                // if the values are  (isActive  = -1;nDeviceID = -1;) domain not exsits in cache - go get data from DB
                isActive = -1;
                nDeviceID = -1;

                return false;
            }
            catch (Exception ex)
            {
                log.Error("IsDeviceExistInDomain - " + string.Format("failed IsDeviceExistInDomain domainID= {0},sUDID ={1},  ex = {2}", domain != null ? domain.m_nDomainID : 0, sUDID, ex.Message), ex);
                return false;
            }
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

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                eDomainResponseStatus = DomainResponseStatus.DomainSuspended;
                return eDomainResponseStatus;
            }

            /** 1. Since frequency is defined at domain level and not in device family level we can pass a fictive (0)
             **     device brand id to ValidateFrequency method
            **/
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
                    log.Debug("ChangeDeviceDomainStatus - " + String.Concat("Failed to update is_active in domains_devices. domains devices id: ", nDomainDeviceID, " enableInt: ", enableInt, " UDID: ", sUDID));
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

            if (eDomainResponseStatus == DomainResponseStatus.OK)
            {
                //remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nDomainID);
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
        /// 
        public DomainResponseStatus AddUserToDomain(int nGroupID, int nDomainID, int nUserID, int nMasterUserGuid, UserDomainType userType)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;
            bool bRemove = false;
            eDomainResponseStatus = AddUserToDomain(nGroupID, nDomainID, nUserID, nMasterUserGuid, userType, out bRemove);
            if (bRemove)    //remove domain from Cache
            {
                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);
                //Remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(nDomainID);
            }

            // if user was added successfully as master - set user role to be master
            long roleId;

            if (eDomainResponseStatus == DomainResponseStatus.OK && DAL.UsersDal.IsUserDomainMaster(nGroupID, nUserID))
            {
                if (long.TryParse(Utils.GetTcmConfigValue("master_role_id"), out roleId) && DAL.UsersDal.Insert_UserRole(nGroupID, nUserID.ToString(), roleId, true) > 0)
                {
                    // add invalidation key for user roles cache
                    string invalidationKey = LayeredCacheKeys.GetAddRoleInvalidationKey(nUserID.ToString());
                    if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.ErrorFormat("Failed to set invalidation key on AddUserToDomain key = {0}", invalidationKey);
                    }
                }
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
                this.m_totalNumOfUsers = m_UsersIDs.Count - m_DefaultUsersIDs.Count;

                eDomainResponseStatus = DomainResponseStatus.OK;
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.Error;
            }

            if (eDomainResponseStatus == DomainResponseStatus.OK)  //remove domain & user from cache
            {
                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);

                //Remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                bool bCached = oDomainCache.RemoveDomain(nDomainID);
                if (!bCached)
                {
                    log.Debug("AddUserToDomain Failed - " + String.Format("failed to remove domain id from CB nGroupID ={0}, nDomainID={1}, nUserID={2},bIsMaster={3}", nGroupID, nDomainID, nUserID, bIsMaster));
                }
            }

            return eDomainResponseStatus;
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserID, string sMasterUsername)
        {
            int nDomainID = 0;
            DomainResponseObject response = SubmitAddUserToDomainRequest(nGroupID, nUserID, sMasterUsername, out nDomainID);
            if (nDomainID != 0)
            {
                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);
                DomainsCache oDomainCache = DomainsCache.Instance();
                bool bRemove = oDomainCache.RemoveDomain(nDomainID);
            }
            return response;
        }

        public static List<Domain> GetDeviceDomains(int deviceID, int groupID)
        {
            try
            {
                List<Domain> retVal = retVal = new List<Domain>();
                DomainsCache oDomainCache = DomainsCache.Instance();
                List<int> dbDomains = DAL.DomainDal.GetDeviceDomains(deviceID, groupID);

                foreach (int newDomainID in dbDomains)
                {
                    Domain domain = oDomainCache.GetDomain(newDomainID, groupID);
                    if (domain != null)
                    {
                        retVal.Add(domain);
                    }
                    else
                    {
                        log.ErrorFormat("Failed getting domain: {0} in GetDeviceDomains", newDomainID);
                    }
                }

                if (retVal.Count == 0)
                    retVal = null;
                return retVal;
            }
            catch (Exception ex)
            {
                log.Error("GetDeviceDomains - " + String.Format("failed ex={0}, deviceID={1}, groupID={2}", ex.Message, deviceID, groupID), ex);
                return null;
            }
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
            Device device = null;

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                eRetVal = DeviceResponseStatus.Error;
                device = new Device(m_nGroupID);
                device.m_state = DeviceState.Error;
                return device;
            }

            bool res = DomainDal.GetDeviceIdAndBrandByPin(sPIN, nGroupID, ref sUDID, ref nBrandID);


            // If devices to register was found in devices table, register it to domain
            if (!string.IsNullOrEmpty(sUDID))
            {
                //Add new Device to Domain
                device = new Device(sUDID, nBrandID, nGroupID);
                device.Initialize(sUDID);
                device.m_deviceName = sDeviceName;
                DomainResponseStatus eStatus = AddDeviceToDomain(nGroupID, nDomainID, sUDID, sDeviceName, nBrandID, ref device);

                switch (eStatus)
                {
                    case DomainResponseStatus.ExceededLimit:
                        eRetVal = DeviceResponseStatus.ExceededLimit;
                        break;

                    case DomainResponseStatus.DeviceAlreadyExists:
                        eRetVal = DeviceResponseStatus.DuplicatePin;
                        break;

                    case DomainResponseStatus.OK:
                        device.m_state = DeviceState.Activated;
                        eRetVal = DeviceResponseStatus.OK;
                        break;
                    case DomainResponseStatus.DeviceTypeNotAllowed:
                        device.m_state = DeviceState.Error;
                        eRetVal = DeviceResponseStatus.Error;
                        break;
                    default:
                        eRetVal = DeviceResponseStatus.DuplicatePin;
                        break;
                }
            }
            else
            {
                // device wasn't found
                eRetVal = DeviceResponseStatus.DeviceNotExists;
                device = new Device(m_nGroupID);
                device.m_state = DeviceState.NotExists;
            }

            return device;
        }

        public DomainResponseStatus ResetDomain(int nFreqencyType = 0)
        {
            bool res = false;
            if (m_DomainStatus != DomainStatus.DomainSuspended)
            {
                res = DomainDal.ResetDomain(m_nDomainID, m_nGroupID, nFreqencyType);
            }
            else
            {
                return DomainResponseStatus.DomainSuspended;
            }

            if (!res)
            {
                return DomainResponseStatus.Error;
            }

            //remove domain from cache 
            DomainsCache oDomainCache = DomainsCache.Instance();
            oDomainCache.RemoveDomain(m_nDomainID);

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
            bool bRemoveDomain = false;
            DomainResponseStatus eDomainResponseStatus = SubmitAddDeviceToDomainRequest(nGroupID, sDeviceUdid, sDeviceName, ref device, out bRemoveDomain);
            if (bRemoveDomain)
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nDomainID);
            }
            return eDomainResponseStatus;
        }

        public DomainResponseStatus ChangeDomainMaster(int nGroupID, int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            #region Validations

            if (m_nDomainID <= 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                return DomainResponseStatus.DomainSuspended;
            }

            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(this.m_nDomainID, nGroupID, true);

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

            if (rowsAffected > 0)
            {
                // Remove both users from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nCurrentMasterID, nGroupID);
                usersCache.RemoveUser(nNewMasterID, nGroupID);

                // remove domain from cache 
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nDomainID);

                return DomainResponseStatus.OK;
            }
            else
            {
                return DomainResponseStatus.Error;
            }
        }

        public DomainResponseStatus ValidateConcurrency(string udid, int nDeviceBrandID, long domainID)
        {
            DomainResponseStatus result = DomainResponseStatus.UnKnown;

            if (!string.IsNullOrEmpty(udid))
            {
                Device device = new Device(udid, nDeviceBrandID, m_nGroupID, string.Empty, (int)domainID);
                DeviceContainer dc = GetDeviceContainer(device.m_deviceFamilyID);
                if (dc != null)
                {
                    if (m_oLimitationsManager.Concurrency <= 0 || IsAgnosticToDeviceLimitation(ValidationType.Concurrency, device.m_deviceFamilyID))
                    {
                        // there are no concurrency limitations at all.
                        result = DomainResponseStatus.OK;
                    }
                    else
                    {
                        int nTotalStreams = 0;
                        Dictionary<int, int> deviceFamiliesStreams = GetConcurrentCount(domainID, udid, ref nTotalStreams);
                        if (deviceFamiliesStreams == null)
                        {
                            // no active streams at all
                            result = DomainResponseStatus.OK;
                        }
                        else
                        {
                            if (nTotalStreams >= m_oLimitationsManager.Concurrency)
                            {
                                // Cannot allow a new stream. Domain reached its max limitation
                                result = DomainResponseStatus.ConcurrencyLimitation;
                            }
                            else
                            {
                                if (deviceFamiliesStreams.ContainsKey(device.m_deviceFamilyID))
                                {
                                    if (deviceFamiliesStreams[device.m_deviceFamilyID] >= dc.m_oLimitationsManager.Concurrency)
                                    {
                                        // device family reached its max limit. Cannot allow a new stream
                                        result = DomainResponseStatus.ConcurrencyLimitation;
                                    }
                                    else
                                    {
                                        // User is able to watch through this device. Hasn't reach the device family max limitation
                                        result = DomainResponseStatus.OK;
                                    }
                                }
                                else
                                {
                                    // no active streams at the device's family.
                                    result = DomainResponseStatus.OK;
                                }
                            }
                        }
                    }
                }
                else
                {
                    result = DomainResponseStatus.DeviceTypeNotAllowed;
                }
            }
            else
            {
                result = DomainResponseStatus.OK;
            }

            return result;
        }

        public DomainResponseStatus ValidateFrequency(string sUDID, int nDeviceBrandID)
        {
            // check if the frequency assigned to the device family is 0 - in that case the device family is excluded from global DLM policy
            if (m_oDeviceFamiliesMapping != null)
            {
                DeviceContainer deviceFamily = GetDeviceFamilyByUDID(sUDID);

                if (deviceFamily != null)
                {
                    if (deviceFamily.m_oLimitationsManager != null)
                    {
                        if (deviceFamily.m_oLimitationsManager.Frequency == 0)
                        {
                            return DomainResponseStatus.OK;
                        }
                    }
                }
            }

            // check DLM device frequency
            if (m_oLimitationsManager.NextActionFreqDate > DateTime.UtcNow)
                return DomainResponseStatus.LimitationPeriod;
            return DomainResponseStatus.OK;
        }

        private int GetDeviceFrequency(string sUDID)
        {
            // check if the frequency assigned to the device family is 0 - in that case the device family is excluded from global DLM policy
            if (m_oDeviceFamiliesMapping != null)
            {
                DeviceContainer deviceFamily = GetDeviceFamilyByUDID(sUDID);

                if (deviceFamily != null)
                {
                    if (deviceFamily.m_oLimitationsManager != null && deviceFamily.m_oLimitationsManager.Frequency != -1)
                    {
                        return deviceFamily.m_oLimitationsManager.Frequency;
                    }
                }
            }

            return m_oLimitationsManager.Frequency;
        }

        private DeviceContainer GetDeviceFamilyByUDID(string sUDID)
        {
            foreach (var family in m_oDeviceFamiliesMapping)
            {
                Device device = family.Value.DeviceInstances.Where(d => d.m_deviceUDID == sUDID).FirstOrDefault();
                if (device != null)
                {
                    return family.Value;
                }
            }

            return null;
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

                    bool deviceAlreadyExistsInOtherDomain = false;
                    if (String.IsNullOrEmpty(device.m_id))
                    {
                        // check if the device exists by UDID
                        if (Device.GetDeviceIDByUDID(device.m_deviceUDID, m_nGroupID) > 0)
                        {
                            deviceAlreadyExistsInOtherDomain = true;
                        }
                    }
                    else
                    {
                        // check if the device exists by device ID
                        List<int> deviceDomainIds = DAL.DomainDal.GetDeviceDomains(Convert.ToInt32(device.m_id), m_nGroupID);
                        if (deviceDomainIds != null && deviceDomainIds.Count > 0)
                        {
                            deviceAlreadyExistsInOtherDomain = true;
                        }
                    }

                    if (deviceAlreadyExistsInOtherDomain)
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

        protected internal void DeviceFamiliesInitializer(int nDomainLimitationModuleID, int nGroupID)
        {
            Dictionary<int, int> concurrencyOverride = new Dictionary<int, int>();
            Dictionary<int, int> quantityOverride = new Dictionary<int, int>();
            List<string[]> dbDeviceFamilies = DomainDal.Get_DeviceFamiliesLimits(nGroupID, nDomainLimitationModuleID, ref concurrencyOverride, ref quantityOverride);

            if (m_deviceFamilies == null)
            {
                m_deviceFamilies = new List<DeviceContainer>(dbDeviceFamilies.Count);
            }
            if (m_oDeviceFamiliesMapping == null)
            {
                m_oDeviceFamiliesMapping = new Dictionary<int, DeviceContainer>(dbDeviceFamilies.Count);
            }
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
                DeviceContainer dc = new DeviceContainer(nFamilyID, sFamilyName, nOverrideQuantityLimit > -1 ? nOverrideQuantityLimit : m_oLimitationsManager.Quantity, nOverrideConcurrencyLimit > -1 ? nOverrideConcurrencyLimit : m_oLimitationsManager.Concurrency, m_oLimitationsManager.Frequency);
                if (!m_oDeviceFamiliesMapping.ContainsKey(nFamilyID))
                {
                    m_deviceFamilies.Add(dc);
                    m_oDeviceFamiliesMapping.Add(nFamilyID, dc);
                }
                else
                {
                    m_oDeviceFamiliesMapping[nFamilyID].m_oLimitationsManager = dc.m_oLimitationsManager;
                    m_oDeviceFamiliesMapping[nFamilyID].m_deviceConcurrentLimit = dc.m_deviceConcurrentLimit;
                    m_oDeviceFamiliesMapping[nFamilyID].m_deviceLimit = dc.m_deviceLimit;
                    for (int j = 0; j < m_deviceFamilies.Count; j++)
                    {
                        if (m_deviceFamilies[j].m_deviceFamilyID == nFamilyID)
                        {
                            m_deviceFamilies[j].m_oLimitationsManager = dc.m_oLimitationsManager;
                            m_deviceFamilies[j].m_deviceConcurrentLimit = dc.m_deviceConcurrentLimit;
                            m_deviceFamilies[j].m_deviceLimit = dc.m_deviceLimit;
                            break;
                        }
                    }
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
                DeviceFamiliesInitializer(m_nLimit, m_nGroupID);
            }

            DataTable dt = DomainDal.Get_DomainDevices(m_nGroupID, m_nDomainID);
            InitializeDomainDevicesData(dt);

            return m_totalNumOfDevices;
        }

        protected DomainResponseStatus GetUserList(int nDomainID, int nGroupID, bool bCache = false)
        {
            DomainResponseStatus domainResponseStatus = DomainResponseStatus.UnKnown;

            m_UsersIDs = new List<int>();
            m_masterGUIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();

            // get UsersList from Cache
            if (bCache)
            {
                domainResponseStatus = GetUserListFromCache(nDomainID, nGroupID);
            }
            else
            {
                domainResponseStatus = GetUserListFromDB(nDomainID, nGroupID);
            }

            this.m_totalNumOfUsers = m_UsersIDs.Count - m_DefaultUsersIDs.Count;
            return domainResponseStatus;
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
            int regionId = 0;

            DomainSuspentionStatus eSuspendStat = DomainSuspentionStatus.OK;

            bool res = DomainDal.GetDomainSettings(nDomainID, nGroupID, ref sName, ref sDescription, ref nDeviceLimitationModule, ref nDeviceLimit,
                ref nUserLimit, ref nConcurrentLimit, ref nStatus, ref nIsActive, ref nFrequencyFlag, ref nDeviceMinPeriodId, ref nUserMinPeriodId,
                ref dDeviceFrequencyLastAction, ref dUserFrequencyLastAction, ref sCoGuid, ref nDeviceRestriction, ref nGroupConcurrentLimit,
                ref eSuspendStat, ref regionId);

            if (res)
            {
                // If the domain is not in status 1, the rest of the initialization has no meaning
                if (nStatus != 1)
                {
                    this.m_DomainStatus = DomainStatus.DomainNotExists;
                }
                else
                {
                    m_sName = sName;
                    m_sDescription = sDescription;
                    m_nLimit = nDeviceLimitationModule;
                    m_nDeviceLimit = nDeviceLimit;
                    m_nUserLimit = nUserLimit;
                    m_nConcurrentLimit = nConcurrentLimit;
                    m_nStatus = nStatus;
                    m_nIsActive = nIsActive;
                    m_frequencyFlag = nFrequencyFlag;
                    m_minPeriodId = nDeviceMinPeriodId;
                    m_minUserPeriodId = nUserMinPeriodId;
                    m_sCoGuid = sCoGuid;
                    m_nRegion = regionId;
                    m_DomainRestriction = (DomainRestriction)nDeviceRestriction;

                    if (eSuspendStat == DomainSuspentionStatus.Suspended)
                    {
                        m_DomainStatus = DomainStatus.DomainSuspended;
                    }

                    if (m_minPeriodId != 0)
                    {
                        m_NextActionFreq = Utils.GetEndDateTime(dDeviceFrequencyLastAction, m_minPeriodId);
                    }

                    long npvrQuotaInSecs = 0;
                    npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, nDeviceLimitationModule, nGroupID, m_NextActionFreq);

                    if (m_minUserPeriodId != 0)
                    {
                        m_NextUserActionFreq = Utils.GetEndDateTime(dUserFrequencyLastAction, m_minUserPeriodId);
                    }
                }
            }
            else // nothing return - DomainNotExists
            {
                this.m_DomainStatus = DomainStatus.DomainNotExists;
                res = true;
            }

            return res;
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
                }
            }
            else
            {
                log.Debug("InitializeDomainDevicesData - " + String.Concat("No devices were extracted from DB. Domain ID: ", m_nDomainID));
            }
        }

        private void IncrementDeviceCount(Device device)
        {
            if (device.IsActivated())
            {
                if (m_oDeviceFamiliesMapping.ContainsKey(device.m_deviceFamilyID) && !m_oDeviceFamiliesMapping[device.m_deviceFamilyID].IsUnlimitedQuantity())
                {
                    m_totalNumOfDevices++;
                }
            }
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
                if (!dc.DeviceInstances.Contains(device))
                {
                    dc.AddDeviceInstance(device);
                    res = true;
                }
            }

            return res;
        }

        private DomainResponseStatus CanAddToDeviceContainer(DeviceContainer dc)
        {
            DomainResponseStatus res = DomainResponseStatus.ExceededLimit;

            int activatedDevices = dc.GetActivatedDeviceCount();

            // m_oLimitationsManager.Quantity == 0 is unlimited 
            if (dc.m_oLimitationsManager.Quantity > 0 &&
                ((m_totalNumOfDevices >= m_oLimitationsManager.Quantity && m_oLimitationsManager.Quantity > 0) ||
                  (activatedDevices >= dc.m_oLimitationsManager.Quantity)))
            {
                res = DomainResponseStatus.ExceededLimit;
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
            List<UserMediaMark> positions = CatalogDAL.GetDomainLastPositions((int)lDomainID, Utils.CONCURRENCY_MILLISEC_THRESHOLD,
                        new List<ePlayType>() { ApiObjects.ePlayType.NPVR, ApiObjects.ePlayType.MEDIA });

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
                            if (nDeviceFamilyID > 0)
                            {
                                m_oUDIDToDeviceFamilyMapping.Add(umm.UDID, nDeviceFamilyID);
                            }
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

        private DomainResponseStatus SubmitAddDeviceToDomainRequest(int nGroupID, string sDeviceUdid, string sDeviceName, ref Device device, out bool bRemoveDomain)
        {
            #region Validations
            bRemoveDomain = false;

            if (this.m_nDomainID <= 0)
            {
                return DomainResponseStatus.DomainNotInitialized;
            }

            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(this.m_nDomainID, nGroupID, true); // get user list from cache

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
                if (rowsAffected > 0)
                {
                    bRemoveDomain = true;
                }
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
            bRemoveDomain = true;

            string sActivationToken = Guid.NewGuid().ToString();
            nDeviceDomainRecordID = DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 3, 3, sActivationToken);

            if (nDeviceDomainRecordID > 0)
            {
                device.m_state = DeviceState.Pending;
                container.AddDeviceInstance(device);

                m_totalNumOfDevices++;
                //m_deviceFamilies.Add(container); - no need. it is reference type

                User masterUser = new User(nGroupID, m_masterGUIDs[0]);

                AddDeviceMailRequest sMailRequest = null;

                if (masterUser != null)
                {
                    sMailRequest = MailFactory.GetAddDeviceMailRequest(nGroupID, masterUser.m_oBasicData.m_sFirstName, masterUser.m_oBasicData.m_sUserName, masterUser.m_oBasicData.m_sEmail, sDeviceUdid, device.m_deviceName, sActivationToken);
                }

                if (sMailRequest != null)
                {
                    bool sendingMailResult = Utils.SendMail(nGroupID, sMailRequest);

                    return sendingMailResult ? DomainResponseStatus.RequestSent : DomainResponseStatus.RequestFailed;

                }
            }

            return DomainResponseStatus.Error;
        }

        private DomainResponseStatus GetUserListFromCache(int nDomainID, int nGroupID)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;
            try
            {
                DomainsCache oDomainCache = DomainsCache.Instance();

                oDomainCache.GetUserList(nDomainID, nGroupID, this, ref m_UsersIDs, ref m_PendingUsersIDs, ref m_masterGUIDs, ref m_DefaultUsersIDs);
                if ((m_UsersIDs != null && m_UsersIDs.Count > 0) || (m_masterGUIDs != null && m_masterGUIDs.Count > 0) || (m_DefaultUsersIDs != null && m_DefaultUsersIDs.Count > 0))
                {
                    eDomainResponseStatus = DomainResponseStatus.OK;
                }
                else
                {
                    eDomainResponseStatus = DomainResponseStatus.NoUsersInDomain;
                }
                return eDomainResponseStatus;
            }
            catch (Exception ex)
            {
                log.Error("GetUserListFromCache - " + string.Format("failed ex = {0}, nDomainID={1}, int nGroupID={2}", ex.Message, nDomainID, nGroupID), ex);
                return DomainResponseStatus.Error;
            }
        }

        private DomainResponseStatus GetUserListFromDB(int nDomainID, int nGroupID)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;
            try
            {
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
            catch (Exception ex)
            {
                log.Error("GetUserListFromDB - " + string.Format("failed ex = {0}, nDomainID={1}, int nGroupID={2}", ex.Message, nDomainID, nGroupID), ex);
                return DomainResponseStatus.Error;
            }
        }

        private DomainResponseStatus AddUserToDomain(int nGroupID, int nDomainID, int nUserID, int nMasterUserGuid, UserDomainType userType, out bool bRemove)
        {
            bRemove = false;
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;
            int numOfUsers = m_UsersIDs.Count;
            Dictionary<int, int> dbTypedUserIDs = DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                bRemove = false;
                return DomainResponseStatus.DomainSuspended;
            }

            // If domain has no users, insert new Master user
            int status = 1;
            int isActive = 1;

            if ((dbTypedUserIDs == null || dbTypedUserIDs.Count == 0) && (nUserID == nMasterUserGuid))
            {
                int inserted = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int)userType, status, isActive, nMasterUserGuid);

                if (inserted > 0)
                {
                    m_UsersIDs.Add(nUserID);
                    m_masterGUIDs.Add(nUserID);

                    m_totalNumOfUsers = m_UsersIDs.Count - m_DefaultUsersIDs.Count;
                    eDomainResponseStatus = DomainResponseStatus.OK;
                    bRemove = true;

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

            // Check if user already exists in domain and Its Status (active or pending)
            DataTable dtUser = DomainDal.GetUserInDomain(nGroupID, nDomainID, nUserID);

            int? userStatus = null;
            int? userIsActive = null;
            int nUserDomainID = 0;

            if (dtUser != null)
            {
                int nCount = dtUser.DefaultView.Count;
                if (nCount > 0)
                {
                    if (dtUser.DefaultView[0].Row["STATUS"] != null && dtUser.DefaultView[0].Row["STATUS"] != DBNull.Value)
                        userStatus = int.Parse(dtUser.DefaultView[0].Row["STATUS"].ToString());

                    if (dtUser.DefaultView[0].Row["IS_ACTIVE"] != null && dtUser.DefaultView[0].Row["IS_ACTIVE"] != DBNull.Value)
                        userIsActive = int.Parse(dtUser.DefaultView[0].Row["STATUS"].ToString());

                    nUserDomainID = int.Parse(dtUser.DefaultView[0].Row["ID"].ToString());
                }
            }

            // user exist
            if (nUserDomainID > 0)  // If user exists, update its status to active
            {
                //in case user already exist, Do nothing , return userAlreadyexistInDomain
                if (userStatus.HasValue && userStatus == 1 && userIsActive.HasValue && userIsActive == 1)
                {
                    eDomainResponseStatus = DomainResponseStatus.UserAlreadyInDomain;
                }
                else
                {
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

                    //in case user exists, but pending , update its status to active, return OK
                    int rowsAffected = DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, nUserDomainID);

                    if (rowsAffected < 1)
                    {
                        eDomainResponseStatus = DomainResponseStatus.Error;
                    }
                    else
                    {
                        bRemove = true;
                        eDomainResponseStatus = GetUserList(nDomainID, nGroupID, false);
                    }
                }

                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);

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
                bRemove = true;
                eDomainResponseStatus = DomainResponseStatus.OK;

                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.Error;
            }

            return eDomainResponseStatus;
        }

        private DomainResponseObject SubmitAddUserToDomainRequest(int nGroupID, int nUserID, string sMasterUsername, out int nDomainID)
        {
            nDomainID = 0;
            User masterUser = new User();
            int nMasterID = masterUser.InitializeByUsername(sMasterUsername, nGroupID);

            if (nMasterID <= 0)
            {
                return new DomainResponseObject(this, DomainResponseStatus.UserNotAllowed);
            }
            if (masterUser.m_eSuspendState == DomainSuspentionStatus.Suspended)
            {
                return new DomainResponseObject(this, DomainResponseStatus.DomainSuspended);

            }
            // Let's try to find the domain of this master
            nDomainID = masterUser.m_domianID;

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
            DomainsCache oDomainCache = DomainsCache.Instance();
            Domain domain = oDomainCache.GetDomain(nDomainID, nGroupID, false);
            bool init = Initialize(domain);

            if (m_masterGUIDs == null || m_masterGUIDs.Count == 0)
            {
                return new DomainResponseObject(this, DomainResponseStatus.Error);
            }
            if (m_UsersIDs == null || m_UsersIDs.Count == 0)
            {
                DomainResponseStatus eDomainResponseStatus = GetUserList(nDomainID, nGroupID, false); // get user list from DB

                if (m_UsersIDs == null || m_UsersIDs.Count == 0)
                {
                    return new DomainResponseObject(this, DomainResponseStatus.NoUsersInDomain);
                }
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

            // Try getting user from cache
            User user = null;
            UsersCache usersCache = UsersCache.Instance();
            user = usersCache.GetUser(nUserID, nGroupID);

            if (user != null)
            {
                sNewUsername = user.m_oBasicData.m_sUserName;
                sNewFirstName = user.m_oBasicData.m_sFirstName;
                sNewEmail = user.m_oBasicData.m_sEmail;
            }
            else
            {
                using (DataTable dtUserBasicData = UsersDal.GetUserBasicData(nUserID, nGroupID))
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
                    AddUserMailRequest sMailRequest = MailFactory.GetAddUserMailRequest(nGroupID, masterUser.m_oBasicData.m_sFirstName, masterUser.m_oBasicData.m_sUserName, masterUser.m_oBasicData.m_sEmail,
                                                                                                sNewUsername, sNewFirstName, sActivationToken);
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

        private DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, ref Device device, out bool bRemove)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;
            bRemove = false;
            int isDevActive = 0;
            int status = 0;
            int tempDeviceID = 0;
            int nDbDomainDeviceID = 0;

            if (m_DomainStatus == DomainStatus.DomainSuspended)
            {
                eRetVal = DomainResponseStatus.DomainSuspended;
                return eRetVal;
            }

            int domainID = DomainDal.GetDeviceDomainData(nGroupID, sUDID, ref tempDeviceID, ref isDevActive, ref status, ref nDbDomainDeviceID);

            // If the device is already contained in any domain
            if (domainID != 0)
            {
                // If the device is already contained in ANOTHER domain
                if (domainID != nDomainID)
                {
                    eRetVal = DomainResponseStatus.DeviceExistsInOtherDomains;
                    return eRetVal;
                }
                // If the device is already contained in THIS domain
                else
                {
                    // Pending master approval
                    if (status == 3 && isDevActive == 3)
                    {
                        bool updated = DomainDal.UpdateDomainsDevicesStatus(nDbDomainDeviceID, 1, 1);
                        if (updated)
                        {
                            eRetVal = DomainResponseStatus.OK;
                            bRemove = true;
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
                    m_oDeviceFamiliesMapping[device.m_deviceFamilyID].AddDeviceInstance(device);
                    m_totalNumOfDevices++;

                    bRemove = true;
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
                        bRemove = true;
                        eRetVal = DomainResponseStatus.OK;
                        device.m_domainID = nDomainID;
                        int deviceID = device.Save(1);

                        // change the device in the container                      
                        m_oDeviceFamiliesMapping[device.m_deviceFamilyID].ChangeDeviceInstanceState(device.m_deviceUDID, DeviceState.Activated);
                    }
                }
                else
                {
                    eRetVal = DomainResponseStatus.DeviceAlreadyExists;
                }
            }

            //GetDeviceList();

            return eRetVal;
        }
        private bool IsDomainRemovedSuccessfully(int statusRes)
        {
            return statusRes == 2;
        }



        /***************************************************************************************************************
         * This method get MediaConcurrencyLimit (int) , domain and mediaID 
         * Get from CB all media play at the last 
         ************************************************************************************************************* */
        internal DomainResponseStatus ValidateAssetConcurrency(int nRuleID, int mediaConcurrencyLimit, long lDomainID, int assetID, string sUDID)
        {
            DomainResponseStatus response = DomainResponseStatus.OK;

            if (mediaConcurrencyLimit == 0)
            {
                // get limitation from DB
                DataTable dt = ApiDAL.GetMCRulesByID(nRuleID);
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    mediaConcurrencyLimit = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "media_concurrency_limit");
                }
            }

            if (mediaConcurrencyLimit > 0) // check concurrency only if limitation  > 0 
            {
                List<UserMediaMark> lUserMediaMark = CatalogDAL.GetDomainLastPositions((int)lDomainID, Utils.CONCURRENCY_MILLISEC_THRESHOLD,
                        new List<ePlayType>() { ApiObjects.ePlayType.NPVR, ApiObjects.ePlayType.MEDIA });
                if (lUserMediaMark != null)
                {
                    List<UserMediaMark> assetConcurrency = lUserMediaMark.Where(currentMark => !currentMark.UDID.Equals(sUDID) && currentMark.AssetID == assetID &&

                        currentMark.CreatedAt.AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow).ToList();
                    if (assetConcurrency != null && assetConcurrency.Count >= mediaConcurrencyLimit)
                    {
                        response = DomainResponseStatus.MediaConcurrencyLimitation;
                    }
                }
            }
            return response;
        }

        /***************************************************************************************************************
        * This method get NPVRConcurrencyLimit (int) , domain and npvrID
        * Get from CB all media play at the last 
        ************************************************************************************************************* */
        internal DomainResponseStatus ValidateNpvrConcurrency(int nNpvrConcurrencyLimit, long lDomainID, string sNPVR)
        {
            DomainResponseStatus res = DomainResponseStatus.OK;
            try
            {

                if (nNpvrConcurrencyLimit == 0)
                {
                    // get limitation from DB ( get it from domain / group table - wait for future implementation)

                }

                if (nNpvrConcurrencyLimit > 0) // check concurrency only if limitation  > 0 
                {
                    List<UserMediaMark> lUserMediaMark = CatalogDAL.GetDomainLastPositions((int)lDomainID, Utils.CONCURRENCY_MILLISEC_THRESHOLD,
                        new List<ePlayType>() { ApiObjects.ePlayType.NPVR, ApiObjects.ePlayType.MEDIA });

                    if (lUserMediaMark != null)
                    {
                        List<UserMediaMark> lMediaConcurrency = lUserMediaMark.Where(c => c.NpvrID == sNPVR && c.CreatedAt.AddMilliseconds(Utils.CONCURRENCY_MILLISEC_THRESHOLD) > DateTime.UtcNow).ToList();
                        if (lMediaConcurrency != null && lMediaConcurrency.Count >= nNpvrConcurrencyLimit)
                        {
                            res = DomainResponseStatus.MediaConcurrencyLimitation;
                        }
                    }
                }
                return res;
            }
            catch (Exception ex)
            {
                log.Error("ValidateNpvrConcurrency - " + String.Concat("Failed ex={0}, nNpvrConcurrencyLimit={1}, lDomainID={2}, sNPVR={3}",
                    ex.Message, nNpvrConcurrencyLimit, lDomainID, sNPVR), ex);
                throw;
            }
        }

        internal void SetDeviceFamiliesMapping(List<DeviceContainer> dc)
        {
            this.m_deviceFamilies = dc;
        }

        internal bool CompareDLM(LimitationsManager oLimitationsManager, ref ChangeDLMObj oChangeDLMObj)
        {
            try
            {
                if (oLimitationsManager != null) // initialize all fileds 
                {
                    #region Devices
                    List<string> devicesChange = new List<string>();
                    DeviceContainer currentDC = new DeviceContainer();

                    foreach (DeviceFamilyLimitations item in oLimitationsManager.lDeviceFamilyLimitations)
                    {
                        devicesChange = new List<string>();
                        if (m_oDeviceFamiliesMapping.ContainsKey(item.deviceFamily))
                        {
                            currentDC = m_oDeviceFamiliesMapping[item.deviceFamily];
                            if (currentDC != null && currentDC.m_oLimitationsManager != null)
                            {
                                // quntity of the new dlm is less than the cuurent dlm only if new dlm is <> 0 (0 = unlimited)
                                if (currentDC.m_oLimitationsManager.Quantity > item.quantity && item.quantity != 0) // need to delete the laset devices
                                {
                                    // get from DB the last update date domains_devices table  change status to is_active = 0  update the update _date
                                    List<int> lDevicesID = currentDC.DeviceInstances.Select(x => int.Parse(x.m_id)).ToList<int>();
                                    if (lDevicesID != null && lDevicesID.Count > 0 && lDevicesID.Count > item.quantity) // only if there is a gap between current devices to needed quntity
                                    {
                                        int nDeviceToDelete = lDevicesID.Count - item.quantity;
                                        devicesChange = DomainDal.SetDevicesDomainStatus(nDeviceToDelete, 0, this.m_nDomainID, lDevicesID);
                                        if (devicesChange != null && devicesChange.Count > 0)
                                        {
                                            oChangeDLMObj.devices.AddRange(devicesChange);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (KeyValuePair<int, DeviceContainer> currentItem in m_oDeviceFamiliesMapping) // all keys not exsits in new DLM - Delete
                    {
                        devicesChange = new List<string>();
                        bool bNeedToDelete = true;
                        // get from DB the last update date domains_devices table  change status to is_active = 0 + status 2 
                        foreach (DeviceFamilyLimitations item in oLimitationsManager.lDeviceFamilyLimitations)
                        {
                            if (item.deviceFamily == currentItem.Value.m_deviceFamilyID)
                            {
                                bNeedToDelete = false;
                            }
                        }
                        if (bNeedToDelete) // family device id not exsits in new DLM - delete all devices
                        {
                            List<int> lDevicesID = currentItem.Value.DeviceInstances.Select(x => int.Parse(x.m_id)).ToList<int>();
                            int nDeviceToDelete = lDevicesID.Count();
                            if (nDeviceToDelete > 0)
                            {
                                devicesChange = DomainDal.SetDevicesDomainStatus(nDeviceToDelete, 0, this.m_nDomainID, lDevicesID);
                                oChangeDLMObj.devices.AddRange(devicesChange);
                            }
                        }
                    }

                    // compare the total quntity of this domain 
                    if (this.m_oLimitationsManager != null && this.m_oLimitationsManager.Quantity > oLimitationsManager.Quantity && oLimitationsManager.Quantity != 0)
                    {
                        devicesChange = new List<string>();

                        List<int> lDevicesID = new List<int>();
                        // from all families that are not 0 delete all last devices by activation date
                        foreach (DeviceFamilyLimitations item in oLimitationsManager.lDeviceFamilyLimitations)
                        {
                            if (item.quantity == 0) // create list of all devices that can't be deleteed!!!!!
                            {
                                if (m_oDeviceFamiliesMapping.ContainsKey(item.deviceFamily))
                                {
                                    List<Device> lDevices = m_oDeviceFamiliesMapping[item.deviceFamily].DeviceInstances;
                                    lDevicesID.AddRange(lDevices.Select(x => int.Parse(x.m_id)));
                                }
                            }
                        }
                        if (lDevicesID.Count > 0)
                        {
                            int nDeviceToDelete = lDevicesID.Count - oLimitationsManager.Quantity;
                            if (nDeviceToDelete > 0)
                            {
                                devicesChange = DomainDal.SetDevicesDomainStatusNotInList(nDeviceToDelete, 0, this.m_nDomainID, lDevicesID);
                                if (devicesChange != null && devicesChange.Count > 0)
                                {
                                    oChangeDLMObj.devices.AddRange(devicesChange);
                                }
                            }
                        }
                    }

                    #endregion

                    #region Users limit
                    List<string> users = new List<string>();
                    if (this.m_nUserLimit > oLimitationsManager.nUserLimit && oLimitationsManager.nUserLimit != 0)
                    {
                        // change users status to Pending
                        if (this.m_UsersIDs != null && this.m_UsersIDs.Count > 0 && this.m_UsersIDs.Count > oLimitationsManager.nUserLimit)
                        {
                            int nUserToDelete = this.m_UsersIDs.Count - oLimitationsManager.nUserLimit;
                            users = DomainDal.SetUsersStatus(this.m_UsersIDs, nUserToDelete, 3, 0, this.m_nDomainID);
                            if (users != null && users.Count > 0)
                            {
                                oChangeDLMObj.users.AddRange(users);
                            }
                        }
                    }
                    #endregion
                }

                // change dlmid in domain table 
                bool bChangeDoamin = DomainDal.ChangeDomainDLM(this.m_nDomainID, oLimitationsManager.domianLimitID);

                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, string.Empty);
                return false;
            }
        }

        protected override bool DoInsert()
        {
            bool success = false;

            DateTime dDateTime = DateTime.UtcNow;

            long npvrQuotaInSecs = 0;

            // try to get the DomainLimitID
            int nDomainID = -1;
            int nDomainLimitID = DomainDal.Get_DomainLimitID(this.m_nGroupID);
            bool bInserRes =
                DomainDal.InsertNewDomain(this.m_sName, this.m_sDescription, this.m_nGroupID, dDateTime, nDomainLimitID, ref nDomainID, this.m_sCoGuid);

            if (!bInserRes)
            {
                m_DomainStatus = DomainStatus.Error;
                return success;
            }

            int nIsActive = 0;
            int nStatus = 0;
            int regionId = 0;

            Domain domainDbObj = this;

            bool resDbObj =
                DomainDal.GetDomainDbObject(this.m_nGroupID, dDateTime, ref this.m_sName, ref this.m_sDescription,
                nDomainID, ref nIsActive, ref nStatus, ref this.m_sCoGuid, ref regionId);

            m_nDomainID = nDomainID;
            m_nIsActive = nIsActive;
            m_nStatus = nStatus;
            m_nRegion = regionId;

            m_nLimit = nDomainLimitID; // the id for GROUPS_DEVICE_LIMITATION_MODULES table 

            // try to get from chace - DomainLimitID by nDomainLimitID

            npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, nDomainLimitID, m_nGroupID, Utils.FICTIVE_DATE);

            m_DomainStatus = DomainStatus.OK;

            m_UsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();

            DomainResponseStatus res = AddUserToDomain(m_nGroupID, m_nDomainID, this.m_nMasterGuID, this.m_nMasterGuID, UserDomainType.Master);

            if (res == DomainResponseStatus.OK)
            {
                m_UsersIDs = new List<int>();
                m_UsersIDs.Add(this.m_nMasterGuID);
                m_totalNumOfUsers++;
                success = true;
            }

            return success;
        }

        protected override bool DoUpdate()
        {
            bool dbRes = DomainDal.UpdateDomain(m_sName, m_sDescription, m_nDomainID, m_nGroupID, (int)m_DomainRestriction);

            if (!dbRes)
            {
                m_DomainStatus = DomainStatus.Error;
            }
            else
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nDomainID);
            }

            return dbRes;
        }

        protected override bool DoDelete()
        {
            bool success = false;
            int isActive = 2;   // Inactive
            int status = 2;     // Removed

            // get household users
            List<int> domainUserIds = new List<int>();
            domainUserIds.AddRange(m_DefaultUsersIDs);
            domainUserIds.AddRange(m_masterGUIDs);
            domainUserIds.AddRange(m_PendingUsersIDs);
            domainUserIds.AddRange(m_UsersIDs);
            domainUserIds = domainUserIds.Distinct().ToList();

            int statusRes = DomainDal.SetDomainStatus(m_nGroupID, m_nDomainID, isActive, status);

            //return statusRes == 2 ? DomainResponseStatus.OK : DomainResponseStatus.Error;
            if (IsDomainRemovedSuccessfully(statusRes))
            {
                //remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nDomainID);

                INPVRProvider npvr;
                if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID, out npvr))
                {
                    if (npvr != null)
                    {
                        NPVRUserActionResponse npvrResponse = npvr.DeleteAccount(new NPVRParamsObj()
                        {
                            EntityID = m_nDomainID.ToString()
                        });

                        if (npvrResponse != null)
                        {
                            if (npvrResponse.isOK)
                            {
                                removeResponse = DomainResponseStatus.OK;
                            }
                            else
                            {
                                removeResponse = DomainResponseStatus.Error;
                                log.Error("Error - " + string.Format("Remove. NPVR DeleteAccount response status is not ok. G ID: {0} , D ID: {1} , Err Msg: {2}", m_nGroupID, m_nDomainID, npvrResponse.msg));
                            }
                        }
                        else
                        {
                            removeResponse = DomainResponseStatus.Error;
                            log.Error("Error - " + string.Format("Remove. DeleteAccount returned response null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID));
                        }
                    }
                    else
                    {
                        removeResponse = DomainResponseStatus.Error;
                        log.Error("Error - " + string.Format("Remove. NPVR Provider is null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID));
                    }
                }
            }
            else
            {
                removeResponse = DomainResponseStatus.Error;
            }

            if (removeResponse == DomainResponseStatus.UnKnown)
            {
                removeResponse = statusRes == 2 ? DomainResponseStatus.OK : DomainResponseStatus.Error;
            }

            success = removeResponse == DomainResponseStatus.OK;

            return success;
        }

        public override CoreObject CoreClone()
        {
            return this.MemberwiseClone() as CoreObject;
        }
    }
}
