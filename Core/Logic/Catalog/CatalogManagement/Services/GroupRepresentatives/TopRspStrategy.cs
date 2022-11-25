using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Catalog.Response;
using Core.Catalog.Response;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public class TopRspStrategy : IRspStrategy
    {
        private static readonly Lazy<IRspStrategy> LazyInstance = new Lazy<IRspStrategy>(
            () => new TopRspStrategy(), LazyThreadSafetyMode.PublicationOnly);

        public static IRspStrategy Instance => LazyInstance.Value;

        public GenericResponse<IEnumerable<UnifiedSearchResult>> ApplyPolicy(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEnumerable<AggregationResult> aggregations)
        {
            var assetsToReorder = aggregations.Select(x => x.topHits[0]).ToList();

            return new GenericResponse<IEnumerable<UnifiedSearchResult>>(Status.Ok, assetsToReorder);
        }
    }
}