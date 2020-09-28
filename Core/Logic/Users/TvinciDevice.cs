using ApiObjects.Response;
using System;
using System.Collections.Generic;
using Core.Users.Cache;

namespace Core.Users
{
    class TvinciDevice : BaseDevice
    {
        protected TvinciDevice()
        {
        }

        public TvinciDevice(int groupID)
            : base(groupID)
        {
        }

        /// <summary>
        /// Generate a PIN code (Unique per account)
        /// Insert a new device to devices table with the new PIN code
        /// Return the new PIN
        /// </summary>
        /// <returns>New PIN</returns>
        public override string GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID)
        {
            Device device = new Device(sDeviceUDID, nBrandID, nGroupID);
            device.Initialize(sDeviceUDID);

            // Device already exists
            if (device.m_pin != string.Empty)
            {
                return device.m_pin;
            }

            // New Device            
            string sNewDevicePIN = DeviceRepository.GenerateNewPIN(nGroupID);
            device.m_pin = sNewDevicePIN;

            var nDeviceID = device.Save(0, 1); // Returns device ID, 0 otherwise

            return nDeviceID != 0 ? sNewDevicePIN : string.Empty;
        }

        [Obsolete]
        public override ApiObjects.Response.Status SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            ApiObjects.Response.Status status = null;
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            device.Initialize(sDeviceUDID);
            bool isSetSucceeded = device.SetDeviceInfo(sDeviceName, "", "");

            // in case set device Succeeded
            // domain should be remove from the cache 
            if (isSetSucceeded)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                Core.Users.BaseDomain baseDomain = null;
                Utils.GetBaseImpl(ref baseDomain, nGroupID);

                if (baseDomain != null)
                {
                    List<Domain> domains = baseDomain.GetDeviceDomains(sDeviceUDID);
                    if (domains != null && domains.Count > 0)
                    {
                        DomainsCache oDomainCache = DomainsCache.Instance();
                        foreach (var domain in domains)
                        {
                            oDomainCache.RemoveDomain(domain.m_nDomainID);

                        }
                    }
                }
            }
            else
            {
                if (device != null)
                {
                    status = Utils.ConvertDeviceStateToResponseObject(device.m_state);
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }
            }

            return status;
        }

        public override DeviceResponseObject SetDevice(int nGroupID, string sDeviceUDID, string sDeviceName, string macAddress, 
            string externalId, bool allowNullExternalId, bool allowNullMacAddress = false)
        {
            DeviceResponseObject ret = new DeviceResponseObject();
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            var init = device.Initialize(sDeviceUDID);

            if (!init)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                return ret;
            }

            //Check if external id already exists
            var device_Id = DeviceRepository.GetDeviceIdByExternalId(nGroupID, externalId);

            //already exists
            if (!string.IsNullOrEmpty(device_Id) && device.m_id != device_Id)
            {
                ret.m_oDevice = device;
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.ExternalIdAlreadyExists;
                return ret;
            }

            var _deviceName = !string.IsNullOrEmpty(sDeviceName) ? sDeviceName : device.m_deviceName;
            bool isSetSucceeded = device.SetDeviceInfo(_deviceName, macAddress, externalId, allowNullExternalId, allowNullMacAddress);

            // in case set device Succeeded
            // domain should be remove from the cache 
            if (isSetSucceeded)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.OK;
                ret.m_oDevice = device;

                Core.Users.BaseDomain baseDomain = null;
                Utils.GetBaseImpl(ref baseDomain, nGroupID);

                if (baseDomain != null)
                {
                    List<Domain> domains = baseDomain.GetDeviceDomains(sDeviceUDID);
                    if (domains != null && domains.Count > 0)
                    {
                        DomainsCache oDomainCache = DomainsCache.Instance();
                        foreach (var domain in domains)
                        {
                            oDomainCache.RemoveDomain(domain.m_nDomainID);

                        }
                    }
                }
            }
            else
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
                if (device != null)
                {
                    ret.m_oDevice = device;
                    if (device.m_state == DeviceState.NotExists)
                    {
                        ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                    }
                }
            }

            return ret;
        }

        public override DeviceResponseObject GetDeviceInfo(int nGroupID, string sID, bool bIsUDID)
        {
            DeviceResponseObject ret = new DeviceResponseObject();
            bool result = false;
            Device device = new Device(nGroupID);
            if (bIsUDID)
            {
                result = device.Initialize(sID);
            }
            else
            {
                int nID = 0;
                bool parseResult = int.TryParse(sID, out nID);
                if (parseResult)
                {
                    result = device.Initialize(nID);
                }
            }
            ret.m_oDevice = device;
            if (!result)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
            }
            else
            {
                if (device.m_state == DeviceState.Error || device.m_state == DeviceState.UnActivated)
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
                }
                else if (device.m_state == DeviceState.NotExists)
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                }
                else
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.OK;
                }
            }
            return ret;
        }
    }
}
