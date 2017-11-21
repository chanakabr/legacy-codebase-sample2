using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            return cas.RecordSeriesBySeriesId(siteGuid, SeriesId, SeasonNumber, SeasonSeed, EpisodeSeed, ChannelId, LookupCriteria);
        }
    }
}
