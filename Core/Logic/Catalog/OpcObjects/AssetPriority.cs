namespace Core.Catalog
{
    public class AssetPriority
    {
        public AssetPriority(Asset asset, int? priorityGroupId)
        {
            Asset = asset;
            PriorityGroupId = priorityGroupId;
        }
        
        public int? PriorityGroupId { get; }

        public Asset Asset { get; }
    }
}
