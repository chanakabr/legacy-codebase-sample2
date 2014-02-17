using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Users
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

        public override string GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID)
        {
            Device device = new Device(sDeviceUDID, nBrandID, nGroupID);
            device.Initialize(sDeviceUDID);
            return device.GetPINForDevice();
        }

        public override bool SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            device.Initialize(sDeviceUDID);
            return device.SetDeviceInfo(sDeviceName);
        }

        public override DeviceResponseObject  GetDeviceInfo(int nGroupID, string sID, bool bIsUDID)
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
                if (device.m_state == DeviceState.Error || device.m_state == DeviceState.UnActivated )
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
