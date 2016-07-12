using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class DomainSeriesRecording
    {
        public string SeriesId { get; set; }

        public int SeasonNumber { get; set; }

        public int EpisodeNumber { get; set; }

        public long EpgId { get; set; }

        public string UserId { get; set; }
    }
}
