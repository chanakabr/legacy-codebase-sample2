using System;
using System.Collections.Generic;
using ApiLogic.Api.Managers.Rule;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.Request;
using Microsoft.Extensions.Logging;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using TVinciShared;

namespace ApiLogic.Catalog
{
    public class UnifiedSearchRequestBuilder
    {
        private const int MaxPageSize = 10000;

        private readonly IFilterAsset _filterAsset;

        private int PageIndex { get; set; }
        private int PageSize { get; set; } = MaxPageSize;
        private string FilterQuery { get; set; }
        private IReadOnlyCollection<AssetOrder> OrderingParameters { get; set; }

        public UnifiedSearchRequestBuilder(IFilterAsset filterAsset)
        {
            _filterAsset = filterAsset;
        }

        public UnifiedSearchRequestBuilder WithPageSize(int pageSize)
        {
            PageSize = pageSize;

            return this;
        }

        public UnifiedSearchRequestBuilder WithPageIndex(int pageIndex)
        {
            PageIndex = pageIndex;

            return this;
        }

        public UnifiedSearchRequestBuilder WithOrdering(IReadOnlyCollection<AssetOrder> orderingParameters)
        {
            OrderingParameters = orderingParameters;

            return this;
        }

        public UnifiedSearchRequestBuilder WithFilterQuery(string filterQuery)
        {
            FilterQuery = filterQuery;

            return this;
        }

        public UnifiedSearchRequest Build(long groupId, UserSearchContext userSearchContext)
        {
            var filterQuery = _filterAsset.UpdateKsql(FilterQuery, (int)groupId,
                userSearchContext.SessionCharacteristicKey);
            var catalogSignString = Guid.NewGuid().ToString();
            var catalogSignatureString = ApplicationConfiguration.Current.CatalogSignatureKey.Value;
            var catalogSignature = WS_Utils.GetCatalogSignature(catalogSignString, catalogSignatureString);

            return new UnifiedSearchRequest
            {
                m_sSignature = catalogSignature,
                m_sSignString = catalogSignString,
                m_nGroupID = (int)groupId,
                m_oFilter = new Filter
                {
                    m_sDeviceId = userSearchContext.Udid,
                    m_nLanguage = userSearchContext.LanguageId,
                    m_bUseStartDate = userSearchContext.UseStartDate,
                    m_bUseFinalDate = userSearchContext.UseFinal,
                    m_bOnlyActiveMedia = userSearchContext.GetOnlyActiveAssets
                },
                filterQuery = filterQuery,
                domainId = (int)userSearchContext.DomainId,
                m_sSiteGuid = userSearchContext.UserId.ToString(),
                m_sUserIP = userSearchContext.UserIp,
                shouldIgnoreEndDate = userSearchContext.IgnoreEndDate,
                isAllowedToViewInactiveAssets = userSearchContext.IsAllowedToViewInactiveAssets,
                m_nPageIndex = PageIndex,
                m_nPageSize = PageSize,
                orderingParameters = OrderingParameters
            };
        }
    }
}