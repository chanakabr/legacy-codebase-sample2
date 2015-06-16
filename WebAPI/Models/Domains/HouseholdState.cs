using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Domains
{
    public enum HouseholdState
    {
        ok,
        created_without_npvr_account,
        suspended,
        no_users_in_household
    }
}