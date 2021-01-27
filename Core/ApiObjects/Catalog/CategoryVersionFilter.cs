using ApiObjects.Base;
using ApiObjects.SearchObjects;

namespace ApiObjects.Catalog
{
    public class CategoryVersionFilter : ICrudFilter
    {
        public OrderByObject OrderBy { get; set; }
    }

    public class CategoryVersionFilterByTree : CategoryVersionFilter
    {
        public long TreeId { get; set; }
        public CategoryVersionState? State { get; set; }
    }
}
