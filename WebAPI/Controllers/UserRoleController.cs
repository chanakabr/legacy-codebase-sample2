using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [RoutePrefix("_service/userRole/action")]
    [OldStandard("listOldStandard", "list")]
    public class UserRoleController : ApiController
    {
        /// <summary>
        /// Retrieving user roles by identifiers, if filter is empty, returns all partner roles
        /// </summary>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaUserRoleListResponse List(KalturaUserRoleFilter filter = null)
        {
            List<KalturaUserRole> list = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
                filter = new KalturaUserRoleFilter();

            try
            {
                // call client
                list = ClientsManager.ApiClient().GetRoles(groupId, filter.getIds());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (list == null)
            {
                throw new InternalServerErrorException();
            }

            return new KalturaUserRoleListResponse() { UserRoles = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Retrieving user roles by identifiers, if filter is empty, returns all partner roles
        /// </summary>
        /// <remarks></remarks>
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public List<KalturaUserRole> ListOldStandard(KalturaUserRoleFilter filter = null)
        {
            List<KalturaUserRole> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
                filter = new KalturaUserRoleFilter();

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetRoles(groupId, filter.getIds());
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
        ///// Creates a new role
        ///// </summary>
        ///// <param name="role">Role to add</param>
        ///// <remarks></remarks>        
        //[Route("add"), HttpPost]
        //[ApiAuthorize]
        //public KalturaUserRole Add(KalturaUserRole role)
        //{
        //    KalturaUserRole response = null;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.ApiClient().AddRole(groupId, role);
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
        ///// Adds a new permission to user role
        ///// </summary>
        ///// <param name="role_id">The identifier of the role to add to</param>
        ///// <param name="permission_id">The identifier of the permission to add</param>
        ///// <remarks></remarks>        
        //[Route("addPermission"), HttpPost]
        //[ApiAuthorize]
        //public bool AddPermission(long role_id, long permission_id)
        //{
        //    bool response = false;

        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        // call client
        //        response = ClientsManager.ApiClient().AddPermissionToRole(groupId, role_id, permission_id);
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
    }
}