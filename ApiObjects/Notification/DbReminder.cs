
namespace ApiObjects.Notification
{
    public class DbReminder
    {
        public int ID { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        public long SendTime { get; set; }
        public string Phrase { get; set; }
        public string Reference { get; set; }
        public string QueueId { get; set; }
        public string QueueName { get; set; }
        public bool IsSent { get; set; }
    }
}
