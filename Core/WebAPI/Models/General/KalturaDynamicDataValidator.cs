using WebAPI.Exceptions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.General
{
    public static class KalturaDynamicDataValidator
    {
        public static void Validate(this SerializableDictionary<string, KalturaStringValue> dynamicData, int maxKeyValues, int maxKeyLength, int maxValueLength)
        {            
            if (dynamicData == null) return;

            if (dynamicData.Count > maxKeyValues) throw new BadRequestException(ARGUMENT_MAX_VALUE_CROSSED, "dynamicData.length", maxKeyValues);

            foreach (var keyValue in dynamicData)
            { 
                if (keyValue.Key.Length > maxKeyLength) throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, "dynamicData.key.length", maxKeyLength);
                if (keyValue.Value?.value?.Length > maxValueLength) throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, "dynamicData.value.length", maxValueLength);
            }
        }
    }
}