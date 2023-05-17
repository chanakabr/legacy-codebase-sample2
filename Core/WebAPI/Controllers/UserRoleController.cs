using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Utils;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Service("userRole")]
    public class UserRoleController : IKalturaController
    {
        /// <summary>
        /// Retrieving user roles by identifiers, if filter is empty, returns all partner roles
        /// </summary>
        /// <param name="filter">User roles filter</param>
        /// <remarks></remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaUserRoleListResponse List(KalturaUserRoleFilter filter = null)
        {
            List<KalturaUserRole> list = null;

            int groupId = KS.GetFromRequest().GroupId;
           

            if (filter == null)
            {
                filter = new KalturaUserRoleFilter();
            }
           
            try
            {

                if (filter.CurrentUserRoleIdsContains.HasValue && filter.CurrentUserRoleIdsContains.Value)
                {                    
                    list = ClientsManager.ApiClient().GetUserRoles(groupId, KS.GetFromRequest().UserId);
                }
                else
                {
                    list = ClientsManager.ApiClient().GetRoles(groupId, filter.getIds());
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (list == null)
            {
                throw new InternalServerErrorException();
            }

            if (filter.TypeEqual.HasValue)
            {
                list = list.Where(r => r.Type == filter.TypeEqual.Value).ToList();
            }

            if (filter.ProfileEqual.HasValue)
            {
                list = list.Where(r => r.Profile == filter.ProfileEqual.Value).ToList();
            }

            return new KalturaUserRoleListResponse() { UserRoles = list, TotalCount = list.Count };
        }

        /// <summary>
        /// Retrieving user roles by identifiers, if filter is empty, returns all partner roles
        /// </summary>
        /// <remarks></remarks>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public List<KalturaUserRole> ListOldStandard(KalturaUserRoleFilter filter = null)
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

        /// <summary>
        /// Creates a new role
        /// </summary>
        /// <param name="role">Role to add</param>
        /// <remarks></remarks>        
        [Action("add")]
        [ApiAuthorize]
        static public KalturaUserRole Add(KalturaUserRole role)
        {
            KalturaUserRole response = null;

            UserRoleValidator.Instance.Validate(role);

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().AddRole(groupId, role);
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
        /// Update role
        /// </summary>
        /// <param name="id">Role Id</param>
        /// <param name="role">Role to Update</param>
        /// <remarks></remarks>        
        [Action("update")]       
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PermissionNameNotExists)]
        [Throws(eResponseStatus.RoleDoesNotExists)]
        [Throws(eResponseStatus.RoleReadOnly)]
        static public KalturaUserRole Update(long id, KalturaUserRole role)
        {
            UserRoleValidator.Instance.Validate(role);

            if (id < 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETER, "id");
            }

            KalturaUserRole response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().UpdateRole(groupId, id, role);
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
        /// Delete role
        /// </summary>
        /// <param name="id">Role id to delete</param>
        /// <remarks></remarks>        
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.RoleDoesNotExists)]
        static public bool Delete(long id)
        {
            bool response = false;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().DeleteRole(groupId, id);
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
    }
}
