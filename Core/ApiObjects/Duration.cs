using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace ApiObjects
{
    [Serializable]
    [JsonObject]
    public class Duration
    {
        [JsonProperty]
        public DurationUnit Unit { get; set; }

        [JsonProperty]
        public long Value { get; set; }

        public Duration()
        {
        }

        public Duration(DurationUnit durationUnit, long value)
        {
            this.Unit = durationUnit;
            this.Value = value;
        }

        public Duration(long tvmDuration)
        {
            if (tvmDuration == (long)TvmDurationUnit.OneMonth)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 1;
            }
            else if (tvmDuration == (long)TvmDurationUnit.TwoMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 2;
            }
            else if (tvmDuration == (long)TvmDurationUnit.ThreeMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 3;
            }
            else if (tvmDuration == (long)TvmDurationUnit.FourMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 4;
            }
            else if (tvmDuration == (long)TvmDurationUnit.FiveMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 5;
            }
            else if (tvmDuration == (long)TvmDurationUnit.SixMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 6;
            }
            else if (tvmDuration == (long)TvmDurationUnit.NineMonths)
            {
                this.Unit = DurationUnit.Months;
                this.Value = 9;
            }
            else if (tvmDuration == (long)TvmDurationUnit.OneYear)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 1;
            }
            else if (tvmDuration == (long)TvmDurationUnit.TwoYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 2;
            }
            else if (tvmDuration == (long)TvmDurationUnit.ThreeYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 3;
            }
            else if (tvmDuration == (long)TvmDurationUnit.FourYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 4;
            }
            else if (tvmDuration == (long)TvmDurationUnit.FiveYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 5;
            }
            else if (tvmDuration == (long)TvmDurationUnit.TenYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 10;
            }
            else if (tvmDuration == (long)TvmDurationUnit.OneHundredYears)
            {
                this.Unit = DurationUnit.Years;
                this.Value = 100;
            }
            else
            {
                if (tvmDuration == 60 * 24)
                {
                    this.Unit = DurationUnit.Days;
                    this.Value = 1;
                }
                else
                {
                    this.Unit = DurationUnit.Minutes;
                    this.Value = tvmDuration;
                }
            }
        }

        public long GetTvmDuration()
        {
            switch (this.Unit)
            {
                case DurationUnit.Minutes:
                    return this.Value;
                case DurationUnit.Hours:
                    return (this.Value * 60);
                case DurationUnit.Days:
                    return (this.Value * 60 * 24);
                case DurationUnit.Months:
                    if (this.Value == 1)
                        return (long)TvmDurationUnit.OneMonth;
                    else if (this.Value == 2)
                        return (long)TvmDurationUnit.TwoMonths;
                    else if (this.Value == 3)
                        return (long)TvmDurationUnit.ThreeMonths;
                    else if (this.Value == 4)
                        return (long)TvmDurationUnit.FourMonths;
                    else if (this.Value == 5)
                        return (long)TvmDurationUnit.FiveMonths;
                    else if (this.Value == 6)
                        return (long)TvmDurationUnit.SixMonths;
                    else if (this.Value == 9)
                        return (long)TvmDurationUnit.NineMonths;
                    else
                        return 0;
                case DurationUnit.Years:
                    if (this.Value == 1)
                        return (long)TvmDurationUnit.OneYear;
                    else if (this.Value == 2)
                        return (long)TvmDurationUnit.TwoYears;
                    else if (this.Value == 3)
                        return (long)TvmDurationUnit.ThreeYears;
                    else if (this.Value == 4)
                        return (long)TvmDurationUnit.FourYears;
                    else if (this.Value == 5)
                        return (long)TvmDurationUnit.FiveYears;
                    else if (this.Value == 10)
                        return (long)TvmDurationUnit.TenYears;
                    else if (this.Value == 100)
                        return (long)TvmDurationUnit.OneHundredYears;
                    else
                        return 0;
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
    }

    public enum DurationUnit
    {
        Minutes = 0,
        Hours = 1,
        Days = 2,
        Months = 3,
        Years = 4
    }

    public enum TvmDurationUnit
    {
        OneMonth = 1111111,
        TwoMonths = 2222222,
        ThreeMonths = 3333333,
        FourMonths = 4444444,
        FiveMonths = 5555555,
        SixMonths = 6666666,
        NineMonths = 9999999,
        OneYear = 11111111,
        TwoYears = 22222222,
        ThreeYears = 33333333,
        FourYears = 44444444,
        FiveYears = 55555555,
        TenYears = 100000000,
        OneHundredYears = 999999999
    }
}
