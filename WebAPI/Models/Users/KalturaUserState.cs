using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Users
{
    public enum KalturaUserState
    {
        OK,
        USER_WITH_NO_HOUSEHOLD,
        USER_CREATED_WITH_NO_ROLE,
        USER_NOT_ACTIVATED
    }
}