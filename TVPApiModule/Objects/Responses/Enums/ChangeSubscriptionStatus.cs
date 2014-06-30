using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVPApiModule.Objects.Responses
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
