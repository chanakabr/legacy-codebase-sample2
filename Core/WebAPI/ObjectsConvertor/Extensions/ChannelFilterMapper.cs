using System;
using System.Collections.Generic;
using WebAPI.Models.Pricing;

namespace WebAPI.ObjectsConvertor.Extensions
{
    internal static class PpvFilterMapper
    {
        public static List<long> GetIdIn(this KalturaPpvFilter model, string field, string ids)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(ids, $"KalturaPpvFilter.{field}", true);
        }
    }
}
