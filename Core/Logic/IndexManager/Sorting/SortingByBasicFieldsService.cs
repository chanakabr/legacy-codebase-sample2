using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.IndexManager.Models;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ElasticSearch.Common;
using ElasticSearch.Searcher;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingByBasicFieldsService : ISortingByBasicFieldsService
    {
        private static readonly Lazy<ISortingByBasicFieldsService> LazyInstance =
            new Lazy<ISortingByBasicFieldsService>(
                () => new SortingByBasicFieldsService(StringComparerService.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        private static readonly Func<string, DateTime> DateTimeConverter =
            s => DateTime.TryParse(s, out var value) ? value : default;

        private readonly IStringComparerService _comparerService;

        public static ISortingByBasicFieldsService Instance => LazyInstance.Value;

        public SortingByBasicFieldsService(IStringComparerService comparerService)
        {
            _comparerService = comparerService;
        }

        [Obsolete]
        public IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            IEsOrderByField field)
        {
            var extendedUnifiedSearchResults = esAssetDocuments.Select(x =>
            {
                var fakeUnifiedSearchResult = new UnifiedSearchResult
                {
                    AssetId = x.asset_id.ToString()
                };

                return new ExtendedUnifiedSearchResult(fakeUnifiedSearchResult, x);
            }).ToArray();
            return GetSortedResult(extendedUnifiedSearchResults, field)
                   ?? throw new NotImplementedException(
                       $"It is not possible to determine how to order results in {nameof(SortingByBasicFieldsService)}.");
        }

        public IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults, IEsOrderByField field) =>
            GetSortedResult(extendedUnifiedSearchResults, field)
            ?? throw new NotImplementedException(
                $"It is not possible to determine how to order results in {nameof(SortingByBasicFieldsService)}.");

        private IEnumerable<(long id, string sortValue)> GetSortedResult(
            IEnumerable<ExtendedUnifiedSearchResult> esAssetDocuments,
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
            IEnumerable<ExtendedUnifiedSearchResult> esAssetDocuments,
            Func<ExtendedUnifiedSearchResult, T> valueSelector,
            IComparer<T> comparer,
            OrderDir orderDir)
        {
            var orderedItems = orderDir == OrderDir.ASC
                ? esAssetDocuments.OrderBy(valueSelector, comparer)
                : esAssetDocuments.OrderByDescending(valueSelector, comparer);

            return orderedItems
                .ThenByDescending(x => x.AssetId)
                .Select(x => ((long id, string value))(x.AssetId, valueSelector(x)?.ToString()))
                .ToArray();
        }

        private static T ExtractFromExtraFields<T>(
            ExtendedUnifiedSearchResult document,
            EsOrderByMetaField orderByField,
            Func<string, T> parseValue)
        {
            var metaValue = document.DocAdapter.GetMetaValue(orderByField);
            return !string.IsNullOrEmpty(metaValue) ? parseValue(metaValue) : default;
        }

        private IEnumerable<(long id, string sortValue)> Sort(
            IEnumerable<ExtendedUnifiedSearchResult> documents,
            EsOrderByField esOrderByField)
        {
            IEnumerable<(long id, string sortValue)> SortInternal<T>(
                Func<ExtendedUnifiedSearchResult, T> valueSelector, IComparer<T> valueComparer = null)
                => Sort(documents, valueSelector, valueComparer, esOrderByField.OrderByDirection);

            // TODO: Please, be aware that the pretty the same switch clause is placed in SortingService class. If you change smth there, you might need changes in SortingService class as well.
            switch (esOrderByField.OrderByField)
            {
                case OrderBy.ID:
                    return SortInternal(document => long.Parse(document.DocAdapter.Id));
                case OrderBy.START_DATE:
                    return SortInternal(document => document.DocAdapter.StartDate);
                case OrderBy.NAME:
                    var comparer = _comparerService.GetComparer(esOrderByField.Language?.Code);
                    return SortInternal(document => document.DocAdapter.Name, comparer);
                case OrderBy.CREATE_DATE:
                    return SortInternal(document => document.DocAdapter.CreateDate);
                case OrderBy.UPDATE_DATE:
                    return SortInternal(document => document.DocAdapter.UpdateDate);
                // TODO: This is used only for export sorting. 
                // case OrderBy.MEDIA_ID:
                // case OrderBy.EPG_ID:
                //     return SortInternal(document => document.asset_id);
                case OrderBy.NONE:
                case OrderBy.RELATED:
                    return SortInternal(document => document.DocAdapter.Score);
                default:
                    return null;
            }
        }

        private IEnumerable<(long id, string sortValue)> SortByMeta(
            IEnumerable<ExtendedUnifiedSearchResult> documents,
            EsOrderByMetaField esOrderByMetaField)
        {
            IEnumerable<(long id, string sortValue)> SortInternal<T>(Func<string, T> parseValue, IComparer<T> valueComparer = null) =>
                Sort(
                    documents,
                    document => ExtractFromExtraFields(document, esOrderByMetaField, parseValue), valueComparer,
                    esOrderByMetaField.OrderByDirection);

            if (esOrderByMetaField.MetaType == typeof(int))
            {
                return SortInternal(int.Parse);
            }

            if (esOrderByMetaField.MetaType == typeof(double))
            {
                return SortInternal(double.Parse);
            }

            if (esOrderByMetaField.MetaType == typeof(DateTime))
            {
                return SortInternal(DateTimeConverter);
            }

            if (esOrderByMetaField.MetaType == typeof(string))
            {
                var comparer = _comparerService.GetComparer(esOrderByMetaField.Language?.Code);
                return SortInternal(x => x, comparer);
            }

            return SortInternal(_ => _);
        }
    }
}
