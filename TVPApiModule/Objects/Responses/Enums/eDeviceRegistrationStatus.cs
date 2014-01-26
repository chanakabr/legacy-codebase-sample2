using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    [Serializable]
    public enum eDeviceRegistrationStatus 
    { 
        Success = 0, 
        Invalid = 1, 
        Error = 2 
    }
}
