using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Users
{
    public enum KalturaUserState
    {
        ok,
        user_with_no_household,
        user_created_with_no_role,
        user_not_activated
    }
}