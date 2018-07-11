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

                if (filter != null && filter.CurrentUserRoleIdsContains.HasValue && filter.CurrentUserRoleIdsContains.Value)
                {                    
                    list = ClientsManager.ApiClient().GetUserRoles(groupId, KS.GetFromRequest().UserId);
                }
                else
                {
                    // call client
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
        static public KalturaUserRole Update(long id, KalturaUserRole role)
        {
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
