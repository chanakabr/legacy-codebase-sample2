using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
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

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetCurrentGroupPermissions(groupId);
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
        public static KalturaPermissionListResponse List(KalturaPermissionFilter filter = null)
        {
            KalturaPermissionListResponse response = null;
            if (filter == null)
            {
                filter = new KalturaPermissionFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                if (!filter.CurrentUserPermissionsContains.HasValue || !filter.CurrentUserPermissionsContains.Value)
                {
                    userId = 0;
                }

                response = ClientsManager.ApiClient().GetPermissions(groupId, userId);
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

        ///// <summary>
        ///// Adds new permission
        ///// </summary>
        ///// <param name="permission">Permission to insert</param>
        ///// <remarks></remarks>
        //[Action("add")]
        //[ApiAuthorize]
        //static public KalturaPermission Add(KalturaPermission permission)
        //{
        //    KalturaPermission response = null;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.ApiClient().AddPermission(groupId, permission);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    if (response == null)
        //    {
        //        throw new InternalServerErrorException();
        //    }

        //    return response;
        //}

        ///// <summary>
        ///// Adds permission item to permission
        ///// </summary>
        ///// <param name="permission_id">Permission identifier to add to</param>
        ///// <param name="permission_item_id">Permission item identifier to add</param>
        ///// <remarks></remarks>
        //[Action("addPermissionItem")]
        //[ApiAuthorize]
        //static public bool AddPermissionItem(long permission_id, long permission_item_id)
        //{
        //    bool response = false;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.ApiClient().AddPermissionItemToPermission(groupId, permission_id, permission_item_id);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}
    }
}