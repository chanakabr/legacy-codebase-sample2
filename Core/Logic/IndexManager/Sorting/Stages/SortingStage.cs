using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting.Stages
{
    public class SortingStage : ISortingStage
    {
        public SortingStage(IEsOrderByField orderByField, StageStatus status)
        {
            OrderByField = orderByField;
            Status = status;
        }

        public IEsOrderByField OrderByField { get; }

        public StageStatus Status { get; }

        public void SetSortedResults(IEnumerable<(long id, string sortValue)> results)
        {
            SortedResults = results;
        }

        public IEnumerable<(long id, string sortValue)> SortedResults { get; private set; }
    }
}
