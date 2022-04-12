using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingByBasicFieldsService : ISortingByBasicFieldsService
    {
        private static readonly Lazy<ISortingByBasicFieldsService> LazyInstance =
            new Lazy<ISortingByBasicFieldsService>(
                () => new SortingByBasicFieldsService(),
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Func<string, DateTime> DateTimeConverter =
            s => DateTime.TryParse(s, out var value) ? value : default;

        public static ISortingByBasicFieldsService Instance => LazyInstance.Value;

        public IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            IEsOrderByField field)
            => GetSortedResult(esAssetDocuments, field)
                ?? throw new NotImplementedException(
                    $"It is not possible to determine how to order results in {nameof(SortingByBasicFieldsService)}.");

        private static IEnumerable<(long id, string sortValue)> GetSortedResult(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            IEsOrderByField field)
        {
            switch (field)
            {
                case EsOrderByMetaField orderByMetaField:
                    return SortByMeta(esAssetDocuments, orderByMetaField);
                case EsOrderByField orderByField:
                    return Sort(esAssetDocuments, orderByField);
                default:
                    return null;
            }
        }

        private static IEnumerable<(long id, string sortValue)> Sort<T>(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            Func<ElasticSearchApi.ESAssetDocument, T> valueSelector,
            OrderDir orderDir)
        {
            var orderedItems = orderDir == OrderDir.ASC
                ? esAssetDocuments.OrderBy(valueSelector)
                : esAssetDocuments.OrderByDescending(valueSelector);

            return orderedItems
                .ThenByDescending(x => x.id)
                .Select(x => ((long id, string value))(x.asset_id, valueSelector(x)?.ToString()))
                .ToArray();
        }

        private static T ExtractFromExtraFields<T>(
            ElasticSearchApi.ESAssetDocument document,
            string metaName,
            Func<string, T> parseValue)
            => document.extraReturnFields.TryGetValue(metaName, out var value) ? parseValue(value) : default;

        private static IEnumerable<(long id, string sortValue)> Sort(
            IEnumerable<ElasticSearchApi.ESAssetDocument> documents,
            EsOrderByField esOrderByField)
        {
            IEnumerable<(long id, string sortValue)> SortInternal<T>(
                Func<ElasticSearchApi.ESAssetDocument, T> valueSelector)
                => Sort(documents, valueSelector, esOrderByField.OrderByDirection);

            // TODO: Please, be aware that the pretty the same switch clause is placed in SortingService class. If you change smth there, you might need changes in SortingService class as well.
            switch (esOrderByField.OrderByField)
            {
                case OrderBy.ID:
                    return SortInternal(document => long.Parse(document.id));
                case OrderBy.START_DATE:
                    return SortInternal(document => document.start_date);
                case OrderBy.NAME:
                    return SortInternal(document => document.name);
                case OrderBy.CREATE_DATE:
                    return SortInternal(document => ExtractFromExtraFields(
                        document, nameof(OrderBy.CREATE_DATE).ToLower(), DateTimeConverter));
                case OrderBy.UPDATE_DATE:
                    return SortInternal(document => document.update_date);
                case OrderBy.MEDIA_ID:
                case OrderBy.EPG_ID:
                    return SortInternal(document => document.asset_id);
                case OrderBy.NONE:
                case OrderBy.RELATED:
                    return SortInternal(document => document.score);
                default:
                    return null;
            }
        }

        private static IEnumerable<(long id, string sortValue)> SortByMeta(
            IEnumerable<ElasticSearchApi.ESAssetDocument> documents,
            EsOrderByMetaField esOrderByMetaField)
        {
            IEnumerable<(long id, string sortValue)> SortInternal<T>(Func<string, T> parseValue) =>
                Sort(
                    documents,
                    document => ExtractFromExtraFields(document, esOrderByMetaField.EsField, parseValue),
                    esOrderByMetaField.OrderByDirection);

            if (esOrderByMetaField.MetaType == typeof(int))
            {
                return SortInternal(int.Parse);
            }

            if (esOrderByMetaField.MetaType == typeof(double))
            {
                return SortInternal(double.Parse);
            }

            return esOrderByMetaField.MetaType == typeof(DateTime)
                ? SortInternal(DateTimeConverter)
                : SortInternal(_ => _);
        }
    }
}
