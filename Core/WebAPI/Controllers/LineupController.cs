using System.Net;
using ApiLogic.Catalog;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
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
        private static readonly ILineupRequestValidator LineupRequestValidator = new LineupRequestValidator();

        /// <summary>
        /// Return regional lineup (list of lineup channel asset objects) based on the requester session characteristics and his region.
        /// </summary>
        /// <param name="pageIndex">Page index - The page index to retrieve, (if it is not sent the default page size is 1).</param>
        /// <param name="pageSize">Page size - The page size to retrieve. Must be one of the follow numbers: 100, 200, 800, 1200, 1600 (if it is not sent the default page size is 500).</param>
        /// <returns>Regional lineup.</returns>
        /// <remarks>Possible status codes: InvalidActionParameters=500013.</remarks>
        /// /api_v3/service/lineup/action/get/partnerId/{{partnerId}}/regionId/{{regionId}}?pageIndex={{pageIndex}}&amp;pageSize={{pageSize}}&amp;language={{language}}
        [Action("get")]
        [AllowContentNotModifiedResponse]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        public static KalturaLineupChannelAssetListResponse Get(int? pageIndex, int? pageSize)
        {
            if (!pageIndex.HasValue)
            {
                pageIndex = LineupRequestValidator.MinPageIndex;
            }
            else if (!LineupRequestValidator.ValidatePageIndex(pageIndex.Value))
            {
                throw new ApiException(new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, nameof(pageIndex), LineupRequestValidator.MinPageIndex), HttpStatusCode.BadRequest);
            }

            if (!pageSize.HasValue)
            {
                pageSize = LineupRequestValidator.DefaultPageSize;
            }
            else if (!LineupRequestValidator.ValidatePageSize(pageSize.Value))
            {
                throw new ApiException(new BadRequestException(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE, nameof(pageSize), string.Join(", ", LineupRequestValidator.AllowedPageSizes)), HttpStatusCode.BadRequest);
            }

            var groupId = KS.GetFromRequest().GroupId;
            var domainId = KS.GetContextData().DomainId ?? 0;
            var regionId = KSUtils.ExtractKSPayload().RegionId;
            var userId = Utils.Utils.GetUserIdFromKs();
            var languageId = Utils.Utils.GetLanguageId(groupId, KS.GetContextData().Language);
            var udid = KS.GetContextData().Udid;
            var userIp = Utils.Utils.GetClientIP();
            var isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            var group = GroupsManager.Instance.GetGroup(groupId);

            var searchContext = new UserSearchContext(domainId, userId, languageId, udid, userIp, false, group.UseStartDate, false, group.GetOnlyActiveAssets, isAllowedToViewInactiveAssets);

            var response = ClientsManager.CatalogClient().GetLineup(groupId, regionId, searchContext, pageIndex.Value - 1, pageSize.Value);

            return response;
        }
    }
}