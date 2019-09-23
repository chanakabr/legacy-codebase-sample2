using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
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
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RegionNotFound)]
        static public KalturaRegionListResponse List(KalturaRegionFilter filter)
        {
            KalturaRegionListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.ApiClient().GetRegions(groupId, filter);
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
    }
}