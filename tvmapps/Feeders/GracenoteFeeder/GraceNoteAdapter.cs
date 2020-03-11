using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GracenoteFeeder
{
    public class GraceNoteAdapter
    {

        public string GetProgramDuration(string sStartDate, string sEndDate)
        {
            string duration = "";
            DateTime dDateStart = DateTime.MinValue;
            DateTime dEndDate = DateTime.MaxValue;
            if (Utils.ParseEPGStrToDate(sStartDate, ref dDateStart) && Utils.ParseEPGStrToDate(sEndDate, ref dEndDate))
            {
                TimeSpan ts = dEndDate.Subtract(dDateStart);
                duration = "PT" + ts.ToString("hh") + "H" + ts.ToString("mm") + "M";
            }
            return duration;
        }
        

    }
}
