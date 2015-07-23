using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.Models.Pricing
{
    public enum KalturaCouponStatus
    {
        valid,
        not_exists,
        already_used,
        expired,
        not_active
    }
}