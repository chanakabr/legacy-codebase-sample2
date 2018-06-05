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
        /// List user's tv personal item to follow.
        /// <remarks>Possible status codes:</remarks>
        /// </summary>
        /// <param name="filter">Personal list filter</param>
        /// <param name="pager">pager</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidUser)]
        public KalturaPersonalListListResponse List(KalturaPersonalListFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaPersonalListListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;

            if (filter == null)
                filter = new KalturaPersonalListFilter();

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                int userId = 0;
                if (!int.TryParse(userID, out userId))
                {
                    throw new ClientException((int)eResponseStatus.InvalidUser, "Invalid userId");
                }

                response = ClientsManager.NotificationClient().GetPersonalListItems(groupId, userId, pager.PageSize.Value, pager.PageIndex.Value, filter.OrderBy, filter.PartnerListTypeEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        /// <summary>
        /// Add a user's personal list item to follow.
        /// </summary>
        /// <param name="personalList">Follow personal list item request parameters</param>
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

                return ClientsManager.NotificationClient().AddPersonalListItemToUser(groupId, userId, personalList);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Remove followed item from user's personal list 
        /// </summary>
        /// <param name="personalListId">personalListId identifier</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("personalListId", MinLong = 1)]
        [Throws(eResponseStatus.UserNotFollowing)]
        [Throws(eResponseStatus.AnnouncementNotFound)]
        [Throws(eResponseStatus.InvalidUser)]
        public void Delete(long personalListId)
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

                ClientsManager.NotificationClient().DeletePersonalListItemFromUser(groupId, userId, personalListId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}