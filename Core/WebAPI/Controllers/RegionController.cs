using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;
using WebAPI.ModelsValidators;

namespace WebAPI.Controllers
{
    [Service("region")]
    public class RegionController : IKalturaController
    {
        /// <summary>
        /// Returns all regions for the partner
        /// </summary>
        /// <param name="filter">Regions filter</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaRegionListResponse List(KalturaBaseRegionFilter filter, KalturaFilterPager pager = null)
        {
            filter.Validate();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            var groupId = KS.GetFromRequest().GroupId;
            var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            var response = filter.GetRegions(groupId, pager, responseProfile);

            return response;
        }

        /// <summary>
        /// Adds a new region for partner
        /// </summary>
        /// <param name="region">Region to add</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.RegionNotFound)]
        [Throws(eResponseStatus.RegionCannotBeParent)]
        [Throws(eResponseStatus.InputFormatIsInvalid)]
        [Throws(eResponseStatus.DuplicateRegionChannel)]
        [Throws(eResponseStatus.ParentAlreadyContainsChannel)]
        public static KalturaRegion Add(KalturaRegion region)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            region.Validate(isMultiLcnsEnabled, true);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().AddRegion(groupId, region, userId);

            return response;
        }

        /// <summary>
        /// Update an existing region
        /// </summary>
        /// <param name="region">Region to update</param>
        /// <param name="id">Region ID to update</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RegionNotFound)]
        [Throws(eResponseStatus.ExternalIdAlreadyExists)]
        [Throws(eResponseStatus.RegionCannotBeParent)]
        [Throws(eResponseStatus.InputFormatIsInvalid)]
        [Throws(eResponseStatus.DuplicateRegionChannel)]
        [Throws(eResponseStatus.ParentAlreadyContainsChannel)]
        public static KalturaRegion Update(int id, KalturaRegion region)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            region.Validate(isMultiLcnsEnabled, false);

            region.Id = id;

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().UpdateRegion(groupId, region, userId);

            return response;
        }

        /// <summary>
        /// Delete an existing region
        /// </summary>
        /// <param name="id">Region ID to delete</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RegionNotFound)]
        [Throws(eResponseStatus.DefaultRegionCannotBeDeleted)]
        [Throws(eResponseStatus.CannotDeleteRegionInUse)]
        [Throws(eResponseStatus.CannotDeleteSubRegionInUse)]
        public static void Delete(int id)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var userId = long.Parse(KS.GetFromRequest().UserId);

            ClientsManager.ApiClient().DeleteRegion(groupId, id, userId);
        }

        /// <summary>
        /// Adds a linear channel to the list of regions.
        /// </summary>
        /// <param name="linearChannelId">The identifier of the linear channel</param>
        /// <param name="regionChannelNumbers">List of regions and number of linear channel in it.</param>
        /// <returns></returns>
        [Action("linearchannelbulkadd")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool BulkAddLinearChannel(long linearChannelId, List<KalturaRegionChannelNumber> regionChannelNumbers)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var isMultiLcnsEnabled = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            RegionChannelNumberValidator.Validate(isMultiLcnsEnabled, regionChannelNumbers);

            var userId = Utils.Utils.GetUserIdFromKs();
            var response = ClientsManager.ApiClient().BulkUpdateRegions(groupId, userId, linearChannelId, regionChannelNumbers.AsReadOnly());

            return response;
        }

        /// <summary>
        /// Deletes a linear channel from the list of regions.
        /// </summary>
        /// <param name="linearChannelId">The identifier of the linear channel</param>
        /// <param name="regionIds">List of identifiers of regions.</param>
        /// <returns></returns>
        [Action("linearchannelbulkdelete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static bool BulkDeleteLinearChannel(long linearChannelId, string regionIds)
        {
            if (string.IsNullOrEmpty(regionIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(regionIds));
            }

            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();

            var response = false;
            try
            {
                var regionChannelNumbers = regionIds
                    .Split(',')
                    .Select(x => new KalturaRegionChannelNumber { RegionId = int.Parse(x), ChannelNumber = -1 })
                    .ToArray();

                response = ClientsManager.ApiClient().BulkUpdateRegions(groupId, userId, linearChannelId, regionChannelNumbers);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}