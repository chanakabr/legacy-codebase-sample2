using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess.FilterActions.Files;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class FilterFileByFileTypeIdActionMapper
    {
        public static HashSet<long> GetFileTypesIds(this KalturaFilterFileByFileTypeIdAction model)
        {
            return model.GetItemsIn<HashSet<long>, long>(model.FileTypeIdIn, "fileTypeIdIn", true);
        }
    }
}