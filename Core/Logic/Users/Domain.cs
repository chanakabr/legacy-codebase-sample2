using ApiLogic.Api.Managers;
using ApiLogic.Users.Security;
using ApiLogic.Users.Services;
using ApiObjects;
using ApiObjects.DRM;
using ApiObjects.MediaMarks;
using ApiObjects.Response;
using ApiObjects.Segmentation;
using CachingProvider.LayeredCache;
using Phx.Lib.Appconfig;
using Core.Users.Cache;
using DAL;
using Phx.Lib.Log;
using Newtonsoft.Json;
using NPVR;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Tvinci.Core.DAL;
using TVinciShared;
using ApiObjects.CanaryDeployment;
using AuthenticationGrpcClientWrapper;
using APILogic.Api.Managers;
using ApiLogic.Users.Managers;
using ApiObjects.CanaryDeployment.Microservices;
using ApiObjects.Roles;
using CanaryDeploymentManager;
using Core.Api;

namespace Core.Users
{
    /// <summary>
    /// This Class Represents Domain Object
    /// </summary>
    [Serializable]
    [JsonObject(Id = "Domain")]
    public class Domain : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
            UdidToDeviceFamilyIdMapping = new Dictionary<string, int>();
        }

        public Domain(int nDomainID) : this()
        {
            m_nDomainID = nDomainID;
        }

        private const string SCHEDULED_TASKS_ROUTING_KEY = "PROCESS_USER_TASK\\{0}";

        //Name of the Domain       
        public string m_sName;

        //Description of the Domain
        public string m_sDescription;

        //CoGuid of the Domain        
        public string m_sCoGuid;

        //Domain ID in Domains table        
        public int m_nDomainID { get; set; }

        //Domain group_id        
        [JsonProperty()]
        public int m_nGroupID
        {
            get { return this.GroupId; }
            set { this.GroupId = value; }
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
        [JsonProperty()] public List<DeviceContainer> m_deviceFamilies;

        [XmlIgnore] protected Dictionary<string, int> UdidToDeviceFamilyIdMapping;

        [XmlIgnore] [JsonProperty()] internal Dictionary<int, DeviceContainer> DeviceFamiliesMapping;

        [JsonProperty()] public DomainStatus m_DomainStatus;

        public int m_frequencyFlag;

        public DateTime m_NextActionFreq;
        public DateTime m_NextUserActionFreq;

        // Domain's Operator ID
        public int m_nSSOOperatorID;

        [JsonProperty()] public DomainRestriction m_DomainRestriction;

        [JsonProperty()] internal int m_totalNumOfDevices;

        [JsonProperty()] protected int m_totalNumOfUsers;

        [JsonProperty()] protected int m_minPeriodId;

        [JsonProperty()] protected int m_minUserPeriodId;

        [JsonProperty()] public List<HomeNetwork> m_homeNetworks;

        [XmlIgnore] [JsonProperty()] public LimitationsManager m_oLimitationsManager { get; protected set; }

        [JsonProperty()] public int m_nRegion;

        [XmlIgnore] protected int MasterGuID;

        [XmlIgnore] [JsonIgnore()] private DomainResponseStatus removeResponse;

        [XmlIgnore] [JsonIgnore()] public bool shouldUpdateInfo;

        [XmlIgnore] [JsonIgnore()] public DomainSuspentionStatus nextSuspensionStatus;

        public int? roleId { get; set; }

        [XmlIgnore] [JsonIgnore()] public bool shouldUpdateSuspendStatus;

        [XmlIgnore] [JsonIgnore()] public bool shouldPurge;

        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        #region Initialize

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

            DomainResponseStatus dStatus = GetUserList(nDomainID, nGroupID, false); // OK or NoUsersInDomain --  get user list from DB (not from cache)

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
                this.DeviceFamiliesMapping = domain.DeviceFamiliesMapping;
                this.m_oLimitationsManager = domain.m_oLimitationsManager;
                this.UdidToDeviceFamilyIdMapping = domain.UdidToDeviceFamilyIdMapping;
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

        private bool Initialize(out long npvrQuotaInSecs, LimitationsManager limitationsManager, DateTime nextAction)
        {
            npvrQuotaInSecs = 0;
            if (limitationsManager != null) // initialize all fileds 
            {
                m_nConcurrentLimit = limitationsManager.Concurrency;
                m_nDeviceLimit = limitationsManager.Quantity;
                m_nUserLimit = limitationsManager.nUserLimit;

                m_oLimitationsManager = new LimitationsManager()
                {
                    Concurrency = limitationsManager.Concurrency,
                    domianLimitID = limitationsManager.domianLimitID,
                    Frequency = limitationsManager.Frequency,
                    npvrQuotaInSecs = limitationsManager.npvrQuotaInSecs,
                    Quantity = limitationsManager.Quantity,
                    NextActionFreqDate = nextAction
                };

                npvrQuotaInSecs = limitationsManager.npvrQuotaInSecs;

                if (m_deviceFamilies == null)
                {
                    m_deviceFamilies = new List<DeviceContainer>();
                }

                if (DeviceFamiliesMapping == null)
                {
                    DeviceFamiliesMapping = new Dictionary<int, DeviceContainer>();
                }

                if (limitationsManager.lDeviceFamilyLimitations != null)
                {
                    m_oLimitationsManager.lDeviceFamilyLimitations = limitationsManager.lDeviceFamilyLimitations;

                    foreach (DeviceFamilyLimitations currDeviceFamilyLimitations in m_oLimitationsManager.lDeviceFamilyLimitations)
                    {
                        DeviceContainer currDeviceContainer = new DeviceContainer(currDeviceFamilyLimitations.deviceFamily,
                            currDeviceFamilyLimitations.deviceFamilyName,
                            currDeviceFamilyLimitations.quantity,
                            currDeviceFamilyLimitations.concurrency,
                            currDeviceFamilyLimitations.Frequency);

                        this.InitDeviceFamily(currDeviceContainer, currDeviceFamilyLimitations.deviceFamily);
                    }
                }
            }

            return true;
        }

        private void InitDeviceFamily(DeviceContainer deviceContainer, int deviceFamilyId)
        {
            if (!DeviceFamiliesMapping.ContainsKey(deviceFamilyId))
            {
                m_deviceFamilies.Add(deviceContainer);
                DeviceFamiliesMapping.Add(deviceFamilyId, deviceContainer);
            }
            else
            {
                DeviceFamiliesMapping[deviceFamilyId].m_oLimitationsManager = deviceContainer.m_oLimitationsManager;
                DeviceFamiliesMapping[deviceFamilyId].m_deviceConcurrentLimit = deviceContainer.m_deviceConcurrentLimit;
                DeviceFamiliesMapping[deviceFamilyId].m_deviceLimit = deviceContainer.m_deviceLimit;

                int currDeviceFamilyIndex = m_deviceFamilies.FindIndex(x => x.m_deviceFamilyID == deviceFamilyId);
                if (currDeviceFamilyIndex != -1)
                {
                    m_deviceFamilies[currDeviceFamilyIndex].m_oLimitationsManager = deviceContainer.m_oLimitationsManager;
                    m_deviceFamilies[currDeviceFamilyIndex].m_deviceConcurrentLimit = deviceContainer.m_deviceConcurrentLimit;
                    m_deviceFamilies[currDeviceFamilyIndex].m_deviceLimit = deviceContainer.m_deviceLimit;
                }
            }
        }

        private long InitializeDLM(long npvrQuotaInSecs, int nDomainLimitID, int nGroupID, DateTime nextAction)
        {
            LimitationsManager oLimitationsManager = GetDLM(nDomainLimitID, nGroupID);
            if (oLimitationsManager != null)
            {
                bool bInitialize = Initialize(out npvrQuotaInSecs, oLimitationsManager, nextAction);
            }

            return npvrQuotaInSecs;
        }

        public void InitializeDLM()
        {
            long npvrQuotaInSecs = 0;
            npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, this.m_nLimit, this.m_nGroupID, this.m_NextActionFreq);
        }

        #endregion

        /// <summary>
        /// Create new Domain record in DB
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="sDescription"></param>
        /// <param name="nGroupID"></param>
        /// <param name="nLimit"></param>
        /// <returns>the new record ID</returns>
        public virtual Domain CreateNewDomain(string sName, string sDescription, int nGroupID, int masterGuID, int? regionId, string sCoGuid = null)
        {
            DateTime dDateTime = DateTime.UtcNow;
            m_sName = sName;
            m_sDescription = sDescription;
            m_nGroupID = nGroupID;
            m_sCoGuid = sCoGuid;

            if (regionId.HasValue)
            {
                m_nRegion = regionId.Value;
            }

            this.MasterGuID = masterGuID;

            // Pending - until proved otherwise
            m_DomainStatus = DomainStatus.Pending;

            this.Insert();

            return this;
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

            //BEO-4478
            if (m_DomainStatus == DomainStatus.DomainSuspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(nGroupID, nUserID))
            {
                if (roleId == 0 || (m_masterGUIDs != null && m_masterGUIDs.Count > 0
                                                          && !RolesPermissionsManager.Instance.IsPermittedPermissionItem(m_nGroupID, m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDUSER_DELETE.ToString())))
                {
                    eRetVal = DomainResponseStatus.DomainSuspended;
                    return eRetVal;
                }
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
            List<int> masterUserKV = dTypedUserIDs.Where(x => x.Value == (int) UserDomainType.Master).Select(y => y.Key).ToList();
            List<int> defaultUserKV = dTypedUserIDs.Where(x => x.Value == (int) UserDomainType.Household).Select(y => y.Key).ToList();

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
                    // GDPR TTV
                    ApiObjects.Segmentation.UserSegment.Remove(nUserID.ToString());

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
                    oDomainCache.RemoveDomain(nGroupID, nDomainID);
                    InvalidateDomain();
                    InvalidateDomainUser(nUserID.ToString());

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

        public DomainResponseStatus RemoveDeviceFromDomain(string udid, bool forceRemove = false, long deviceId = 0)
        {
            DomainResponseStatus bRes = DomainResponseStatus.UnKnown;

            // if next allowed action is in future, return LimitationPeriod status
            // Since frequency is defined at domain level, and not in device family level, we can pass a fictive (0)
            // device brand id to ValidateFrequency method
            if (!forceRemove && ValidateFrequency(udid) == DomainResponseStatus.LimitationPeriod)
            {
                return DomainResponseStatus.LimitationPeriod;
            }

            //BEO-4478
            if (!forceRemove &&
                m_DomainStatus == DomainStatus.DomainSuspended &&
                (roleId == 0 || m_masterGUIDs != null && m_masterGUIDs.Count > 0 &&
                    !RolesPermissionsManager.Instance.IsPermittedPermissionItem(m_nGroupID, m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDDEVICE_DELETE.ToString()) &&
                    !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(m_nGroupID, m_masterGUIDs[0])))
            {
                return DomainResponseStatus.DomainSuspended;
            }

            Device device = null;

            // try to get device from cache 
            bool bDeviceExist = IsDeviceExistInDomain(this, udid, ref deviceId, out device);
            if (!bDeviceExist && !forceRemove) //BEO-BEO-7897
            {
                return DomainResponseStatus.DeviceNotInDomain;
            }

            try
            {
                RemoveDeviceDrmId(udid);
            }
            catch (Exception ex)
            {
                log.Error("RemoveDeviceFromDomain - " + String.Format("Failed to remove DevicesDrmID from db / cache : domainID= {0}, UDID= {1}, ex= {2}", m_nDomainID, udid, ex), ex);
            }

            // set is_Active = 2; status = 2
            DomainDevice domainDevice = new DomainDevice()
            {
                Udid = udid,
                DeviceId = deviceId,
                GroupId = m_nGroupID,
                DomainId = m_nDomainID,
                ActivataionStatus = device != null ? device.m_state : DeviceState.UnActivated
            };

            if (device != null) //BEO-8622
            {
                domainDevice.ActivataionStatus = device.m_state;
                domainDevice.DeviceBrandId = device.m_deviceBrandID;
                domainDevice.ActivatedOn = device.m_activationDate;
                domainDevice.Name = device.m_deviceName;
                domainDevice.DeviceFamilyId = device.m_deviceFamilyID;
                domainDevice.ExternalId = device.ExternalId;
            }

            bool deleted = domainDevice.Delete();

            if (!deleted)
            {
                log.Debug("RemoveDeviceFromDomain - " + String.Format("Failed to update domains_device table. Status=2, Is_Active=2, ID in m_nDomainID={0}, sUDID={1}", m_nDomainID, udid));
                return DomainResponseStatus.Error;
            }

            // set is_Active = 2; status = 2
            deleted = DomainDal.UpdateDeviceStatus(deviceId, 2, 2);

            if (!deleted)
            {
                log.ErrorFormat("Failed to update device in devices table. Status=2, Is_Active=2, UDID={0}, deviceId={1}", udid, deviceId);
                return DomainResponseStatus.OK;
            }

            if (bDeviceExist)
            {
                DeviceContainer container = null;
                device = GetDomainDevice(udid, ref container);
                if (container != null && device != null)
                {
                    if (container.RemoveDeviceInstance(udid))
                    {
                        bRes = DomainResponseStatus.OK;

                        if (!forceRemove && m_minPeriodId != 0 && GetDeviceFrequency(udid) != 0)
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
                bRes = DomainResponseStatus.OK;
            }

            //BEO-9305 
            ConcurrencyManager.HandleRevokePlaybackSession(m_nGroupID, udid);

            return bRes;
        }

        private bool RemoveDeviceDrmId(string sUDID)
        {
            DrmPolicy drmPolicy = Utils.GetDrmPolicy(m_nGroupID);

            if (drmPolicy != null)
            {
                var deviceIds = new List<long>();

                // check that udid exsits in doimain device list
                DeviceContainer deviceContainer = this.m_deviceFamilies.FirstOrDefault(x => x.DeviceInstances != null && x.DeviceInstances.Find(u => u.m_deviceUDID == sUDID) != null ? true : false);
                if (deviceContainer == null || deviceContainer.DeviceInstances == null || deviceContainer.DeviceInstances.Count == 0)
                {
                    log.ErrorFormat("udid not exsits in Domain devices list groupId={0}, domainId={1}, udid ={2}", m_nGroupID, this.m_nDomainID, sUDID);
                    return false; // error 
                }

                // check that device family in the Family policy roles
                if (drmPolicy.FamilyLimitation.Contains(deviceContainer.m_deviceFamilyID))
                {
                    // get domainDrmId by deviceIds list 
                    deviceIds = deviceContainer.DeviceInstances.Select(d => long.Parse(d.m_id)).ToList();

                    return ClearDevicesDrmId(deviceIds);
                }

                switch (drmPolicy.Policy)
                {
                    case DrmSecurityPolicy.DeviceLevel: // device - clear only device 
                        // get specific device by udid 
                        deviceIds = (this.m_deviceFamilies.SelectMany(x => x.DeviceInstances).ToList<Device>()).Where(f => f.m_deviceUDID == sUDID).Select(y => long.Parse(y.m_id)).ToList();
                        break;
                    case DrmSecurityPolicy.HouseholdLevel: // hh - cleare all devices 
                        // get all devices for the domain
                        deviceIds = (this.m_deviceFamilies.SelectMany(x => x.DeviceInstances).ToList<Device>()).Select(y => long.Parse(y.m_id)).ToList();
                        break;
                    default:
                        break;
                }

                // get all drmId for devices      
                return ClearDevicesDrmId(deviceIds);
            }

            return false;
        }

        private bool ClearDevicesDrmId(List<long> deviceIds)
        {
            Dictionary<int, string> domainDrmId;
            if (deviceIds != null && deviceIds.Count > 0)
            {
                domainDrmId = Utils.GetDomainDrmId(m_nGroupID, m_nDomainID);

                if (DomainDal.ClearDevicesDrmID(m_nGroupID, deviceIds, m_nDomainID))
                {
                    if (domainDrmId != null && domainDrmId.Count > 0)
                    {
                        return Utils.RemoveDrmId(domainDrmId.Where(x => deviceIds.Contains(x.Key)).Select(x => x.Value).ToList(), m_nGroupID);
                    }
                }
            }

            return false;
        }

        private bool IsDeviceExistInDomain(Domain domain, string sUDID, ref long nDeviceID, out Device resultDevice)
        {
            resultDevice = null;

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
                                nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(device.m_id);
                                bContinue = false;
                                resultDevice = device;
                            }
                        }
                    }

                    if (!bContinue)
                    {
                        return true;
                    }
                }

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
            DomainResponseStatus domainResponseStatus = DomainResponseStatus.UnKnown;

            //BEO-4478
            if (m_DomainStatus == DomainStatus.DomainSuspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(nGroupID, m_masterGUIDs[0]))
            {
                if (roleId == 0 || (m_masterGUIDs != null && m_masterGUIDs.Count > 0
                                                          && !RolesPermissionsManager.Instance.IsPermittedPermissionItem(m_nGroupID, m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDDEVICE_UPDATESTATUS.ToString())))
                {
                    domainResponseStatus = DomainResponseStatus.DomainSuspended;
                    return domainResponseStatus;
                }
            }

            /** 1. Since frequency is defined at domain level and not in device family level we can pass a fictive (0)
             **     device brand id to ValidateFrequency method
            **/
            if (!bIsEnable && ValidateFrequency(sUDID) == DomainResponseStatus.LimitationPeriod)
            {
                domainResponseStatus = DomainResponseStatus.LimitationPeriod;
                return domainResponseStatus;
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
                domainResponseStatus = ValidateQuantity(sUDID, device.m_deviceBrandID, ref container, ref device);
                eNewDeviceState = DeviceState.Activated;
            }

            int isActive = 0;
            long nDeviceID = 0;
            var nDomainDeviceID = DomainDal.DoesDeviceExistInDomain(m_nDomainID, nGroupID, sUDID, ref isActive, ref nDeviceID); //DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            if (nDomainDeviceID > 0 && domainResponseStatus != DomainResponseStatus.ExceededLimit)
            {
                bool resUpdated = DomainDal.UpdateDomainsDevicesIsActive(nDomainDeviceID, enableInt, bIsEnable);

                if (resUpdated)
                {
                    container.ChangeDeviceInstanceState(sUDID, eNewDeviceState);
                    if (eNewDeviceState == DeviceState.UnActivated)
                    {
                        SetDomainFlag(m_nDomainID, 1);
                    }

                    domainResponseStatus = DomainResponseStatus.OK;
                }
                else
                {
                    log.Debug("ChangeDeviceDomainStatus - " + String.Concat("Failed to update is_active in domains_devices. domains devices id: ", nDomainDeviceID, " enableInt: ", enableInt, " UDID: ", sUDID));
                    domainResponseStatus = DomainResponseStatus.Error;
                }
            }
            else
            {
                if (nDomainDeviceID == 0)
                {
                    domainResponseStatus = DomainResponseStatus.DeviceNotInDomain;
                }
                else
                {
                    domainResponseStatus = DomainResponseStatus.ExceededLimit;
                }
            }

            if (domainResponseStatus == DomainResponseStatus.OK)
            {
                //remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(nGroupID, m_nDomainID);
                InvalidateDomain();
            }

            return domainResponseStatus;
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
            if (bRemove) //remove domain from Cache
            {
                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);
                //Remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(nGroupID, nDomainID);
                InvalidateDomain();
                InvalidateDomainUser(nUserID.ToString());
            }

            RolesResponse userRolesResponse = api.GetUserRoles(nGroupID, nMasterUserGuid.ToString());
            User currUser = UsersCache.Instance().GetUser(nUserID, nGroupID);
            bool isUpdateNeeded = false;
            if (currUser != null && userRolesResponse.Roles != null & userRolesResponse.Roles.Count > 0)
            {
                List<Role> allMasterUserGuidRoles = userRolesResponse.Roles;
                allMasterUserGuidRoles = allMasterUserGuidRoles.FindAll(r => r.Profile == RoleProfileType.Profile);
                foreach (Role role in allMasterUserGuidRoles)
                {
                    if (!currUser.m_oBasicData.RoleIds.Contains(role.Id))
                    {
                        isUpdateNeeded = true;
                        currUser.m_oBasicData.RoleIds.Add(role.Id);
                    }
                }
            }
            
            // if user was added successfully as master - set user role to be master
            if (eDomainResponseStatus == DomainResponseStatus.OK && UsersDal.IsUserDomainMaster(nGroupID, nUserID))
            {
                if (currUser != null)
                {
                    long roleId = ApplicationConfiguration.Current.RoleIdsConfiguration.MasterRoleId.Value;
                    if (roleId > 0 && !currUser.m_oBasicData.RoleIds.Contains(roleId))
                    {
                        isUpdateNeeded = true;
                        currUser.m_oBasicData.RoleIds.Add(roleId);
                    }
                }
            }

            if (isUpdateNeeded)
            {
                if (UsersDal.UpsertUserRoleIds(m_nGroupID, nUserID, currUser.m_oBasicData.RoleIds))
                {
                    // add invalidation key for user roles cache
                    string invalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(nGroupID, nUserID.ToString());
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.Error($"Failed to set invalidation key on GetUserRoles key = {invalidationKey}");
                    }
                }
                else
                {
                    log.Warn($"Upsert user role ids failed. userId = {nUserID}");
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

            if (eDomainResponseStatus == DomainResponseStatus.OK) //remove domain & user from cache
            {
                //Remove user from cache
                UsersCache usersCache = UsersCache.Instance();
                usersCache.RemoveUser(nUserID, nGroupID);

                //Remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                bool cacheRemoveResult = oDomainCache.RemoveDomain(nGroupID, nDomainID);
                InvalidateDomain();
                InvalidateDomainUser(nUserID.ToString());

                if (!cacheRemoveResult)
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
                bool bRemove = oDomainCache.RemoveDomain(nGroupID, nDomainID);
                InvalidateDomain();
                InvalidateDomainUser(nUserID.ToString());
            }

            return response;
        }

        public static List<Domain> GetDeviceDomains(long deviceID, int groupID)
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

            //BEO-4478
            if (m_DomainStatus == DomainStatus.DomainSuspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(nGroupID, m_masterGUIDs[0]))
            {
                if (roleId == 0 || (m_masterGUIDs != null && m_masterGUIDs.Count > 0
                                                          && !RolesPermissionsManager.Instance.IsPermittedPermissionItem(m_nGroupID, m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDDEVICE_ADDBYPIN.ToString())))
                {
                    eRetVal = DeviceResponseStatus.Error;
                    device = new Device(m_nGroupID);
                    device.m_state = DeviceState.Error;
                    return device;
                }
            }

            GetDeviceIdAndBrandByPin(nGroupID, sPIN, ref sUDID, ref nBrandID);

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

        public DomainResponseStatus AddDeviceToDomain(int groupId, int domainId, string udid, string deviceName, int brandId, ref Device device)
        {
            DomainResponseStatus responseStatus = DomainResponseStatus.UnKnown;
            bool insertSuccess = false;
            
            responseStatus = DomainManager.Instance.AddDeviceToDomain(groupId, domainId, udid, deviceName, brandId, this, ref device, out insertSuccess);

            try
            {
                // changes made on the domain - remove it from Cache
                if (insertSuccess)
                {
                    //Remove domain from cache
                    DomainsCache domainCache = DomainsCache.Instance();
                    domainCache.RemoveDomain(groupId, domainId);
                    InvalidateDomain();
                    
                    // remove device play data if exists (if device was removed from domain before)
                    ConcurrencyManager.DeleteDevicePlayData(udid);

                    var domainDevices = Core.Api.api.Instance.GetDomainDevices(domainId, groupId);
                    if (domainDevices != null && !domainDevices.ContainsKey(udid))
                    {
                        domainDevices.Add(udid, device.m_deviceFamilyID);
                        CatalogDAL.SaveDomainDevices(domainDevices, domainId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddDeviceToDomain - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", domainId, udid, ex);
            }

            return responseStatus;
        }

        private static void GetDeviceIdAndBrandByPin(int groupId, string pin, ref string udid, ref int brandId)
        {
            if (CanaryDeploymentFactory.Instance.GetMicroservicesCanaryDeploymentManager().IsDataOwnershipFlagEnabled(groupId, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginPin))
            {
                var authClient = AuthenticationClient.GetClientFromTCM();
                udid = authClient.GetDeviceLoginPin(groupId, pin);

                if (string.IsNullOrEmpty(udid))
                {
                    log.Warn($"Failed getting device by PIN {pin} on partner {groupId}");
                }
                else
                {
                    brandId = DomainDal.GetDeviceBrandIdByUdid(groupId, udid);
                }
            }
            else
            {
                DomainDal.GetDeviceIdAndBrandByPin(pin, groupId, ref udid, ref brandId);
            }
        }

        public DomainResponseStatus ResetDomain(int nFreqencyType = 0)
        {
            bool res = false;
            if (m_DomainStatus != DomainStatus.DomainSuspended || m_DomainStatus == DomainStatus.DomainSuspended && RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(m_nGroupID, m_masterGUIDs[0]))
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
            oDomainCache.RemoveDomain(m_nGroupID, m_nDomainID);
            InvalidateDomain();

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

            try
            {
                if (bRemoveDomain)
                {
                    DomainsCache oDomainCache = DomainsCache.Instance();
                    oDomainCache.RemoveDomain(nGroupID, m_nDomainID);
                    InvalidateDomain();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SubmitAddDeviceToDomainRequest - Failed to remove domain from cache : m_nDomainID= {0}, UDID= {1}, ex= {2}", m_nDomainID, sDeviceUdid, ex);
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

            //BEO-4478
            if (m_DomainStatus == DomainStatus.DomainSuspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(nGroupID, m_masterGUIDs[0]))
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
                oDomainCache.RemoveDomain(nGroupID, m_nDomainID);
                InvalidateDomain();
                InvalidateDomainUser(nCurrentMasterID.ToString());
                InvalidateDomainUser(nNewMasterID.ToString());

                return DomainResponseStatus.OK;
            }
            else
            {
                return DomainResponseStatus.Error;
            }
        }

        public DomainResponseStatus ValidateFrequency(string udid)
        {
            // check if the frequency assigned to the device family is 0 - in that case the device family is excluded from global DLM policy
            if (DeviceFamiliesMapping != null)
            {
                DeviceContainer deviceFamily = GetDeviceContainerByUdid(udid);

                if (deviceFamily != null && deviceFamily.m_oLimitationsManager != null && deviceFamily.m_oLimitationsManager.Frequency == 0)
                {
                    return DomainResponseStatus.OK;
                }
            }

            // check DLM device frequency
            if (m_oLimitationsManager.NextActionFreqDate > DateTime.UtcNow)
            {
                return DomainResponseStatus.LimitationPeriod;
            }

            return DomainResponseStatus.OK;
        }

        private int GetDeviceFrequency(string sUDID)
        {
            // check if the frequency assigned to the device family is 0 - in that case the device family is excluded from global DLM policy
            if (DeviceFamiliesMapping != null)
            {
                DeviceContainer deviceFamily = GetDeviceContainerByUdid(sUDID);

                if (deviceFamily != null && deviceFamily.m_oLimitationsManager != null && deviceFamily.m_oLimitationsManager.Frequency != -1)
                {
                    return deviceFamily.m_oLimitationsManager.Frequency;
                }
            }

            return m_oLimitationsManager.Frequency;
        }

        public DomainResponseStatus ValidateQuantity(string udid, int deviceBrandId)
        {
            DeviceContainer dc = null;
            Device device = null;
            return ValidateQuantity(udid, deviceBrandId, ref dc, ref device);
        }

        public DomainResponseStatus ValidateQuantity(string udid, int deviceBrandId, ref DeviceContainer dc, ref Device device, 
            bool skipOtherDomainCheck = false)
        {
            DomainResponseStatus res = DomainResponseStatus.UnKnown;
            if (device == null)
                device = new Device(udid, deviceBrandId, m_nGroupID);
            if (dc == null)
                dc = GetDeviceContainerByFamilyId(device.m_deviceFamilyID);
            if (dc == null)
            {
                // device type not allowed for this domain
                res = DomainResponseStatus.DeviceTypeNotAllowed;
            }
            else
            {
                var bIsDeviceActivated = false;
                if (dc.IsContainingDevice(device, ref bIsDeviceActivated))
                {
                    // device is associated to this domain and activated
                    res = DomainResponseStatus.DeviceAlreadyExists;
                    if (!bIsDeviceActivated)
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
                        if (DeviceDal.GetDeviceIdByUDID(device.m_deviceUDID, m_nGroupID) > 0)
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

                    if (deviceAlreadyExistsInOtherDomain && !skipOtherDomainCheck)
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
                m_oLimitationsManager.NextActionFreqDate = Core.ConditionalAccess.Utils.GetEndDateTime(dtLastActionDate, nDeviceFrequencyLimit);
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

            if (DeviceFamiliesMapping == null)
            {
                DeviceFamiliesMapping = new Dictionary<int, DeviceContainer>(dbDeviceFamilies.Count);
            }

            foreach (string[] currDeviceFamily in dbDeviceFamilies)
            {
                int currDeviceFamilyId = string.IsNullOrEmpty(currDeviceFamily[0]) ? 0 : Int32.Parse(currDeviceFamily[0]);
                string currDeviceFamilyName = currDeviceFamily[1];

                int overrideConcurrencyLimit = -1;
                if (concurrencyOverride != null && concurrencyOverride.Count > 0 && concurrencyOverride.ContainsKey(currDeviceFamilyId))
                {
                    overrideConcurrencyLimit = concurrencyOverride[currDeviceFamilyId];
                }

                int overrideQuantityLimit = -1;
                if (quantityOverride != null && quantityOverride.Count > 0 && quantityOverride.ContainsKey(currDeviceFamilyId))
                {
                    overrideQuantityLimit = quantityOverride[currDeviceFamilyId];
                }

                DeviceContainer currDeviceContainer = new DeviceContainer(currDeviceFamilyId,
                    currDeviceFamilyName,
                    overrideQuantityLimit > -1 ? overrideQuantityLimit : m_oLimitationsManager.Quantity,
                    overrideConcurrencyLimit > -1 ? overrideConcurrencyLimit : m_oLimitationsManager.Concurrency,
                    m_oLimitationsManager.Frequency);

                this.InitDeviceFamily(currDeviceContainer, currDeviceFamilyId);
            }
        }

        public int GetDeviceList()
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
            InitializeDomainDevicesData(dt, m_nGroupID);

            return m_totalNumOfDevices;
        }

        //This function also changes state and not just gets data 
        //be careful when removing calls to this function!
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

        public DeviceContainer GetDeviceContainerByFamilyId(int deviceFamilyId)
        {
            InitDeviceFamilyMapping();

            if (DeviceFamiliesMapping.ContainsKey(deviceFamilyId))
                return DeviceFamiliesMapping[deviceFamilyId];

            // old code
            if (m_deviceFamilies != null)
            {
                var currDeviceFamily = m_deviceFamilies.FirstOrDefault(x => x.m_deviceFamilyID == deviceFamilyId);
                if (currDeviceFamily != null)
                {
                    MapIdToDeviceFamily(deviceFamilyId, currDeviceFamily);
                    return currDeviceFamily;
                }
            }

            return null;
        }

        public DeviceContainer GetDeviceContainerByUdid(string udid)
        {
            InitDeviceFamilyMapping();

            if (UdidToDeviceFamilyIdMapping.ContainsKey(udid))
            {
                int deviceFamily = UdidToDeviceFamilyIdMapping[udid];
                if (DeviceFamiliesMapping.ContainsKey(deviceFamily))
                {
                    return DeviceFamiliesMapping[deviceFamily];
                }
            }

            // old code
            if (m_deviceFamilies != null)
            {
                var currDeviceFamily = m_deviceFamilies.FirstOrDefault(x => x.DeviceInstances.Any(y => y.m_deviceUDID == udid));

                if (currDeviceFamily != null)
                {
                    MapIdToDeviceFamily(currDeviceFamily.m_deviceFamilyID, currDeviceFamily);
                    MapUdidToDeviceFamilyId(udid, currDeviceFamily.m_deviceFamilyID);

                    return currDeviceFamily;
                }
            }

            return null;
        }

        private void InitDeviceFamilyMapping()
        {
            if (DeviceFamiliesMapping == null)
            {
                DeviceFamiliesMapping = new Dictionary<int, DeviceContainer>();
            }

            if (UdidToDeviceFamilyIdMapping == null)
            {
                UdidToDeviceFamilyIdMapping = new Dictionary<string, int>();
            }
        }

        private void MapUdidToDeviceFamilyId(string udid, int deviceFamilyId)
        {
            InitDeviceFamilyMapping();

            if (!string.IsNullOrEmpty(udid) && deviceFamilyId > 0 && !UdidToDeviceFamilyIdMapping.ContainsKey(udid))
            {
                UdidToDeviceFamilyIdMapping.Add(udid, deviceFamilyId);
            }
        }

        private void MapIdToDeviceFamily(int deviceFamilyId, DeviceContainer deviceFamily)
        {
            InitDeviceFamilyMapping();

            if (deviceFamilyId > 0 && !DeviceFamiliesMapping.ContainsKey(deviceFamilyId))
            {
                DeviceFamiliesMapping.Add(deviceFamilyId, deviceFamily);
            }
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
            bool res = true;

            var dr = DomainDal.GetDomainSettings(nDomainID, nGroupID);
            if (dr == null)
            {
                this.m_DomainStatus = DomainStatus.DomainNotExists;
                return res;
            }

            var status = ODBCWrapper.Utils.GetIntSafeVal(dr, "STATUS");
            if (status != 1)
            {
                this.m_DomainStatus = DomainStatus.DomainNotExists;
                return res;
            }

            this.m_nStatus = status;
            this.m_sName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
            this.m_sDescription = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
            this.m_nLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "MODULE_ID");
            this.m_nDeviceLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "MAX_LIMIT");
            this.m_nUserLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "USER_MAX_LIMIT");
            this.m_nConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "CONCURRENT_MAX_LIMIT");
            this.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_ACTIVE");
            this.m_frequencyFlag = ODBCWrapper.Utils.GetIntSafeVal(dr, "FREQUENCY_FLAG");
            this.m_minPeriodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "freq_period_id");
            this.m_minUserPeriodId = ODBCWrapper.Utils.GetIntSafeVal(dr, "user_freq_period_id");
            this.m_sCoGuid = ODBCWrapper.Utils.GetSafeStr(dr, "COGUID");
            this.m_nRegion = ODBCWrapper.Utils.GetIntSafeVal(dr, "REGION_ID");
            this.m_DomainRestriction = (DomainRestriction) ODBCWrapper.Utils.GetIntSafeVal(dr, "RESTRICTION");            
			int? roleId = ODBCWrapper.Utils.GetNullableInt(dr, "ROLE_ID");
            this.roleId = roleId.HasValue && roleId.Value == 0 ? null : roleId;			
            this.CreateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE");
            this.UpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE");
            var nGroupConcurrentLimit = ODBCWrapper.Utils.GetIntSafeVal(dr, "GROUP_CONCURRENT_MAX_LIMIT");
            int suspendStatInt = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_SUSPENDED");

            if (Enum.IsDefined(typeof(DomainSuspentionStatus), suspendStatInt))
            {
                if ((DomainSuspentionStatus) suspendStatInt == DomainSuspentionStatus.Suspended)
                {
                    m_DomainStatus = DomainStatus.DomainSuspended;
                }
            }

            if (m_minPeriodId != 0)
            {
                m_NextActionFreq = Core.ConditionalAccess.Utils.GetEndDateTime(ODBCWrapper.Utils.GetDateSafeVal(dr, "FREQUENCY_LAST_ACTION"), m_minPeriodId);
            }

            if (m_minUserPeriodId != 0)
            {
                m_NextUserActionFreq = Core.ConditionalAccess.Utils.GetEndDateTime(ODBCWrapper.Utils.GetDateSafeVal(dr, "USER_FREQUENCY_LAST_ACTION"), m_minUserPeriodId);
            }

            long npvrQuotaInSecs = 0;
            npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, this.m_nLimit, nGroupID, m_NextActionFreq);

            return res;
        }

        private void InitializeDomainDevicesData(DataTable dt, int groupID)
        {
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                string sUDID = string.Empty;
                int nDeviceBrandID = 0;
                string sDeviceName = string.Empty;
                int nDeviceFamilyID = 0;
                string sPin = string.Empty;
                DateTime dtActivationDate = Utils.FICTIVE_DATE;
                DateTime dtUpdateDate = Utils.FICTIVE_DATE;
                DeviceState eState = DeviceState.UnKnown;
                int nDeviceID = 0;
                string externalId = string.Empty;
                List<ApiObjects.KeyValuePair> dynamicData = null;
                string macAddress = string.Empty;
                string model = string.Empty;
                long? manufacturerId = null;
                string manufacturer = string.Empty;

                Dictionary<string, int> domainDevices = new Dictionary<string, int>();

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
                    dtUpdateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["update_date"]);
                    nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["device_id"]);
                    externalId = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i], "external_id");
                    dynamicData = DeviceDal.DeserializeDynamicData(ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["dynamic_data"]));
                    macAddress = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i], "mac_address");
                    model = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i], "model");
                    manufacturerId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "manufacturer_id");
                    if (manufacturerId.HasValue && manufacturerId.Value > 0)
                    {
                        var deviceReferenceData = DeviceReferenceDataManager.Instance.GetByManufacturerId(groupID, manufacturerId.Value);
                        manufacturer = deviceReferenceData?.Name;
                    }

                    Device device = new Device(sUDID, nDeviceBrandID, m_nGroupID, sDeviceName, m_nDomainID, nDeviceID, nDeviceFamilyID, string.Empty, sPin,
                        dtActivationDate, eState, dtUpdateDate);

                    if (!string.IsNullOrEmpty(externalId))
                        device.ExternalId = externalId;
                    if (!string.IsNullOrEmpty(macAddress))
                        device.MacAddress = macAddress;
                    if (dynamicData != null) device.DynamicData = dynamicData;
                    if (!string.IsNullOrEmpty(model))
                        device.Model = model;
                    if (manufacturerId.HasValue)
                        device.ManufacturerId = manufacturerId.Value;
                    if (!string.IsNullOrEmpty(manufacturer))
                        device.Manufacturer = manufacturer;

                    if (AddDeviceToContainer(device))
                    {
                        MapUdidToDeviceFamilyId(device.m_deviceUDID, device.m_deviceFamilyID);
                        IncrementDeviceCount(device);
                    }

                    if (bIsActiveInDevices && bIsActiveInDomainsDevices && !domainDevices.ContainsKey(device.m_deviceUDID))
                    {
                        domainDevices.Add(device.m_deviceUDID, device.m_deviceFamilyID);
                    }
                }

                CatalogDAL.SaveDomainDevices(domainDevices, this.m_nDomainID);
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
                if (DeviceFamiliesMapping.ContainsKey(device.m_deviceFamilyID) && !DeviceFamiliesMapping[device.m_deviceFamilyID].IsUnlimitedQuantity())
                {
                    m_totalNumOfDevices++;
                }
            }
        }

        private bool AddDeviceToContainer(Device device)
        {
            bool res = false;
            DeviceContainer dc = GetDeviceContainerByFamilyId(device.m_deviceFamilyID);
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
            var res = DomainResponseStatus.OK;

            var totalDevicesLimitReached = dc.m_oLimitationsManager.Quantity > 0 && m_oLimitationsManager.Quantity > 0 && m_totalNumOfDevices >= m_oLimitationsManager.Quantity; // m_oLimitationsManager.Quantity == 0 is unlimited 
            var containerDevicesLimitReached = dc.m_oLimitationsManager.Quantity > 0 && dc.GetActivatedDeviceCount() >= dc.m_oLimitationsManager.Quantity;

            if (totalDevicesLimitReached || containerDevicesLimitReached)
            {
                res = DomainResponseStatus.ExceededLimit;

                if (!GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfiguration(m_nGroupID).HasObjects())
                    return res;

                var generalPartnerConfig = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfiguration(m_nGroupID).Objects.FirstOrDefault();
                if (generalPartnerConfig?.RollingDeviceRemovalData.RollingDeviceRemovalPolicy != null &&
                    generalPartnerConfig.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds?.Any(x => x == dc.m_deviceFamilyID) == true)
                {
                    return TryRemoveHouseholdDevice(generalPartnerConfig.RollingDeviceRemovalData.RollingDeviceRemovalPolicy.Value, dc.m_deviceFamilyID);
                }
            }

            return res;
        }

        private DomainResponseStatus TryRemoveHouseholdDevice(RollingDevicePolicy rollingDeviceRemovalPolicy, int rollingDeviceRemovalFamilyId)
        {
            //remove by policy based on the dates
            var udid = DeviceRemovalPolicyHandler.Instance.GetDeviceRemovalCandidate(m_nGroupID, rollingDeviceRemovalPolicy, rollingDeviceRemovalFamilyId, m_deviceFamilies);

            if (udid.IsNullOrEmptyOrWhiteSpace())
            {
                return DomainResponseStatus.ExceededLimit;
            }

            //call to remove household device
            var tryRemoveHouseholdDevice = RemoveDeviceFromDomain(udid, true);

            if (tryRemoveHouseholdDevice != DomainResponseStatus.OK) return tryRemoveHouseholdDevice;
            foreach (var usersId in m_UsersIDs)
            {
                SessionManager.SessionManager.UpdateUsersSessionsRevocationTime(m_nGroupID, string.Empty, 0, 0, usersId.ToString(), udid, DateUtils.GetUtcUnixTimestampNow(), 0);
            }

            return tryRemoveHouseholdDevice;
        }

        public List<DevicePlayData> GetConcurrentCount(string udid, ref int concurrentDeviceFamilyIdCount, int deviceFamilyId)
        {
            MapUdidToDeviceFamilyId(udid, deviceFamilyId);
            concurrentDeviceFamilyIdCount = 0;

            bool shouldExcludeFreeContentFromConcurrency = 
                api.GetShouldExcludeFreeContentFromConcurrency(this.m_nGroupID);

            List<DevicePlayData> streamingDevicePlayData = new List<DevicePlayData>();
            List<DevicePlayData> devicePlayDataList =
                CatalogDAL.GetDevicePlayDataList(Api.api.Instance.GetDomainDevices(this.m_nDomainID, this.m_nGroupID),
                    new List<ePlayType>() {ePlayType.NPVR, ePlayType.MEDIA, ePlayType.EPG},
                    ConcurrencyManager.GetConcurrencyMillisecThreshold(this.m_nGroupID), udid, 
                    shouldExcludeFreeContentFromConcurrency);

            if (devicePlayDataList != null)
            {
                foreach (DevicePlayData devicePlayData in devicePlayDataList)
                {
                    MapUdidToDeviceFamilyId(devicePlayData.UDID, devicePlayData.DeviceFamilyId);

                    if (devicePlayData.DeviceFamilyId > 0)
                    {
                        if (!IsAgnosticToDeviceLimitation(ValidationType.Concurrency, devicePlayData.DeviceFamilyId))
                        {
                            // we have device family id and its not agnostic to limitation. increment its value in the result dictionary.
                            if (devicePlayData.DeviceFamilyId == deviceFamilyId)
                            {
                                concurrentDeviceFamilyIdCount++;
                            }

                            streamingDevicePlayData.Add(devicePlayData);
                        }
                    }
                }
            }

            return streamingDevicePlayData;
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

            Device retDevice = null;
            if (UdidToDeviceFamilyIdMapping.ContainsKey(udid) && DeviceFamiliesMapping.ContainsKey(UdidToDeviceFamilyIdMapping[udid]))
            {
                DeviceContainer dc = DeviceFamiliesMapping[UdidToDeviceFamilyIdMapping[udid]];

                retDevice = dc.DeviceInstances.FirstOrDefault(x => x.m_deviceUDID.Equals(udid));
                if (retDevice != null)
                {
                    cont = dc;
                }

                return retDevice;
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
                                retDevice = device;
                                cont = container;
                                break;
                            }
                        }
                    }
                }
            }

            return retDevice;
        }

        public bool IsAgnosticToDeviceLimitation(ValidationType validationType, int deviceFamilyId)
        {
            bool res = false;
            switch (validationType)
            {
                case ValidationType.Concurrency:
                {
                    res = DeviceFamiliesMapping != null &&
                          DeviceFamiliesMapping.ContainsKey(deviceFamilyId) &&
                          DeviceFamiliesMapping[deviceFamilyId].IsUnlimitedConcurrency();
                    break;
                }
                case ValidationType.Quantity:
                {
                    res = DeviceFamiliesMapping != null &&
                          DeviceFamiliesMapping.ContainsKey(deviceFamilyId) &&
                          DeviceFamiliesMapping[deviceFamilyId].IsUnlimitedQuantity();
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
            long deviceID = 0;
            long nDeviceDomainRecordID = 0;

            // Now let's see which domain the device belongs to
            int nDeviceDomainID = DomainDal.Instance.GetDeviceDomainData(nGroupID, sDeviceUdid, ref deviceID, ref isActive, ref status, ref nDeviceDomainRecordID);

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

            if (isActive == 3) // device pending activation in this or other domain; reset this association
            {
                int rowsAffected = DomainDal.SetDeviceStatusInDomain(deviceID, this.m_nDomainID, nGroupID, nDeviceDomainRecordID, 2, 2);
                if (rowsAffected > 0)
                {
                    bRemoveDomain = true;
                }
            }

            //Check if exceeded limit for users
            DeviceContainer container = GetDeviceContainerByFamilyId(device.m_deviceFamilyID);

            //Check if exceeded limit for the device type
            DomainResponseStatus responseStatus = ValidateQuantity(sDeviceUdid, device.m_deviceBrandID, ref container, ref device);
            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed || responseStatus == DomainResponseStatus.DeviceAlreadyExists)
            {
                return responseStatus;
            }

            // Get row id from devices table (not udid)
            device.m_domainID = this.m_nDomainID;
            deviceID = device.Save(0, 3, null, device.MacAddress, device.ExternalId, device.Model, device.ManufacturerId, device.Manufacturer, device.DynamicData);
            bRemoveDomain = true;

            string sActivationToken = Guid.NewGuid().ToString();

            DomainDevice domainDevice = new DomainDevice()
            {
                ActivataionStatus = DeviceState.Pending,
                ActivationToken = sActivationToken,
                DeviceBrandId = device.m_deviceBrandID,
                DeviceId = deviceID,
                DomainId = device.m_domainID,
                Name = device.m_deviceName,
                Udid = sDeviceUdid,
                GroupId = m_nGroupID,
                DeviceFamilyId = device.m_deviceFamilyID,
                MacAddress = device.MacAddress,
                ExternalId = device.ExternalId,
                Model = device.Model,
                Manufacturer = device.Manufacturer,
                DynamicData = device.DynamicData
            };

            bool domainDeviceInsertSuccess = domainDevice.Insert();

            //nDeviceDomainRecordID = DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 3, 3, sActivationToken);

            if (domainDeviceInsertSuccess && domainDevice.Id > 0)
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
                    m_UsersIDs = dbTypedUserIDs.Where(ut => ut.Value != (int) UserDomainType.Household).Select(ut => ut.Key).ToList();
                    m_masterGUIDs = dbTypedUserIDs.Where(ut => ut.Value == (int) UserDomainType.Master).Select(ut => ut.Key).ToList();
                    m_DefaultUsersIDs = dbTypedUserIDs.Where(ut => ut.Value == (int) UserDomainType.Household).Select(ut => ut.Key).ToList();

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

            //BEO-4478
            if (m_DomainStatus == DomainStatus.DomainSuspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(m_nGroupID, m_masterGUIDs[0]))
            {
                if (roleId == 0 || (m_masterGUIDs != null && m_masterGUIDs.Count > 0
                                                          && !RolesPermissionsManager.Instance.IsPermittedPermissionItem(m_nGroupID, m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDUSER_ADD.ToString())))
                {
                    bRemove = false;
                    return DomainResponseStatus.DomainSuspended;
                }
            }

            // If domain has no users, insert new Master user
            int status = 1;
            int isActive = 1;

            if ((dbTypedUserIDs == null || dbTypedUserIDs.Count == 0) && (nUserID == nMasterUserGuid))
            {
                int inserted = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int) userType, status, isActive, nMasterUserGuid);

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
            List<int> masterUserIDs = dbTypedUserIDs.Where(ut => ut.Value == (int) UserDomainType.Master).Select(ut => ut.Key).ToList();

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
            if (nUserDomainID > 0) // If user exists, update its status to active
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

            int inserted1 = DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int) userType, status, isActive, nMasterUserGuid);

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

            if (masterUser.m_eSuspendState == DomainSuspentionStatus.Suspended && !RolesPermissionsManager.Instance.AllowActionInSuspendedDomain(m_nGroupID, masterUser.Id))
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
            Domain domain = oDomainCache.GetDomain(nDomainID, nGroupID);
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
                            sNewUsername = UserDataEncryptor.Instance().DecryptUsername(nGroupID, sNewUsername);
                            sNewFirstName = dtUserBasicData.DefaultView[0].Row["FIRST_NAME"].ToString();
                            sNewEmail = dtUserBasicData.DefaultView[0].Row["EMAIL_ADD"].ToString();
                        }
                    }
                }
            }

            // Add the new user to the domain
            int isMaster = 0;
            int status = 3; // Pending
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
                    saved = uBasic.Save(nUserID, nGroupID);

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

        private bool IsDomainRemovedSuccessfully(int statusRes)
        {
            return statusRes == 2;
        }

        /// <summary>
        /// This method get NPVRConcurrencyLimit (int) , domain and npvrID
        /// Get from CB all media play at the last 
        /// </summary>
        /// <param name="npvrConcurrencyLimit"></param>
        /// <param name="domainId"></param>
        /// <param name="npvr"></param>
        /// <returns></returns>
        internal DomainResponseStatus ValidateNpvrConcurrency(int npvrConcurrencyLimit, long domainId, string npvr, string udid)
        {
            try
            {
                if (npvrConcurrencyLimit == 0)
                {
                    // get limitation from DB ( get it from domain / group table - wait for future implementation)
                }

                if (npvrConcurrencyLimit > 0) // check concurrency only if limitation  > 0 
                {
                    int concurrencyMillisecThreshold = ConcurrencyManager.GetConcurrencyMillisecThreshold(this.m_nGroupID);
                    List<DevicePlayData> devicePlayDataList =
                        CatalogDAL.GetDevicePlayDataList(Api.api.Instance.GetDomainDevices((int) domainId, this.m_nGroupID),
                            new List<ePlayType>() {ePlayType.ALL},
                            concurrencyMillisecThreshold, udid,
                            api.GetShouldExcludeFreeContentFromConcurrency(this.m_nGroupID));

                    if (devicePlayDataList != null)
                    {
                        int mediaConcurrencyCount = devicePlayDataList.Count(c =>
                            c.NpvrId == npvr &&
                            DateUtils.UtcUnixTimestampSecondsToDateTime(c.TimeStamp).AddMilliseconds(concurrencyMillisecThreshold) > DateTime.UtcNow);
                        if (mediaConcurrencyCount >= npvrConcurrencyLimit)
                        {
                            return DomainResponseStatus.MediaConcurrencyLimitation;
                        }
                    }
                }

                return DomainResponseStatus.OK;
            }
            catch (Exception ex)
            {
                log.Error("ValidateNpvrConcurrency - " + String.Concat("Failed ex={0}, nNpvrConcurrencyLimit={1}, lDomainID={2}, sNPVR={3}",
                    ex.Message, npvrConcurrencyLimit, domainId, npvr), ex);
                throw;
            }
        }

        internal void SetDeviceFamiliesMapping(List<DeviceContainer> dc)
        {
            this.m_deviceFamilies = dc;
        }

        internal bool CompareDLM(LimitationsManager oLimitationsManager, ref ChangeDLMObj dlmObjectToChange)
        {
            try
            {
                if (oLimitationsManager != null) // initialize all fields 
                {
                    #region Devices
                    DeviceContainer oldDeviceFamilyContainer = new DeviceContainer();
                    List<Device> devicesToRemove = new List<Device>();
                    GenericListResponse<GeneralPartnerConfig> PartnerConfigResponse = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfiguration(GroupId);
                    if (!PartnerConfigResponse.HasObjects() || !PartnerConfigResponse.IsOkStatusCode())
                    {
                        dlmObjectToChange.resp = PartnerConfigResponse.Status;
                        return false;
                    }

                    GeneralPartnerConfig generalPartnerConfig = PartnerConfigResponse.Objects[0];
                    // Get group downgrade policy for desc/Asc/ACTIVE_DATE
                    DowngradePolicy downgradePolicy = (DowngradePolicy)generalPartnerConfig.DowngradePolicy;
                    
                    foreach (KeyValuePair<int, DeviceContainer> currentItem in DeviceFamiliesMapping) // all keys not exists in new DLM - Delete
                    {
                        oldDeviceFamilyContainer = DeviceFamiliesMapping[currentItem.Value.m_deviceFamilyID];
                        List<Device> devices = oldDeviceFamilyContainer.DeviceInstances.FindAll( d=> d.IsActivated());
                        if (!oLimitationsManager.lDeviceFamilyLimitations.Exists(x =>
                            x.deviceFamily == currentItem.Value.m_deviceFamilyID))
                        {
                            if (devices.Count > 0)
                            {
                                devicesToRemove.AddRange(devices);
                                var devicesUDID = devices.Select(d => d.m_deviceUDID).ToList();
                                foreach (var deviceUDID in devicesUDID)
                                {
                                    oldDeviceFamilyContainer.RemoveDeviceInstance(deviceUDID);
                                }
                            }
                        }
                        else
                        {
                            foreach (var device in devices)
                            {
                                device.LastActivityTime = DeviceRemovalPolicyHandler.Instance.GetUdidLastActivity(this.m_nGroupID, device.m_deviceUDID);

                            }
                        }
                    }
                    
                    foreach (DeviceFamilyLimitations item in oLimitationsManager.lDeviceFamilyLimitations)
                    {
                        if (DeviceFamiliesMapping.ContainsKey(item.deviceFamily))
                        {
                            oldDeviceFamilyContainer = DeviceFamiliesMapping[item.deviceFamily];
                            if (oldDeviceFamilyContainer != null && oldDeviceFamilyContainer.m_oLimitationsManager != null)
                            {
                                // quantity of the new dlm is less than the current dlm only if new dlm is <> 0 (0 = unlimited)
                                if (oldDeviceFamilyContainer.m_oLimitationsManager.Quantity > item.quantity && item.quantity != 0) // need to delete the laset devices
                                {
                                    List<Device> devices = oldDeviceFamilyContainer.DeviceInstances.FindAll( d=> d.IsActivated());
                                    if (devices != null && devices.Count > 0 && devices.Count > item.quantity)
                                    {
                                        devices = getOrderByDowngradePolicyDevices(devices, downgradePolicy);
                                        int amountToDelete = devices.Count - item.quantity;
                                        var devicesToDelete = devices.Take(amountToDelete);
                                        devicesToRemove.AddRange(devicesToDelete);
                                        
                                        var devicesUDID = devicesToDelete.Select(d => d.m_deviceUDID).ToList();
                                        foreach (var deviceUDID in devicesUDID)
                                        {
                                            oldDeviceFamilyContainer.RemoveDeviceInstance(deviceUDID);
                                        }
                                    }
                                }
                                
                                // If the device family become unlimited dont remove the actual devices but delete them from the counter 
                                if (item.quantity == 0 && !oldDeviceFamilyContainer.IsUnlimitedQuantity())
                                {
                                    m_totalNumOfDevices -= oldDeviceFamilyContainer.DeviceInstances.FindAll(d=> d.IsActivated()).Count;
                                }
                            }
                        }
                    }
                    
                    // compare the total quntity of this domain 
                    if (this.m_oLimitationsManager.Quantity > oLimitationsManager.Quantity && oLimitationsManager.Quantity != 0 && (m_totalNumOfDevices - devicesToRemove.Count) > oLimitationsManager.Quantity)
                    {
                        int countToDelete = m_totalNumOfDevices - devicesToRemove.Count - oLimitationsManager.Quantity;
                        var priority = generalPartnerConfig.DowngradePriorityFamilyIds;
                        var deviceFamilyLimitations = oLimitationsManager.lDeviceFamilyLimitations.Where(x => x.quantity != 0).ToList();
                        if (priority != null && priority.Count > 0)
                        {
                            for (int i = 0; i < priority.Count && countToDelete > 0; i++)
                            {
                                if (deviceFamilyLimitations.Exists(x =>
                                    x.deviceFamily == priority[i]))
                                {
                                    oldDeviceFamilyContainer = DeviceFamiliesMapping[priority[i]];
                                    if (oldDeviceFamilyContainer != null && oldDeviceFamilyContainer.m_oLimitationsManager != null)
                                    {
                                        List<Device> devices = oldDeviceFamilyContainer.DeviceInstances.FindAll( d=> d.IsActivated());
                                        if (devices.Count > 0)
                                        {
                                            if (countToDelete > devices.Count)
                                            {
                                                countToDelete -= devices.Count;
                                                devicesToRemove.AddRange(devices);
                                               var devicesUDID = devices.Select(d => d.m_deviceUDID).ToList();
                                               foreach (var deviceUDID in devicesUDID)
                                               {
                                                   oldDeviceFamilyContainer.RemoveDeviceInstance(deviceUDID);
                                               }
                                            } else
                                            {
                                                devices = getOrderByDowngradePolicyDevices(devices, downgradePolicy);
                                                var devicesToDelete = devices.Take(countToDelete);
                                                devicesToRemove.AddRange(devicesToDelete);
                                                var devicesUDID = devicesToDelete.Select(d => d.m_deviceUDID).ToList();
                                                foreach (var deviceUDID in devicesUDID)
                                                {
                                                    oldDeviceFamilyContainer.RemoveDeviceInstance(deviceUDID);
                                                }

                                                countToDelete = 0;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if(countToDelete > 0)
                        {
                            List<Device> allDevices = new List<Device>();
                            foreach (var deviceFamilyLimitation in deviceFamilyLimitations)
                            {
                                if (DeviceFamiliesMapping.ContainsKey(deviceFamilyLimitation.deviceFamily))
                                {
                                    allDevices.AddRange((DeviceFamiliesMapping[deviceFamilyLimitation.deviceFamily].DeviceInstances.FindAll(x=> x.IsActivated())));
                                }
                            }

                            allDevices = getOrderByDowngradePolicyDevices(allDevices, downgradePolicy);
                            devicesToRemove.AddRange(allDevices.Take(countToDelete));
                        }
                    }

                    if (devicesToRemove.Count > 0)
                    {
                        var devicesIdsToRemove = devicesToRemove.Select(x => int.Parse(x.m_id)).ToList();
                        if (DomainDal.SetDevicesDomainUnActive(this.m_nDomainID, devicesIdsToRemove) == 0)
                        {
                            dlmObjectToChange.resp = new ApiObjects.Response.Status((int) eResponseStatus.Error, string.Empty);
                            return false;
                        }
                        foreach (Device device in devicesToRemove)
                        {
                            if (device != null && !string.IsNullOrEmpty(device.m_deviceUDID))
                                Utils.AddInitiateNotificationActionToQueue(this.GroupId, eUserMessageAction.DeleteDevice, 0, device.m_deviceUDID);
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
                                dlmObjectToChange.users.AddRange(users);
                            }
                        }
                    }

                    #endregion
                }

                // change dlmid in domain table 
                bool bChangeDoamin = DomainDal.ChangeDomainDLM(this.m_nDomainID, oLimitationsManager.domianLimitID);

                dlmObjectToChange.resp = new ApiObjects.Response.Status((int) eResponseStatus.OK, string.Empty);
                return true;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                dlmObjectToChange.resp = new ApiObjects.Response.Status((int) eResponseStatus.Error, string.Empty);
                return false;
            }
        }

        private List<Device> getOrderByDowngradePolicyDevices(List<Device> devices, DowngradePolicy downgradePolicy)
        {
            switch(downgradePolicy)
            {
                case DowngradePolicy.FIFO:
                {
                    devices = devices.OrderBy(x => x.m_updateDate).ToList();
                    break;
                }
                case DowngradePolicy.LIFO:
                {
                    devices = devices.OrderByDescending(x => x.m_updateDate).ToList();
                    break;
                }
                case DowngradePolicy.ACTIVE_DATE:
                {
                    devices = devices.OrderBy(x => x.LastActivityTime).ToList();
                    break;
                }
            }

            return devices;
        }

        protected override bool DoInsert()
        {
            bool success = false;
            DateTime dDateTime = DateTime.UtcNow;

            // try to get the DomainLimitID
            int nDomainID = -1;
            this.m_nLimit = DomainDal.Get_DomainLimitID(this.m_nGroupID); // the id for GROUPS_DEVICE_LIMITATION_MODULES table 
            bool bInserRes =
                DomainDal.InsertNewDomain(this.m_sName, this.m_sDescription, this.m_nGroupID, dDateTime, this.m_nLimit, ref nDomainID, m_nRegion, this.m_sCoGuid);

            if (!bInserRes)
            {
                m_DomainStatus = DomainStatus.Error;
                return success;
            }

            var dr = DomainDal.GetDomainDbObject(this.m_nGroupID, nDomainID);
            if (dr != null)
            {
                this.m_sName = ODBCWrapper.Utils.GetSafeStr(dr, "name");
                this.m_sDescription = ODBCWrapper.Utils.GetSafeStr(dr, "description");
                this.m_nDomainID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                this.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(dr, "is_active");
                this.m_nStatus = ODBCWrapper.Utils.GetIntSafeVal(dr, "status");
                this.m_sCoGuid = ODBCWrapper.Utils.GetSafeStr(dr, "CoGuid");
                this.m_nRegion = ODBCWrapper.Utils.GetIntSafeVal(dr, "Region_ID");
            }

            this.CreateDate = dDateTime;
            this.UpdateDate = dDateTime;

            // try to get from chace - DomainLimitID by nDomainLimitID
            long npvrQuotaInSecs = 0;
            npvrQuotaInSecs = InitializeDLM(npvrQuotaInSecs, this.m_nLimit, m_nGroupID, Utils.FICTIVE_DATE);

            m_DomainStatus = DomainStatus.OK;

            m_UsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();

            DomainResponseStatus res = AddUserToDomain(m_nGroupID, m_nDomainID, this.MasterGuID, this.MasterGuID, UserDomainType.Master);

            if (res == DomainResponseStatus.OK)
            {
                m_UsersIDs = new List<int>();
                m_UsersIDs.Add(this.MasterGuID);
                m_totalNumOfUsers++;
                success = true;
            }

            m_DomainStatus = CreateNpvrAccount( m_nGroupID, m_nDomainID, npvrQuotaInSecs);

            return success;
        }

        public static DomainStatus CreateNpvrAccount(int m_nGroupID, int m_nDomainID, long npvrQuotaInSecs)
        {
            INPVRProvider npvr;
            if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID, out npvr, null) && npvr.SynchronizeNpvrWithDomain && Utils.IsServiceAllowed(m_nGroupID, m_nDomainID, eService.NPVR))
            {
                try
                {
                    NPVRUserActionResponse resp = npvr.CreateAccount(new NPVRParamsObj() {EntityID = m_nDomainID.ToString(), Quota = npvrQuotaInSecs});
                    if (resp != null)
                    {
                        if (resp.isOK)
                        {
                            return DomainStatus.OK;
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("CreateNewDomain. NPVR Provider returned null from Factory. G ID: {0} , D ID: {1} , NPVR Err Msg: {2}", m_nGroupID, m_nDomainID, resp.msg));
                            return DomainStatus.DomainCreatedWithoutNPVRAccount;
                        }
                    }
                    else
                    {
                        log.Error("Error - " + string.Format("CreateNewDomain. NPVR Provider CreateAccount response is null. G ID: {0} , D ID: {1}", m_nGroupID, m_nDomainID));
                        return DomainStatus.DomainCreatedWithoutNPVRAccount;
                    }
                }
                catch (Exception ex)
                {
                    log.ErrorFormat("CreateNewDomain. NPVR Provider return with ex from Factory. G ID: {0} , D ID: {1} , ex: {2}", m_nGroupID, m_nDomainID, ex);
                    return DomainStatus.DomainCreatedWithoutNPVRAccount;
                }
            }
            return DomainStatus.OK;
        }

        protected override bool DoUpdate()
        {
            bool result = true;
            var updateDate = DateTime.UtcNow;

            if (shouldUpdateInfo)
            {
                bool updateInfoResult = DomainDal.UpdateDomain(m_sName, m_sDescription, m_nDomainID, m_nGroupID, updateDate, (int) m_DomainRestriction, m_nRegion, m_sCoGuid);

                if (!updateInfoResult)
                {
                    m_DomainStatus = DomainStatus.Error;
                }

                this.UpdateDate = updateDate;

                result &= updateInfoResult;
            }

            if (shouldUpdateSuspendStatus)
            {
                bool suspendUpdateSuccess = DAL.DomainDal.ChangeSuspendDomainStatus(m_nDomainID, m_nGroupID, nextSuspensionStatus, roleId);

                result &= suspendUpdateSuccess;
            }

            if (result)
            {
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nGroupID, m_nDomainID);
                InvalidateDomain();
            }

            return result;
        }

        protected override bool DoDelete()
        {
            bool success = false;
            int isActive = 2; // Inactive
            int status = 2; // Removed

            // get household users
            List<int> domainUserIds = new List<int>();
            domainUserIds.AddRange(m_DefaultUsersIDs);
            domainUserIds.AddRange(m_masterGUIDs);
            domainUserIds.AddRange(m_PendingUsersIDs);
            domainUserIds.AddRange(m_UsersIDs);
            domainUserIds = domainUserIds.Distinct().ToList();

            // get household devices            
            if (m_deviceFamilies != null && m_deviceFamilies.Count > 0)
            {
                List<string> domainUdids = new List<string>();
                domainUdids = m_deviceFamilies.SelectMany(x => x.DeviceInstances).Where(y => !string.IsNullOrEmpty(y.m_deviceUDID)).Select(z => z.m_deviceUDID).ToList();

                domainUdids = domainUdids.Distinct().ToList();

                foreach (string udid in domainUdids)
                {
                    DomainResponseStatus domainResponseStatus = RemoveDeviceFromDomain(udid, true);
                    if (domainResponseStatus != DomainResponseStatus.OK)
                    {
                        log.ErrorFormat("Error while RemoveDeviceFromDomain. domainId: {0}, udid: {1}", m_nDomainID, udid);
                    }
                }
            }

            int statusRes = DomainDal.SetDomainStatus(m_nGroupID, m_nDomainID, isActive, status, shouldPurge);

            //return statusRes == 2 ? DomainResponseStatus.OK : DomainResponseStatus.Error;
            if (IsDomainRemovedSuccessfully(statusRes))
            {
                //remove domain from cache
                DomainsCache oDomainCache = DomainsCache.Instance();
                oDomainCache.RemoveDomain(m_nGroupID, m_nDomainID);
                InvalidateDomain();

                // delete users from cache
                UsersCache usersCache = UsersCache.Instance();
                foreach (var userId in domainUserIds)
                {
                    usersCache.RemoveUser(userId, m_nGroupID);

                    // GDPR TTV
                    ApiObjects.Segmentation.UserSegment.Remove(userId.ToString());

                    // add invalidation key for user roles cache
                    string userRoleInvalidationKey = LayeredCacheKeys.GetUserRolesInvalidationKey(m_nGroupID, userId.ToString());
                    if (!CachingProvider.LayeredCache.LayeredCache.Instance.SetInvalidationKey(userRoleInvalidationKey))
                        log.ErrorFormat("Failed to set invalidation key on RemoveDomain key = {0}", userRoleInvalidationKey);
                }

                INPVRProvider npvr;
                if (NPVRProviderFactory.Instance().IsGroupHaveNPVRImpl(m_nGroupID, out npvr, null) && npvr.SynchronizeNpvrWithDomain)
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
                else //BEO-8378
                {
                    var accountSettings = ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(m_nGroupID);
                    if (accountSettings != null && accountSettings.IsCdvrEnabled.HasValue && accountSettings.IsCdvrEnabled.Value)
                    {
                        var queue = new QueueWrapper.GenericCeleryQueue();
                        ApiObjects.QueueObjects.UserTaskData message = new ApiObjects.QueueObjects.UserTaskData(m_nGroupID, UserTaskType.DeleteDomain, string.Empty, m_nDomainID);
                        queue.Enqueue(message, string.Format(SCHEDULED_TASKS_ROUTING_KEY, m_nGroupID));
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

        public void InvalidateDomainUsersRoles()
        {
            List<string> invalidationKeys = new List<string>();
            foreach (int userID in this.m_UsersIDs)
            {
                // Remove Users Roles
                invalidationKeys.Add(LayeredCacheKeys.GetUserRolesInvalidationKey(m_nGroupID, userID.ToString()));
            }

            foreach (int userID in this.m_DefaultUsersIDs)
            {
                // Remove Users Roles
                invalidationKeys.Add(LayeredCacheKeys.GetUserRolesInvalidationKey(m_nGroupID, userID.ToString()));
            }

            foreach (int userID in this.m_masterGUIDs)
            {
                // Remove Users Roles
                invalidationKeys.Add(LayeredCacheKeys.GetUserRolesInvalidationKey(m_nGroupID, userID.ToString()));
            }

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }

        public void InvalidateDomain()
        {
            List<string> invalidationKeys = new List<string>()
                {
                    LayeredCacheKeys.GetHouseholdInvalidationKey(m_nGroupID, this.m_nDomainID)
                };

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }

        private void InvalidateDomainUser(string userId)
        {
            List<string> invalidationKeys = new List<string>()
                {
                    LayeredCacheKeys.GetHouseholdUserInalidationKey(m_nGroupID, this.m_nDomainID, userId)
                };

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }
    }
}