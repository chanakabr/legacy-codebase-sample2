using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiLogic.Catalog
{
    public class CategoryItem : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ParentId { get; set; }
        public List<long> ChildrenIds { get; set; }
        public List<UnifiedChannel> UnifiedChannels { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }

        public CategoryItem()
        {
            ChildrenIds = new List<long>();
            UnifiedChannels = new List<UnifiedChannel>();
            DynamicData = null;
        }
    }   
}