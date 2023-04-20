using ApiObjects.Base;

namespace ApiObjects.Lineup
{
    public class LineupRegionalChannelRequest
    {
        public LineupRegionalChannelOrderBy OrderBy { get; set; }

        public int PartnerId { get; set; }

        public long RegionId { get; set; }

        public bool ParentRegionIncluded { get; set; }

        public int? LcnGreaterThanOrEqual { get; set; }

        public int? LcnLessThanOrEqual { get; set; }

        public string Ksql { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
}