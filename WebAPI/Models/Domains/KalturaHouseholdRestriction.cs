using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Domains
{
    public enum KalturaHouseholdRestriction
    {
        not_restricted,
        user_master_restricted,
        device_master_restricted,
        device_user_master_restricted,
    }
}