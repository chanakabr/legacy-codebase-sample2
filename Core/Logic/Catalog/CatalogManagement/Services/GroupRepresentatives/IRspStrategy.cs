using System.Collections.Generic;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Catalog.Response;
using Core.Catalog.Response;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public interface IRspStrategy
    {
        GenericResponse<IEnumerable<UnifiedSearchResult>> ApplyPolicy(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            IEnumerable<AggregationResult> aggregations);
    }
}