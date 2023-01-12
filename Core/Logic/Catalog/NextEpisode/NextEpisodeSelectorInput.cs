using System.Collections.Generic;
using ApiObjects.NextEpisode;

namespace ApiLogic.Catalog.NextEpisode
{
    public class NextEpisodeSelectorInput
    {
        public NextEpisodeContext Context { get; set; }
        public IEnumerable<long> AssetStructIds { get; set; }
        public string SeriesIdMetaName { get; set; }
        public string SeasonNumberMetaName { get; set; }
        public string EpisodeNumberMetaName { get; set; }
        public int ExactGroupId { get; set; }
        public string SeriesId { get; set; }
    }
}