using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaBillingItemsType
    {
        UNKNOWN = 0,
        PPV = 1,
        SUBSCRIPTION = 2,
        PRE_PAID = 3,
        PRE_PAID_EXPIRED = 4,
        COLLECTION = 5
    }
}