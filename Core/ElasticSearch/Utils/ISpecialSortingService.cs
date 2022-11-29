using ElasticSearch.Searcher;

namespace ElasticSearch.Utils
{
    public interface ISpecialSortingService
    {
        bool IsSpecialSortingField(EsOrderByField field);
        bool IsSpecialSortingMeta(EsOrderByMetaField meta);
    }
}