using System.Collections.Generic;
using ElasticSearch.NEST;
using Nest;

namespace ApiLogic.IndexManager.QueryBuilders
{
    public interface IUnifiedSearchNestBuilder
    {
        List<string> GetIndices();
        AggregationDictionary GetAggs();
        QueryContainer GetQuery();
        SearchDescriptor<T> SetSizeAndFrom<T>(SearchDescriptor<T> searchDescriptor) where T : class;
        SearchDescriptor<NestBaseAsset> SetFields(SearchDescriptor<NestBaseAsset> searchRequest);
        SortDescriptor<NestBaseAsset> GetSort();
    }
}
