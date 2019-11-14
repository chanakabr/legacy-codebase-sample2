using Core.Users;
using System;

namespace TVPApiModule.Objects.CRM
{
    public class DeviceDTO
    {
        public string m_id;

        public string m_deviceUDID;

        public string m_deviceFamily;
        
        public int m_deviceFamilyID;
        
        public int m_domainID;
        
        public string m_deviceName;
        
        public int m_deviceBrandID;
        
        public string m_pin;
        
        public DateTime m_activationDate;

        public DeviceState m_state;

        public string m_sStreamType;

        public string m_sProfile;
        /*public int m_groupID;
        
        public string m_deviceBrand;

        public string LicenseData;*/

        public static DeviceDTO ConvertToDTO(Device device)
        {
            if (device == null)
            {
                return null;
            } 
            DeviceDTO res = new DeviceDTO();
            res.m_id = device.m_id;
            res.m_deviceUDID = device.m_deviceUDID;
            res.m_deviceFamily = device.m_deviceFamily;
            res.m_deviceFamilyID = device.m_deviceFamilyID;
            res.m_domainID = device.m_domainID;
            res.m_deviceName = device.m_deviceName;
            res.m_deviceBrandID = device.m_deviceBrandID;
            res.m_pin = device.m_pin;
            res.m_activationDate = device.m_activationDate;
            res.m_state = device.m_state;
            res.m_sStreamType = device.m_sStreamType;
            res.m_sProfile = device.m_sProfile;

            return res;
        }
    }
}
