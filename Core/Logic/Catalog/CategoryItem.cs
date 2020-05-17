using ApiObjects;
using ApiObjects.Base;
using System;
using System.Collections.Generic;

namespace ApiLogic.Catalog
{
    public class CategoryItem : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public long? ParentId { get; set; }
        public List<long> ChildrenIds { get; set; }
        public List<UnifiedChannel> UnifiedChannels { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool? IsActive { get; set; }
        public TimeSlot TimeSlot { get; set; }

        public CategoryItem()
        {
            ChildrenIds = new List<long>();
            UnifiedChannels = new List<UnifiedChannel>();
            DynamicData = null;
            TimeSlot = new TimeSlot();
        }

        public bool IsValid()
        {
            return IsActive.Value && (TimeSlot == null || TimeSlot.IsValid());
        }
    }   
}