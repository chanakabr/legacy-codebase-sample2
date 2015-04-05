using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
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
