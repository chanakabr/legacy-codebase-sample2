using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaPaymentMethodType
    {
        unknown,
        credit_card,
        sms,
        pay_pal,
        debit_card,
        ideal,
        incaso,
        gift,
        visa,
        master_card,
        in_app,
        m1,
        change_subscription,
        offline
    }
}