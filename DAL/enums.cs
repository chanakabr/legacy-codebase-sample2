using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL
{
    public enum UserActivationState
    {
        Error = -2,
        UserDoesNotExist = -1,
        Activated = 0,
        NotActivated = 1,
        NotActivatedByMaster = 2,
        UserRemovedFromDomain = 3,
        UserNotInDomain = 4         // Or removed 
        
    }
    
}
