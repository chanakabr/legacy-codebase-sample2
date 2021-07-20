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
    [Service("permission")]
    public class PermissionController : IKalturaController
    {

        /// <summary>
        /// Returns permission names as comma separated string
        /// </summary>
        /// <remarks></remarks>
        [Action("getCurrentPermissions")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public static string GetCurrentPermissions()
        {
            string response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetCurrentUserPermissions(groupId, userId.ToString());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Retrieving permissions by identifiers, if filter is empty, returns all partner permissions
        /// </summary>
        /// <param name="filter">Filter for permissions</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaPermissionListResponse List(KalturaBasePermissionFilter filter = null)
        {
            KalturaPermissionListResponse response = null;
            var contextData = KS.GetContextData();

            if (filter == null)
            {
                filter = new KalturaPermissionFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                filter.Validate(contextData);

                response = filter.GetPermissions(contextData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == null)
            {
                throw new InternalServerErrorException();
            }

            return response;
        }

        /// <summary>
        /// Adds new permission
        /// </summary>
        /// <param name="permission">Permission to insert</param>
        /// <remarks></remarks>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PermissionNameAlreadyInUse)]
        [Throws(eResponseStatus.CanModifyOnlyNormalPermission)]
        [Throws(eResponseStatus.CannotAddPermissionTypeGroup)]
        [Throws(eResponseStatus.PermissionItemNotFound)]
        static public KalturaPermission Add(KalturaPermission permission)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();


            try
            {
                if (string.IsNullOrEmpty(permission.Name))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPermission.name");
                }

                if (permission.Type == KalturaPermissionType.SPECIAL_FEATURE && !string.IsNullOrEmpty(permission.DependsOnPermissionNames))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaPermission.type, KalturaPermission.dependsOnPermissionNames");
                }

                permission.ValidateForInsert();

                // call client
                return ClientsManager.ApiClient().AddPermission(groupId, permission, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return null;
        }

        /// <summary>
        /// Deletes an existing permission
        /// </summary>
        /// <param name="id">Permission ID to delete</param>
        /// <remarks></remarks>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PermissionNotFound)]
        static public void Delete(long id)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                ClientsManager.ApiClient().DeletePermission(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Adds permission item to permission
        /// </summary>
        /// <param name="permissionId">Permission ID to add to</param>
        /// <param name="permissionItemId">Permission item ID to add</param>
        /// <remarks></remarks>
        [Action("addPermissionItem")]
        [Throws(eResponseStatus.PermissionNotFound)]
        [Throws(eResponseStatus.PermissionItemNotFound)]
        [Throws(eResponseStatus.PermissionPermissionItemAlreadyExists)]
        [Throws(eResponseStatus.PermissionReadOnly)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public void AddPermissionItem(long permissionId, long permissionItemId)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                ClientsManager.ApiClient().AddPermissionItemToPermission(groupId, permissionId, permissionItemId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Removes permission item from permission
        /// </summary>
        /// <param name="permissionId">Permission ID to remove from</param>
        /// <param name="permissionItemId">Permission item ID to remove</param>
        /// <remarks></remarks>
        [Action("removePermissionItem")]
        [Throws(eResponseStatus.PermissionNotFound)]
        [Throws(eResponseStatus.PermissionItemNotFound)]
        [Throws(eResponseStatus.PermissionPermissionItemNotFound)]
        [Throws(eResponseStatus.PermissionReadOnly)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public void RemovePermissionItem(long permissionId, long permissionItemId)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                ClientsManager.ApiClient().RemovePermissionItemFromPermission(groupId, permissionId, permissionItemId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Update an existing permission.
        /// </summary>
        /// <param name="id">Permission  Identifier</param>
        /// <param name="permission">Permission object</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PermissionNotFound)]
        [Throws(eResponseStatus.CanModifyOnlyNormalPermission)]
        [Throws(eResponseStatus.PermissionItemNotFound)]
        static public KalturaPermission Update(long id, KalturaPermission permission)
        {
            KalturaPermission response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                permission.ValidateForUpdate();
                response = ClientsManager.ApiClient().UpdatePermission(groupId, id, permission, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}