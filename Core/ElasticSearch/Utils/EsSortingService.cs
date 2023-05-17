using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.SearchObjects;
using ElasticSearch.NEST;
using ElasticSearch.Searcher;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElasticSearch.Utils
{
    public class EsSortingService : IEsSortingService
    {
        private static readonly Lazy<IEsSortingService> LazyInstanceV2 = new Lazy<IEsSortingService>(
            () => new EsSortingService(ElasticsearchVersion.ES_2_3), LazyThreadSafetyMode.PublicationOnly);

        private static readonly Lazy<IEsSortingService> LazyInstanceV7 = new Lazy<IEsSortingService>(
            () => new EsSortingService(ElasticsearchVersion.ES_7), LazyThreadSafetyMode.PublicationOnly);

        private readonly ElasticsearchVersion _version = ElasticsearchVersion.ES_2_3;

        public static IEsSortingService Instance(ElasticsearchVersion version)
            => version == ElasticsearchVersion.ES_2_3 ? LazyInstanceV2.Value : LazyInstanceV7.Value;

        public EsSortingService(ElasticsearchVersion version)
        {
            _version = version;
        }

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

        public bool ShouldReorderMissedKeyBucket(
            IReadOnlyCollection<IEsOrderByField> esOrderByFields,
            GroupByDefinition distinctGroup,
            GroupingOption groupingOption)
            => !IsBucketsReorderingRequired(esOrderByFields, distinctGroup)
                && !string.IsNullOrEmpty(distinctGroup?.Key)
                && groupingOption == GroupingOption.Include;

        public SortDescriptor<NestBaseAsset> GetSortingV7(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false)
        {
            var orderingsToInclude = AdjustOrderingFields(orderByFields, functionScoreSort);
            var sortDescriptor = new SortDescriptor<NestBaseAsset>();
            foreach (var orderByField in orderingsToInclude)
            {
                sortDescriptor = sortDescriptor.Field(x =>
                {
                    var order = orderByField.Field.OrderByDirection == OrderDir.ASC
                        ? SortOrder.Ascending
                        : SortOrder.Descending;
                    x = x.Field(orderByField.EsV7Field).Order(order);
                    if (orderByField.Field is EsOrderByMetaField esOrderByMetaField &&
                        esOrderByMetaField.IsMissingFirst)
                    {
                        x = x.MissingFirst();
                    }

                    return x;
                });
            }

            return sortDescriptor;
        }
        public string GetSorting(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort = false)
        {
            var orderingsToInclude = AdjustOrderingFields(orderByFields, functionScoreSort);
            var result = new JArray();
            foreach (var orderByField in orderingsToInclude)
            {
                result.Add(GenerateSortObject(orderByField));
            }

            return $"\"sort\" : {result.ToString(Formatting.None)}";
        }

        private List<EsOrderByFieldAdapter> AdjustOrderingFields(IEnumerable<IEsOrderByField> orderByFields, bool functionScoreSort)
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

            return orderingsToInclude.Select(x => new EsOrderByFieldAdapter(x)).ToList();
        }

        public IEnumerable<string> BuildExtraReturnFields(IEnumerable<IEsOrderByField> orderByFields)
            => _version == ElasticsearchVersion.ES_7
                ? BuildExtraReturnFields(orderByFields, field => new EsOrderByFieldAdapter(field).EsV7Field.ToString())
                : BuildExtraReturnFields(orderByFields, field => new EsOrderByFieldAdapter(field).EsField);

        private IEnumerable<string> BuildExtraReturnFields(
            IEnumerable<IEsOrderByField> orderByFields,
            Func<IEsOrderByField, string> valueExtractor)
            => orderByFields.Any(ShouldSortByStatistics)
                ? orderByFields
                    .Where(x => !ShouldSortByStatistics(x)
                        && !(x is EsOrderByField esOrderByField && esOrderByField.OrderByField == OrderBy.ID)) // ID is included by default
                    .Select(valueExtractor)
                    .ToArray()
                : Enumerable.Empty<string>().ToArray();

        private static bool IsBucketsOrderingSupportedByEs(IEsOrderByField esOrderByField)
            => esOrderByField is EsOrderByField field
                && (field.OrderByField == OrderBy.START_DATE
                || field.OrderByField == OrderBy.CREATE_DATE
                || field.OrderByField == OrderBy.NONE
                || field.OrderByField == OrderBy.RELATED);

        private static JObject GenerateSortObject(EsOrderByFieldAdapter orderByField)
        {
            if (string.IsNullOrEmpty(orderByField.EsField))
            {
                return null;
            }

            var esFieldObject = new JObject
            {
                ["order"] = JToken.FromObject(orderByField.Field.OrderByDirection.ToString().ToLower())
            };

            if (orderByField.Field is EsOrderByMetaField orderByMetaField && orderByMetaField.IsMissingFirst)
            {
                esFieldObject["missing"] = JToken.FromObject("_first");
            }

            return new JObject { [orderByField.EsField] = esFieldObject };
        }
    }
}
