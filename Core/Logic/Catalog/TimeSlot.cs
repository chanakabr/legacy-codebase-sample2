using System;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Catalog
{
    public class TimeSlot
    {
        public long? StartDateInSeconds { get; set; }

        public long? EndDateInSeconds { get; set; }

        public long? StartTimeInMinutes { get; set; }

        public long? EndTimeInMinutes { get; set; }

        public List<DayOfTheWeek> DaysOfTheWeek { get; set; }
    }

    public enum DayOfTheWeek
    {
        SUNDAY = 1,
        MONDAY = 2,
        TUESDAY = 3,
        WEDNESDAY = 4,
        THURSDAY = 5,
        FRIDAY = 6,
        SATURDAY = 7
    }
}
