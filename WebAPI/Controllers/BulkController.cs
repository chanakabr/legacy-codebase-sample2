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
    /// Bulk service is used to manage bulk actions
    /// </summary>
    [RoutePrefix("_service/bulk/action")]
    public class BulkController: ApiController
    {

        /// <summary>
        /// List bulk actions
        /// </summary>
        /// <param name="filter">Filtering the bulk action request</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]        
        public KalturaBulkListResponse List(KalturaBulkFilter filter = null,  KalturaFilterPager pager = null)
        {
            KalturaBulkListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                throw new NotImplementedException();
                //response = ClientsManager.CatalogClient().ListBulkUpload(groupId, filter, pager);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// ServeLog action returns the log file for the bulk action
        /// </summary>
        /// <param name="id">bulk action id</param>
        /// <returns></returns>
        [Route("serveLog"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaBulk ServeLog(long id)
        {
            KalturaBulk response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                //response = ClientsManager.CatalogClient().ServeLogBulkUpload(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}