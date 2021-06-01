using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
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
            string pinCode = string.Empty;

            if (CanaryDeploymentFactory.Instance.GetCanaryDeploymentManager().
                IsDataOwnershipFlagEnabled(nGroupID, CanaryDeploymentDataOwnershipEnum.AuthenticationDeviceLoginPin))
            {
                var authClient = AuthenticationGrpcClientWrapper.AuthenticationClient.GetClientFromTCM();
                pinCode = authClient.GenerateDeviceLoginPin(nGroupID, sDeviceUDID, nBrandID);
            }
            else
            {
                pinCode = GetPinForDeviceFromDB(nGroupID, sDeviceUDID, nBrandID);
            }

            return pinCode;
        }

        private static string GetPinForDeviceFromDB(int nGroupID, string sDeviceUDID, int nBrandID)
        {
            string pinCode;
            Device device = new Device(sDeviceUDID, nBrandID, nGroupID);
            device.Initialize(sDeviceUDID);

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
                RemoveDomainFromCache(m_nGroupID, sDeviceUDID);

                status = ApiObjects.Response.Status.Ok;
            }
            else
            {
                status = Utils.ConvertDeviceStateToResponseObject(device.m_state);
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

            bool isSetSucceeded = device.SetDeviceInfo(dDevice, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);

            // in case set device Succeeded
            // domain should be remove from the cache 
            if (isSetSucceeded)
            {
                RemoveDomainFromCache(m_nGroupID, dDevice.Udid);

                ret.m_oDeviceResponseStatus = DeviceResponseStatus.OK;
                ret.m_oDevice = device;
            }
            else
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
                ret.m_oDevice = device;
                if (device.m_state == DeviceState.NotExists)
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
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

        public override GenericResponse<ApiObjects.KeyValuePair> UpsertDeviceDynamicData(string udid, ApiObjects.KeyValuePair value)
        {
            GenericResponse<ApiObjects.KeyValuePair> response;

            var device = new Device(m_nGroupID);
            var deviceInitialized = device.Initialize(udid);
            if (deviceInitialized)
            {
                var newDynamicData = device.DynamicData ?? new List<ApiObjects.KeyValuePair>();
                var existingItem = newDynamicData.FirstOrDefault(x => x.key == value.key);
                if (existingItem == null)
                {
                    newDynamicData.Add(value);
                }
                else
                {
                    existingItem.value = value.value;
                }

                var validateStatus = ValidateDynamicData(newDynamicData);
                if (validateStatus.IsOkStatusCode())
                {
                    var domainDevice = new DomainDevice { DynamicData = newDynamicData };
                    var saveResult = device.Save(-1, 1, domainDevice);
                    if (saveResult > 0)
                    {
                        RemoveDomainFromCache(m_nGroupID, udid);
                        
                        response = new GenericResponse<ApiObjects.KeyValuePair>(ApiObjects.Response.Status.Ok, value);
                    }
                    else
                    {
                        var status = Utils.ConvertDeviceStateToResponseObject(device.m_state);
                        response = new GenericResponse<ApiObjects.KeyValuePair>(status);
                    }
                }
                else
                {
                    response = new GenericResponse<ApiObjects.KeyValuePair>(validateStatus);
                }
            }
            else
            {
                response = new GenericResponse<ApiObjects.KeyValuePair>(eResponseStatus.DeviceNotExists);
            }

            return response;
        }

        public override ApiObjects.Response.Status DeleteDeviceDynamicData(string udid, string key)
        {
            ApiObjects.Response.Status response;

            var device = new Device(m_nGroupID);
            var deviceInitialized = device.Initialize(udid);
            if (deviceInitialized)
            {
                var existingItem = device.DynamicData?.FirstOrDefault(x => x.key == key);
                if (existingItem == null)
                {
                    response = new ApiObjects.Response.Status(eResponseStatus.ItemNotFound, $"Dynamic data with key {key} was not found.");
                }
                else
                {
                    var newDynamicData = device.DynamicData.Where(x => x != existingItem).ToList();
                    var domainDevice = new DomainDevice { DynamicData = newDynamicData };
                    var saveResult = device.Save(-1, 1, domainDevice);
                    if (saveResult > 0)
                    {
                        RemoveDomainFromCache(m_nGroupID, udid);

                        response = ApiObjects.Response.Status.Ok;
                    }
                    else
                    {
                        response = Utils.ConvertDeviceStateToResponseObject(device.m_state);
                    }
                }
            }
            else
            {
                response = new ApiObjects.Response.Status(eResponseStatus.DeviceNotExists);
            }

            return response;
        }

        private ApiObjects.Response.Status ValidateDynamicData(IReadOnlyCollection<ApiObjects.KeyValuePair> dynamicData)
        {
            const int maxKeyValues = 5; // numbers from BEO-8671
            const int maxKeyLength = 128;
            const int maxValueLength = 255;

            if (dynamicData.Count > maxKeyValues)
            {
                return new ApiObjects.Response.Status(eResponseStatus.ExceededMaxCapacity, $"The maximum count of items in dynamic data is {maxKeyValues}.");
            }

            foreach (var item in dynamicData)
            {
                if (item.key.Length > maxKeyLength)
                {
                    return new ApiObjects.Response.Status(eResponseStatus.ExceededMaxLength, $"The maximum length of {nameof(item.key)} is {maxKeyLength}.");
                }

                if (item.value?.Length > maxValueLength)
                {
                    return new ApiObjects.Response.Status(eResponseStatus.ExceededMaxLength, $"The maximum length of {nameof(item.value)} is {maxValueLength}.");
                }
            }

            return ApiObjects.Response.Status.Ok;
        }

        private void RemoveDomainFromCache(int groupId, string udid)
        {
            BaseDomain baseDomain = null;
            Utils.GetBaseImpl(ref baseDomain, groupId);

            var domains = baseDomain?.GetDeviceDomains(udid);
            if (domains != null && domains.Count > 0)
            {
                var domainCache = DomainsCache.Instance();
                foreach (var domain in domains)
                {
                    domainCache.RemoveDomain(groupId, domain.m_nDomainID);
                }
            }
        }
    }
}
