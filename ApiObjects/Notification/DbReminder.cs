
using System;
namespace ApiObjects.Notification
{
    public class DbReminder
    {
        public int ID { get; set; }
        public int GroupId { get; set; }
        public string Name { get; set; }
        public long SendTime { get; set; }
        public string Phrase { get; set; }
        public long Reference { get; set; }
        public string QueueId { get; set; }
        public string RouteName { get; set; }
        public bool IsSent { get; set; }
        public string ExternalPushId { get; set; }
        public string ExternalResult { get; set; }
        public string Message { get; set; }
    }

    public class DbSeriesReminder : DbReminder
    {
        public string SeriesId { get; set; }
        public long? SeasonNumber { get; set; }
        public long EpgChannelId { get; set; }
        public DateTime LastSendDate { get; set; }
    }

    public enum ReminderType
    {
        Single,
        Series
    }
}
