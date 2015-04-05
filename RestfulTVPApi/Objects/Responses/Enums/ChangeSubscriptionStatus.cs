using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum ChangeSubscriptionStatus
    {
        OK,
        UserNotExists,
        OldSubNotExists,
        NewSubNotExits,
        OldSubNotRenewable,
        UserHadNewSub,
        Error
    }
}
