using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class Scheduling
    {
        public DateTime StartDate { get; set; }
        public DateTime EndTime { get; set; }

        public Scheduling()
        {

        }

        public Scheduling(DateTime start, DateTime end)
        {
            this.StartDate = start;
            this.EndTime = end;
        }
    }
}
