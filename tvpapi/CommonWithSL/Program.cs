using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonWithSL
{
    public class Program : IItemTemplate
    {
        string _startDate = "";
        string _endDate = "";


        public string EpgId { get; set; }
        public string Title { get; set; }
        public string ID { get; set; }
        public string ChannelID { get; set; }
        public string Description { get; set; }
        public int DayIndex { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ImageLink { get; set; }
        public string PictureSize { get; set; }
        public bool IsBlackout { get; set; }
        public CommonWithSL.Media Media { get; set; }
        public bool HasVod { get; set; }
        public string ChannelCode { get; set; }
        public string VersionID { get; set; }
        public string RecordDate { get; set; }
        public string RecordTime { get; set; }
        public IList<string> Genre { get; set; }

        public string StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = formatDateTime(value, "MM/dd/yyyy HH:mm");
                StartTime = formatDateTime(value, "HH:mm");
                DayIndex = getDayIndex(value);
                RecordDate = formatDateTime(value, "MM/dd/yyyy");
                RecordTime = formatDateTime(value, "HH:mm:ss");
            }
        }
        public string EndDate
        {
            get { return _endDate; }
            set
            {
                _endDate = formatDateTime(value, "MM/dd/yyyy HH:mm");
                EndTime = formatDateTime(value, "HH:mm");
            }
        }

        public string TemplateName
        {
            get;
            set;
        }

        string formatDateTime(string requestedDate, string requestedFormat)
        {
            DateTime formatedDate = DateTime.MinValue;
            string formatedDateStr = "";
            if ((DateTime.TryParseExact(requestedDate, "d/M/yyyy HH:mm:ss", null, DateTimeStyles.None, out formatedDate)) ||
             (DateTime.TryParse(requestedDate, out formatedDate)) ||
             (DateTime.TryParse(requestedDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out formatedDate)))
            {
                formatedDateStr = formatedDate.ToString(requestedFormat);
            }
            return formatedDateStr;
        }

        //function for calculating the date index
        int getDayIndex(string startDateStr)
        {
            int index = -1;
            DateTime startDate = DateTime.MinValue;
            if ((DateTime.TryParseExact(startDateStr, "d/M/yyyy HH:mm:ss", null, DateTimeStyles.None, out startDate)) ||
                (DateTime.TryParse(startDateStr, out startDate)) ||
            (DateTime.TryParse(startDateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate)))
            {
                index = (int)Math.Round((startDate.Date - DateTime.Now.Date).TotalDays);
            }
            return index;
        }
    }
}
