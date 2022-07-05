using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class RegionalChannelMapper
    {
        public static IEnumerable<int> ParsedLcns(this KalturaRegionalChannelMultiLcns model) => string.IsNullOrEmpty(model.LCNs)
            ? new[] { model.ChannelNumber }
            : Utils.Utils.ParseCommaSeparatedValues<int>(model.LCNs, $"lcns", true, true);
    }
}