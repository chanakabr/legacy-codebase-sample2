using Core.Catalog;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Phx.Lib.Appconfig;
using System;
using System.Threading;
using ApiLogic.Api.Managers.Rule;
using Phx.Lib.Log;

namespace ApiLogic.Catalog
{
    public interface ISearchProvider
    {
        UnifiedSearchResponse SearchAssets(long groupId, UserSearchContext searchContext, string filterQuery);
        UnifiedSearchResponse SearchAssets(long groupId, UserSearchContext searchContext, string filterQuery, int pageIndex, int pageSize);
    }

    public class SearchProvider : ISearchProvider
    {
        private const int MAX_PAGE_SIZE = 10000;

        private static readonly Lazy<SearchProvider> Lazy = new Lazy<SearchProvider>(
            () => new SearchProvider(new KLogger(nameof(ISearchProvider))),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IKLogger _logger;

        public static SearchProvider Instance => Lazy.Value;

        public SearchProvider(IKLogger logger)
        {
            _logger = logger;
        }

        public UnifiedSearchResponse SearchAssets(long groupId, UserSearchContext searchContext, string filterQuery)
        {
            return SearchAssets(groupId, searchContext, filterQuery, 0, MAX_PAGE_SIZE);
        }

        public UnifiedSearchResponse SearchAssets(long groupId, UserSearchContext searchContext, string filterQuery, int pageIndex, int pageSize)
        {
            UnifiedSearchResponse result = null;

            try
            {
                var catalogSignString = Guid.NewGuid().ToString();
                var catalogSignatureString = ApplicationConfiguration.Current.CatalogSignatureKey.Value;

                var catalogSignature = TVinciShared.WS_Utils.GetCatalogSignature(catalogSignString, catalogSignatureString);

                filterQuery = FilterAsset.Instance.UpdateKsql(filterQuery, (int)groupId, searchContext.SessionCharacteristicKey);

                try
                {
                    var request = new UnifiedSearchRequest
                    {
                        m_sSignature = catalogSignature,
                        m_sSignString = catalogSignString,
                        m_nGroupID = (int)groupId,
                        m_oFilter = new Filter
                        {
                            m_sDeviceId = searchContext.Udid,
                            m_nLanguage = searchContext.LanguageId,
                            m_bUseStartDate = searchContext.UseStartDate,
                            m_bUseFinalDate = searchContext.UseFinal,
                            m_bOnlyActiveMedia = searchContext.GetOnlyActiveAssets
                        },
                        filterQuery = filterQuery,
                        domainId = (int)searchContext.DomainId,
                        m_sSiteGuid = searchContext.UserId.ToString(),
                        m_sUserIP = searchContext.UserIp,
                        shouldIgnoreEndDate = searchContext.IgnoreEndDate,
                        isAllowedToViewInactiveAssets = searchContext.IsAllowedToViewInactiveAssets,
                        m_nPageIndex = pageIndex,
                        m_nPageSize = pageSize
                    };

                    var response = request.GetResponse(request);
                    result = (UnifiedSearchResponse)response;
                }
                catch (Exception ex)
                {
                    _logger.Error("Couldn't retrieve media assets from ElasticSearch", ex);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Configuration Reading - Couldn't read values from configuration ", ex);
            }

            return result;
        }
    }
}