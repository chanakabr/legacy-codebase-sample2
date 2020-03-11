using ApiObjects;
using Core.Catalog;
using System.Collections.Generic;

namespace ApiLogic.Catalog
{
    public class CategoryTree
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<LanguageContainer> NamesInOtherLanguages { get; set; }
        public List<CategoryTree> Children { get; set; }
        public List<UnifiedChannelInfo> UnifiedChannels { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }
        public List<Image> Images { get; set; }

        public CategoryTree(CategoryItem categoryItem)
        {
            this.Id = categoryItem.Id;
            this.Name = categoryItem.Name;
            this.NamesInOtherLanguages = categoryItem.NamesInOtherLanguages;
            this.DynamicData = categoryItem.DynamicData;
        }
    }
}