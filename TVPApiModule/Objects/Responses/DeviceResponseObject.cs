using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class DeviceResponseObject
    {
        public Device m_oDevice { get; set; }

        public DeviceResponseStatus m_oDeviceResponseStatus { get; set; }
    }

    public enum DeviceResponseStatus
    {

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        DuplicatePin,

        /// <remarks/>
        DeviceNotExists,

        /// <remarks/>
        OK,
    }
}
