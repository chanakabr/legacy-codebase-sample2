using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.DRM
{
    public enum DrmSecurityPolicy
    {
        DeviceLevel = 0,     // default configuration is : DRM ID is bound to the specific UDID
        HouseholdLevel = 1,   // DRM ID exists on any one of the devices in the household
    }  
}
