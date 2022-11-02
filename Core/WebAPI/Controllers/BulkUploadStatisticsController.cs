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
    /// Bulk Upload Statistics monitor bulk actions
    /// </summary>
    [Service("bulkUploadStatistics")]
    public class BulkUploadStatisticsController : IKalturaController
    {
        /// <summary>
        /// Get BulkUploadStatistics count summary by status
        /// </summary>
        /// <param name="bulkObjectTypeEqual">bulkUploadObject for status summary</param>
        /// <param name="createDateGreaterThanOrEqual">date created filter</param>
        /// <returns></returns>
        [Action("get")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        static public KalturaBulkUploadStatistics Get(string bulkObjectTypeEqual, long createDateGreaterThanOrEqual)
        {
            KalturaBulkUploadStatistics response = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                response = ClientsManager.CatalogClient().GetBulkUploadStatusSummary(groupId, bulkObjectTypeEqual, createDateGreaterThanOrEqual);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}