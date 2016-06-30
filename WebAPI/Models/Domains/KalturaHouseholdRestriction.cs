using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Domains
{
    public enum KalturaHouseholdRestriction
    {
        NOT_RESTRICTED,
        USER_MASTER_RESTRICTED,
        DEVICE_MASTER_RESTRICTED,
        DEVICE_USER_MASTER_RESTRICTED,
    }
}