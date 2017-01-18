using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Core.Users
{   
    [Serializable]
    [JsonObject(Id = "DeviceContainer")]
    public class DeviceContainer
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

            Device deviceToRemove = null;
            if (m_DeviceInstances != null)
            {
                foreach (Device device in m_DeviceInstances)
                {
                    if (device.m_deviceUDID.Equals(deviceUDID))
                    {
                        deviceToRemove = device;
                        break;
                    }
                }
                if (deviceToRemove != null)
                {
                    bRes = m_DeviceInstances.Remove(deviceToRemove);
                }
            }
            return bRes;
        }

        public int GetActivatedDeviceCount()
        {
            int retVal = 0;
            if (m_DeviceInstances != null)
            {
                foreach (Device device in m_DeviceInstances)
                {
                    if (device.IsActivated())
                    {
                        retVal++;
                    }
                }
            }
            return retVal;
        }

        public bool IsContainingDevice(Device device, ref bool bIsDeviceActivated)
        {
            if (m_DeviceInstances != null)
            {
                for (int i = 0; i < m_DeviceInstances.Count; i++)
                {
                    if (m_DeviceInstances[i].Equals(device))
                    {
                        bIsDeviceActivated = m_DeviceInstances[i].IsActivated();
                        return true;
                    }
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
