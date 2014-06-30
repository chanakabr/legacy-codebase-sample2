using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
{
    public enum DomainRestriction
    {
        /// <remarks/>
        Unrestricted,

        /// <remarks/>
        UserMasterRestricted,

        /// <remarks/>
        DeviceMasterRestricted,

        /// <remarks/>
        DeviceUserMasterRestricted,
    }
}
