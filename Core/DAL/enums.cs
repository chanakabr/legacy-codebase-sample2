using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL
{

    // Must correspond exactly to Users\enums.cs. Please verify if changes were made!!!
    //
    public enum DALUserActivationState
    {
        Error = -2,
        UserDoesNotExist = -1,
        Activated = 0,
        NotActivated = 1,
        NotActivatedByMaster = 2,
        UserRemovedFromDomain = 3,
        UserWIthNoDomain = 4,
        UserDomainSuspended = 5
    }

    public enum DomainSuspentionStatus
    {
        OK = 0,
        Suspended = 1
    }
    
}
