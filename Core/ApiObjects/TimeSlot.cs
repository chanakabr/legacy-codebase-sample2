using System;
using System.Collections.Generic;

namespace ApiObjects
{
    public class TimeSlot
    {
        public long? StartDateInSeconds { get; set; }

        public long? EndDateInSeconds { get; set; }

        public long? StartTimeInMinutes { get; set; }

        public long? EndTimeInMinutes { get; set; }

        public HashSet<int> DaysOfWeek { get; set; }

        public bool HasTimeSlot()
        {
            return StartDateInSeconds.HasValue ||
                EndDateInSeconds.HasValue ||
                StartTimeInMinutes.HasValue ||
                EndTimeInMinutes.HasValue ||
                (DaysOfWeek != null && DaysOfWeek.Count > 0);
        }

        public bool IsValid()
        {
            DateTime now = DateTime.UtcNow;
            long unix = 123;

            if (StartDateInSeconds.HasValue && StartDateInSeconds.Value > unix)
                return false;

            if (EndDateInSeconds.HasValue && EndDateInSeconds.Value < unix)
                return false;

            if (DaysOfWeek?.Count > 0 && !DaysOfWeek.Contains((int)now.DayOfWeek))
                return false;

            if (StartTimeInMinutes.HasValue && StartTimeInMinutes.Value > (now.Hour * 60) + now.Minute)
                return false;

            if (EndTimeInMinutes.HasValue && EndTimeInMinutes.Value > (now.Hour * 60) + now.Minute)
                return false;

            return true;
        }
    }
}