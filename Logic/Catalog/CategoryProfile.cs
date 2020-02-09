using ApiObjects.Base;
using System.Collections.Generic;

namespace ApiLogic.Catalog
{
    public class CategoryProfile : ICrudHandeledObject
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long? ParentCategoryId { get; set; }
        public List<long> ChildCategoriesIds { get; set; }
        public List<long> ChannelsIds { get; set; }
        public List<long> ImagesIds { get; set; }       
    }

    public class CategoryProfileFilter : ICrudFilter
    {
        public long CategoryProfileIdEqual { get; set; }
        public string CategoryProfileNameEqual { get; set; }
    }
}