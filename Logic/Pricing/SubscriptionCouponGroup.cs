using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Pricing
{
    [Serializable]
    public class SubscriptionCouponGroup : CouponsGroup
    {
        public DateTime startDate;
        public DateTime endDate;

        public SubscriptionCouponGroup()
            : base()
        {
            startDate = new DateTime(2000, 1, 1);
            endDate = new DateTime(2020, 1, 1);
        }

        public void Initialize(DateTime startDate, DateTime endDate, CouponsGroup couponGroupData)
        {
            this.startDate = startDate;
            this.endDate = endDate;

            base.Initialize(couponGroupData);
        }
    }
}
