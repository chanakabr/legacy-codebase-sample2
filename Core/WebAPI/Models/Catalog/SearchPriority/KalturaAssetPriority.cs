namespace WebAPI.Models.Catalog.SearchPriority
{
    public class KalturaAssetPriority
    {
        public KalturaAssetPriority(KalturaAsset asset, long? priorityGroupId)
        {
            Asset = asset;
            PriorityGroupId = priorityGroupId;
        }
        
        public KalturaAsset Asset { get; }

        public long? PriorityGroupId { get; }
    }
}
