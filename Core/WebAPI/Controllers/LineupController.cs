using System.Collections.Generic;
using System.Linq;
using System.Net;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiObjects.Lineup;
using ApiObjects.Response;
using AutoMapper;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.Catalog.Lineup;
using WebAPI.Models.General;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Lineup Service API
    /// </summary>
    [Service("lineup")]
    public class LineupController : IKalturaController
    {
        private static readonly ILineupRequestValidator _lineupRequestValidator = LineupRequestValidator.Instance;
        private static readonly IMediaFileFilter _mediaFileFilter = MediaFileFilter.Instance;

        /// <summary>
        /// Returns regional lineup (list of lineup channel asset objects) based on the requester session characteristics and his region.
        /// NOTE: Calling lineup.get action using HTTP POST is supported only for tests (non production environment) and is rate limited or blocked.
        /// For production, HTTP GET shall be used: GET https://{Host_IP}/{build version}/api_v3/service/lineup/action/get
        /// </summary>
        /// <param name="pageIndex">Page index - The page index to retrieve, (if it is not sent the default page size is 1).</param>
        /// <param name="pageSize">Page size - The page size to retrieve. Must be one of the follow numbers: 100, 200, 800, 1200, 1600 (if it is not sent the default page size is 500).</param>
        /// <returns>Regional lineup.</returns>
        /// <remarks>Possible status codes: InvalidActionParameters=500013.</remarks>
        /// /api_v3/service/lineup/action/get/partnerId/{{partnerId}}/regionId/{{regionId}}?pageIndex={{pageIndex}}&amp;pageSize={{pageSize}}&amp;language={{language}}
        [Action("get")]
        [ApiAuthorize]
        [AllowContentNotModifiedResponse]
        [AllowUnauthorizedResponse]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static KalturaLineupChannelAssetListResponse Get(int? pageIndex, int? pageSize)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = _lineupRequestValidator.MinPageIndex;
            }
            else if (!_lineupRequestValidator.ValidatePageIndex(pageIndex.Value))
            {
                throw new ApiException(
                    new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, nameof(pageIndex), _lineupRequestValidator.MinPageIndex),
                    HttpStatusCode.BadRequest);
            }

            if (!pageSize.HasValue)
            {
                pageSize = _lineupRequestValidator.DefaultPageSize;
            }
            else if (!_lineupRequestValidator.ValidatePageSize(pageSize.Value))
            {
                throw new ApiException(new BadRequestException(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE, nameof(pageSize), string.Join(", ", _lineupRequestValidator.AllowedPageSizes)), HttpStatusCode.BadRequest);
            }

            var groupId = KS.GetFromRequest().GroupId;
            var regionId = KSUtils.ExtractKSPayload().RegionId;
            var searchContext = Utils.Utils.GetUserSearchContext();

            var response = ClientsManager.CatalogClient().GetLineup(groupId, regionId, searchContext, pageIndex.Value - 1, pageSize.Value);
            if (response.Objects.Count > 0)
            {
                _mediaFileFilter.FilterAssetFiles(response.Objects, groupId, searchContext.SessionCharacteristicKey);
            }

            return response;
        }

        /// <summary>
        /// Returns list of lineup regional linear channels associated with one LCN and its region information. Allows to apply sorting and filtering by LCN and linear channels.
        /// </summary>
        /// <param name="filter">Request filter</param>
        /// <param name="pager">Paging the request</param>
        /// <returns>Regional lineup.</returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.RegionNotFound)]
        public static KalturaLineupChannelAssetListResponse List(KalturaLineupRegionalChannelFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            _lineupRequestValidator.ValidateRequestFilter(filter);
            var searchContext = Utils.Utils.GetUserSearchContext();
            pager = pager ?? new KalturaFilterPager();
            var lineupRequest = new LineupRegionalChannelRequest
            {
                RegionId = filter.RegionIdEqual,
                Ksql = filter.KSql,
                LcnLessThanOrEqual = filter.LcnLessThanOrEqual,
                LcnGreaterThanOrEqual = filter.LcnGreaterThanOrEqual,
                ParentRegionIncluded = filter.ParentRegionIncluded ?? default,
                PartnerId = contextData.GroupId,
                PageIndex = pager.PageIndex.Value,
                PageSize = pager.PageSize.Value,
                OrderBy = Map(filter.OrderBy)
            };

            var response = LineupService.Instance.GetLineupChannelAssetsWithFilter(searchContext, lineupRequest);
            var result = new KalturaLineupChannelAssetListResponse
            {
                Objects = Mapper.Map<List<KalturaLineupChannelAsset>>(response.Objects),
                TotalCount = response.TotalItems
            };

            if (response.Objects.Count > 0)
            {
                _mediaFileFilter.FilterAssetFiles(result.Objects, contextData.GroupId, searchContext.SessionCharacteristicKey);
            }

            return result;
        }

        /// <summary>
        /// Sends lineup update requested notification.
        /// </summary>
        /// <param name="regionIds">Region IDs separated by commas.</param>
        /// /api_v3/service/lineup/action/sendUpdatedNotification
        [Action("sendUpdatedNotification")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.RegionNotFound)]
        public static bool SendUpdatedNotification(string regionIds)
        {
            var regionIdsList = Utils.Utils
                .ParseCommaSeparatedValues<long>(regionIds, nameof(regionIds))
                .ToList();

            if (!regionIds.Any())
            {
                throw new BadRequestException(
                    BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(regionIds),
                    HttpStatusCode.BadRequest);
            }

            var ks = KS.GetFromRequest();

            return ClientsManager.CatalogClient().SendUpdatedNotification(ks.GroupId, ks.UserId, regionIdsList);
        }

        private static LineupRegionalChannelOrderBy Map(KalturaLineupRegionalChannelOrderBy orderBy)
        {
            switch (orderBy)
            {
                case KalturaLineupRegionalChannelOrderBy.LCN_ASC:
                    return LineupRegionalChannelOrderBy.LCN_ASC;
                case KalturaLineupRegionalChannelOrderBy.LCN_DESC:
                    return LineupRegionalChannelOrderBy.LCN_DESC;
                case KalturaLineupRegionalChannelOrderBy.NAME_ASC:
                    return LineupRegionalChannelOrderBy.NAME_ASC;
                case KalturaLineupRegionalChannelOrderBy.NAME_DESC:
                    return LineupRegionalChannelOrderBy.NAME_DESC;
                default:
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, nameof(orderBy));
            }
        }
    }
}