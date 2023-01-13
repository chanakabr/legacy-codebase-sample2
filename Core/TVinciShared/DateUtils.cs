using ApiObjects.SearchObjects;
using System;
using System.Globalization;

namespace TVinciShared
{
    /// <summary>
    /// Summary description for DateUtils
    /// </summary>
    public static class DateUtils
    {
        public static readonly Type DateTimeType = typeof(DateTime);
        public static readonly Type NullableDateTimeType = typeof(DateTime?);
        public const string MAIN_FORMAT = "dd/MM/yyyy HH:mm:ss";

        public static string GetDateForSchedule(DateTime theDate)
        {
            string sRet = "";
            if (theDate.Day < 10)
                sRet += "0";
            sRet += theDate.Day.ToString();
            sRet += ".";
            if (theDate.Month < 10)
                sRet += "0";
            sRet += theDate.Month.ToString();
            return sRet;
        }

        public static string GetTimeString(DateTime theDate)
        {
            string sRet = "";
            if (theDate.Hour < 10)
                sRet += "0";
            sRet += theDate.Hour.ToString();
            sRet += ":";
            if (theDate.Minute < 10)
                sRet += "0";
            sRet += theDate.Minute.ToString();
            return sRet;
        }

        public static DateTime GetDateFromStr(string sDate)
        {
            try
            {
                if (sDate == "")
                    return DateTime.UtcNow;
                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length > 1)
                    sDate = timeHour[0];
                string[] splited = sDate.Split('/');
                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                nYear = int.Parse(splited[2].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[0].ToString());
                return new DateTime(nYear, nMounth, nDay);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        public static DateTime GetDateFromStrUTF(string sDate)
        {
            try
            {
                if (sDate == "")
                    return DateTime.UtcNow;
                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length > 1)
                    sDate = timeHour[0];
                string[] splited = sDate.Split('/');
                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                nYear = int.Parse(splited[0].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[2].ToString());
                return new DateTime(nYear, nMounth, nDay);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        public static DateTime GetDateTimeFromStrUTF(string sDate)
        {
            try
            {
                string sTime = "";
                if (sDate == "")
                    return DateTime.UtcNow;

                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length == 2)
                {
                    sDate = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.UtcNow;
                string[] splited = sDate.Split('/');

                Int32 nYear = 1;
                Int32 nMounth = 1;
                Int32 nDay = 1;
                Int32 nHour = 0;
                Int32 nMin = 0;
                Int32 nSec = 0;
                nYear = int.Parse(splited[0].ToString());
                nMounth = int.Parse(splited[1].ToString());
                nDay = int.Parse(splited[2].ToString());
                if (timeHour.Length == 2)
                {
                    string[] splited1 = sTime.Split(':');
                    nHour = int.Parse(splited1[0].ToString());
                    nMin = int.Parse(splited1[1].ToString());
                    nSec = int.Parse(splited1[2].ToString());
                }

                return new DateTime(nYear, nMounth, nDay, nHour, nMin, nSec);
            }
            catch
            {
                return DateTime.UtcNow;
            }
        }

        public static string GetStrFromDateExp(DateTime theDate)
        {
            try
            {
                string sDate = "";
                TimeSpan ts = DateTime.UtcNow - theDate;
                if (ts.TotalSeconds <= 1)
                    sDate = "îîù îòëùéå";
                else if (ts.TotalSeconds == 2)
                    sDate = "ìôðé ùúé ùðéåú";
                else if (ts.TotalSeconds == 3)
                    sDate = "ìôðé ùìåù ùðéåú";
                else if (ts.TotalSeconds < 60)
                    sDate = "ìôðé " + Math.Round(ts.TotalSeconds, 0).ToString() + " ùðéåú";
                else if (ts.TotalMinutes < 60)
                {

                    if (ts.TotalMinutes == 1)
                        sDate += "ìôðé ã÷ä";
                    else if (ts.TotalMinutes == 2)
                        sDate += "ìôðé ùúé ã÷åú";
                    else if (ts.TotalMinutes == 3)
                        sDate += "ìôðé ùìåù ã÷åú";
                    else
                    {
                        sDate = "ìôðé ";
                        //if (ts.TotalMinutes < 10)
                        //sDate += "0";
                        sDate += Math.Round(ts.TotalMinutes, 0).ToString();
                        sDate += " ã÷åú";
                    }
                    //if (ts.Seconds < 10)
                    //sDate += "0";

                }
                else if (ts.TotalHours < 24)
                {
                    if (ts.TotalHours == 1)
                        sDate += "ìôðé ùòä";
                    else if (ts.TotalHours == 2)
                        sDate += "ìôðé ùòúééí";
                    else if (ts.TotalHours == 3)
                        sDate += "ìôðé ùìåù ùòåú";
                    else
                    {
                        sDate = "ìôðé ";
                        //if (ts.TotalHours < 10)
                        //sDate += "0";
                        sDate += Math.Round(ts.TotalHours, 0).ToString();
                        sDate += " ùòåú";
                    }
                }
                else if (ts.TotalDays < 2)
                    sDate = "àúîåì";
                else
                {
                    Int32 nYear = theDate.Year;
                    Int32 nMounth = theDate.Month;
                    Int32 nDay = theDate.Day;
                    if (nDay < 10)
                        sDate += "0";
                    sDate += nDay.ToString();
                    sDate += ".";
                    if (nMounth < 10)
                        sDate += "0";
                    sDate += nMounth.ToString();
                    sDate += ".";
                    sDate += nYear.ToString();
                    return sDate;
                }
                return sDate;
            }
            catch
            {
                return GetStrFromDateExp(DateTime.UtcNow);
            }
        }

        public static string GetStrFromDate(DateTime theDate)
        {
            try
            {
                string sDate = "";
                Int32 nYear = theDate.Year;
                Int32 nMounth = theDate.Month;
                Int32 nDay = theDate.Day;
                if (nDay < 10)
                    sDate += "0";
                sDate += nDay.ToString();
                sDate += "/";
                if (nMounth < 10)
                    sDate += "0";
                sDate += nMounth.ToString();
                sDate += "/";
                sDate += nYear.ToString();
                return sDate;
            }
            catch
            {
                return GetStrFromDate(DateTime.UtcNow);
            }
        }

        public static string GetLongStrFromDate(DateTime theDate)
        {
            try
            {
                string sDate = "";
                Int32 nYear = theDate.Year;
                Int32 nMounth = theDate.Month;
                Int32 nDay = theDate.Day;

                Int32 nHour = theDate.Hour;
                Int32 nMin = theDate.Minute;
                Int32 nSec = theDate.Second;

                if (nDay < 10)
                    sDate += "0";
                sDate += nDay.ToString();
                sDate += "/";
                if (nMounth < 10)
                    sDate += "0";
                sDate += nMounth.ToString();
                sDate += "/";
                sDate += nYear.ToString();
                sDate += " ";
                if (nHour < 10)
                    sDate += "0";
                sDate += nHour.ToString();
                sDate += ":";
                if (nMin < 10)
                    sDate += "0";
                sDate += nMin.ToString();
                sDate += ":";
                if (nSec < 10)
                    sDate += "0";
                sDate += nSec.ToString();

                return sDate;
            }
            catch
            {
                return GetStrFromDate(DateTime.UtcNow);
            }
        }

        public static string GetDayName(DayOfWeek theDay)
        {
            if (theDay == DayOfWeek.Friday)
                return "ùéùé";
            if (theDay == DayOfWeek.Monday)
                return "ùðé";
            if (theDay == DayOfWeek.Saturday)
                return "ùáú";
            if (theDay == DayOfWeek.Sunday)
                return "øàùåï";
            if (theDay == DayOfWeek.Thursday)
                return "çîéùé";
            if (theDay == DayOfWeek.Tuesday)
                return "ùìéùé";
            if (theDay == DayOfWeek.Wednesday)
                return "øáéòé";
            return "";

        }

        private static DateTime GetTruncDateTimeUtc()
        {
            DateTime truncDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return truncDateTimeUtc;
        }

        public static long GetUtcUnixTimestampNow()
        {
            TimeSpan ts = DateTime.UtcNow - GetTruncDateTimeUtc();
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        // DateTime to Milliseconds
        public static long DateTimeToUtcUnixTimestampMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - GetTruncDateTimeUtc()).TotalMilliseconds;
        }

        // Milliseconds to DateTime
        public static DateTime UtcUnixTimestampMillisecondsToDateTime(long unixTimeStamp)
        {
            DateTime origin = GetTruncDateTimeUtc();
            return origin.AddMilliseconds(unixTimeStamp);
        }

        // DateTime to Seconds
        public static long DateTimeToUtcUnixTimestampSeconds(DateTime dateTime)
        {
            return dateTime.ToUtcUnixTimestampSeconds();
        }

        // DateTime to Seconds
        public static long? DateTimeToUtcUnixTimestampSeconds(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
            {
                return null;
            }

            return dateTime.Value.ToUtcUnixTimestampSeconds();
        }

        // DateTime to Seconds
        public static long ToUtcUnixTimestampSeconds(this DateTime dateTime)
        {
            return (long)(dateTime - GetTruncDateTimeUtc()).TotalSeconds;
        }

        public static long ToUtcUnixTimestampMilliseconds(this DateTime dateTime)
        {
            return (long)(dateTime - GetTruncDateTimeUtc()).TotalMilliseconds;
        }

        // Seconds to DateTime
        public static DateTime UtcUnixTimestampSecondsToDateTime(long unixTimeStamp)
        {
            DateTime origin = GetTruncDateTimeUtc();
            return origin.AddSeconds(unixTimeStamp);
        }

        public static string UtcUnixTimestampSecondsToDateTime(long unixTimeStamp, string dateFormat)
        {
            return UtcUnixTimestampSecondsToDateTime(unixTimeStamp).ToString(dateFormat);
        }

        // Seconds? to DateTime?
        public static DateTime? UtcUnixTimestampSecondsToDateTime(long? unixTimeStamp)
        {
            if (!unixTimeStamp.HasValue)
            {
                return null;
            }

            return UtcUnixTimestampSecondsToDateTime(unixTimeStamp.Value);
        }

        public static DateTime UtcUnixTimestampAbsSecondsToDateTime(long? unixTimeStamp)
        {
            if (!unixTimeStamp.HasValue)
            {
                return DateTime.MinValue;
            }

            return UtcUnixTimestampSecondsToDateTime(unixTimeStamp.Value);
        }

        /// <summary>
        /// convert string to dateTime in format dd/MM/yyyy HH:mm:ss
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long StringExactToUtcUnixTimestampSeconds(string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(date, MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
                if (DateTime.TryParseExact(date, MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
                {
                    return DateTimeToUtcUnixTimestampSeconds(parsedDate);
                }

            return 0;
        }

        public static long StringToUtcUnixTimestampSeconds(string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(date, MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                return DateTimeToUtcUnixTimestampSeconds(parsedDate);
            }

            return StringExactToUtcUnixTimestampSeconds(date);
        }

        public static DateTime? TryExtractDate(string dateTime, string format = MAIN_FORMAT)
        {
            DateTime result;
            if (!string.IsNullOrEmpty(dateTime) && DateTime.TryParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out result))
            {
                return result;
            }

            return null;
        }

        public static DateTime ExtractDate(string dateTime, string format)
        {
            DateTime result = DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            return result;
        }
        
        public static DateTime TruncateMilliSeconds(this DateTime dateTime)
        {
            return dateTime.AddTicks(-(dateTime.Ticks % TimeSpan.TicksPerSecond));
        }

        public static DateTime StartOfDay(this DateTime dateTime)
        {
            return dateTime.Date;
        }

        public static DateTime EndOfDay(this DateTime dateTime)
        {
            return dateTime.Date.AddDays(1).AddTicks(-1);
        }

        public static string ToESDateFormat(this DateTime dateTime)
        {
            return dateTime.ToString(ESMediaFields.DATE_FORMAT);
        }

        public static DateTime Round(this DateTime date, TimeSpan span)
        {
            long ticks = (date.Ticks + (span.Ticks / 2) + 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }

        public static DateTime Floor(this DateTime date, TimeSpan span)
        {
            long ticks = (date.Ticks / span.Ticks);
            return new DateTime(ticks * span.Ticks);
        }

        public static DateTime Ceil(this DateTime date, TimeSpan span)
        {
            long ticks = (date.Ticks + span.Ticks - 1) / span.Ticks;
            return new DateTime(ticks * span.Ticks);
        }
    }
}
