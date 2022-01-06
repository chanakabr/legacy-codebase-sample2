using Phx.Lib.Log;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("permissionItem")]
    public class PermissionItemController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        /// <summary>        
        /// Return a list of permission items with filtering options
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaPermissionItemListResponse List(KalturaPermissionItemFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaPermissionItemListResponse response = null;
            if (filter == null)
            {
                filter = new KalturaPermissionItemFilter();
            }

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            filter.Validate();
               
            try
            {
                response = filter.GetPermissionItems(pager);
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