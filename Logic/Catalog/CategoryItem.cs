using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiLogic.Catalog
{
    public class CategoryItem : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ParentCategoryId { get; set; }
        public List<long> ChildCategoriesIds { get; set; }
        public List<UnifiedChannelInfo> UnifiedChannels { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }
        public bool HasDynamicData { get; set; }
    }

    public class CategoryItemFilter : ICrudFilter
    {
        public List<long> Ids { get; set; }
        public string Ksql { get; set; }
        public bool ParentOnly { get; set; }
    }
}