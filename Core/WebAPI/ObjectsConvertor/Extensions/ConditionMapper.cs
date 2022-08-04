using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ChannelConditionMapper
    {
        public static List<long> GetChannelIds(this KalturaChannelCondition model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.IdIn, "idIn", true);
        }
    }

    public static class FileTypeConditionMapper
    {
        public static List<long> GetFileTypeIds(this KalturaFileTypeCondition model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.IdIn, "idIn", true);
        }
    }
}