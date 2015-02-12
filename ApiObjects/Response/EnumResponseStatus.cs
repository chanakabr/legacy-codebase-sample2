using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.Response
{
    public enum eResponseStatus
    {
        OK = 0,
        Error = 1,

        // Domain Section 1000 - 1999

        DomainAlreadyExists = 1000,
        ExceededLimit = 1001,
        DeviceTypeNotAllowed = 1002,
        DeviceNotInDomin = 1003,
        MasterEmailAlreadyExists = 1004,
        UserNotInDomain = 1005,
        DomainNotExists = 1006,
        HouseholdUserFailed = 1007,
        DomainCreatedWithoutNPVRAccount = 1008,
        DomainSuspended = 1009,
        DlmNotExist = 1010,
        WrongPasswordOrUserName = 1011,
        NoDlmId = 1012,

        // User Section 2000 - 2999
        UserNotExists = 2000,

        // CAS Section 3000 - 3999

        InvalidPurchase = 3000,
        CancelationWindowPeriodExpired = 3001,
        SubscriptionNotRenewable = 3002,

    }
}
