using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonWithSL
{
    public class Forcast
    {
        public string EpgId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DayIndex { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ImageLink { get; set; }
        public bool IsBlackout { get; set; }       
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool HasVod { get; set; }
    }
}
