using ApiObjects.ConditionalAccess;
using System.Collections.Generic;

namespace Core.ConditionalAccess
{
    public class RecordSeriesBySeriesIdNPVRCommand : BaseNPVRCommand
    {        
        public string SeriesId { get; set; }

        public int SeasonNumber { get; set; }

        public int SeasonSeed { get; set; }

        public int EpisodeSeed { get; set; }

        public int ChannelId { get; set; }

        public List<string> LookupCriteria { get; set; }

        protected override NPVRResponse ExecuteFlow(BaseConditionalAccess cas)
        {
            return cas.RecordSeriesBySeriesId(siteGuid, SeriesId, SeasonNumber, SeasonSeed, EpisodeSeed, ChannelId, LookupCriteria, Version);
        }
    }
}
