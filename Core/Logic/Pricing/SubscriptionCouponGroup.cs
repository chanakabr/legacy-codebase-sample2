using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;

namespace Core.Pricing
{
    [Serializable]
    public class SubscriptionCouponGroup : CouponsGroup, IDeepCloneable<SubscriptionCouponGroup>
    {
        public DateTime? startDate;
        public DateTime? endDate;

        public SubscriptionCouponGroup()
            : base()
        {
            startDate = null;
            endDate = null;
        }

        public SubscriptionCouponGroup(SubscriptionCouponGroup other) : base(other)
        {
            startDate = other.startDate;
            endDate = other.endDate;
        }

        public void Initialize(DateTime? startDate, DateTime? endDate, CouponsGroup couponGroupData)
        {
            this.startDate = startDate;
            this.endDate = endDate;

            base.Initialize(couponGroupData);
        }

        public SubscriptionCouponGroup Clone()
        {
            return new SubscriptionCouponGroup(this);
        }
    }
}