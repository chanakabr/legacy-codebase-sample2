using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum BillingAction
    {
        /// <remarks/>
        Unknown,

        /// <remarks/>
        Purchase,

        /// <remarks/>
        RenewPayment,

        /// <remarks/>
        RenewCancledSubscription,

        /// <remarks/>
        CancelSubscriptionOrder,

        /// <remarks/>
        SubscriptionDateChanged,
    }
}
