using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public class DeviceRegistration
    {
        public string udid;
        public eDeviceRegistrationStatus reg_status;
    }
}
