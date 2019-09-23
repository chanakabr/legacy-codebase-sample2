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

                return new DateTime(nYear, nMounth, nDay , nHour , nMin , nSec);
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
                    sDate = "ממש מעכשיו";
                else if (ts.TotalSeconds == 2)
                    sDate = "לפני שתי שניות";
                else if (ts.TotalSeconds == 3)
                    sDate = "לפני שלוש שניות";
                else if (ts.TotalSeconds < 60)
                    sDate = "לפני " + Math.Round(ts.TotalSeconds, 0).ToString() + " שניות";
                else if (ts.TotalMinutes < 60)
                {

                    if (ts.TotalMinutes == 1)
                        sDate += "לפני דקה";
                    else if (ts.TotalMinutes == 2)
                        sDate += "לפני שתי דקות";
                    else if (ts.TotalMinutes == 3)
                        sDate += "לפני שלוש דקות";
                    else
                    {
                        sDate = "לפני ";
                        //if (ts.TotalMinutes < 10)
                        //sDate += "0";
                        sDate += Math.Round(ts.TotalMinutes, 0).ToString();
                        sDate += " דקות";
                    }
                    //if (ts.Seconds < 10)
                    //sDate += "0";

                }
                else if (ts.TotalHours < 24)
                {
                    if (ts.TotalHours == 1)
                        sDate += "לפני שעה";
                    else if (ts.TotalHours == 2)
                        sDate += "לפני שעתיים";
                    else if (ts.TotalHours == 3)
                        sDate += "לפני שלוש שעות";
                    else
                    {
                        sDate = "לפני ";
                        //if (ts.TotalHours < 10)
                        //sDate += "0";
                        sDate += Math.Round(ts.TotalHours, 0).ToString();
                        sDate += " שעות";
                    }
                }
                else if (ts.TotalDays < 2)
                    sDate = "אתמול";
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
                return "שישי";
            if (theDay == DayOfWeek.Monday)
                return "שני";
            if (theDay == DayOfWeek.Saturday)
                return "שבת";
            if (theDay == DayOfWeek.Sunday)
                return "ראשון";
            if (theDay == DayOfWeek.Thursday)
                return "חמישי";
            if (theDay == DayOfWeek.Tuesday)
                return "שלישי";
            if (theDay == DayOfWeek.Wednesday)
                return "רביעי";
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

        /// <summary>
        /// convert string to dateTime in format dd/MM/yyyy HH:mm:ss
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long StringExactToUtcUnixTimestampSeconds(string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(date, MAIN_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                return DateTimeToUtcUnixTimestampSeconds(parsedDate);
            }
            
            return 0;
        }

        public static long StringToUtcUnixTimestampSeconds(string date)
        {
            DateTime parsedDate;
            if (DateTime.TryParse(date, null, DateTimeStyles.AdjustToUniversal, out parsedDate))
            {
                return DateTimeToUtcUnixTimestampSeconds(parsedDate);
            }

            return StringExactToUtcUnixTimestampSeconds(date);
        }

        public static DateTime? TryExtractDate(string dateTime, string format)
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
    }
}