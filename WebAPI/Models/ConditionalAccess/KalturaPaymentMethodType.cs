using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaPaymentMethodType
    {
        UNKNOWN,
        CREDIT_CARD,
        SMS,
        PAY_PAL,
        DEBIT_CARD,
        IDEAL,
        INCASO,
        GIFT,
        VISA,
        MASTER_CARD,
        IN_APP,
        M1,
        CHANGE_SUBSCRIPTION,
        OFFLINE
    }
}