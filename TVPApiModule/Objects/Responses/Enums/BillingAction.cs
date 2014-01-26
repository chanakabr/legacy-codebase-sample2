using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
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
