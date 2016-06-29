using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaBillingAction
    {
        UNKNOWN = 0,
        PURCHASE = 1,
        RENEW_PAYMENT = 2,
        RENEW_CANCELED_SUBSCRIPTION = 3,
        CANCEL_SUBSCRIPTION_ORDER = 4,
        SUBSCRIPTION_DATE_CHANGED = 5
    }
}