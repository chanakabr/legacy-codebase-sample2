using System.Collections.Generic;

namespace ApiObjects.SearchObjects.GroupRepresentatives
{
    public class TopSubscriptionEntitledRsp : RepresentativeSelectionPolicy
    {
        public IReadOnlyCollection<AssetOrder> OrderingParameters { get; set; }
    }
}