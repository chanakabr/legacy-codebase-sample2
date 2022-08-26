using System.Linq;
using System.Net;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
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
        /// Return regional lineup (list of lineup channel asset objects) based on the requester session characteristics and his region.
        /// </summary>
        /// <param name="pageIndex">Page index - The page index to retrieve, (if it is not sent the default page size is 1).</param>
        /// <param name="pageSize">Page size - The page size to retrieve. Must be one of the follow numbers: 100, 200, 800, 1200, 1600 (if it is not sent the default page size is 500).</param>
        /// <returns>Regional lineup.</returns>
        /// <remarks>Possible status codes: InvalidActionParameters=500013.</remarks>
        /// /api_v3/service/lineup/action/get/partnerId/{{partnerId}}/regionId/{{regionId}}?pageIndex={{pageIndex}}&amp;pageSize={{pageSize}}&amp;language={{language}}
        [Action("get")]
        [ApiAuthorize]
        [AllowContentNotModifiedResponse]
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
    }
}