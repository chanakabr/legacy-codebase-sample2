using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DomainResponseObject
    {
        public Domain m_oDomain { get; set; }
        
        public DomainResponseStatus m_oDomainResponseStatus { get; set; }
    }

    public class Domain
    {
        public string m_sName { get; set; }

        public string m_sDescription { get; set; }

        public string m_sCoGuid { get; set; }

        public int m_nDomainID { get; set; }

        public int m_nGroupID { get; set; }

        public int m_nLimit { get; set; }

        public int m_nDeviceLimit { get; set; }

        public int m_nUserLimit { get; set; }

        public int m_nConcurrentLimit { get; set; }

        public int m_nStatus { get; set; }

        public int m_nIsActive { get; set; }

        public int[] m_UsersIDs { get; set; }

        public DeviceContainer[] m_deviceFamilies { get; set; }

        public int[] m_masterGUIDs { get; set; }

        public DomainStatus m_DomainStatus { get; set; }

        public int m_frequencyFlag { get; set; }

        public System.DateTime m_NextActionFreq { get; set; }
    }

    public class DeviceContainer
    {
        public string m_deviceFamilyName { get; set; }

        public int m_deviceFamilyID { get; set; }

        public int m_deviceLimit { get; set; }

        public int m_deviceConcurrentLimit { get; set; }

        public Device[] DeviceInstances { get; set; }
    }


    public class Device
    {
        public string m_id { get; set; }
        
        public string m_deviceUDID { get; set; }
    
        public string m_deviceBrand { get; set; }

        public string m_deviceFamily { get; set; }

        public int m_deviceFamilyID { get; set; }

        public int m_domainID { get; set; }

        public string m_deviceName { get; set; }

        public int m_deviceBrandID { get; set; }

        public string m_pin { get; set; }

        public System.DateTime m_activationDate { get; set; }

        public DeviceState m_state { get; set; }
    }


    public enum DomainResponseStatus
    {

        /// <remarks/>
        LimitationPeriod,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        DomainAlreadyExists,

        /// <remarks/>
        ExceededLimit,

        /// <remarks/>
        DeviceTypeNotAllowed,

        /// <remarks/>
        DeviceNotInDomain,

        /// <remarks/>
        DeviceNotExists,

        /// <remarks/>
        DeviceAlreadyExists,

        /// <remarks/>
        UserNotExistsInDomain,

        /// <remarks/>
        OK,

        /// <remarks/>
        ActionUserNotMaster,

        /// <remarks/>
        UserNotAllowed,

        /// <remarks/>
        ExceededUserLimit,

        /// <remarks/>
        NoUsersInDomain,

        /// <remarks/>
        UserExistsInOtherDomains,

        /// <remarks/>
        DomainNotExists,
    }

    public enum DomainStatus
    {

        /// <remarks/>
        OK,

        /// <remarks/>
        DomainAlreadyExists,

        /// <remarks/>
        ExceededLimit,

        /// <remarks/>
        DeviceTypeNotAllowed,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        DeviceNotInDomin,

        /// <remarks/>
        MasterEmailAlreadyExists,

        /// <remarks/>
        UserNotInDomain,

        /// <remarks/>
        DomainNotExists,
    }

    public enum DeviceState
    {

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        NotExists,

        /// <remarks/>
        Pending,

        /// <remarks/>
        Activated,

        /// <remarks/>
        UnActivated,
    }
}
