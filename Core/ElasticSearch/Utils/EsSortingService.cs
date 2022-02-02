using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.SearchObjects;
using ElasticSearch.Searcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Utils
{
    public class EsSortingService : IEsSortingService
    {
        private static readonly Lazy<IEsSortingService> LazyInstance = new Lazy<IEsSortingService>(
            () => new EsSortingService(), LazyThreadSafetyMode.PublicationOnly);

        public static IEsSortingService Instance => LazyInstance.Value;

        public bool ShouldSortByStartDateOfAssociationTags(IEnumerable<IEsOrderByField> esOrderByFields)
            => esOrderByFields.Any(x => x is EsOrderByStartDateAndAssociationTags);

        public bool ShouldSortByStatistics(IEnumerable<IEsOrderByField> esOrderByFields)
            => esOrderByFields.Any(ShouldSortByStatistics);

        public bool ShouldSortByStatistics(IEsOrderByField esOrderByField)
            => esOrderByField is EsOrderByStatisticsField
                || esOrderByField is EsOrderBySlidingWindow
                || esOrderByField is EsOrderByField field && field.OrderByField == OrderBy.RECOMMENDATION
                || esOrderByField is EsOrderByStartDateAndAssociationTags;

        public bool IsBucketsReorderingRequired(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields,
            GroupByDefinition distinctGroup)
            => !string.IsNullOrEmpty(distinctGroup?.Key)
                && (esOrderByFields.Count != 1 || !IsBucketsOrderingSupportedByEs(esOrderByFields.Single()));

        public string GetSorting(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false)
        {
            var fields = new List<IEsOrderByField>(orderByFields);
            if (functionScoreSort)
            {
                fields.RemoveAll(x => x is EsOrderByField field
                    && (field.OrderByField == OrderBy.NONE || field.OrderByField == OrderBy.RELATED));
                fields.Insert(0, new EsOrderByField(OrderBy.RELATED, OrderDir.DESC));
            }

            var orderingsToInclude = fields
                .TakeWhile(x => !ShouldSortByStatistics(x))
                .ToList();

            // Always add sort by _id to avoid ES weirdness of same sort-value
            if (!orderingsToInclude.Any(x => x is EsOrderByField field && field.OrderByField == OrderBy.ID))
            {
                orderingsToInclude.Add(new EsOrderByField(OrderBy.ID, OrderDir.DESC));
            }

            var result = new JArray();
            foreach (var orderByField in orderingsToInclude)
            {
                result.Add(orderByField.EsOrderByObject);
            }

            return $"\"sort\" : {result.ToString(Formatting.None)}";
        }

        public IEnumerable<string> BuildExtraReturnFields(IEnumerable<IEsOrderByField> orderByFields)
            => orderByFields.Any(ShouldSortByStatistics)
                ? orderByFields
                    .Where(x => !ShouldSortByStatistics(x))
                    .Select(x => x.EsField)
                    .ToArray()
                : Enumerable.Empty<string>().ToArray();

        private static bool IsBucketsOrderingSupportedByEs(IEsOrderByField esOrderByField)
            => esOrderByField is EsOrderByField field
                && (field.OrderByField == OrderBy.START_DATE
                || field.OrderByField == OrderBy.CREATE_DATE
                || field.OrderByField == OrderBy.NONE
                || field.OrderByField == OrderBy.RELATED);
    }
}