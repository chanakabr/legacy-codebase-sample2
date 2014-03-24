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
        UserNotInDomain = 3         // Or removed 
    }
    
}
