using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Globalization;
namespace TVinciShared
{
    /// <summary>
    /// Summary description for DateUtils
    /// </summary>
    public static class DateUtils
    {
        static public string GetDateForSchedule(DateTime theDate)
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

        static public string GetTimeString(DateTime theDate)
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
                    return DateTime.Now;
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
                return DateTime.Now;
            }
        }

        public static DateTime GetDateFromStrUTF(string sDate)
        {
            try
            {
                if (sDate == "")
                    return DateTime.Now;
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
                return DateTime.Now;
            }
        }

        public static DateTime GetDateTimeFromStrUTF(string sDate)
        {
            try
            {
                string sTime = "";
                if (sDate == "")
                    return DateTime.Now;

                string[] timeHour = sDate.Split(' ');
                if (timeHour.Length == 2)
                {
                    sDate = timeHour[0];
                    sTime = timeHour[1];
                }
                else
                    return DateTime.Now;
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
                return DateTime.Now;
            }
        }

        public static string GetStrFromDateExp(DateTime theDate)
        {
            try
            {
                string sDate = "";
                TimeSpan ts = DateTime.Now - theDate;
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
                return GetStrFromDateExp(DateTime.Now);
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
                return GetStrFromDate(DateTime.Now);
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
                return GetStrFromDate(DateTime.Now);
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

        public static DateTime GetStartOfDay()
        {
            return DateTime.Now.Date;
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static DateTime UnixTimeStampMillisecondsToDateTime(long unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }
        
        public static long DateTimeToUnixTimestampMilliseconds(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalMilliseconds;
        }

        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        public static DateTime UnixTimestampToDateTime(this long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long ToUnixTimestamp(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }
    }
}