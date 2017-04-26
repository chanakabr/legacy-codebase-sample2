using System;

namespace ApiObjects.Notification
{
    public class Engagement
    {
        public int Id { get; set; }
        public DateTime SendTime { get; set; }
        public int TotalNumberOfRecipients { get; set; }
        public eEngagementType EngagementType { get; set; }
        public int AdapterId { get; set; }
        public string AdapterDynamicData { get; set; }
        public int IntervalSeconds { get; set; }
        public string UserList { get; set; }
        public bool IsActive { get; set; }
        public int CouponGroupId { get; set; }
    }
}
