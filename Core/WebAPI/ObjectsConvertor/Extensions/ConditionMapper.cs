using System.Collections.Generic;
using WebAPI.Models.API;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class ConditionMapper
    {
        public static List<int> getSegmentsIds(this KalturaSegmentsCondition model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.SegmentsIds, "segmentsIds", true);
        }

        internal static List<int> GetDeviceFamilyIds(this KalturaDeviceFamilyCondition model)
        {
            return !string.IsNullOrEmpty(model.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.IdIn, "KalturaDeviceFamilyCondition.IdIn", true) : null;
        }

        internal static List<int> GetDeviceBrandIds(this KalturaDeviceBrandCondition model)
        {
            return !string.IsNullOrEmpty(model.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.IdIn, "KalturaDeviceBrandCondition.IdIn", true) : null;
        }

        internal static List<int> GetDeviceManufacturerIds(this KalturaDeviceManufacturerCondition model)
        {
            return !string.IsNullOrEmpty(model.IdIn)
                ? Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.IdIn, "KalturaDeviceManufacturerCondition.IdIn", true) : null;
        }

        internal static List<string> GetValues(this KalturaDynamicKeysCondition model)
        {
            return !string.IsNullOrWhiteSpace(model.Values)
                ? Utils.Utils.ParseCommaSeparatedValues<List<string>, string>(model.Values, "KalturaDynamicKeysCondition.values", true)
                : null;
        }

        public static List<int> getCountries(this KalturaCountryCondition model)
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(model.Countries, "KalturaCountryCondition.countries");
        }
    }

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