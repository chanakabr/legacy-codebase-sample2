using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using APILogic;
using Google.Protobuf;

namespace Core.Users
{   
    [Serializable]
    [JsonObject(Id = "DeviceContainer")]
    public class DeviceContainer : IDeepCloneable<DeviceContainer>
    {
        [XmlIgnore]
        [JsonProperty()]
        private List<Device> m_DeviceInstances;

        [JsonProperty()]
        public string m_deviceFamilyName;

        [JsonProperty()]
        public int m_deviceFamilyID;
       
        [JsonIgnore]
        public int m_deviceLimit;
        
        [JsonIgnore]
        public int m_deviceConcurrentLimit;

        [XmlIgnore]
        [JsonIgnore]
        public LimitationsManager m_oLimitationsManager;

        public DeviceContainer()
        {
        }

        public DeviceContainer(DeviceContainer other) {
            m_DeviceInstances = Extensions.Clone(other.DeviceInstances);
            m_deviceFamilyName = other.m_deviceFamilyName;
            m_deviceFamilyID = other.m_deviceFamilyID;
            m_deviceLimit = other.m_deviceLimit;
            m_deviceConcurrentLimit = other.m_deviceConcurrentLimit;
            m_oLimitationsManager = Extensions.Clone(other.m_oLimitationsManager);
        }

        public DeviceContainer Clone()
        {
            return new DeviceContainer(this);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("DeviceContainer: ");
            sb.Append(String.Concat(" Family Name: ", m_deviceFamilyName));
            sb.Append(String.Concat(" Device Family ID: ", m_deviceFamilyID));
            sb.Append(String.Concat(" Device Limit: ", m_deviceLimit));
            sb.Append(String.Concat(" Device concurrent limit: ", m_deviceConcurrentLimit));
            sb.Append(String.Concat(" num of devices: ", DeviceInstances.Count));
            sb.Append(String.Concat(" Lmts Mngr: ", m_oLimitationsManager != null ? m_oLimitationsManager.ToString() : "null"));

            return sb.ToString();
        }

        public DeviceContainer(int id, string name, int limit, int nConcurrentLimit = 1, int frequency = -1)
        {
            m_deviceFamilyID = id;
            m_deviceFamilyName = name;
            m_deviceLimit = limit;
            m_deviceConcurrentLimit = nConcurrentLimit;
            m_DeviceInstances = new List<Device>();
            m_oLimitationsManager = new LimitationsManager(nConcurrentLimit, limit, frequency);
        }

        [JsonIgnore]
        public List<Device> DeviceInstances
        {
            get
            {
                if (m_DeviceInstances == null)
                {
                    m_DeviceInstances = new List<Device>();
                }
                return m_DeviceInstances;
            }
        }

        public void AddDeviceInstance(Device device)
        {
            if (m_DeviceInstances == null)
            {
                m_DeviceInstances = new List<Device>();
            }
            m_DeviceInstances.Add(device);
        }

        public bool ChangeDeviceInstanceState(string deviceUDID, DeviceState eState)
        {
            bool bRes = false;

            Device deviceToChange = null;
            if (m_DeviceInstances != null)
            {
                foreach (Device device in m_DeviceInstances)
                {
                    if (device.m_deviceUDID.Equals(deviceUDID))
                    {
                        deviceToChange = device;
                        break;
                    }
                }
                if (deviceToChange != null)
                {
                    deviceToChange.m_state = eState;
                    bRes = true;
                }
            }
            return bRes;
        }

        public bool RemoveDeviceInstance(string deviceUDID)
        {
            bool bRes = false;

            if (m_DeviceInstances == null) return false;

            var deviceToRemove = m_DeviceInstances.FirstOrDefault(device => device.m_deviceUDID.Equals(deviceUDID));

            if (deviceToRemove != null)
            {
                bRes = m_DeviceInstances.Remove(deviceToRemove);
            }
            return bRes;
        }

        public int GetActivatedDeviceCount()
        {
            return m_DeviceInstances?.Count(x => x.IsActivated()) ?? 0;
        }

        public bool IsContainingDevice(Device device, ref bool bIsDeviceActivated)
        {
            if (m_DeviceInstances == null) 
                return false;

            for (int i = 0; i < m_DeviceInstances.Count; i++)
            {
                if (m_DeviceInstances[i].Equals(device))
                {
                    bIsDeviceActivated = m_DeviceInstances[i].IsActivated();
                    return true;
                }
            }

            return false;
        }

        public bool IsUnlimitedConcurrency()
        {
            return m_oLimitationsManager != null && m_oLimitationsManager.Concurrency == 0;
        }

        public bool IsUnlimitedQuantity()
        {
            return m_oLimitationsManager != null && m_oLimitationsManager.Quantity == 0;
        }
    }
}
