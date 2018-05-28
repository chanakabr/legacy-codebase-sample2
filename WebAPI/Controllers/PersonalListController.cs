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
    [RoutePrefix("_service/personalList/action")]
    public class PersonalListController : ApiController
    {
        /// <summary>
        /// List user's tv series follows.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter">Follow TV series filter</param>
        /// <param name="pager">pager</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaPersonalListListResponse List(KalturaPersonalListFilter filter, KalturaFilterPager pager = null)
        {
            KalturaPersonalListListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                // TODO IRENA
                //response = ClientsManager.NotificationClient().ListUserTvSeriesFollows(groupId, userID, pager.PageSize.Value, pager.PageIndex.Value, filter.OrderBy);
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
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [OldStandardArgument("assetId", "asset_id")]
        [SchemeArgument("assetId", MinInteger = 1)]
        [Throws(eResponseStatus.UserNotFollowing)]
        [Throws(eResponseStatus.InvalidAssetId)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        public bool Delete(int assetId)
        {
            // TODO SHIR
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
        /// <param name="personalList">Follow series request parameters</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.UserAlreadyFollowing)]
        [Throws(eResponseStatus.InvalidUser)]
        public KalturaPersonalList Add(KalturaPersonalList personalList)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            try
            {
                int userId = 0;
                if (!int.TryParse(userID, out userId))
                {
                    throw new ClientException((int)eResponseStatus.InvalidUser, "Invalid Username");
                }

                return ClientsManager.NotificationClient().AddUserPersonalList(groupId, userId, personalList);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }
        
        /// <summary>
        /// Delete a user's tv series follow.
        /// </summary>
        /// <param name="assetId">Asset identifier</param>
        /// <param name="token">User's token identifier</param>
        /// <param name="partnerId">Partner identifier</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Route("deleteWithToken"), HttpPost]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidToken)]
        public void DeleteWithToken(int assetId, string token, int partnerId)
        {
            //TODO SHIR
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