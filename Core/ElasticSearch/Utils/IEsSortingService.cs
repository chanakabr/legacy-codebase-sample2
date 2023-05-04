using System;
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using Nest;

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
        bool ShouldReorderMissedKeyBucket(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields,
            GroupByDefinition distinctGroup,
            GroupingOption groupingOption);
        string GetSorting(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false);
        IEnumerable<string> BuildExtraReturnFields(IEnumerable<IEsOrderByField> orderByFields);
        SortDescriptor<NestBaseAsset> GetSortingV7(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false);
    }
}