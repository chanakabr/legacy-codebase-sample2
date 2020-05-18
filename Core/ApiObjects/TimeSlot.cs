using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class TimeSlot
    {
        public long? StartDateInSeconds { get; set; }

        public long? EndDateInSeconds { get; set; }

        public bool HasTimeSlot()
        {
            return StartDateInSeconds.HasValue || EndDateInSeconds.HasValue;
        }

        public bool IsValid()
        {
            DateTime now = DateTime.UtcNow;
            long unix = (long)(now - GetTruncDateTimeUtc()).TotalSeconds;
            
            if (StartDateInSeconds.HasValue && StartDateInSeconds.Value > unix)
                return false;

            if (EndDateInSeconds.HasValue && EndDateInSeconds.Value < unix)
                return false;           

            return true;
        }

        private static DateTime GetTruncDateTimeUtc()
        {
            DateTime truncDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return truncDateTimeUtc;
        }
    }
}