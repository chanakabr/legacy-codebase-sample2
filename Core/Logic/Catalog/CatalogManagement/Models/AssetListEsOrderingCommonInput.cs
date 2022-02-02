using System.Collections.Generic;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class AssetListEsOrderingCommonInput
    {
        public int GroupId { get; set; }
        public bool ShouldSearchEpg { get; set; }
        public bool ShouldSearchMedia { get; set; }
        public bool ShouldSearchRecordings { get; set; }
        public IDictionary<int, int> ParentMediaTypes { get; set; }
        public IDictionary<int, string> AssociationTags { get; set; }
    }
}