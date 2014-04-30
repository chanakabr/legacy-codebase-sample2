using CommonWithSL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonWithSL
{
    public class Program : IItemTemplate
    {
        public string EpgId { get; set; }
        public string Title { get; set; }
        public string ID { get; set; }
        public string ChannelID { get; set; }
        public string Description { get; set; }
        public int DayIndex { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ImageLink { get; set; }
        public bool IsBlackout { get; set; }
        public CommonWithSL.Media Media { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool HasVod { get; set; }
        public string ChannelCode { get; set; }
        public string VersionID { get; set; }
        public string RecordDate { get; set; }
        public string RecordTime { get; set; }
        public IList<string> Genre { get; set; }

        public string TemplateName
        {
            get;
            set;
        }
    }
}
