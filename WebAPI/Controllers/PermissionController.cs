using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/asset/action")]
    public class PermissionController : ApiController
    {
        /// <summary>
        /// Retrieving permissions by identifiers, if filter is empty, returns all partner permissions
        /// </summary>
        /// <param name="filter">Filter for permissions</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public List<KalturaPermission> List(KalturaPermissionsFilter filter = null)
        {
            List<KalturaPermission> response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
                filter = new KalturaPermissionsFilter();

            try
            {
                // call client
                response = ClientsManager.ApiClient().GetPermissions(groupId, filter.Ids != null ? filter.Ids.Select(id => id.value).ToArray() : null);
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
        /// Adds new Permission
        /// </summary>
        /// <param name="permission">Permission to insert</param>
        /// <remarks></remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaPermission Add(KalturaPermission permission)
        {
            KalturaPermission response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.ApiClient().AddPermission(groupId, permission);
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