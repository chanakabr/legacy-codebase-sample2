using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Phx.Lib.Log;
using System.Reflection;
using APILogic;
using ApiLogic.Repositories;
using ApiLogic.Users.Services;
using CachingProvider.LayeredCache;
using Google.Protobuf;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace Core.Users
{
    [Serializable]
    [JsonObject(Id = "Device")]
    public class Device : IEquatable<Device>, IDeepCloneable<Device>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string m_id;

        public string m_deviceUDID;

        public string m_deviceBrand;

        public string m_deviceFamily;

        public int m_deviceFamilyID;

        public int m_domainID;

        public string m_deviceName;

        private int m_groupID;

        public int m_deviceBrandID;

        public string m_pin;

        public DateTime m_activationDate;
        public DateTime? m_updateDate;

        public DeviceState m_state;

        public string m_sStreamType;

        public string m_sProfile;

        public string LicenseData;

        public string ExternalId;

        public string MacAddress;

        public List<ApiObjects.KeyValuePair> DynamicData;

        public string Model;
        
        public long? LastActivityTime;

        public string Manufacturer { get; set; }

        public long? ManufacturerId { get; set; }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string deviceName, int domainID)
        {            
            m_id = string.Empty;
            m_deviceUDID = sUDID;

            if (!string.IsNullOrEmpty(sUDID))
            {
                if (nDeviceBrandID > 0)
                {
                    m_deviceBrandID = nDeviceBrandID;
                    var deviceFamilyResponse = DeviceFamilyRepository.Instance.GetByDeviceBrandId(nGroupID, nDeviceBrandID);
                    if (deviceFamilyResponse.IsOkStatusCode())
                    {
                        m_deviceFamily = deviceFamilyResponse.Object.Name;
                        m_deviceFamilyID = deviceFamilyResponse.Object.Id;
                    }
                }
                else
                {
                    int nBrandID = 0;
                    m_deviceFamilyID = DeviceDal.GetDeviceFamilyID(nGroupID, sUDID, ref nBrandID);
                    m_deviceBrandID = nBrandID;
                }
            }

            m_domainID = domainID;
            m_groupID = nGroupID;
            m_deviceName = deviceName;
            m_pin = string.Empty;
            m_activationDate = DateTime.UtcNow;
            m_state = DeviceState.UnKnown;
        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string sDeviceName, int nDomainID, int nDeviceID, int nDeviceFamilyID,
            string sDeviceFamilyName, string sPin, DateTime dtActivationDate, DeviceState eState, DateTime? dtUpdateDate = null)
        {
            m_deviceUDID = sUDID;
            m_deviceBrandID = nDeviceBrandID;
            m_groupID = nGroupID;
            m_deviceName = sDeviceName;
            m_domainID = nDomainID;
            m_id = nDeviceID.ToString();
            m_deviceFamilyID = nDeviceFamilyID;
            m_deviceFamily = sDeviceFamilyName;
            m_pin = sPin;
            m_activationDate = dtActivationDate;
            m_state = eState;
            m_updateDate = dtUpdateDate;

            if (nDeviceBrandID > 0)
            {
                PopulateDeviceStreamTypeAndProfile();
            }

        }

        public Device(Device other) {
            m_id = other.m_id;
            m_deviceUDID = other.m_deviceUDID;
            m_deviceBrand = other.m_deviceBrand;
            m_deviceFamily = other.m_deviceFamily;
            m_deviceFamilyID = other.m_deviceFamilyID;
            m_domainID = other.m_domainID;
            m_deviceName = other.m_deviceName;
            m_groupID = other.m_groupID;
            m_deviceBrandID = other.m_deviceBrandID;
            m_pin = other.m_pin;
            m_activationDate = other.m_activationDate;
            m_updateDate = other.m_updateDate;
            m_state = other.m_state;
            m_sStreamType = other.m_sStreamType;
            m_sProfile = other.m_sProfile;
            LicenseData = other.LicenseData;
            ExternalId = other.ExternalId;
            MacAddress = other.MacAddress;
            DynamicData = Extensions.Clone(other.DynamicData);
            Model = other.Model;
            LastActivityTime = other.LastActivityTime;
            Manufacturer = other.Manufacturer;
            ManufacturerId = other.ManufacturerId;
        }
        
        private void PopulateDeviceStreamTypeAndProfile()
        {
            // 4.12.14. Used only by Vodafone. It is used when VodafoneConditionalAccess calculates NPVR Licensed Link against ALU.
            // When NPVR is extended for Harmonic, and other provider, find a way to abstract those fields, and save it in DB, rather in config.
            m_sStreamType = TVinciShared.WS_Utils.GetTcmConfigValue($"DEVICE_STREAM_TYPE_{m_groupID}_{m_deviceBrandID}");
            m_sProfile = TVinciShared.WS_Utils.GetTcmConfigValue($"DEVICE_PROFILE_{m_groupID}_{m_deviceBrandID}");
        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string sDeviceName)
            : this(sUDID, nDeviceBrandID, nGroupID, sDeviceName, 0)
        {

        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID)
            : this(sUDID, nDeviceBrandID, nGroupID, string.Empty, 0)
        {

        }

        public Device(int nGroupID)
            : this(string.Empty, 0, nGroupID)
        {

        }

        public Device()
            : this(string.Empty, 0, 0)
        {
        }

        public bool Initialize(string sDeviceUDID)
        {
            bool result = InitDeviceInfo(sDeviceUDID, true);
            return result;
        }

        public bool Initialize(int nDeviceID)
        {
            bool result = InitDeviceInfo(nDeviceID.ToString(), false);
            return result;
        }
        
        public long Save(int isActive, int status = 1, DomainDevice device = null, bool allowNullExternalId = false, bool allowNullMacAddress = false, bool allowNullDynamicData = false)
        {
            if (device == null)
            {
                device = new DomainDevice();
            }

            m_deviceName = string.IsNullOrEmpty(device.Name) ? m_deviceName : device.Name;

            return Save(isActive, status, device.DeviceId, device.MacAddress, device.ExternalId, device.Model, device.ManufacturerId, device.Manufacturer, device.DynamicData, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);
        }

        public long Save(
            int isActive,
            int status = 1,
            long? deviceId = null,
            string macAddress = "",
            string externalId = "",
            string model = "",
            long? manufacturerId = null,
            string manufacturer = null,
            List<ApiObjects.KeyValuePair> dynamicData = null,
            bool allowNullExternalId = false,
            bool allowNullMacAddress = false,
            bool allowNullDynamicData = false)
        {
            var retVal = deviceId > 0
                ? deviceId.Value
                : DeviceDal.GetDeviceId(m_deviceUDID, m_groupID, m_deviceBrandID, m_deviceFamilyID, status);

            bool deviceFound = retVal > 0;

            log.Debug($"Device for Save: {nameof(isActive)}: {isActive}, {nameof(status)}: {status}, {nameof(deviceId)}: {deviceId}, " +
                      $"{nameof(macAddress)}: {macAddress}, {nameof(externalId)}: {externalId}, {nameof(model)}: {model}, manufacturerId: {manufacturerId}, " +
                      $"{nameof(dynamicData)}: {{{string.Join(", ", dynamicData?.Select(pair => $"\"{pair.key}\": \"{pair.value}\"") ?? Enumerable.Empty<string>())}}}, " +
                      $"{nameof(allowNullExternalId)}: {allowNullExternalId}, {nameof(allowNullMacAddress)}: {allowNullMacAddress}, {nameof(allowNullDynamicData)}: {allowNullDynamicData}, " +
                      $"{nameof(deviceFound)}: {deviceFound}, {nameof(retVal)}: {retVal}");

            if (!deviceFound) // New Device
            {
                retVal = DeviceDal.InsertNewDevice(m_deviceUDID, m_deviceBrandID, m_deviceFamilyID, m_deviceName, m_groupID, isActive, status, m_pin, externalId, macAddress, model, manufacturerId, dynamicData);
            }
            else // Update Device
            {
                bool bUpdateRetVal = DeviceDal.UpdateDevice(retVal, m_deviceUDID, m_deviceBrandID, m_deviceFamilyID, m_groupID, m_deviceName, isActive, status, externalId, macAddress, model, manufacturerId, dynamicData, allowNullExternalId, allowNullMacAddress, allowNullDynamicData);

                if (!bUpdateRetVal)
                {
                    retVal = 0;
                }
            }

            m_id = retVal.ToString();

            InvalidateDomainDevice();

            if (retVal > 0)
            {
                SetUpdatedValues(externalId, macAddress, model, manufacturerId, manufacturer, dynamicData);
            }

            return retVal;
        }

        public bool SetDeviceInfo(DomainDevice device, bool allowNullExternalId = false, bool allowNullMacAddress = false, bool allowNullDynamicData = false)
        {
            bool res = false;

            log.Debug($"SetDeviceInfo: DomainDevice: {JsonConvert.SerializeObject(device)}, device: {ToString()} " +
                $"m_state: {m_state}");

            if (m_state >= DeviceState.Pending)
            {
                var nDeviceID = Save(-1, 1, device, allowNullExternalId, allowNullMacAddress, allowNullDynamicData); // Returns device ID, 0 otherwise
                if (nDeviceID != 0)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }


        private bool InitDeviceInfo(string sID, bool isUDID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            try
            {
                if (string.IsNullOrEmpty(sID))
                {
                    return false;
                }

                DataTable dtDeviceInfo = DeviceDal.Get_DeviceInfo(sID, isUDID, m_groupID);

                if (dtDeviceInfo == null)
                {
                    m_state = DeviceState.Error;
                    return false;
                }

                int count = dtDeviceInfo.Rows.Count;

                if (count == 0) // Not found
                {
                    m_state = DeviceState.NotExists;
                    return false;
                }

                // Device found

                DataRow dr = dtDeviceInfo.Rows[0];
                int nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                m_id = nDeviceID.ToString();
                m_deviceUDID = ODBCWrapper.Utils.GetSafeStr(dr["device_id"]);
                m_deviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_brand_id"]);
                m_deviceName = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                m_groupID = ODBCWrapper.Utils.GetIntSafeVal(dr["group_id"]);
                m_deviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_family_id"]);
                m_pin = ODBCWrapper.Utils.GetSafeStr(dr["pin"]);
                ExternalId = ODBCWrapper.Utils.GetSafeStr(dr["external_id"]);
                MacAddress = ODBCWrapper.Utils.GetSafeStr(dr["mac_address"]);
                DynamicData = DeviceDal.DeserializeDynamicData(ODBCWrapper.Utils.GetSafeStr(dr["dynamic_data"]));
                Model = ODBCWrapper.Utils.GetSafeStr(dr["model"]);
                ManufacturerId = ODBCWrapper.Utils.GetIntSafeVal(dr["manufacturer_id"]);
                if (ManufacturerId.HasValue && ManufacturerId.Value > 0)
                {
                    var deviceReferenceData = ApiLogic.Users.Managers.DeviceReferenceDataManager.Instance.GetByManufacturerId(m_groupID, ManufacturerId.Value);
                    Manufacturer = deviceReferenceData?.Name;
                }
                LastActivityTime = DeviceRemovalPolicyHandler.Instance.GetUdidLastActivity(m_groupID, m_deviceUDID);

                PopulateDeviceStreamTypeAndProfile();

                int nDeviceActive = ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]);
                if (nDeviceActive == 0)
                {
                    m_state = DeviceState.Pending;
                    return true;
                }


                selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery1 += "select domain_id, last_activation_date, is_active,status from domains_devices with (nolock) where status=1 and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    count = selectQuery1.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        m_domainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "domain_id", 0);
                        m_activationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);
                        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
                        int nStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "status", 0);

                        m_state = (nActive == 1) ? DeviceState.Activated :
                                                                ((nStatus == 3) ? DeviceState.Pending : DeviceState.UnActivated);
                    }
                    else
                    {
                        m_state = DeviceState.UnActivated;
                    }
                }
                else
                {
                    m_state = DeviceState.Error;
                }

                return true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at InitDeviceInfo. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" ID: ", sID));
                sb.Append(String.Concat(" IsUDID: ", isUDID.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }
            finally
            {
                if (selectQuery1 != null)
                {
                    selectQuery1.Finish();
                }
            }

            return false;

        }

        public bool IsActivated()
        {
            return m_state == DeviceState.Activated;
        }


        public bool Equals(Device other)
        {
            return m_deviceFamilyID == other.m_deviceFamilyID && m_deviceUDID.Equals(other.m_deviceUDID);
        }

        public Device Clone()
        {
            return new Device(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat(" Device ID: ", m_id));
            sb.Append(String.Concat(" Device UDID: ", m_deviceUDID));
            sb.Append(String.Concat(" Device Brand ID: ", m_deviceBrandID));
            sb.Append(String.Concat(" Device Family ID: ", m_deviceFamilyID));
            sb.Append(String.Concat(" Group ID: ", m_groupID));
            sb.Append(String.Concat(" Domain ID: ", m_domainID));
            sb.Append(String.Concat(" State: ", m_state.ToString()));

            return sb.ToString();
        }

        private void InvalidateDomainDevice()
        {
            List<string> invalidationKeys = new List<string>()
            {
                LayeredCacheKeys.GetDomainDeviceInvalidationKey(m_groupID, m_domainID, m_id)
            };

            LayeredCache.Instance.InvalidateKeys(invalidationKeys);
        }

        private void SetUpdatedValues(string externalId, string macAddress, string model, long? manufacturerId, string manufacturer, List<ApiObjects.KeyValuePair> dynamicData)
        {
            if (externalId != null)
            {
                ExternalId = externalId;
            }

            if (macAddress != null)
            {
                MacAddress = macAddress;
            }

            if (model != null)
            {
                Model = model;
            }

            if (manufacturerId.HasValue)
            {
                ManufacturerId = manufacturerId;
                Manufacturer = manufacturer;
            }

            if (dynamicData != null)
            {
                DynamicData = dynamicData.Any() ? dynamicData : null;
            }
        }
    }

    public class DeviceRepository
    {
        private static readonly Dictionary<string, DeviceState> stateDict = new Dictionary<string, DeviceState> {
                { "UnKnown", DeviceState.UnKnown },
                { "Error", DeviceState.Error },
                { "NotExists", DeviceState.NotExists },
                { "Pending", DeviceState.Pending },
                { "Activated", DeviceState.Activated },
                { "UnActivated", DeviceState.UnActivated }
            };

        // method doesn't fill all fields(externalId, dynamicData), TryGet is better, but not tested
        public static Device Get(string udid, int domainId, int groupId)
        {
            var device = new Device(groupId);
            var deviceId = DeviceDal.GetDeviceIdByUDID(udid, groupId);
            if (deviceId <= 0)
            {
                device.m_state = DeviceState.NotExists;
                return device;
            }
            
            device.m_id = deviceId.ToString();

            string sDbState = string.Empty;

            DeviceDal.InitDeviceInDb(
                deviceId,
                domainId,
                groupId,
                ref device.m_deviceUDID,
                ref device.m_deviceBrandID,
                ref device.m_deviceName, 
                ref device.m_deviceFamilyID,
                ref device.m_pin,
                ref device.m_activationDate,
                ref sDbState);            

            device.m_state = stateDict[sDbState];

            return device;
        }

        // not used for now, should be instead of initialize
        public static bool TryGet(string udid, int groupId, out Device device)
        {
            return TryGet(udid, true, groupId, out device);
        }

        public static bool TryGet(int deviceId, int groupId, out Device device)
        {
            return TryGet(deviceId.ToString(), false, groupId, out device);
        }

        private static bool TryGet(string sID, bool isUDID, int m_groupID, out Device device)
        {
            device = null;
            if (string.IsNullOrEmpty(sID))
            {
                return false;
            }

            string dbState = string.Empty;
            int m_id = 0;
            string m_deviceUDID = string.Empty;
            int m_deviceBrandID = 0;
            string m_deviceName = string.Empty;
            int m_deviceFamilyID = 0;
            string m_deviceFamily = string.Empty;
            string m_pin = string.Empty;
            string externalId = null;
            string macAddress = null;
            string model = null;
            long? manufacturerId = null;
            List<ApiObjects.KeyValuePair> dynamicData = null;
            int m_domainID = 0;
            DateTime m_activationDate = DateTime.UtcNow;

            var success = DeviceDal.InitDeviceInfo(sID, isUDID, m_groupID,
                ref dbState,
                ref m_id,
                ref m_deviceUDID,
                ref m_deviceBrandID,
                ref m_deviceName,
                ref m_deviceFamilyID,
                ref m_pin,
                ref externalId,
                ref macAddress,
                ref model,
                ref manufacturerId,
                ref dynamicData,
                ref m_domainID,
                ref m_activationDate);

            if (!success) return false;

            var m_state = stateDict[dbState];

            if (success && m_deviceBrandID > 0)
            {
                var deviceFamilyResponse = DeviceFamilyRepository.Instance.GetByDeviceBrandId(m_groupID, m_deviceBrandID);
                if (deviceFamilyResponse.IsOkStatusCode())
                {
                    m_deviceFamily = deviceFamilyResponse.Object.Name;
                }
            }

            string manufacturer = null;
            if (manufacturerId.HasValue && manufacturerId.Value > 0)
            {
                var deviceReferenceData = ApiLogic.Users.Managers.DeviceReferenceDataManager.Instance.GetByManufacturerId(m_groupID, manufacturerId.Value);
                manufacturer = deviceReferenceData?.Name;
            }

            device = new Device(m_deviceUDID, m_deviceBrandID, m_groupID, m_deviceName, m_domainID, m_id,
                m_deviceFamilyID, m_deviceFamily, m_pin, m_activationDate, m_state)                
            {
                ExternalId = externalId,
                MacAddress = macAddress,
                Model = model,
                ManufacturerId = manufacturerId,
                Manufacturer = manufacturer,
                DynamicData = dynamicData
            };

            return true;
        }

        public static string GetDeviceIdByExternalId(int nGroupID, string externalId)
        {
            return DeviceDal.GetDeviceIdByExternalId(nGroupID, externalId);
        }

        public static string GenerateNewPIN(int groupId)
        {
            return DeviceDal.GenerateNewPIN(groupId);
        }
    }
}
