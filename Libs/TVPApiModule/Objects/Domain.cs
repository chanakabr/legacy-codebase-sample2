using Core.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Linq;
using ApiObjects;
using TVPApiModule.Objects.CRM;

namespace TVPApiModule.Objects
{
    [JsonObject()]
    public class Domain : ApiObjects.CoreObject
    {
        //Name of the Domain
        [JsonProperty()]
        public string m_sName;

        //Description of the Domain
        [JsonProperty()]
        public string m_sDescription;

        //CoGuid of the Domain        
        [JsonProperty()]
        public string m_sCoGuid;

        //Domain ID in Domains table        
        [JsonProperty()]
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
        [JsonProperty()]
        public int m_nLimit;

        //Domain Device Max_Limit        
        [JsonProperty()]
        public int m_nDeviceLimit;

        //Domain User Max_Limit        
        [JsonProperty()]
        public int m_nUserLimit;

        //Domain User Max_Limit        
        [JsonProperty()]
        public int m_nConcurrentLimit;

        //Domain Status        
        [JsonProperty()]
        public int m_nStatus;

        //Domain IsActive        
        [JsonProperty()]
        public int m_nIsActive;

        //List of Users        
        [JsonProperty()]
        public List<int> m_UsersIDs;

        //List of Master Users        
        [JsonProperty()]
        public List<int> m_masterGUIDs;

        //List of Master-approval Pending Users        
        [JsonProperty()]
        public List<int> m_PendingUsersIDs;

        //List of Household Devices (STB/ConnectedTV) Users
        [JsonProperty()]
        public List<int> m_DefaultUsersIDs;

        //List of device brands
        [JsonProperty()]
        public List<DeviceContainer> m_deviceFamilies;

        [JsonProperty()]
        public DomainStatus m_DomainStatus;

        [JsonProperty()]
        public int m_frequencyFlag;

        [JsonProperty()]
        public DateTime m_NextActionFreq;

        [JsonProperty()]
        public DateTime m_NextUserActionFreq;

        // Domain's Operator ID
        [JsonProperty()]
        public int m_nSSOOperatorID;

        [JsonProperty()]
        public DomainRestriction m_DomainRestriction;

        [JsonProperty()]
        public List<HomeNetwork> m_homeNetworks;

        [JsonProperty()]
        public int m_nRegion;

        [JsonProperty()]
        public int? roleId;

        public Domain(Core.Users.Domain origin)
        {
            if (origin != null)
            {
                this.m_DefaultUsersIDs = origin.m_DefaultUsersIDs;

                if (origin.m_deviceFamilies != null)
                {
                    this.m_deviceFamilies = origin.m_deviceFamilies.Select(d => new DeviceContainer(d)).ToList();
                }

                this.m_DomainRestriction = origin.m_DomainRestriction;
                this.m_DomainStatus = ConvertDomainStatus(origin.m_DomainStatus);
                this.m_frequencyFlag = origin.m_frequencyFlag;
                this.m_homeNetworks = origin.m_homeNetworks;
                this.m_masterGUIDs = origin.m_masterGUIDs;
                this.m_nConcurrentLimit = origin.m_nConcurrentLimit;
                this.m_nDeviceLimit = origin.m_nDeviceLimit;
                this.m_nDomainID = origin.m_nDomainID;
                this.m_NextActionFreq = origin.m_NextActionFreq;
                this.m_NextUserActionFreq = origin.m_NextUserActionFreq;
                this.m_nGroupID = origin.m_nGroupID;
                this.m_nIsActive = origin.m_nIsActive;
                this.m_nLimit = origin.m_nLimit;
                this.m_nRegion = origin.m_nRegion;
                this.m_nSSOOperatorID = origin.m_nSSOOperatorID;
                this.m_nStatus = origin.m_nStatus;
                this.m_nUserLimit = origin.m_nUserLimit;
                this.m_PendingUsersIDs = origin.m_PendingUsersIDs;
                this.m_sCoGuid = origin.m_sCoGuid;
                this.m_sDescription = origin.m_sDescription;
                this.m_sName = origin.m_sName;
                this.m_UsersIDs = origin.m_UsersIDs;
                this.roleId = origin.roleId;
            }
        }

        private DomainStatus ConvertDomainStatus(Core.Users.DomainStatus m_DomainStatus)
        {
            switch (m_DomainStatus)
            {
                case Core.Users.DomainStatus.OK:
                    return DomainStatus.OK;
                    break;
                case Core.Users.DomainStatus.DomainAlreadyExists:
                    return DomainStatus.DomainAlreadyExists;
                    break;
                case Core.Users.DomainStatus.ExceededLimit:
                    return DomainStatus.ExceededLimit;
                    break;
                case Core.Users.DomainStatus.DeviceTypeNotAllowed:
                    return DomainStatus.DeviceTypeNotAllowed;
                    break;
                case Core.Users.DomainStatus.UnKnown:
                    return DomainStatus.UnKnown;
                    break;
                case Core.Users.DomainStatus.Error:
                    return DomainStatus.Error;
                    break;
                case Core.Users.DomainStatus.DeviceNotInDomin:
                    return DomainStatus.DeviceNotInDomin;
                    break;
                case Core.Users.DomainStatus.MasterEmailAlreadyExists:
                    return DomainStatus.MasterEmailAlreadyExists;
                    break;
                case Core.Users.DomainStatus.UserNotInDomain:
                    return DomainStatus.UserNotInDomain;
                    break;
                case Core.Users.DomainStatus.DomainNotExists:
                    return DomainStatus.DomainNotExists;
                    break;
                case Core.Users.DomainStatus.HouseholdUserFailed:
                    return DomainStatus.HouseholdUserFailed;
                    break;
                case Core.Users.DomainStatus.DomainCreatedWithoutNPVRAccount:
                    return DomainStatus.DomainCreatedWithoutNPVRAccount;
                    break;
                case Core.Users.DomainStatus.DomainSuspended:
                    return DomainStatus.DomainSuspended;
                    break;
                case Core.Users.DomainStatus.NoUsersInDomain:
                    return DomainStatus.NoUsersInDomain;
                    break;
                case Core.Users.DomainStatus.UserExistsInOtherDomains:
                    return DomainStatus.UserExistsInOtherDomains;
                    break;
                case Core.Users.DomainStatus.Pending:
                    return DomainStatus.Pending;
                    break;
                case Core.Users.DomainStatus.RegionDoesNotExist:
                    return DomainStatus.RegionDoesNotExist;
                    break;
                default:
                    return DomainStatus.OK;
                    break;
            }
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }

    public class DeviceContainer
    {
        [JsonProperty()]
        public List<DeviceDTO> DeviceInstances;

        [JsonProperty()]
        public string m_deviceFamilyName;

        [JsonProperty()]
        public int m_deviceFamilyID;

        [JsonProperty()]
        public int m_deviceLimit;

        [JsonProperty()]
        public int m_deviceConcurrentLimit;

        public DeviceContainer(Core.Users.DeviceContainer origin)
        {
            this.DeviceInstances = origin.DeviceInstances.ConvertAll(DeviceDTO.ConvertToDTO);
            this.m_deviceFamilyID = origin.m_deviceFamilyID;
            this.m_deviceFamilyName = origin.m_deviceFamilyName;
            this.m_deviceLimit = origin.m_deviceLimit;
            this.m_deviceConcurrentLimit = origin.m_deviceConcurrentLimit;
        }
    }

    public class DomainResponseObject
    {
        
        public Domain m_oDomain;
        public DomainResponseStatus m_oDomainResponseStatus;


        public DomainResponseObject(Core.Users.DomainResponseObject origin)
        {
            this.m_oDomain = origin.m_oDomain == null ? null : new Domain(origin.m_oDomain);
            this.m_oDomainResponseStatus = origin.m_oDomainResponseStatus;
        }
    }

    public enum DomainStatus
    {
        OK = 0,
        DomainAlreadyExists,
        ExceededLimit,
        DeviceTypeNotAllowed,
        UnKnown,
        Error,
        DeviceNotInDomin,
        MasterEmailAlreadyExists,
        UserNotInDomain,
        DomainNotExists,
        HouseholdUserFailed,
        DomainCreatedWithoutNPVRAccount,
        DomainSuspended,
        NoUsersInDomain,
        UserExistsInOtherDomains,
        Pending,
        RegionDoesNotExist

    }
}
