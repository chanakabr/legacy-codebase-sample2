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
    /// Bulk upload service is used to manage bulk uploads
    /// </summary>
    [RoutePrefix("_service/bulkUpload/action")]
    public class BulkUploadController: ApiController
    {

        /// <summary>
        /// Aborts the bulk upload
        /// </summary>
        /// <param name="id">bulk upload id</param>
        /// <returns></returns>
        [Route("abort"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaBulkUpload Abort(long id)
        {
            KalturaBulkUpload response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                throw new NotImplementedException();
                //response = ClientsManager.CatalogClient().AbortBulkUpload(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get bulk upload by id
        /// </summary>
        /// <param name="id">bulk upload id</param>
        /// <returns></returns>
        [Route("get"), HttpPost]
        [ApiAuthorize]        
        public KalturaBulkUpload Get(long id)
        {
            KalturaBulkUpload response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                throw new NotImplementedException();
                //response = ClientsManager.CatalogClient().GetBulkUpload(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// List bulk uploads
        /// </summary>
        /// <param name="filter">Filtering the bulk upload request</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]        
        public KalturaBulkUploadListResponse List(KalturaBulkUploadFilter filter = null,  KalturaFilterPager pager = null)
        {
            KalturaBulkUploadListResponse response = null;
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
        /// Serve action returns the original file
        /// </summary>
        /// <param name="id">bulk upload id</param>
        /// <returns></returns>
        [Route("serve"), HttpPost]
        [ApiAuthorize]        
        public KalturaBulkUpload Serve(long id)
        {
            KalturaBulkUpload response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                //response = ClientsManager.CatalogClient().ServeBulkUpload(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// ServeLog action returns the log file for the bulk upload
        /// </summary>
        /// <param name="id">bulk upload id</param>
        /// <returns></returns>
        [Route("serveLog "), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        public KalturaBulkUpload ServeLog(long id)
        {
            KalturaBulkUpload response = null;
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