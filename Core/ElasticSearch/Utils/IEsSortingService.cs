using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ElasticSearch.Utils
{
    public interface IEsSortingService
    {
        bool ShouldSortByStartDateOfAssociationTags(IEnumerable<IEsOrderByField> esOrderByFields);
        bool ShouldSortByStatistics(IEnumerable<IEsOrderByField> esOrderByFields);
        bool ShouldSortByStatistics(IEsOrderByField esOrderByField);
        bool IsBucketsReorderingRequired(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields,
            GroupByDefinition distinctGroup);
        string GetSorting(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false);
        IEnumerable<string> BuildExtraReturnFields(IEnumerable<IEsOrderByField> orderByFields);
    }
}