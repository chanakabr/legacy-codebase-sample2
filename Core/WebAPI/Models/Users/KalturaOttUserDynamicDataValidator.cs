using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.Models.Users
{
    public static class KalturaOttUserDynamicDataValidator
    {
        private const int MAX_KEY_LENGTH = 50;
        private const int MAX_VALUE_LENGTH = 512;

        public static void Validate(string key, KalturaStringValue value)
        {
            if (key.Length > MAX_KEY_LENGTH)
            {
                throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, nameof(key), MAX_KEY_LENGTH);
            }

            if (value?.value?.Length > MAX_VALUE_LENGTH)
            {
                throw new BadRequestException(ARGUMENT_MAX_LENGTH_CROSSED, nameof(value), MAX_VALUE_LENGTH);
            }
        }

        public static void Validate(IDictionary<string, KalturaStringValue> dynamicData)
        {
            if (dynamicData != null)
            {
                foreach (var item in dynamicData)
                {
                    Validate(item.Key, item.Value);
                }
            }
        }
    }
}