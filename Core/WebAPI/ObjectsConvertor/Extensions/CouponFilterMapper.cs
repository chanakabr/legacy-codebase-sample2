using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class CouponFilterMapper
    {
        public static List<string> getCouponCodesIn(this KalturaCouponFilter model)
        {
            if (string.IsNullOrEmpty(model.CouponCodesIn))
                return null;

            return model.CouponCodesIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
