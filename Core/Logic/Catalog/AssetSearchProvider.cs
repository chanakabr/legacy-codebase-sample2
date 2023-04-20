using System;
using System.Threading;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Catalog
{
    public interface ISearchProvider
    {
        UnifiedSearchResponse SearchAssets(UnifiedSearchRequest request);
    }

    public class SearchProvider : ISearchProvider
    {
        private static readonly Lazy<SearchProvider> Lazy = new Lazy<SearchProvider>(
            () => new SearchProvider(),
            LazyThreadSafetyMode.PublicationOnly);

        public static SearchProvider Instance => Lazy.Value;

        public UnifiedSearchResponse SearchAssets(UnifiedSearchRequest request)
        {
            var response = request.GetResponse(request);

            return (UnifiedSearchResponse)response;
        }
    }
}