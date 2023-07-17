using System;
using Newtonsoft.Json;

namespace ApiObjects.Recordings
{
    public class Program
    {
        public long Id { get; set; }
        public long EpgId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime __updated { get; set; }

        [JsonIgnore]
        public long EpgChannelId { get; set; }

        [JsonIgnore]
        public string Crid { get; set; }

        public Program(long epgId, DateTime startDate, DateTime endDate)
        {
            EpgId = epgId;
            StartDate = startDate;
            EndDate = endDate;
        }

        public Program(long epgId, DateTime startDate, DateTime endDate, long epgChannelId, string crid)
            : this(epgId, startDate, endDate)
        {
            EpgChannelId = epgChannelId;
            Crid = crid;
        }
    }
}