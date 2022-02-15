namespace Core.Catalog
{
    public class AssetPriority
    {
        public AssetPriority(Asset asset, long? priorityGroupId)
        {
            Asset = asset;
            PriorityGroupId = priorityGroupId;
        }
        
        public long? PriorityGroupId { get; }

        public Asset Asset { get; }
    }
}
