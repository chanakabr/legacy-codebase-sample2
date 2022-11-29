using System.Collections.Concurrent;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ElasticSearch.Utils
{
    public abstract class SpecialSortingServiceBase : ISpecialSortingService
    {
        private static readonly ConcurrentDictionary<string, bool> SpecialSortingLanguages = new ConcurrentDictionary<string, bool>();

        protected abstract bool IsSpecialSortingLanguage(string languageCode);

        protected abstract string GetLanguageKey(string languageCode);

        public bool IsSpecialSortingField(EsOrderByField field)
        {
            var isSpecialSorting = field.OrderByField == OrderBy.NAME
                                   && IsSpecialSorting(field.Language?.Code);

            return isSpecialSorting;
        }

        public bool IsSpecialSortingMeta(EsOrderByMetaField metaField)
        {
            return metaField.MetaType == typeof(string)
                   && IsSpecialSorting(metaField.Language?.Code);
        }

        private bool IsSpecialSorting(string languageCode)
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
    }
}