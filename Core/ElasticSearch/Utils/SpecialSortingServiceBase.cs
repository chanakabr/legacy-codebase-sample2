using System;
using System.Collections.Concurrent;

namespace ElasticSearch.Utils
{
    public abstract class SpecialSortingServiceBase : ISpecialSortingService
    {
        private static readonly ConcurrentDictionary<string, bool> SpecialSortingLanguages = new ConcurrentDictionary<string, bool>();

        protected abstract bool IsSpecialSortingLanguage(string languageCode);

        protected abstract string GetLanguageKey(string languageCode);

        public bool IsSpecialSortingField(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return false;
            }

            var languageKey = GetLanguageKey(languageCode);
            if (!SpecialSortingLanguages.TryGetValue(languageKey, out var isSpecialSorting))
            {
                isSpecialSorting = IsSpecialSortingLanguage(languageCode);
                SpecialSortingLanguages.AddOrUpdate(languageKey, isSpecialSorting, (existingKey, oldValue) => isSpecialSorting);
            }

            return isSpecialSorting;
        }

        public bool IsSpecialSortingMeta(string languageCode, Type metaType)
        {
            return metaType == typeof(string) && IsSpecialSortingField(languageCode);
        }
    }
}