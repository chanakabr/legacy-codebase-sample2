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
        public List<UnifiedChannel> UnifiedChannels { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }
        public bool HasDynamicData { get; set; }
    }

    public class CategoryItemFilter : ICrudFilter
    {
    }

    public class CategoryItemByIdInFilter : CategoryItemFilter
    {
        public string IdIn { get; set; }        
    }

    public class CategoryItemByKsqlFilter : CategoryItemFilter
    {
        public string Ksql { get; set; }
    }

    public class CategoryItemByRootFilter : CategoryItemFilter
    {
    }
}