using System.Collections.Generic;
using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class OTTObjectSupportNullableMapper
    {
        public static void AddNullableProperty(this KalturaOTTObjectSupportNullable model, string str)
        {
            if (model.NullableProperties == null)
                model.NullableProperties = new HashSet<string>();

            model.NullableProperties.Add(str.ToLower());
        }
    }
}
