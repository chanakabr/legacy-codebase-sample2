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
using WebAPI.Models.Upload;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Bulk Upload service is used to manage bulk actions
    /// </summary>
    [Service("bulkUpload")]
    public class BulkUploadController: IKalturaController
    {
        /// <summary>
        /// Get list of KalturaBulkUpload by filter
        /// </summary>
        /// <param name="filter">Filtering the bulk action request</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaBulkUploadListResponse List(KalturaBulkUploadFilter filter = null)
        {
            KalturaBulkUploadListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // TODO SHIR - List KalturaBulkUploadListResponse
                throw new NotImplementedException();
                //response = ClientsManager.CatalogClient().ListBulkUpload(groupId, filter, pager);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
        
        // TODO SHIR - Get KalturaBulkUpload
        //// Permissions: ?
        //// Get KalturaBulkUpload by Id
        //public static KalturaBulkUpload Get(long ida)
        //{
        //}
    }
}