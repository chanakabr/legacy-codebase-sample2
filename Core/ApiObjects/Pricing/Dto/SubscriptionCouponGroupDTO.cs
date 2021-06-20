using System;

namespace ApiObjects.Pricing.Dto
{
    // TODO move to DAL/DTO
    public class SubscriptionCouponGroupDTO
    {
        public DateTime? StartDate;
        public DateTime? EndDate;
        public string GroupCode;

        public SubscriptionCouponGroupDTO(string groupCode, DateTime? startDate, DateTime? endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            GroupCode = groupCode;
        }
    }
}
