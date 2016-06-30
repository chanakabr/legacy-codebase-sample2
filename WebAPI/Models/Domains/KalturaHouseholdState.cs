using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Domains
{
    public enum KalturaHouseholdState
    {
        OK,
        CREATED_WITHOUT_NPVR_ACCOUNT,
        SUSPENDED,
        NO_USERS_IN_HOUSEHOLD
    }
}