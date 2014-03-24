using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Users
{
    /// <summary>
    /// This Class Represents Domain Object
    /// </summary>
    public class Domain
    {
        #region Private Fields
        
        //Name of the Domain
        public string                   m_sName;

        //Description of the Domain
        public string                   m_sDescription;

        //CoGuid of the Domain
        public string                   m_sCoGuid;

        //Domain ID in Domains table
        public int                      m_nDomainID;

        //Domain group_id
        public int                      m_nGroupID;

        //Domain Max_Limit [Obsolete]
        public int                      m_nLimit;

        //Domain Device Max_Limit
        public int                      m_nDeviceLimit;

        //Domain User Max_Limit
        public int                      m_nUserLimit;

        //Domain User Max_Limit
        public int                      m_nConcurrentLimit;

        //Domain Status
        public int                      m_nStatus;

        //Domain IsActive
        public int                      m_nIsActive;

        //List of Users
        public List<int>                m_UsersIDs;

        //List of Master Users
        public List<int>                m_masterGUIDs;

        //List of Master-approval Pending Users
        public List<int>                m_PendingUsersIDs;

        //List of Household Devices (STB/ConnectedTV) Users
        public List<int>                m_DefaultUsersIDs;

        //List of device brands
        public List<DeviceContainer>    m_deviceFamilies;

        public DomainStatus             m_DomainStatus;

        //public DomainState m_DomainState;

        public int                      m_frequencyFlag;

        public DateTime                 m_NextActionFreq;

        public DateTime                 m_NextUserActionFreq;

        // Domain's Operator ID
        public int                      m_nSSOOperatorID;

        public DomainRestriction        m_DomainRestriction;


        protected int                   m_deviceLimitationModule;

        protected int                   m_totalNumOfDevices;

        protected int                   m_totalNumOfUsers;

        protected int                   m_minPeriodId;

        protected int                   m_minUserPeriodId;

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

        }

        public Domain(int nDomainID) : this()
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
            DateTime dDateTime = DateTime.Now;

            int nDeviceLimit = 0;
            int nUserLimit = 0;
            int nConcurrentLimit = 0;
            int nGroupConcurrentLimit = 0;
            int nDomainLimitID = DAL.DomainDal.GetDomainDefaultLimitsID(nGroupID, ref nDeviceLimit, ref nUserLimit, ref nConcurrentLimit, ref nGroupConcurrentLimit);

            bool bInserRes = DAL.DomainDal.InsertNewDomain(sName, sDescription, nGroupID, dDateTime, nDomainLimitID, sCoGuid);

            if (!bInserRes)
            {
                m_DomainStatus = DomainStatus.Error;
                return this;
            }


            int nDomainID = -1;
            int nIsActive = 0;
            int nStatus = 0;

            Domain domainDbObj = this;
            bool resDbObj = DAL.DomainDal.GetDomainDbObject(nGroupID, dDateTime, ref sName, ref sDescription, ref nDomainID, ref nIsActive, ref nStatus, ref sCoGuid);

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

            m_DomainStatus = DomainStatus.OK;

            m_deviceFamilies = InitializeDeviceFamilies(nDomainLimitID, nGroupID);

            m_UsersIDs = new List<int>();
            m_PendingUsersIDs = new List<int>();
            m_DefaultUsersIDs = new List<int>();

            DomainResponseStatus res = AddUserToDomain(m_nGroupID, m_nDomainID, nMasterGuID, nMasterGuID, UserDomainType.Master);
            //DomainResponseStatus res = AddUserToDomain(m_nGroupID, m_nDomainID, nMasterGuID, nMasterGuID, true); //AddUserToDomain(m_nGroupID, m_nDomainID, nMasterGuID, true);

            if (res == DomainResponseStatus.OK)
            {
                m_UsersIDs = new List<int>();
                m_UsersIDs.Add(nMasterGuID);
            }

            // Add Monkey User 
            //TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            //string sIP = "1.1.1.1";
            //string sWSUserName = "";
            //string sWSPass = "";
            //TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
            //string sWSURL = Utils.GetWSURL("users_ws");
            //if (sWSURL != "")
            //{
            //    u.Url = sWSURL;
            //}

            //ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
            //if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
            //{
            //    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
            //    ret.m_sRecieptCode = "";
            //    ret.m_sStatusDescription = "Cant charge an unknown user";

            //    return ret;
            //}

            //u.Dispose();


            return this;
        }

        public DomainResponseStatus Remove()
        {
            int isActive = 2;   // Inactive
            int status = 2;     // Removed

            int statusRes = DAL.DomainDal.SetDomainStatus(m_nGroupID, m_nDomainID, isActive, status);

            return (statusRes == 2) ? DomainResponseStatus.OK : DomainResponseStatus.Error;
        }

        public DomainResponseStatus TryRemove()
        {
            int isActive = 2;   // Inactive
            int status = 2;     // Removed
            int statusRes = DAL.DomainDal.SetDomainStatus(m_nGroupID, m_nDomainID, isActive, status);

            return DomainResponseStatus.DomainNotExists;
            //return (statusRes == 2) ? DomainResponseStatus.OK : DomainResponseStatus.Error;

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

            bool domainExists = GetDomainSettings(nDomainID, nGroupID);
                                        
            if (!domainExists)
            {
                m_DomainStatus = DomainStatus.Error;
                return false;
            }

            m_sName = sName;
            m_sDescription = sDescription;

            DomainResponseStatus dStatus =  GetUserList(nDomainID, nGroupID);   // OK or NoUsersInDomain
            int numOfDevices = GetDeviceList();

            m_DomainStatus = (dStatus == DomainResponseStatus.OK) ? DomainStatus.OK : DomainStatus.Error;

            return (m_DomainStatus == DomainStatus.OK);
        }

        /// <summary>
        /// Init New Domain Object according to GroupId and DomainId
        /// </summary>
        /// <param name="nGroupID">The GroupId</param>
        /// <param name="nDomainId">The DomainId</param>
        public bool Initialize(int nGroupID, int nDomainID)
        {
            m_nGroupID = nGroupID;
            m_nDomainID = nDomainID;

            if (GetDomainSettings(nDomainID, nGroupID) == false)
            {
                m_DomainStatus = DomainStatus.DomainNotExists;
                return false;
            }

            // Init Users
            DomainResponseStatus domainRes = GetUserList(nDomainID, nGroupID);

            // Init Devices
            m_deviceFamilies = InitializeDeviceFamilies(m_deviceLimitationModule, nGroupID);
            int devCount = GetDeviceList(false);

            m_DomainStatus = DomainStatus.OK;

            return true;
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

            if (GetDomainSettings(nDomainID, nGroupID) == false)
            {
                m_DomainStatus = DomainStatus.Error;
                return false;
            }

            // Users are stored on parent (nGroupID) accound 
            DomainResponseStatus domainRes = GetUserList(nDomainID, nGroupID);

            // Device families (limits) are per sub-account
            m_deviceFamilies = InitializeDeviceFamilies(m_deviceLimitationModule, nSubGroupID);
            GetDeviceList(false);

            m_DomainStatus = DomainStatus.OK;
            return true;
        }

        /// <summary>
        /// Remove User from the Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        public DomainResponseStatus RemoveUserFromDomain(int nGroupID, int nDomainID, int nUserGuid)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;

            // if next allowed action is in future, return LimitationPeriod status
            if (m_NextUserActionFreq >= DateTime.UtcNow)
            {
                eRetVal = DomainResponseStatus.LimitationPeriod;
                return eRetVal;
            }


            int nUserDomainID = DAL.DomainDal.DoesUserExistInDomain(nGroupID, nDomainID, nUserGuid, false);

            if (nUserDomainID <= 0)
            {
                eRetVal = DomainResponseStatus.UserNotExistsInDomain;
                return eRetVal;
            }

            try
            {
                int nStatus = 2;
                int nIsActive = 0;
                int rowsAffected = DAL.DomainDal.SetUserStatusInDomain(nUserGuid, nDomainID, nGroupID, nUserDomainID, nStatus, nIsActive);

                if (rowsAffected > 0)
                {
                    SetDomainFlag(nDomainID, 1, false);
                    eRetVal = RemoveUserFromList(nUserGuid);
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
        

            //}
            //else if (m_totalNumOfDevices >= m_nLimit)
            //{
            //    eRetVal = DomainResponseStatus.ExceededLimit;
            //}
            //container = GetDeviceContainer(device.m_deviceFamilyID);
            //else if (container != null && container.GetActivatedDeviceCount() >= container.m_deviceLimit)
            //{
            //    eRetVal = DomainResponseStatus.ExceededLimit;
            //}

            //return eRetVal;
        }

        public DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, ref Device device)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;
            int isDevActive = 0;
            int status = 0;
            int tempDeviceID = 0;
            int nDbDomainDeviceID = 0;
            int domainID = DAL.DomainDal.GetDeviceDomainData(nGroupID, sUDID, ref tempDeviceID, ref isDevActive, ref status, ref nDbDomainDeviceID);

            //Very Patchy - change the group check to be configurable!!
            if (domainID != 0 && m_nGroupID != 147)
            {
                if (status == 3 && isDevActive == 3)    // Pending master approval
                {
                    bool updated = DAL.DomainDal.UpdateDomainsDevicesStatus(nDbDomainDeviceID, 1, 1);
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
            DomainResponseStatus responseStatus = CheckDeviceLimit(device, container);
            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed)
            {
                eRetVal = responseStatus;
                return eRetVal;
            }

            int isActive = 0;
            int nDeviceID = 0;
            // Get row id from domains_devices
            int nDomainsDevicesID = DAL.DomainDal.DoesDeviceExistInDomain(m_nDomainID, m_nGroupID, sUDID, ref isActive, ref nDeviceID);
            //int nDomainsDevicesID = DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            //New Device Domain Connection
            if (nDomainsDevicesID == 0)
            {
                // Get row id from devices table (not udid)
                device.m_domainID = nDomainID;
                int deviceID = device.Save(1);

                int domainDeviceRecordID = DAL.DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 1, 1);
                //bool inserted = DAL.DomainDal.InsertToDomainsDevices(deviceID, m_nDomainID, m_nGroupID, 1, 1);

                if (domainDeviceRecordID > 0)
                {
                    device.m_state = DeviceState.Activated;
                    container.AddDeviceInstance(device);

                    m_deviceFamilies.Add(container);
                    
                    //m_Devices.Add(device);
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
                    bool updated = DAL.DomainDal.UpdateDomainsDevicesStatus(nDomainsDevicesID, 1, 1);

                    if (updated)
                    {
                        eRetVal = DomainResponseStatus.OK;
                        device.m_domainID = nDomainID;
                        int deviceID = device.Save(1);
                        //GetDeviceList();
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
            if (m_NextActionFreq >= DateTime.UtcNow)
            {
                bRes = DomainResponseStatus.LimitationPeriod;
                return bRes;
            }


            int isActive = 0;
            int nDeviceID = 0;

            int nDomainDeviceID = DAL.DomainDal.DoesDeviceExistInDomain(m_nDomainID, m_nGroupID, sUDID, ref isActive, ref nDeviceID);   //DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            if (nDomainDeviceID > 0)
            {
                // set is_Active = 2; status = 2
                bool bUpdate = DAL.DomainDal.UpdateDomainsDevicesStatus(nDomainDeviceID, 2, 2);

                if (!bUpdate)
                {
                    return DomainResponseStatus.Error;
                }


                int nDomainsDevicesCount = DAL.DomainDal.GetDomainsDevicesCount(m_nGroupID, nDeviceID);
                bool bDeleteDevice = (nDomainsDevicesCount == 0);   // No other domains attached to this device

                if (bDeleteDevice)
                {
                    // set is_Active = 2; status = 2
                    bUpdate = DAL.DomainDal.UpdateDeviceStatus(nDeviceID, 2, 2);

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


            if ((!bIsEnable) && (m_NextActionFreq >= DateTime.UtcNow))
            {
                eDomainResponseStatus = DomainResponseStatus.LimitationPeriod;
                return eDomainResponseStatus;
            }


            DeviceContainer container = null;
            Device device = GetDomainDevice(sUDID, ref container);
            // device.Initialize(udid, nDomainID, m_nGroupID);

            int enableInt = 1;
            DeviceState eNewDeviceState = DeviceState.Activated;
            if (!bIsEnable)
            {
                enableInt = 0;
                eNewDeviceState = DeviceState.UnActivated;
            }
            else
            {
                eDomainResponseStatus = CheckDeviceLimit(device, container);
                eNewDeviceState = DeviceState.Activated;
            }

            int isActive = 0;
            int nDeviceID = 0;
            int nDomainDeviceID = DAL.DomainDal.DoesDeviceExistInDomain(m_nDomainID, nGroupID, sUDID, ref isActive, ref nDeviceID);     //DoesDeviceExistInDomain(m_nDomainID, sUDID, ref isActive, ref nDeviceID);

            if (nDomainDeviceID > 0 && eDomainResponseStatus != DomainResponseStatus.ExceededLimit)
            {
                bool resUpdated = DAL.DomainDal.UpdateDomainsDevicesIsActive(nDomainDeviceID, enableInt, bIsEnable);

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

            Dictionary<int, int> dbTypedUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);
            //List<int> domainUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);


            // If domain has no users, insert new Master user
            //
            int status = 1;
            int isActive = 1;

            if ((dbTypedUserIDs == null || dbTypedUserIDs.Count == 0) &&
                (nUserID == nMasterUserGuid))  // && (userType == UserDomainType.Master)))
            {
                int inserted = DAL.DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int)userType, status, isActive, nMasterUserGuid);

                if (inserted > 0)
                {
                    m_UsersIDs.Add(nUserID);
                    m_masterGUIDs.Add(nUserID);

                    m_totalNumOfUsers = m_UsersIDs.Count - m_DefaultUsersIDs.Count;  //m_UsersIDs.Count; //m_totalNumOfUsers++;
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

            if ((masterUserIDs != null) && (masterUserIDs.Count > 0) && (!masterUserIDs.Contains(nMasterUserGuid)))
            {
                return DomainResponseStatus.ActionUserNotMaster;
            }


            // Check if user already exists in domain (active or pending)
            int nUserDomainID = DAL.DomainDal.DoesUserExistInDomain(nGroupID, nDomainID, nUserID, false);  //IsUserExistsInDomain(nGroupID, nDomainID, nUserGuid);

            if (nUserDomainID > 0)  // If user exists, update its status to active
            {
                int rowsAffected = DAL.DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, nUserDomainID);  //UpdateUserInDomain(nUserGuid, nDomainID, nGroupID, nUserDomainID);

                if (rowsAffected < 1)
                {
                    eDomainResponseStatus = DomainResponseStatus.Error;
                }
                else
                {
                    eDomainResponseStatus = GetUserList(nDomainID, nGroupID);
                }

                return eDomainResponseStatus;

                #region Commented
                //ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("users_domains");
                ////updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", 1);
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Is_Active", "=", 1);
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_MASTER", "=", nIsMaster);
                //updateQuery += "where";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserGuid);
                //updateQuery += "and";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", nDomainID);
                //updateQuery += "and";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                //updateQuery += "and";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUserDomainID);


                //if (updateQuery.Execute() == false)
                //{
                //    eDomainResponseStatus = DomainResponseStatus.Error;
                //}
                //else
                //{
                //    eDomainResponseStatus = GetUserList(nDomainID, nGroupID);
                //}
                //updateQuery.Finish();
                //updateQuery = null;
                #endregion
            }


            // Process New User

            //Check if exceeded limit for users
            if (userType != UserDomainType.Household) // && (userType != UserDomainType.Master))
            {
                DomainResponseStatus responseStatus = CheckUserLimit(nDomainID, nUserID);
                if (responseStatus == DomainResponseStatus.ExceededUserLimit || responseStatus == DomainResponseStatus.UserNotAllowed)
                {
                    eDomainResponseStatus = responseStatus;
                    return eDomainResponseStatus;
                }
            }

            //int isMaster = Convert.ToInt32(bIsMaster);  //Convert.ToInt32(nUserID == nMasterUserGuid);
            //int status = 1;
            //int isActive = 1;
            int inserted1 = DAL.DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, (int)userType, status, isActive, nMasterUserGuid);
            //int inserted = DAL.DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, isMaster, status, isActive, nMasterUserGuid);

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

                m_totalNumOfUsers = m_UsersIDs.Count;   // -m_MonkeyUsersIDs.Count;  //m_UsersIDs.Count; //m_totalNumOfUsers++;
                eDomainResponseStatus = DomainResponseStatus.OK;
            }
            else
            {
                eDomainResponseStatus = DomainResponseStatus.Error;
            }

            #region Commented
            //ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("users_domains");
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserGuid);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", nDomainID);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("IS_MASTER", "=", nIsMaster);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Status", "=", 1);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("Is_Active", "=", 1);
            //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("UPDATER_ID", "=", 43);

            //insertQuery.SetConnectionKey("USERS_CONNECTION_STRING");

            //if (insertQuery.Execute())
            //{
            //    //User user = new User();
            //    //user.Initialize(nUserGuid, nGroupID);

            //    m_UsersIDs.Add(nUserGuid);
            //    eDomainResponseStatus = DomainResponseStatus.OK;
            //}
            //else
            //{
            //    eDomainResponseStatus = DomainResponseStatus.Error;
            //}

            //insertQuery.Finish();
            //insertQuery = null;
            #endregion


            return eDomainResponseStatus;
        }

        public DomainResponseStatus AddUserToDomain(int nGroupID, int nDomainID, int nUserID, bool bIsMaster)
        {
            DomainResponseStatus eDomainResponseStatus = DomainResponseStatus.UnKnown;

            int numOfUsers = m_UsersIDs.Count;

            List<int> lDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nUserID);

            if (lDomainIDs != null && lDomainIDs.Count > 0)
            {
                if (lDomainIDs.Count == 1 && lDomainIDs[0] == nDomainID)
                {
                    int rowsAffected = DAL.DomainDal.SetUserStatusInDomain(nUserID, nDomainID, nGroupID, null, 0, 0);  //UpdateUserInDomain(nUserGuid, nDomainID, nGroupID, nUserDomainID);
                    lDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nUserID);
                }
                else if ((!lDomainIDs.Contains<int>(nDomainID)) || (lDomainIDs.Count > 1))
                {
                    // The user belongs to other domain(s), maybe pending activation
                    return DomainResponseStatus.UserExistsInOtherDomains;   //return new DomainResponseObject(this, DomainResponseStatus.UserExistsInOtherDomains);
                }
            }

            int inserted = DAL.DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, Convert.ToInt32(bIsMaster), 1, 1, 43);

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
            List<int> lDomainIDs = DAL.UsersDal.GetUserDomainIDs(nGroupID, nUserID);

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


            // The user is not in the domain
            //if (lDomainIDs == null || lDomainIDs.Count == 0)   //if (nUserDomainID <= 0)
            //{

            //Check if exceeded limit for users
            DomainResponseStatus responseStatus = CheckUserLimit(nDomainID, nUserID);
            if (responseStatus == DomainResponseStatus.ExceededUserLimit || responseStatus == DomainResponseStatus.UserNotAllowed)
            {
                //eDomainResponseStatus = responseStatus;
                return new DomainResponseObject(this, responseStatus);
            }

            // Let's fetch the new user's Username and First name (for e-mail)
            string sNewUsername = string.Empty;
            string sNewFirstName = string.Empty;
            string sNewEmail = string.Empty;
            //string sActivationToken = string.Empty;

            using (DataTable dtUserBasicData = DAL.UsersDal.GetUserBasicData(nUserID))
            {
                if (dtUserBasicData != null)
                {
                    int nCount = dtUserBasicData.DefaultView.Count;
                    if (nCount > 0)
                    {
                        sNewUsername    = dtUserBasicData.DefaultView[0].Row["USERNAME"].ToString();
                        sNewFirstName   = dtUserBasicData.DefaultView[0].Row["FIRST_NAME"].ToString();
                        sNewEmail       = dtUserBasicData.DefaultView[0].Row["EMAIL_ADD"].ToString();
                        //sActivationToken = dtUserBasicData.DefaultView[0].Row["ACTIVATION_TOKEN"].ToString();
                    }
                }
            }

            // Add the new user to the domain
            int isMaster = 0;
            int status = 3;     // Pending
            int isActive = 0;
            string sActivationToken = Guid.NewGuid().ToString();
            
            int inserted = DAL.DomainDal.InsertUserToDomain(nUserID, nDomainID, nGroupID, isMaster, status, isActive, nMasterID, sActivationToken);

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
            bool dbRes = DAL.DomainDal.UpdateDomain(m_sName, m_sDescription, m_nDomainID, m_nGroupID, (int)m_DomainRestriction);

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

            if ((dbDomains != null) && (dbDomains.Count > 0))
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
            List<string> retVal = new List<string>();

            //List<int> domainUsers = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, 1, 1);
            //List<string> retVal = DAL.UsersDal.GetDomainUsers(nDomainID, nGroupID);
            int status = 1;
            int isActive = 1;
            Dictionary<int, int> dbTypedUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);           

            if (dbTypedUserIDs != null && dbTypedUserIDs.Count > 0)
            {
                retVal = dbTypedUserIDs.Select(ut => ut.Key.ToString()).ToList();
            }

            // Add Pending Users (with minus)
            status = 3;
            isActive = 0;
            Dictionary<int, int> dbPendingTypedUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);

            if (dbPendingTypedUserIDs != null && dbPendingTypedUserIDs.Count > 0)
            {
                List<string> pendingIDs = dbPendingTypedUserIDs.Select(ut => (ut.Key * (-1)).ToString()).ToList();
                retVal.AddRange(pendingIDs);
            }

            return retVal;
        }     
 
        public Device RegisterDeviceToDomainWithPIN(int nGroupID, string sPIN, int nDomainID, string sDeviceName, ref DeviceResponseStatus eRetVal)
        {

            string sUDID = string.Empty;
            int nBrandID = 0;

            bool res = DAL.DomainDal.GetDeviceIdAndBrandByPin(sPIN, nGroupID, ref sUDID, ref nBrandID);

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
            bool res = DAL.DomainDal.ResetDomain(m_nDomainID, m_nGroupID, nFreqencyType);

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

            //User masterUser = new User();
            //int nMasterID = masterUser.InitializeByUsername(sMasterUsername, nGroupID);

            //if (nMasterID <= 0)
            //{
            //    return DomainResponseStatus.UserNotAllowed;
            //}

            // Let's try to find the domain of this master
            //int nDomainID = masterUser.m_domianID;

            // No domain - no luck :(
            //if (nDomainID <= 0)
            //{
            //    return DomainResponseStatus.UnKnown;
            //}

            // Found domain, but username is not a master 
            //if (!masterUser.m_isDomainMaster)
            //{
            //    return DomainResponseStatus.ActionUserNotMaster;
            //}

            // Domain found, let's initialize it (users, device families, ...)
            //bool init = Initialize(nGroupID, nDomainID);

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
            int nDeviceDomainID = DAL.DomainDal.GetDeviceDomainData(nGroupID, sDeviceUdid, ref deviceID, ref isActive, ref status, ref nDeviceDomainRecordID);

            if (isActive == 1)
            {
                if ((nDeviceDomainID > 0) && (nDeviceDomainID != this.m_nDomainID))
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
                int rowsAffected = DAL.DomainDal.SetDeviceStatusInDomain(deviceID, this.m_nDomainID, nGroupID, nDeviceDomainRecordID, 2, 2);
            }


            //Check if exceeded limit for users
            DeviceContainer container = GetDeviceContainer(device.m_deviceFamilyID);

            //Check if exceeded limit for the device type
            DomainResponseStatus responseStatus = CheckDeviceLimit(device, container);
            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed)
            {
                return responseStatus;
            }


            // Get row id from devices table (not udid)
            device.m_domainID = this.m_nDomainID;
            deviceID = device.Save(0, 3);

            string sActivationToken = Guid.NewGuid().ToString();
            nDeviceDomainRecordID = DAL.DomainDal.InsertDeviceToDomain(deviceID, m_nDomainID, m_nGroupID, 3, 3, sActivationToken);

            if (nDeviceDomainRecordID > 0)
            {
                device.m_state = DeviceState.Pending;
                container.AddDeviceInstance(device);
                
                m_totalNumOfDevices++;
                m_deviceFamilies.Add(container);

                User masterUser = new User();
                bool masterInit = masterUser.Initialize(this.m_masterGUIDs[0], nGroupID);

                TvinciAPI.AddDeviceMailRequest sMailRequest = null;

                if (masterInit)
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

            int rowsAffected = DAL.DomainDal.SwitchDomainMaster(nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);

            return (rowsAffected > 0) ? DomainResponseStatus.OK : DomainResponseStatus.Error;
        }

        #endregion


        #region Protected Methods

        internal List<DeviceContainer> InitializeDeviceFamilies(int nDomainLimitID, int nGroupID)
        {
            List<string[]> dbDeviceFamilies = DAL.DomainDal.InitializeDeviceFamilies(nDomainLimitID, nGroupID);

            List<DeviceContainer> deviceFamilies = new List<DeviceContainer>();

            for (int i = 0; i < dbDeviceFamilies.Count; i++)
            {
                string[] currentDeviceFamily = dbDeviceFamilies[i];

                int nFamilyID               = string.IsNullOrEmpty(currentDeviceFamily[0]) ? 0 : int.Parse(currentDeviceFamily[0]);
                int nFamilyLimit            = string.IsNullOrEmpty(currentDeviceFamily[1]) ? 0 : int.Parse(currentDeviceFamily[1]);
                int nFamilyConcurrentLimit  = string.IsNullOrEmpty(currentDeviceFamily[2]) ? 0 : int.Parse(currentDeviceFamily[2]);
                string sFamilyName          = currentDeviceFamily[3];

                DeviceContainer dc = new DeviceContainer(nFamilyID, sFamilyName, nFamilyLimit, nFamilyConcurrentLimit);
                deviceFamilies.Add(dc);
            }

            return deviceFamilies;
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
                m_deviceFamilies = InitializeDeviceFamilies(m_deviceLimitationModule, m_nGroupID);
            }

            List<int> devicesInDomain = DAL.DomainDal.GetDevicesInDomain(m_nGroupID, m_nDomainID);

            if (devicesInDomain != null && devicesInDomain.Count > 0)
            {
                for (int i = 0; i < devicesInDomain.Count; i++)
                {
                    Device device = new Device(m_nGroupID);
                    device.Initialize(devicesInDomain[i], m_nDomainID);

                    DeviceContainer container = GetDeviceContainer(device.m_deviceFamilyID);
                    if (container != null)
                    {
                        container.AddDeviceInstance(device);
                        //m_Devices.Add(device);
                        if (device.m_state == DeviceState.Activated)
                        {
                            m_totalNumOfDevices++;
                        }
                    }
                }
            }

            return m_totalNumOfDevices;
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
            Dictionary<int, int> dbTypedUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);

            if (dbTypedUserIDs != null && dbTypedUserIDs.Count > 0)
            {
                m_UsersIDs          = dbTypedUserIDs.Where(ut => ut.Value != (int)UserDomainType.Household).Select(ut => ut.Key).ToList();
                m_masterGUIDs       = dbTypedUserIDs.Where(ut => ut.Value == (int)UserDomainType.Master).Select(ut => ut.Key).ToList();
                m_DefaultUsersIDs    = dbTypedUserIDs.Where(ut => ut.Value == (int)UserDomainType.Household).Select(ut => ut.Key).ToList();

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
            Dictionary<int, int> dbPendingUserIDs = DAL.DomainDal.GetUsersInDomain(nDomainID, nGroupID, status, isActive);

            if (dbPendingUserIDs != null && dbPendingUserIDs.Count > 0)
            {
                m_PendingUsersIDs = dbPendingUserIDs.Select(ut => ut.Key).ToList();
            }

            return eDomainResponseStatus;
        }

        //protected bool SetDomainFlag(int domainId, int val)
        //{
        //    DateTime dt = DateTime.UtcNow;

        //    bool res = DAL.DomainDal.SetDomainFlag(domainId, val, dt);

        //    if (res)
        //    {
        //        m_NextActionFreq = dt;
        //        return true;
        //    }
        //    else return false;
        //}

        protected bool SetDomainFlag(int domainId, int val, bool deviceFlag = true)
        {
            DateTime dt = DateTime.UtcNow;

            bool res = DAL.DomainDal.SetDomainFlag(domainId, val, dt, Convert.ToInt32(deviceFlag));

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

        #region Commented
        /// <summary>
        /// Get the Domain Deafult device limit according to Group configurations
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <returns>the default limit</returns>
        //protected int GetDomainDefaultDeviceLimitID(int nGroupID, ref int defaultDeviceLimit, ref int defaultUserLimit)
        //{
        //    int retVal = DAL.DomainDal.GetDomainDefaultLimitsID(nGroupID, ref defaultDeviceLimit, ref defaultUserLimit);
        //    return retVal;
        //}

        /// <summary>
        /// Check if User is assign to Domain
        /// </summary>
        /// <param name="nGroupID"></param>
        /// <param name="nDomainID"></param>
        /// <param name="nUserGuid"></param>
        /// <returns>True if User is assign to Domain</returns>
        //protected int IsUserExistsInDomain(int nGroupID, int nDomainID, int nUserGuid)
        //{
        //    int nUserDomainID = 0;

        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    //selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

        //    selectQuery += "select id from users_domains where ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DOMAIN_ID", "=", nDomainID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USER_ID", "=", nUserGuid);
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        int nCount = selectQuery.Table("query").DefaultView.Count;
        //        if (nCount > 0)
        //        {
        //            nUserDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        //        }
        //    }

        //    selectQuery.Finish();
        //    selectQuery = null;

        //    return nUserDomainID;
        //}
        #endregion


        protected DeviceContainer GetDeviceContainer(int deviceFamilyID)
        {
            DeviceContainer retVal = null;
            if (m_deviceFamilies != null)
            {
                foreach (DeviceContainer container in m_deviceFamilies)
                {
                    if (container.m_deviceFamilyID == deviceFamilyID)
                    {
                        retVal = container;
                        break;
                    }
                }
            }
            return retVal;
        }


        /// <summary>
        /// Check if name for Domain is already exists 
        /// </summary>
        /// <param name="sName"></param>
        /// <param name="nGroupID"></param>
        /// <returns>True if name is exists</returns>
        //protected bool IsDomainNameExists(string sName, int nGroupID)
        //{
        //    bool isExists = DAL.DomainDal.DoesDomainNameExist(sName, nGroupID);
        //    return isExists;
        //}

        /// <summary>
        /// set Domain Object name, description and limit,
        /// according to DomainId and GroupID
        /// </summary>
        /// <param name="nDomainID"></param>
        /// <param name="nGroupID"></param>
        /// <returns>true if Query is valid</returns>
        protected bool GetDomainSettings(int nDomainID, int nGroupID)
        // ref string sName, ref string sDescription, ref int nDeviceLimitationModule, ref int nDeviceLimit, ref int nUserLimit, ref int nStatus, ref int nIsActive, ref int nFrequencyFlag, ref int nMinPeriodId)
        {
            DateTime dDeviceFrequencyLastAction = new DateTime(2000, 1, 1);
            DateTime dUserFrequencyLastAction = new DateTime(2000, 1, 1);

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

            bool res = DAL.DomainDal.GetDomainSettings(nDomainID, 
                                                        nGroupID,
                                                        ref sName, 
                                                        ref sDescription, 
                                                        ref nDeviceLimitationModule, 
                                                        ref nDeviceLimit, 
                                                        ref nUserLimit, 
                                                        ref nConcurrentLimit, 
                                                        ref nStatus, 
                                                        ref nIsActive, 
                                                        ref nFrequencyFlag, 
                                                        ref nDeviceMinPeriodId, 
                                                        ref nUserMinPeriodId, 
                                                        ref dDeviceFrequencyLastAction,
                                                        ref dUserFrequencyLastAction, 
                                                        ref sCoGuid,
                                                        ref nDeviceRestriction);

            if (res)
            {
                m_sName                     = sName;
                m_sDescription              = sDescription;
                m_deviceLimitationModule    = nDeviceLimitationModule;
                m_nDeviceLimit = m_nLimit   = nDeviceLimit;
                m_nUserLimit                = nUserLimit;
                m_nConcurrentLimit          = nConcurrentLimit;
                m_nStatus                   = nStatus;
                m_nIsActive                 = nIsActive;
                m_frequencyFlag             = nFrequencyFlag;
                m_minPeriodId               = nDeviceMinPeriodId;
                m_minUserPeriodId           = nUserMinPeriodId;
                m_sCoGuid                   = sCoGuid;
                m_DomainRestriction         = (DomainRestriction)nDeviceRestriction;

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

        private DomainResponseStatus CheckDeviceLimit(Device device, DeviceContainer container)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;

            if (container == null)
            {
                eRetVal = DomainResponseStatus.DeviceTypeNotAllowed;
            }
            else if (m_totalNumOfDevices >= m_nDeviceLimit)
            {
                eRetVal = DomainResponseStatus.ExceededLimit;
            }
            //container = GetDeviceContainer(device.m_deviceFamilyID);
            else if (container != null && container.GetActivatedDeviceCount() >= container.m_deviceLimit)
            {
                eRetVal = DomainResponseStatus.ExceededLimit;
            }
            else
            {
                eRetVal = DomainResponseStatus.OK;
            }

            return eRetVal;
        }

        private DomainResponseStatus CheckUserLimit(int nDomainID, int nUserGuid)
        {
            DomainResponseStatus eRetVal = DomainResponseStatus.UnKnown;

            if (nUserGuid <= 0)
            {
                eRetVal = DomainResponseStatus.UserNotAllowed;

            }
            else if ((m_nUserLimit > 0) && (m_totalNumOfUsers >= m_nUserLimit))
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
            Device retVal = null;
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

        #endregion     


    }
}
