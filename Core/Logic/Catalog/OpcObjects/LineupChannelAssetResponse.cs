using System.Collections.Generic;

namespace Core.Catalog
{
    public class LineupChannelAssetResponse
    {
        public List<LineupChannelAsset> LineupChannelAssets { get; set; }

        public string LineupExternalId { get; set; }

        public string ParentLineupExternalId { get; set; }

        public int TotalCount { get; set; }
    }
}
