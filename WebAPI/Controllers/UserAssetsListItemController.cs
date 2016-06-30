using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/userAssetsListItem/action")]
    public class UserAssetsListItemController : ApiController
    {
        /// <summary>
        /// Adds a new item to user’s private asset list
        /// </summary>
        /// <param name="userAssetsListItem">A list item to add</param>
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaUserAssetsListItem Add(KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (userAssetsListItem.ListType == KalturaUserAssetsListType.ALL || userAssetsListItem.Type == KalturaUserAssetsListItemType.ALL)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "type and listType cannot be empty or all");
            }

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                response = ClientsManager.UsersClient().AddItemToUsersList(groupId, userId, userAssetsListItem);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get an item from user’s private asset list
        /// </summary>
        /// <param name="userAssetsListItem">A list item to get</param>
        /// <remarks>Possible status codes: 
        /// Item was not found in list = 2032</remarks>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        public KalturaUserAssetsListItem Get(KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                response = ClientsManager.UsersClient().GetItemFromUsersList(groupId, userId, userAssetsListItem);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Deletes an item from user’s private asset list
        /// </summary>
        /// <param name="userAssetsListItem">A list item to delete</param>
        /// <remarks>Possible status codes: 
        /// Item was not found in list = 2032</remarks>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        public bool Delete(KalturaUserAssetsListItem userAssetsListItem)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (userAssetsListItem.ListType == KalturaUserAssetsListType.ALL || userAssetsListItem.Type == KalturaUserAssetsListItemType.ALL)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "type and listType cannot be empty or all");
            }

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                response = ClientsManager.UsersClient().DeleteItemFromUsersList(groupId, userId, userAssetsListItem);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}