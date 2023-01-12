using System.Collections.Generic;

namespace ApiObjects.NextEpisode
{
    public class SeriesType
    {
        public IEnumerable<long> AssetStructIds { get; set; }
        public string SeriesIdMeta { get; set; }
        public string SeasonNumberMeta { get; set; }
        public string EpisodeNumberMeta { get; set; }
    }
}