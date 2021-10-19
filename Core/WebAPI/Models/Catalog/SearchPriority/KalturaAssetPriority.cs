namespace WebAPI.Models.Catalog.SearchPriority
{
    public class KalturaAssetPriority
    {
        public KalturaAssetPriority(KalturaAsset asset, int? priorityGroupId)
        {
            Asset = asset;
            PriorityGroupId = priorityGroupId;
        }
        
        public KalturaAsset Asset { get; }

        public int? PriorityGroupId { get; }
    }
}
