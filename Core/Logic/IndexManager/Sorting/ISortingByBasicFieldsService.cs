using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingByBasicFieldsService
    {
        IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            IEsOrderByField field);
    }
}