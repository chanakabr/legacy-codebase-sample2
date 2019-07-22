using AutoMapper;
using Core.Pricing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;

namespace WebAPI.Utils
{
    public class PricingUtils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        internal static void SetCouopnData(int groupId, long householdId, KalturaBaseResponseProfile responseProfile, List<KalturaHouseholdCoupon> householdCoupons)
        {
            if (responseProfile != null)
            {
                string profileName = string.Empty;
                KalturaDetachedResponseProfile profile = (KalturaDetachedResponseProfile)responseProfile; // always KalturaDetachedResponseProfile
                if (profile != null)
                {
                    List<KalturaDetachedResponseProfile> profiles = profile.RelatedProfiles;
                    if (profiles != null && profiles.Count > 0)
                    {
                        profileName = profiles.Where(x => x.Filter is KalturaHouseoldCouponCodeFilter).Select(x => x.Name).FirstOrDefault();
                    }
                }

                foreach (var householdCoupon in householdCoupons)
                {
                    householdCoupon.relatedObjects = new SerializableDictionary<string, KalturaListResponse>();
                    KalturaCoupon result = GetCoupon(groupId, householdCoupon.Code, householdId);
                    if (result != null)
                    {
                        KalturaCouponListResponse res = new KalturaCouponListResponse()
                        {
                            Objects = new List<KalturaCoupon>()
                        };
                        res.Objects.Add(result);

                        householdCoupon.relatedObjects.Add(profileName, res);
                    }
                }
            }
        }

        internal static KalturaCoupon GetCoupon(int groupId, string couponCode, long householdId)
        {
            CouponDataResponse response = null;
            KalturaCoupon coupon = new KalturaCoupon();

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.Pricing.Module.GetCouponStatus(groupId, couponCode, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling pricing service. exception: {0}", ex);                
            }

            if (response?.Coupon != null)
            {
                coupon = Mapper.Map<KalturaCoupon>(response.Coupon);
            }

            return coupon;
        }
    }
}