using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EPG_XDTVTransform
{
    public class GraceNoteAdapter
    {

        public string GetProgramDuration(string sStartDate, string sEndDate)
        {
            string duration = "";
            DateTime dDateStart = DateTime.MinValue;
            DateTime dEndDate = DateTime.MaxValue;
            if (ParseEPGStrToDate(sStartDate, ref dDateStart) && ParseEPGStrToDate(sEndDate, ref dEndDate))
            {
                TimeSpan ts = dEndDate.Subtract(dDateStart);
                duration = "PT" + ts.ToString("hh") + "H" + ts.ToString("mm") + "M";
            }
            return duration;
        }

        public static bool ParseEPGStrToDate(string dateStr, ref DateTime theDate)
        {
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14)
                return false;

            string format = "yyyy-MM-ddTHH:mm";
            bool res = DateTime.TryParseExact(dateStr, format, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }


    }
}
