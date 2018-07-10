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
using WebAPI.Models.Users;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("userAssetsListItem")]
    public class UserAssetsListItemController : IKalturaController
    {
        /// <summary>
        /// Adds a new item to user’s private asset list
        /// </summary>
        /// <param name="userAssetsListItem">A list item to add</param>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaUserAssetsListItem Add(KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (userAssetsListItem.ListType == KalturaUserAssetsListType.all || userAssetsListItem.Type == KalturaUserAssetsListItemType.all)
            {
                throw new BadRequestException(BadRequestException.LIST_TYPE_CANNOT_BE_EMPTY_OR_ALL, "KalturaUserAssetsListItem.listType");
            }

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserAssetsListItem.id");
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
        /// <param name="assetId">Asset id to get</param>
        /// <param name="listType">Asset list type to get from</param>
        /// <param name="itemType">item type to get</param>
        /// <remarks>Possible status codes: 
        /// Item was not found in list = 2032</remarks>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.ItemNotFound)]
        static public KalturaUserAssetsListItem Get(string assetId, KalturaUserAssetsListType listType, KalturaUserAssetsListItemType itemType)
        {
            KalturaUserAssetsListItem response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.UsersClient().GetItemFromUsersList(groupId, userId, assetId, listType, itemType);
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
        [Action("getOldStandard")]
        [OldStandardAction("get")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.ItemNotFound)]
        static public KalturaUserAssetsListItem GetOldStandard(KalturaUserAssetsListItem userAssetsListItem)
        {
            KalturaUserAssetsListItem response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserAssetsListItem.id");
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
        /// <param name="assetId">Asset id to delete</param>
        /// <param name="listType">Asset list type to delete from</param>
        /// <remarks>Possible status codes: 
        /// Item was not found in list = 2032</remarks>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.ItemNotFound)]
        static public bool Delete(string assetId, KalturaUserAssetsListType listType)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (listType == KalturaUserAssetsListType.all)
            {
                throw new BadRequestException(BadRequestException.LIST_TYPE_CANNOT_BE_EMPTY_OR_ALL, "listType");
            }

            try
            {
                response = ClientsManager.UsersClient().DeleteItemFromUsersList(groupId, userId, assetId, listType);
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
        [Action("deleteOldStandard")]
        [OldStandardAction("delete")]
        [ApiAuthorize]
        [Obsolete]
        [Throws(eResponseStatus.ItemNotFound)]
        static public bool DeleteOldStandard(KalturaUserAssetsListItem userAssetsListItem)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            if (userAssetsListItem.ListType == KalturaUserAssetsListType.all || userAssetsListItem.Type == KalturaUserAssetsListItemType.all)
            {
                throw new BadRequestException(BadRequestException.LIST_TYPE_CANNOT_BE_EMPTY_OR_ALL, "KalturaUserAssetsListItem.listType");
            }

            if (string.IsNullOrEmpty(userAssetsListItem.Id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaUserAssetsListItem.id");
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