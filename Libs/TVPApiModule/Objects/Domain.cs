using Core.Users;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace TVPApiModule.Objects
{
    public class Domain
    {
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
        public int m_nGroupID;

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
        public List<HomeNetwork> m_homeNetworks;
        
        [JsonProperty()]
        public int m_nRegion;

        public int? roleId;

        public Domain(Core.Users.Domain origin)
        {
            if (origin != null)
            {
                this.m_DefaultUsersIDs = origin.m_DefaultUsersIDs;
                this.m_deviceFamilies = origin.m_deviceFamilies;
                this.m_DomainRestriction = origin.m_DomainRestriction;
                this.m_DomainStatus = origin.m_DomainStatus;
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
    }

    public class DomainResponseObject
    {
        public Domain m_oDomain;
        public DomainResponseStatus m_oDomainResponseStatus;
        

        public DomainResponseObject(Core.Users.DomainResponseObject origin)
        {
            this.m_oDomain = new Domain(origin.m_oDomain);
            this.m_oDomainResponseStatus = origin.m_oDomainResponseStatus;
        }
    }
}
