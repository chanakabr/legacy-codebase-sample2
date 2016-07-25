using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/followTvSeries/action")]
    [OldStandardAction("addOldStandard", "add")]
    [OldStandardAction("listOldStandard", "list")]
    public class FollowTvSeriesController : ApiController
    {
        /// <summary>
        /// List user's tv series follows.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTvSeriesListResponse List(KalturaFollowTvSeriesFilter filter, KalturaFilterPager pager = null)
        {
            KalturaFollowTvSeriesListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                response = ClientsManager.NotificationClient().ListUserTvSeriesFollows(groupId, userID, pager.PageSize.Value, pager.PageIndex.Value, filter.OrderBy);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// List user's tv series follows.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="order_by"></param>
        /// <param name="pager"></param>
        /// <returns></returns>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaListFollowDataTvSeriesResponse ListOldStandard(KalturaOrder? order_by = null, KalturaFilterPager pager = null)
        {
            KalturaListFollowDataTvSeriesResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                response = ClientsManager.NotificationClient().GetUserTvSeriesFollows(groupId, userID, pager.PageSize.Value, pager.PageIndex.Value, order_by);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete a user's tv series follow.
        /// <remarks>Possible status codes: UserNotFollowing = 8012, NotFound = 500007, InvalidAssetId = 4024, AnnouncementNotFound = 8006</remarks>
        /// </summary>
        /// <param name="assetId"></param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandard("assetId", "asset_id")]
        public bool Delete(int assetId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (assetId <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal asset ID");

            try
            {
                response = ClientsManager.NotificationClient().DeleteUserTvSeriesFollow(groupId, userID, assetId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add a user's tv series follow.
        /// <remarks>Possible status codes: UserAlreadyFollowing = 8013, NotFound = 500007, InvalidAssetId = 4024</remarks>
        /// </summary>
        /// <param name="followTvSeries"></param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaFollowTvSeries Add(KalturaFollowTvSeries followTvSeries)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (followTvSeries.AssetId <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal asset ID");

            try
            {
                return ClientsManager.NotificationClient().AddUserTvSeriesFollow(groupId, userID, followTvSeries.AssetId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Add a user's tv series follow.
        /// <remarks>Possible status codes: UserAlreadyFollowing = 8013, NotFound = 500007, InvalidAssetId = 4024</remarks>
        /// </summary>
        /// <param name="asset_id"></param>
        /// <returns></returns>
        [Route("addOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public bool AddOldStandard(int asset_id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (asset_id <= 0)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal asset ID");

            try
            {
                ClientsManager.NotificationClient().AddUserTvSeriesFollow(groupId, userID, asset_id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return true;
        }
    }
}