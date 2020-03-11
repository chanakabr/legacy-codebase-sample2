using System.Collections.Generic;

namespace ApiObjects.Notification
{
    public class RemindersResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<DbReminder> Reminders { get; set; }

        public int TotalCount { get; set; }
    }

    public class SeriesRemindersResponse
    {
        public ApiObjects.Response.Status Status { get; set; }

        public List<DbSeriesReminder> Reminders { get; set; }

        public int TotalCount { get; set; }
    }
}
