using ApiObjects.Response;
using System;
using System.Collections.Generic;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Bulk Upload service is used to manage and monitor bulk actions
    /// </summary>
    [Service("bulkUpload")]
    public class BulkUploadController : IKalturaController
    {
        /// <summary>
        /// Get list of KalturaBulkUpload by filter
        /// </summary>
        /// <param name="filter">Filtering the bulk action request</param>
        /// <param name="pager">Paging the request</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidFileType)]
        public static KalturaBulkUploadListResponse List(KalturaBulkUploadFilter filter, KalturaFilterPager pager = null)
        {
            KalturaBulkUploadListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                filter.Validate();
                DateTime createDate = filter.GetCreateDate();
                
                long? userId = null;
                if (filter.UploadedByUserIdEqualCurrent.HasValue && filter.UploadedByUserIdEqualCurrent.Value)
                {
                    userId = Utils.Utils.GetUserIdFromKs();
                }

                var statuses = Utils.Utils.ParseCommaSeparatedValues<List<KalturaBulkUploadJobStatus>, KalturaBulkUploadJobStatus>(filter.StatusIn, "statusIn");
                response = ClientsManager.CatalogClient().GetBulkUploadList(groupId, filter.BulkObjectTypeEqual, statuses, createDate, userId, filter.OrderBy, pager);
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
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.CatalogClient().GetBulkUpload(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        //TODO bulkUpload/action/summary is used for testing and was not approved yet by SA, once approved we can get it back
        ///// <summary>
        ///// Get BulkUpload count summary by status
        ///// </summary>
        ///// <param name="bulkObjectTypeEqual">bulkUploadObject for status summary</param>
        ///// <param name="createDateGreaterThanOrEqual">date created filter</param>
        ///// <returns></returns>
        //[Action("summary")]
        //// TODO: Arthur only commit for OHS testing so we wont have to update DB otherwise remove before pr review or PROD!!!!!!
        ////[ApiAuthorize]
        //[ValidationException(SchemeValidationType.ACTION_NAME)]
        //[ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        //static public KalturaBulkUploadSummary Summary(string bulkObjectTypeEqual, long createDateGreaterThanOrEqual)
        //{
        //    KalturaBulkUploadSummary response = null;
        //    int groupId = KS.GetFromRequest().GroupId;

        //    try
        //    {
        //        response = ClientsManager.CatalogClient().GetBulkUploadStatusSummary(groupId, bulkObjectTypeEqual, createDateGreaterThanOrEqual);
        //    }
        //    catch (ClientException ex)
        //    {
        //        ErrorUtils.HandleClientException(ex);
        //    }

        //    return response;
        //}
    }
}