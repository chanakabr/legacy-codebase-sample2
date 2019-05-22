
namespace ApiObjects.Notification
{
    public class EngagementBulkMessage
    {
        public int Id { get; set; }
        public int EngagementId { get; set; }
        public bool IsSent { get; set; }
        public int IterationOffset { get; set; }
        public int IterationSize { get; set; }
    }
}
