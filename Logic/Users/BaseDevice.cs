using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public abstract class BaseDevice
    {
        protected int m_nGroupID;

        protected BaseDevice() { }
        public BaseDevice(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract string GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID);
        public abstract ApiObjects.Response.Status SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName);
        public abstract DeviceResponseObject SetDevice(int nGroupID, string sDeviceUDID, string sDeviceName);
        public abstract DeviceResponseObject GetDeviceInfo(int nGroupID, string sID, bool bIsUDID);
    }
}
