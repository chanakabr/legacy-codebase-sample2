using System.Collections.Generic;

namespace ApiObjects.SearchObjects.GroupRepresentatives
{
    public class TopAssetRsp : RepresentativeSelectionPolicy
    {
        public IReadOnlyCollection<AssetOrder> OrderingParameters { get; set; }
    }
}