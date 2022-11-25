using System;
using System.Collections.Generic;
using System.Threading;
using ApiLogic.Api.Managers.Rule;
using ApiObjects.SearchObjects;
using ApiObjects.SearchObjects.GroupRepresentatives;
using Core.Catalog;
using Core.Catalog.Request;

namespace ApiLogic.Catalog.CatalogManagement.Services.GroupRepresentatives
{
    public class GroupRepresentativesExtendedRequestMapper : IGroupRepresentativesExtendedRequestMapper
    {
        // The restriction for groups count is defined in spec (ASU-01):
        // https://kaltura.atlassian.net/wiki/spaces/RSA/pages/3790733313/VIP-2179+Asset+aggregation+rep+logic+Design+Specification+Overview
        private const int GROUP_REPRESENTATIVES_MAX_COUNT = 4000;

        private static readonly Lazy<IGroupRepresentativesExtendedRequestMapper> LazyInstance =
            new Lazy<IGroupRepresentativesExtendedRequestMapper>(
                () => new GroupRepresentativesExtendedRequestMapper(),
                LazyThreadSafetyMode.PublicationOnly);

        public static IGroupRepresentativesExtendedRequestMapper Instance => LazyInstance.Value;

        public ExtendedSearchRequest BuildRequest(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            string extraReturnField)
        {
            var extendedSearchRequest = BuildBaseRequest(request, clientData);
            extendedSearchRequest.filterQuery = request.Filter;
            extendedSearchRequest.GroupByOption = MapToGroupingOption(request.UnmatchedItemsPolicy);
            extendedSearchRequest.ExtraReturnFields = !string.IsNullOrEmpty(extraReturnField)
                ? new List<string> { extraReturnField }
                : null;

            return extendedSearchRequest;
        }

        public ExtendedSearchRequest BuildRequest(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            BooleanPhraseNode filterTree)
        {
            var extendedSearchRequest = BuildBaseRequest(request, clientData);
            extendedSearchRequest.filterQuery = request.Filter;
            extendedSearchRequest.filterTree = filterTree;

            return extendedSearchRequest;
        }

        public ExtendedSearchRequest BuildEntitledRequest(
            GroupRepresentativesRequest request,
            CatalogClientData clientData,
            string extraReturnField)
        {
            var extendedSearchRequest = BuildBaseRequest(request, clientData);
            extendedSearchRequest.ExtraReturnFields = !string.IsNullOrEmpty(extraReturnField)
                ? new List<string> { extraReturnField }
                : null;
            extendedSearchRequest.filterQuery = string.IsNullOrEmpty(request.Filter)
                ? KsqlBuilder.EntitledAssetsOnly
                : KsqlBuilder.And(KsqlBuilder.EntitledAssetsOnly, request.Filter);

            return extendedSearchRequest;
        }

        private static GroupingOption MapToGroupingOption(UnmatchedItemsPolicy unmatchedItemsPolicy)
        {
            switch (unmatchedItemsPolicy)
            {
                case UnmatchedItemsPolicy.IncludeAndMerge:
                    return GroupingOption.Include;
                case UnmatchedItemsPolicy.Group:
                    return GroupingOption.Group;
                default:
                    return GroupingOption.Omit;
            }
        }

        private static IReadOnlyCollection<AssetOrder> ResolveGroupOrdering(GroupRepresentativesRequest request)
        {
            if (request.SelectionPolicy is TopSubscriptionEntitledRsp topSubscriptionEntitledRsp)
            {
                return topSubscriptionEntitledRsp.OrderingParameters;
            }

            return request.SelectionPolicy is TopAssetRsp topAssetRsp
                ? topAssetRsp.OrderingParameters
                : new[] { new AssetOrder { Field = OrderBy.CREATE_DATE, Direction = OrderDir.DESC } };
        }

        private static ExtendedSearchRequest BuildBaseRequest(GroupRepresentativesRequest request, CatalogClientData clientData) =>
            new ExtendedSearchRequest
            {
                m_sSignature = clientData.Signature,
                m_sSignString = clientData.SignString,
                m_oFilter = new Filter
                {
                    m_sDeviceId = request.Udid,
                    m_nLanguage = request.LanguageId,
                    m_bUseStartDate = request.UseStartDate,
                    m_bOnlyActiveMedia = request.GetOnlyActiveAssets
                },
                m_sUserIP = request.UserIp,
                m_nGroupID = request.PartnerId,
                m_dServerTime = clientData.ServerTime,
                m_sSiteGuid = request.UserId.ToString(),
                domainId = (int)request.DomainId,
                isAllowedToViewInactiveAssets = request.IsAllowedToViewInactiveAssets,
                searchGroupBy = new SearchAggregationGroupBy
                {
                    groupBy = new List<string> { request.GroupByValue },
                    distinctGroup = request.GroupByValue,
                    topHitsCount = 1
                },
                m_nPageIndex = 0,
                m_nPageSize = GROUP_REPRESENTATIVES_MAX_COUNT,
                orderingParameters = ResolveGroupOrdering(request)
            };
    }
}