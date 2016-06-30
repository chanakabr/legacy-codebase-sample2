using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaBillingItemsType
    {
        unknown = 0,
        ppv = 1,
        subscription = 2,
        pre_paid = 3,
        pre_paid_expired = 4,
        collection = 5
    }
}