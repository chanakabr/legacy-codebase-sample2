using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class TvmGeoRuleMapper
    {
        public static HashSet<int> GetCountryIds(this KalturaTvmGeoRule model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(model.CountryIds, "KalturaTvmGeoRule.countryIds");        }
    }
}