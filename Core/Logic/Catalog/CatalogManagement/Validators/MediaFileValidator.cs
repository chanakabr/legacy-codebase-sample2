using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Catalog;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public class MediaFileValidator : IMediaFileValidator
    {
        public const int MAX_DYNAMIC_DATA_KEY_LENGTH = 50;
        public const int MAX_DYNAMIC_DATA_ITEMS_COUNT = 15;
        public const int MAX_DYNAMIC_DATA_VALUE_LENGTH = 50;
        public const int MAX_DYNAMIC_DATA_VALUES_COUNT = 50;

        private static readonly IKLogger Log = new KLogger(nameof(MediaFileValidator));

        private static readonly Lazy<IMediaFileValidator> Lazy = new Lazy<IMediaFileValidator>(
            () => new MediaFileValidator(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileValidator Instance => Lazy.Value;

        public IDictionary<string, IEnumerable<string>> GetValidatedDynamicData(MediaFileType mediaFileType, IDictionary<string, IEnumerable<string>> dynamicData)
        {
            var result = new Dictionary<string, IEnumerable<string>>();
            foreach (var item in dynamicData)
            {
                var key = item.Key;
                if (key.Length > MAX_DYNAMIC_DATA_KEY_LENGTH)
                {
                    Log.Error($"DynamicData with key {key} can not be imported. MediaFileType [{mediaFileType.Id}]. Key's maximum length is [{MAX_DYNAMIC_DATA_KEY_LENGTH}].");
                    continue;
                }

                if (!mediaFileType.DynamicDataKeys.Contains(key))
                {
                    Log.Error($"MediaFileType [{mediaFileType.Id}] does not contain key {key}.");
                    continue;
                }

                var validValues = item.Value
                    .Where(ValidateDynamicDataValue)
                    .Take(MAX_DYNAMIC_DATA_VALUES_COUNT)
                    .ToArray();
                if (validValues.Any())
                {
                    result.Add(key, validValues);
                }

                var invalidValues = item.Value
                    .Except(validValues)
                    .ToArray();
                if (invalidValues.Any())
                {
                    Log.Error($"DynamicData with key {key} can not be imported. MediaFileType [{mediaFileType.Id}]. Invalid values are [{string.Join(",", invalidValues)}]. Value's maximum length is [{MAX_DYNAMIC_DATA_VALUE_LENGTH}]. Values maximum count is [{MAX_DYNAMIC_DATA_VALUES_COUNT}].");
                }
            }

            if (result.Count > MAX_DYNAMIC_DATA_ITEMS_COUNT)
            {
                var extraKeys = result
                    .Skip(MAX_DYNAMIC_DATA_ITEMS_COUNT)
                    .Select(x => x.Key);
                Log.Error($"DynamicData with keys [{string.Join(",", extraKeys)}] can not be imported. MediaFileType [{mediaFileType.Id}]. Keys maximum count is [{MAX_DYNAMIC_DATA_ITEMS_COUNT}].");
            }

            return result;
        }

        private static bool ValidateDynamicDataValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            if (value.Length > MAX_DYNAMIC_DATA_VALUE_LENGTH)
            {
                return false;
            }

            if (value.Contains(','))
            {
                return false;
            }

            return true;
        }
    }
}