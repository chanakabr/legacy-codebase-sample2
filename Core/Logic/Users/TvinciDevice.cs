using ApiObjects.Response;
using System;
using System.Collections.Generic;
using ApiLogic.CanaryDeployment;
using ApiObjects.CanaryDeployment;
using ApiObjects.DataMigrationEvents;
using Core.Users.Cache;
using EventBus.Kafka;

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

            string pinCode;
            // Device already exists
            if (device.m_pin != string.Empty)
            {
                pinCode = device.m_pin;
            }
            else
            {
                // New Device            
                string sNewDevicePIN = DeviceRepository.GenerateNewPIN(nGroupID);
                device.m_pin = sNewDevicePIN;

                var nDeviceID = device.Save(0, 1, new DomainDevice()); // Returns device ID, 0 otherwise
                
                pinCode = nDeviceID != 0 ? sNewDevicePIN : string.Empty;
            }
            
            SendCanaryMigrationEvent(nGroupID, sDeviceUDID, pinCode);
            
            return pinCode;
        }

        private static void SendCanaryMigrationEvent(int nGroupID, string sDeviceUDID, string pinCode)
        {
            if (CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().IsEnabledMigrationEvent(nGroupID, CanaryDeploymentMigrationEvent.DevicePinCode))
            {
                var migrationEvent = new ApiObjects.DataMigrationEvents.DeviceLoginPin()
                {
                    Operation = eMigrationOperation.Create,
                    PartnerId = nGroupID,
                    Udid = sDeviceUDID,
                    Pin = pinCode,
                };
                KafkaPublisher.GetFromTcmConfiguration().Publish(migrationEvent);
            }
        }

        [Obsolete]
        public override ApiObjects.Response.Status SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            ApiObjects.Response.Status status = null;
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            device.Initialize(sDeviceUDID);
            var dDevice = new DomainDevice { Udid = sDeviceUDID, Name = sDeviceName };
            bool isSetSucceeded = device.SetDeviceInfo(dDevice);

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
                            oDomainCache.RemoveDomain(nGroupID, domain.m_nDomainID);

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

        public override DeviceResponseObject SetDevice(
            int nGroupID,
            DomainDevice dDevice,
            bool allowNullExternalId,
            bool allowNullMacAddress = false,
            bool allowNullDynamicData = false)
        {
            DeviceResponseObject ret = new DeviceResponseObject();
            Device device = new Device(dDevice.Udid, 0, nGroupID, dDevice.Name);
            var init = device.Initialize(dDevice.Udid);

            if (!init)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                return ret;
            }

            //Check if external id already exists
            var device_Id = DeviceRepository.GetDeviceIdByExternalId(nGroupID, dDevice.ExternalId);

            //already exists
            if (!string.IsNullOrEmpty(device_Id) && device.m_id != device_Id)
            {
                ret.m_oDevice = device;
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.ExternalIdAlreadyExists;
                return ret;
            }

            dDevice.Name = string.IsNullOrEmpty(dDevice.Name) ? device.m_deviceName : dDevice.Name;
            bool isSetSucceeded = device.SetDeviceInfo(dDevice, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);

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
                    List<Domain> domains = baseDomain.GetDeviceDomains(dDevice.Udid);
                    if (domains != null && domains.Count > 0)
                    {
                        DomainsCache oDomainCache = DomainsCache.Instance();
                        foreach (var domain in domains)
                        {
                            oDomainCache.RemoveDomain(m_nGroupID, domain.m_nDomainID);
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
