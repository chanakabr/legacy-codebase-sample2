using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Models.Catalog;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BookmarkMapper
    {
        internal static int getPosition(this KalturaBookmark model)
        {
            return model.Position.HasValue ? model.Position.Value : 0;
        }
    }
}
