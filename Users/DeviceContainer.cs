using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
{
    public class DeviceContainer
    {
        private List<Device> m_DeviceInstances;
        public string m_deviceFamilyName;
        public int m_deviceFamilyID;
        public int m_deviceLimit;
        public int m_deviceConcurrentLimit;
        public LimitationsManager m_oLimitationsManager;

        public DeviceContainer()
        {
        }

        public DeviceContainer(int id, string name, int limit, int nConcurrentLimit = 1)
        {
            m_deviceFamilyID = id;
            m_deviceFamilyName = name;
            m_deviceLimit = limit;
            m_deviceConcurrentLimit = nConcurrentLimit;
            m_DeviceInstances = new List<Device>();
            m_oLimitationsManager = new LimitationsManager();
        }

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
                    if (device.m_state == DeviceState.Activated)
                    {
                        retVal++;
                    }
                }
            }
            return retVal;
        }
    }
}
