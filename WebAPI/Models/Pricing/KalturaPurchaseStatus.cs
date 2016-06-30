using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Pricing
{
    public enum KalturaPurchaseStatus
    {
        PPV_PURCHASED,
        FREE,
        FOR_PURCHASE_SUBSCRIPTION_ONLY,
        SUBSCRIPTION_PURCHASED,
        FOR_PURCHASE,
        SUBSCRIPTION_PURCHASED_WRONG_CURRENCY, 
        PRE_PAID_PURCHASED,
        GEO_COMMERCE_BLOCKED, 
        ENTITLED_TO_PREVIEW_MODULE, 
        FIRST_DEVICE_LIMITATION, 
        COLLECTION_PURCHASED,
        USER_SUSPENDED,
        NOT_FOR_PURCHASE
    }
}