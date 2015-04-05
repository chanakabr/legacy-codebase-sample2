using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses.Enums
{
    public enum PriceReason
    {

        /// <remarks/>
        PPVPurchased,

        /// <remarks/>
        Free,

        /// <remarks/>
        ForPurchaseSubscriptionOnly,

        /// <remarks/>
        SubscriptionPurchased,

        /// <remarks/>
        ForPurchase,

        /// <remarks/>
        UnKnown,

        /// <remarks/>
        SubscriptionPurchasedWrongCurrency,

        /// <remarks/>
        PrePaidPurchased,

        /// <remarks/>
        GeoCommerceBlocked,

        /// <remarks/>
        EntitledToPreviewModule,

        /// <remarks/>
        FirstDeviceLimitation
    }
}
