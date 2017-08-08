using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    [Serializable]
    public enum KalturaBillingPriceInfo
    {
        FullPeriod = 0,
        PartialPeriod = 1
    }
}