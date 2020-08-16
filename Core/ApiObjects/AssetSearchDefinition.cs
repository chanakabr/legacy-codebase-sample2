using ApiObjects.SearchObjects;

namespace ApiObjects
{
    public class AssetSearchDefinition
    {
        public string Filter { get; set; }

        public bool IsAllowedToViewInactiveAssets { get; set; }

        public long UserId { get; set; }

        public bool NoSegmentsFilter { get; set; }

        public long AssetStructId { get; set; }
    }
}