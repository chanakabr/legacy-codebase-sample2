using System;

namespace ElasticSearch.Utils
{
    public interface ISpecialSortingService
    {
        bool IsSpecialSortingField(string languageCode);
        bool IsSpecialSortingMeta(string languageCode, Type metaType);
    }
}