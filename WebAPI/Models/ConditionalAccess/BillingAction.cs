using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    public enum BillingAction
    {
        Unknown = 0,
        Purchase = 1,
        RenewPayment = 2,
        RenewCancledSubscription = 3,
        CancelSubscriptionOrder = 4,
        SubscriptionDateChanged = 5
    }
}