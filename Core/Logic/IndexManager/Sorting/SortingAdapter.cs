using System;
using System.Collections.Generic;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects.SearchObjects;

namespace ApiLogic.IndexManager.Sorting
{
    public class SortingAdapter : ISortingAdapter
    {
        private static readonly Lazy<ISortingAdapter> LazyInstance = new Lazy<ISortingAdapter>(
            () => new SortingAdapter(AssetOrderingService.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static ISortingAdapter Instance => LazyInstance.Value;

        private readonly IAssetOrderingService _assetOrderingService;

        public SortingAdapter(IAssetOrderingService assetOrderingService)
        {
            _assetOrderingService = assetOrderingService ?? throw new ArgumentNullException(nameof(assetOrderingService));
        }

        /// <summary>
        /// Temporary class to resolve ordering.
        /// </summary>
        /// <param name="definitions"></param>
        /// <returns>Collection of order by fields.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotImplementedException"></exception>
        public IReadOnlyCollection<IEsOrderByField> ResolveOrdering(UnifiedSearchDefinitions definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            if (definitions.orderByFields?.Count > 0)
            {
                return definitions.orderByFields;
            }

            if (definitions.order == null)
            {
                throw new NotImplementedException("It is impossible to determine ordering.");
            }

            var model = new AssetListEsOrderingCommonInput
            {
                GroupId = definitions.groupId,
                ShouldSearchEpg = definitions.shouldSearchEpg,
                ShouldSearchMedia = definitions.shouldSearchRecordings,
                ShouldSearchRecordings = definitions.shouldSearchRecordings,
                AssociationTags = definitions.associationTags,
                ParentMediaTypes = definitions.parentMediaTypes
            };

            var result = _assetOrderingService.MapToEsOrderByFields(definitions.order, model);

            return result.EsOrderByFields;
        }
    }
}
