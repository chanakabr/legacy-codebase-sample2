using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting.Stages
{
    public interface ISortingStage
    {
        IEsOrderByField OrderByField { get; }
        StageStatus Status { get; }
        void SetSortedResults(IEnumerable<(long id, string sortValue)> results);
        IEnumerable<(long id, string sortValue)> SortedResults { get; }
    }
}
