using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/bookmark/action")]
    [OldStandardAction("listOldStandard", "list")]
    public class BookmarkController : ApiController
    {
        /// <summary>
        /// Returns player position record/s for the requested asset and the requesting user. 
        /// If default user makes the request – player position records are provided for all of the users in the household.
        /// If non-default user makes the request - player position records are provided for the requesting user and the default user of the household.
        /// </summary>
        /// <param name="filter">Filter option for the last position</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: User not exists in domain = 1020, Invalid user = 1026, Invalid asset type = 4021
        /// </remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaAssetsBookmarksResponse ListOldStandard(KalturaAssetsFilter filter)
        {
            KalturaAssetsBookmarksResponse response = null;

            if (filter.Assets == null || filter.Assets.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Assets cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int domain = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                response = ClientsManager.CatalogClient().GetAssetsBookmarksOldStandard(userID, groupId, domain, udid, filter.Assets);
                
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns player position record/s for the requested asset and the requesting user. 
        /// If default user makes the request – player position records are provided for all of the users in the household.
        /// If non-default user makes the request - player position records are provided for the requesting user and the default user of the household.
        /// </summary>
        /// <param name="filter">Filter option for the last position</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: User not exists in domain = 1020, Invalid user = 1026, Invalid asset type = 4021
        /// </remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaBookmarkListResponse List(KalturaBookmarkFilter filter)
        {
            KalturaBookmarkListResponse response = null;

            if (filter.AssetIn == null || filter.AssetIn.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Assets cannot be empty");
            }

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int domain = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

                response = ClientsManager.CatalogClient().GetAssetsBookmarks(userID, groupId, domain, udid, filter.AssetIn, filter.OrderBy);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        /// <summary>
        /// Report player position and action for the user on the watched asset. Player position is used to later allow resume watching.
        /// </summary>
        /// <param name="asset_id">Internal identifier of the asset </param>
        /// <param name="asset_type">The type of the asset</param>
        /// <param name="file_id">Identifier of the file</param>
        /// <param name="player_asset_data">Data regarding players status for the asset</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: BadRequest = 500003, ConcurrencyLimitation = 4001, InvalidAssetType = 4021, 
        /// ProgramDoesntExist = 4022, ActionNotRecognized = 4023, InvalidAssetId = 4024,</remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize(true)]
        public bool Add(string asset_id, KalturaAssetType asset_type, long file_id, KalturaPlayerAssetData player_asset_data)
        {
            bool response = false;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                int householdId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
                string siteGuid = KS.GetFromRequest().UserId;
                response = ClientsManager.CatalogClient().AddBookmark(groupId, siteGuid, householdId, udid, asset_id, asset_type, file_id, player_asset_data);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}