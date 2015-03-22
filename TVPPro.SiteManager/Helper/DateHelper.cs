using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.Helper
{
    public class DateHelper
    {
        public static string FormatRemindingTime(DateTime CurrentDate, DateTime EndDate)
        {
            int DaysRemind = 0;
            int HoursRemind = 0;

            TVinci.TimePeriod.DateDiff tdiff = new TVinci.TimePeriod.DateDiff(CurrentDate, EndDate);

            //less than 4 days taranslate days to hour and display 0 days
            if (tdiff.ElapsedDays < 4)
            {
                HoursRemind = 24 * tdiff.ElapsedDays + tdiff.ElapsedHours;
            }
            else
            {
                HoursRemind = tdiff.ElapsedHours;
                DaysRemind = tdiff.ElapsedDays;
            }


            return BuildDateString(tdiff.ElapsedYears, tdiff.ElapsedMonths, DaysRemind, HoursRemind, tdiff.ElapsedMinutes);
        }


        private static string BuildDateString(int years, int Months, int Days, int Hours, int Minutes)
        {
            StringBuilder sb = new StringBuilder();

            if (years > 0)
            {
                sb.Append(string.Format("{0} {1} ", years, TextLocalization.Instance["Years"]));
            }
            if (Months > 0)
            {
                sb.Append(string.Format("{0} {1} ", Months, TextLocalization.Instance["Months"]));
            }
            if (Days > 0)
            {
                sb.Append(string.Format("{0} {1} ", Days, TextLocalization.Instance["Days"]));
            }
            if(Hours > 0)
            {
                sb.Append(string.Format("{0} {1} ", Hours, TextLocalization.Instance["Hours"]));
            }
            if (Minutes > 0)
            {
                sb.Append(string.Format("{0} {1} ", Minutes, TextLocalization.Instance["Minutes"]));
            }
            return sb.ToString();
        }
    }
}
