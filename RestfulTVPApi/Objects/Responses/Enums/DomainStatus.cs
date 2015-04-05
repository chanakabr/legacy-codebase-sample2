using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum DomainStatus
    {

        /// <remarks/>
        OK,

        /// <remarks/>
        DomainAlreadyExists,

        /// <remarks/>
        ExceededLimit,

        /// <remarks/>
        DeviceTypeNotAllowed,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        DeviceNotInDomin,

        /// <remarks/>
        MasterEmailAlreadyExists,

        /// <remarks/>
        UserNotInDomain,

        /// <remarks/>
        DomainNotExists,

        /// <remarks/>
        HouseholdUserFailed,
    }
}
