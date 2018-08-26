using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Notification;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("followTvSeries")]
    public class FollowTvSeriesController : IKalturaController
    {
        /// <summary>
        /// List user's tv series follows.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter">Follow TV series filter</param>
        /// <param name="pager">pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaFollowTvSeriesListResponse List(KalturaFollowTvSeriesFilter filter, KalturaFilterPager pager = null)
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
        [Action("listOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        static public KalturaListFollowDataTvSeriesResponse ListOldStandard(KalturaOrder? order_by = null, KalturaFilterPager pager = null)
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
        /// <param name="assetId">Asset identifier</param>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [OldStandardArgument("assetId", "asset_id")]
        [SchemeArgument("assetId", MinInteger = 1)]
        [Throws(eResponseStatus.UserNotFollowing)]
        [Throws(eResponseStatus.InvalidAssetId)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        static public bool Delete(int assetId)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

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
        /// <param name="followTvSeries">Follow series request parameters</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserAlreadyFollowing)]
        [Throws(eResponseStatus.InvalidAssetId)]
        static public KalturaFollowTvSeries Add(KalturaFollowTvSeries followTvSeries)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

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
        [Action("addOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("add")]
        [Obsolete]
        [SchemeArgument("assetId", MinInteger = 1)]
        [Throws(eResponseStatus.UserAlreadyFollowing)]
        [Throws(eResponseStatus.InvalidAssetId)]
        static public bool AddOldStandard(int asset_id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

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

        /// <summary>
        /// Delete a user's tv series follow.
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="token">User's token identifier</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Action("deleteWithToken")]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidToken)]
        static public void DeleteWithToken(int assetId, string token, int partnerId)
        {
            HttpContext.Current.Items.Add(Filters.RequestParser.REQUEST_GROUP_ID, partnerId);

            try
            {
                int userId = ClientsManager.NotificationClient().GetUserIdByToken(partnerId, token);

                ClientsManager.NotificationClient().DeleteUserTvSeriesFollow(partnerId, userId.ToString(), assetId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}