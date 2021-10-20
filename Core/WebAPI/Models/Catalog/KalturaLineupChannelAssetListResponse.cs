using System.Collections.Generic;
using System.Linq;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaLineupChannelAssetListResponse : KalturaListResponse<KalturaLineupChannelAsset>
    {
        public KalturaLineupChannelAssetListResponse()
        {
        }
        
        public KalturaLineupChannelAssetListResponse(IEnumerable<KalturaLineupChannelAsset> channelAssets, int totalCount)
        {
            Objects = channelAssets.ToList();
            TotalCount = totalCount;
        }
    }
}