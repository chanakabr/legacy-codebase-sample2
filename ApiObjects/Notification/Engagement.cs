using System;

namespace ApiObjects.Notification
{
    public class Engagement
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime SendTime { get; set; }
        public int TotalNumberOfRecipients { get; set; }
        public int TemplateId { get; set; }
        public int AdapterId { get; set; }
        public string AdapterDynamicData { get; set; }
        public int Interval { get; set; }
        public string UserList { get; set; }
    }
}
