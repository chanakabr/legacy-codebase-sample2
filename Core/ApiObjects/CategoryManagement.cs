using System.Collections.Generic;

namespace ApiObjects
{
    public class CategoryManagement
    {
        public long? DefaultCategoryTree { get; set; }
        public Dictionary<int, long> DeviceFamilyToCategoryTree { get; set; }
    }
}