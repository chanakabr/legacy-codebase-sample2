using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.Catalog.Searcher.GroupBy
{
    internal interface IGroupBySearch
    {
        ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, int pageSize, int fromIndex, ESUnifiedQueryBuilder queryBuilder, string responseBody);
        void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder);
    }
}