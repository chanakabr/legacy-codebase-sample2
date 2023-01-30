using System.Collections.Generic;
using ApiObjects;
using Catalog.Response;
using Core.Catalog.Request;
using Core.Catalog.Response;

namespace Core.Catalog
{
    public interface ICatalogLogic
    {
        /// <summary>
        /// Builds search object and performs query to get asset Ids that match the request requirements
        /// </summary>
        /// <param name="request"></param>
        /// <param name="totalItems"></param>
        /// <param name="to"></param>
        /// <param name="aggregationsResults"></param>
        /// <returns></returns>
        List<UnifiedSearchResult> GetAssetIdFromSearcher(
            UnifiedSearchRequest request,
            ref int totalItems,
            ref int to,
            out List<AggregationsResult> aggregationsResults);
    }
}