
using ApiObjects.Base;
using AutoMapper;
using Core.Pricing;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using WebAPI.Models.Domains;
using WebAPI.Models.Pricing;

namespace WebAPI.Utils
{
    public class PricingUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        internal static KalturaCouponListResponse GetCouponListResponse(ContextData contextData, KalturaHouseholdCoupon householdCoupon)
        {
            CouponDataResponse response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponStatus(contextData.GroupId, householdCoupon.Code, contextData.DomainId ?? 0);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);                
            }

            KalturaCouponListResponse res = null;
            if (response?.Coupon != null)
            {
                var coupon = Mapper.Map<KalturaCoupon>(response.Coupon);
                res = new KalturaCouponListResponse()
                {
                    Objects = new List<KalturaCoupon>() { coupon },
                    TotalCount = 1
                };
            }
            
            return res;
        }
    }
}