using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Users
{
    public class DeviceResponseObject
    {
        public Device m_oDevice;
        public DeviceResponseStatus m_oDeviceResponseStatus;

        public DeviceResponseObject()
        {

        }

        public DeviceResponseObject(Device oDevice, DeviceResponseStatus oDeviceResponseStatus)
        {
            m_oDevice = oDevice;
            m_oDeviceResponseStatus = oDeviceResponseStatus;
        }
    }
}
