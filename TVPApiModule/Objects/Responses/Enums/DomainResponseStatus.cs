using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public enum DomainResponseStatus
    {

        /// <remarks/>
        LimitationPeriod,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        Error,

        /// <remarks/>
        DomainAlreadyExists,

        /// <remarks/>
        ExceededLimit,

        /// <remarks/>
        DeviceTypeNotAllowed,

        /// <remarks/>
        DeviceNotInDomain,

        /// <remarks/>
        DeviceNotExists,

        /// <remarks/>
        DeviceAlreadyExists,

        /// <remarks/>
        UserNotExistsInDomain,

        /// <remarks/>
        OK,

        /// <remarks/>
        ActionUserNotMaster,

        /// <remarks/>
        UserNotAllowed,

        /// <remarks/>
        ExceededUserLimit,

        /// <remarks/>
        NoUsersInDomain,

        /// <remarks/>
        UserExistsInOtherDomains,

        /// <remarks/>
        DomainNotExists,

        /// <remarks/>
        HouseholdUserFailed,

        /// <remarks/>
        DeviceExistsInOtherDomains,

        /// <remarks/>
        DomainNotInitialized,

        /// <remarks/>
        RequestSent,

        /// <remarks/>
        DeviceNotConfirmed,

        /// <remarks/>
        RequestFailed,

        /// <remarks/>
        InvalidUser,

        /// <remarks/>
        ConcurrencyLimitation,

        /// <remarks/>
        DomainCreatedWithoutNPVRAccount
    }
}
