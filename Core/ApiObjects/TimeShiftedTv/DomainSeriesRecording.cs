using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class DomainSeriesRecording
    {
        public RecordingType Type { get; set; }

        public long Id { get; set; }

        public string SeriesId { get; set; }

        public int SeasonNumber { get; set; }
        
        public int EpisodeNumber { get; set; }

        public long EpgId { get; set; }

        public string UserId { get; set; }

        public long EpgChannelId { get; set; }

        public List<int> ExcludedSeasons { get; set; }

        public SeriesRecordingOption SeriesRecordingOption { get; set; }

        public int? HouseholdSpecificSeriesStartTimeOffset { get; set; }
        public int? HouseholdSpecificSeriesEndTimeOffset { get; set; }

        public DomainSeriesRecording()         
        {
        }
    }
}
