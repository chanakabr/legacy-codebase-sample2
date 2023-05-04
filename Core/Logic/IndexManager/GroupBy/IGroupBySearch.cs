using ApiLogic.IndexManager.QueryBuilders;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.Catalog.IndexManager.GroupBy
{
    public interface IGroupBySearch
    {
        ESAggregationsResult HandleQueryResponse(UnifiedSearchDefinitions search, ESUnifiedQueryBuilder queryBuilder, string responseBody);
        void SetQueryPaging(UnifiedSearchDefinitions unifiedSearchDefinitions, ESUnifiedQueryBuilder queryBuilder);
    }
}