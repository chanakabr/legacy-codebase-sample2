using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum BillingItemsType
    {
        Unknown = 0,
        PPV = 1,
        Subscription = 2,
        PrePaid = 3,
        PrePaidExpired = 4,
        Collection = 5
    }
}