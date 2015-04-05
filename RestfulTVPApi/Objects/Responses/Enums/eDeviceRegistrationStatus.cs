using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    [Serializable]
    public enum eDeviceRegistrationStatus 
    {        
        UnKnown,        
        Error,        
        DuplicatePin,        
        DeviceNotExists,
        OK,
    }
}
