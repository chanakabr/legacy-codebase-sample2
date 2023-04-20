using System;

namespace ApiObjects.Pricing.Dto
{
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

    [Serializable]
    public class SubscriptionItemDTO
    {
        public long Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }

    [Serializable]
    public class CollectionItemDTO
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public long? AssetUserRuleId { get; set; }
        public string Name { get; set; }
    }
}
