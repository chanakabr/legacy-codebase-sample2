using WebAPI.Exceptions;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.General
{
    public static class KalturaDeviceDynamicDataValidator
    {
        private const int MAX_KEY_VALUES = 5; // numbers from BEO-8671
        private const int MAX_KEY_LENGTH = 128;
        private const int MAX_VALUE_LENGTH = 255;

        public static void Validate(string key, KalturaStringValue value)
        {
            Validate(key, value, true);
        }

        public static void Validate(this SerializableDictionary<string, KalturaStringValue> dynamicData)
        {
            if (dynamicData == null) return;

            if (dynamicData.Count > MAX_KEY_VALUES)
            {
                throw new BadRequestException(ARGUMENT_MAX_VALUE_CROSSED, "dynamicData.length", MAX_KEY_VALUES);
            }

            foreach (var keyValue in dynamicData)
            {
                Validate(keyValue.Key, keyValue.Value, false);
            }
        }

        public static void Validate(string key, KalturaStringValue value, bool valueIsRequired)
        {
            if (key.Length > MAX_KEY_LENGTH)
            {
                throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, nameof(key), MAX_KEY_LENGTH);
            }

            if (value?.value?.Length > MAX_VALUE_LENGTH)
            {
                throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, nameof(value), MAX_VALUE_LENGTH);
            }

            if (valueIsRequired && string.IsNullOrEmpty(value?.value))
            {
                throw new BadRequestException(ARGUMENT_CANNOT_BE_EMPTY, nameof(value));
            }
        }
    }
}