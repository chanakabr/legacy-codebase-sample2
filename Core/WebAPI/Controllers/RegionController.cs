using System.Collections.Generic;
using System.Linq;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

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
        [Throws(eResponseStatus.RegionNotFound)]
        static public KalturaRegionListResponse List(KalturaBaseRegionFilter filter, KalturaFilterPager pager = null)
        {
            KalturaRegionListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            KalturaBaseResponseProfile responseProfile = Utils.Utils.GetResponseProfileFromRequest();


            // parameters validation
            if (pager == null)
                pager = new KalturaFilterPager();

            filter.Validate();

            try
            {
                response = filter.GetRegions(groupId, pager, responseProfile);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

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
        static public KalturaRegion Add(KalturaRegion region)
        {
            KalturaRegion response = null;

            region.Validate(true);

            int groupId = KS.GetFromRequest().GroupId;
            long userId = long.Parse(KS.GetFromRequest().UserId);

            try
            {
                response = ClientsManager.ApiClient().AddRegion(groupId, region, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

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
        static public KalturaRegion Update(int id, KalturaRegion region)
        {
            KalturaRegion response = null;

            region.Validate();

            int groupId = KS.GetFromRequest().GroupId;
            long userId = long.Parse(KS.GetFromRequest().UserId);

            region.Id = id;

            try
            {
                response = ClientsManager.ApiClient().UpdateRegion(groupId, region, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

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
        static public void Delete(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = long.Parse(KS.GetFromRequest().UserId);

            try
            {
                ClientsManager.ApiClient().DeleteRegion(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
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
            if (regionChannelNumbers == null || regionChannelNumbers.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(regionChannelNumbers));
            }

            var groupId = KS.GetFromRequest().GroupId;
            var userId = Utils.Utils.GetUserIdFromKs();

            var response = false;
            try
            {
                response = ClientsManager.ApiClient().BulkUpdateRegions(groupId, userId, linearChannelId, regionChannelNumbers.AsReadOnly());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

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
