using System.Collections.Generic;

namespace ApiObjects.CouchbaseWrapperObjects
{
    public class CBCategoryDynamicData
    {
        public long Id { get; set; }
        public Dictionary<string, string> DynamicData { get; set; }        

        public static string GetCategoryDynamicDataKey(long id)
        {
            return $"category_dynamicdata_{id}";
        }
    }
}
