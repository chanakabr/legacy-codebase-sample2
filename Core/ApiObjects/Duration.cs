using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects
{
    [Serializable]
    [JsonObject]
    public class Duration
    {
        private static readonly Dictionary<long, Tuple<DurationUnit, long>> map = new Dictionary<long, Tuple<DurationUnit, long>>()
            {
            { (long) TvmDurationUnit.OneYear, new Tuple<DurationUnit, long>(DurationUnit.Years, 1) },
            { (long) TvmDurationUnit.TwoYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 2) },
            { (long) TvmDurationUnit.ThreeYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 3) },
            { (long) TvmDurationUnit.FourYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 4) },
            { (long) TvmDurationUnit.FiveYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 5) },
            { (long) TvmDurationUnit.TenYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 10) },
            { (long) TvmDurationUnit.OneHundredYears, new Tuple<DurationUnit, long>(DurationUnit.Years, 100) },
            { (long) TvmDurationUnit.OneMonth, new Tuple<DurationUnit, long>(DurationUnit.Months, 1) },
            { (long) TvmDurationUnit.TwoMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 2) },
            { (long) TvmDurationUnit.ThreeMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 3) },
            { (long) TvmDurationUnit.FourMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 4) },
            { (long) TvmDurationUnit.FiveMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 5) },
            { (long) TvmDurationUnit.SixMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 6) },
            { (long) TvmDurationUnit.SevenMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 7) },
            { (long) TvmDurationUnit.EightMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 8) },
            { (long) TvmDurationUnit.NineMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 9) },
            { (long) TvmDurationUnit.TenMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 10) },
            { (long) TvmDurationUnit.ElevenMonths, new Tuple<DurationUnit, long>(DurationUnit.Months, 11)},
            { (long) TvmDurationUnit.OneWeeks, new Tuple<DurationUnit, long>(DurationUnit.Weeks, 1) },
            { (long) TvmDurationUnit.TwoWeeks, new Tuple<DurationUnit, long>(DurationUnit.Weeks, 2) },
            { (long) TvmDurationUnit.ThreeWeeks, new Tuple<DurationUnit, long>(DurationUnit.Weeks, 3) },
            { (long) TvmDurationUnit.FourWeeks, new Tuple<DurationUnit, long>(DurationUnit.Weeks, 4) },
            { (long) TvmDurationUnit.OneDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 1) },
            { (long) TvmDurationUnit.TwoDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 2) },
            { (long) TvmDurationUnit.ThreeDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 3) },
            { (long) TvmDurationUnit.FiveDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 5) },
            { (long) TvmDurationUnit.TenDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 10) },
            { (long) TvmDurationUnit.ThirtyDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 30) },
            { (long) TvmDurationUnit.ThirtyOneDays, new Tuple<DurationUnit, long>(DurationUnit.Days, 31) },
            { (long) TvmDurationUnit.OneHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 1) },
            { (long) TvmDurationUnit.TwoHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 2) },
            { (long) TvmDurationUnit.ThreeHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 3) },
            { (long) TvmDurationUnit.SixHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 6) },
            { (long) TvmDurationUnit.NineHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 9) },
            { (long) TvmDurationUnit.TwelveHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 12) },
            { (long) TvmDurationUnit.EighteenHours, new Tuple<DurationUnit, long>(DurationUnit.Hours, 18) },
            { (long) TvmDurationUnit.OneMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 1) },
            { (long) TvmDurationUnit.ThreeMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 3) },
            { (long) TvmDurationUnit.FiveMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 5) },
            { (long) TvmDurationUnit.TenMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 10) },
            { (long) TvmDurationUnit.FifteenMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 15) },
            { (long) TvmDurationUnit.ThirtyMinutes, new Tuple<DurationUnit, long>(DurationUnit.Minutes, 30) }
        };

        [JsonProperty]
        public DurationUnit Unit { get; set; }

        [JsonProperty]
        public long Value { get; set; }

        [JsonIgnore]
        public long TvmCode { get; set; }

        public Duration()
        {
        }

        public Duration(DurationUnit durationUnit, long value)
        {
            this.Unit = durationUnit;
            this.Value = value;
            this.TvmCode = this.GetTvmDuration();
        }

        public Duration(long tvmDuration)
        {
            this.TvmCode = tvmDuration;

            if (!map.ContainsKey(tvmDuration))
            {
                this.Unit = DurationUnit.Minutes;
                this.Value = tvmDuration;
            }
            else
            {
                this.Unit = map[tvmDuration].Item1;
                this.Value = map[tvmDuration].Item2;
            }
        }

        public long GetTvmDuration()
        {
            var duration = map.FirstOrDefault(ele => ele.Value.Item1 == this.Unit && ele.Value.Item2 == this.Value);

            if (!default(KeyValuePair<long, Tuple<DurationUnit, long>>).Equals(duration))
            {
                return duration.Key;
            }

            return 0;
        }

        public bool Equals(Duration other)
        {
            if (other == null)
            {
                return false;
            }

            return (this.Value == other.Value) && (this.Unit == other.Unit);
        }

        public static List<Duration> GetDurationsByUnit(DurationUnit unit)
        {
            return map.Where(x => x.Value.Item1 == unit).Select(x => new Duration(x.Value.Item1, x.Value.Item2)).ToList();
        }

        public bool IsMonthlyLifeCycle()
        {
           return this.Unit == DurationUnit.Months;
        }

        // Canot get Duration object because by defult its Minutes DurationUnit and nedded different treatment in case code not exists 
        public static DateTime GetSlidingWindowStart(long minPeriodId)
        {
            var duration = new Duration(minPeriodId);
            switch (duration.Unit)
            {
                case DurationUnit.Minutes:
                    return DateTime.UtcNow.AddMinutes(-duration.Value);
                case DurationUnit.Hours:
                    return DateTime.UtcNow.AddHours(-duration.Value);
                case DurationUnit.Days:
                    return DateTime.UtcNow.AddDays(-duration.Value);
                case DurationUnit.Weeks:
                    return DateTime.UtcNow.AddDays(-(int)(duration.Value * 7));
                case DurationUnit.Months:
                    return DateTime.UtcNow.AddMonths(-(int)duration.Value);
                case DurationUnit.Years:
                    return DateTime.UtcNow.AddYears(-(int)duration.Value);
                default:
                    return DateTime.MinValue;
            }
        }

        public static int GetDaysFromDuration(int minPeriodId)
        {
            var duration = new Duration(minPeriodId);
            if (duration.Unit == DurationUnit.Months)
            {
                TimeSpan ts = (DateTime.Today - DateTime.Today.AddMonths(-(int)duration.Value));
                return ts.Days;
            }
            else if (duration.Unit == DurationUnit.Years)
            {
                TimeSpan ts = (DateTime.Today - DateTime.Today.AddYears(-(int)duration.Value));
                return ts.Days;
            }
            else
            {
                return (int)duration.TvmCode / 1440;
            }
        }

        public override string ToString()
        {
            return $"{this.Value} {this.Unit}";
        }
    }

    public enum DurationUnit
    {
        Minutes = 0,
        Hours = 1,
        Days = 2,
        Weeks = 3,
        Months = 4,
        Years = 5
    }

    public enum TvmDurationUnit
    {
        OneMinutes      = 1, // mins
        ThreeMinutes    = 3,
        FiveMinutes     = 5,
        TenMinutes      = 10,
        FifteenMinutes  = 15,
        ThirtyMinutes   = 30, 
        OneHours        = 60, //hours
        TwoHours        = 120,
        ThreeHours      = 180,
        SixHours        = 360,
        NineHours       = 540,
        TwelveHours     = 720,
        EighteenHours   = 1080, 
        OneDays         = 1440, //Days
        TwoDays         = 2880,
        ThreeDays       = 4320,
        FiveDays        = 7200,
        TenDays         = 14400,
        ThirtyDays      = 43200,
        ThirtyOneDays   = 44600, 
        OneWeeks        = 10080,//weeks
        TwoWeeks        = 20160,
        ThreeWeeks      = 30240,
        FourWeeks       = 40320, 
        OneMonth        = 1111111, //Month bigger then 1111111 and smaller then 11111111
        TwoMonths       = 2222222,
        ThreeMonths     = 3333333,
        FourMonths      = 4444444,
        FiveMonths      = 5555555,
        SixMonths       = 6666666,
        SevenMonths     = 7777777,
        EightMonths     = 8888888,
        NineMonths      = 9999999,
        TenMonths       = 10000000,
        ElevenMonths    = 11000000,
        OneYear         = 11111111, //Years bigget then 11111111
        TwoYears        = 22222222,
        ThreeYears      = 33333333,
        FourYears       = 44444444,
        FiveYears       = 55555555,
        TenYears        = 100000000,
        OneHundredYears = 999999999
    }
}