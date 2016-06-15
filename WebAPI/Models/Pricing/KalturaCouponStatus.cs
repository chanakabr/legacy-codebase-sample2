using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Pricing
{
    public enum KalturaCouponStatus
    {
        VALID,
        NOT_EXISTS,
        ALREADY_USED,
        EXPIRED,
        INACTIVE
    }
}