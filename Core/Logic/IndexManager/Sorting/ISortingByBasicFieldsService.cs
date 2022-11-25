using System;
using System.Collections.Generic;
using ApiLogic.IndexManager.Models;
using ApiObjects.SearchObjects;
using Core.Catalog.Response;
using ElasticSearch.Common;

namespace ApiLogic.IndexManager.Sorting
{
    public interface ISortingByBasicFieldsService
    {
        /// <summary>
        /// You should not use this method! This is only for IndexManagerV2.
        /// </summary>
        /// <param name="esAssetDocuments"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        [Obsolete]
        IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ElasticSearchApi.ESAssetDocument> esAssetDocuments,
            IEsOrderByField field);
        
        IEnumerable<(long id, string sortValue)> ListOrderedIdsWithSortValues(
            IEnumerable<ExtendedUnifiedSearchResult> extendedUnifiedSearchResults,
            IEsOrderByField field);

        IEnumerable<(long id, string sortValue)> GetSortedAssets(
            IEnumerable<ExtendedSearchResult> searchResults, IEsOrderByField orderByField, string extraReturnField);
    }
}
