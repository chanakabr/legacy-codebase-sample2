using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using TVinciShared;
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
    /// Bulk Upload service is used to manage and monitor bulk actions
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
        public static KalturaBulkUploadListResponse List(KalturaBulkUploadFilter filter, KalturaFilterPager pager = null)
        {
            KalturaBulkUploadListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;
            
            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            try
            {
                filter.Validate();

                long? userId = null;
                if (filter.UserIdEqualCurrent.HasValue && filter.UserIdEqualCurrent.Value)
                {
                    userId = Utils.Utils.GetUserIdFromKs();
                }

                var uploadDate = DateUtils.UtcUnixTimestampSecondsToDateTime(filter.CreateDateGreaterThanOrEqual);
                response = ClientsManager.CatalogClient().GetBulkUploadList(groupId, filter.FileObjectNameEqual, userId, uploadDate, filter.OrderBy, pager.getPageSize(), pager.getPageIndex());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get BulkUpload by ID
        /// </summary>
        /// <param name="id">ID to get</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [Throws(eResponseStatus.BulkUploadDoesNotExist)]
        static public KalturaBulkUpload Get(long id)
        {
            KalturaBulkUpload response = null;
            
            try
            {
                response = ClientsManager.CatalogClient().GetBulkUpload(id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}