using System;

namespace ApiObjects.Recordings
{
    public class Program
    {
        public long Id { get; set; }
        public long EpgId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime __updated { get; set; }

        public Program(long epgId, DateTime startDate, DateTime endDate)
        {
            EpgId = epgId;
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}