using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Pricing
{
    public enum KalturaPurchaseStatus
    {
        ppv_purchased,
        free,
        for_purchase_subscription_only,
        subscription_purchased,
        for_purchase,
        subscription_purchased_wrong_currency, 
        pre_paid_purchased,
        geo_commerce_blocked, 
        entitled_to_preview_module, 
        first_device_limitation, 
        collection_purchased,
        user_suspended,
        not_for_purchase
    }
}