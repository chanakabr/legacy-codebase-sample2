using ApiObjects;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class VodIngestPublishContext
    {
        public int GroupId { get; set; }
        public IngestMedia Media { get; set; }
        public MediaAsset MediaAsset { get; set; }
        public IngestAssetStatus AssetStatus { get; set; }
        public string FileName { get; set; }
        public long? ShopAssetUserRuleId { get; set; }
    }
}