using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class TvmDeviceRuleMapper
    {
        public static HashSet<int> GetDeviceBrandIds(this KalturaTvmDeviceRule model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<HashSet<int>, int>(model.DeviceBrandIds, "KalturaTvmDeviceRule.deviceBrandIds");        }
    }
}