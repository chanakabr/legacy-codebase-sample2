using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class RegionChannelNumberMapper
    {
        public static IEnumerable<int> ParsedLcns(this KalturaRegionChannelNumberMultiLcns model) =>
            Utils.Utils.ParseCommaSeparatedValues<int>(model.LCNs, $"lcns", true);
    }
}