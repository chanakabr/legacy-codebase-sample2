using ApiObjects;
using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("dynamicList")]
    [AddAction]
    [UpdateAction]
    [DeleteAction]
    [ListAction(IsFilterOptional = false, IsPagerOptional = true)]
    public class DynamicListController : KalturaCrudController<KalturaDynamicList, KalturaDynamicListListResponse, DynamicList, long, KalturaDynamicListFilter>
    {
        /// <summary>
        /// Add new bulk upload batch job Conversion profile id can be specified in the API.
        /// </summary>
        /// <param name="fileData">fileData</param>
        /// <param name="jobData">jobData</param>
        /// <param name="bulkUploadAssetData">bulkUploadAssetData</param>
        /// <returns></returns>
        [Action("addFromBulkUpload")]
        [ApiAuthorize]
        [Throws(StatusCode.ArgumentCannotBeEmpty)]
        [Throws(eResponseStatus.FileDoesNotExists)]
        [Throws(eResponseStatus.FileAlreadyExists)]
        [Throws(eResponseStatus.ErrorSavingFile)]
        [Throws(eResponseStatus.FileIdNotInCorrectLength)]
        [Throws(eResponseStatus.InvalidFileType)]
        [Throws(eResponseStatus.IllegalExcelFile)]
        [Throws(eResponseStatus.EnqueueFailed)]
        [Throws(eResponseStatus.InvalidBulkUploadStructure)]
        [Throws(eResponseStatus.ExcelMandatoryValueIsMissing)]
        [Throws(eResponseStatus.InvalidArgumentValue)]
        [Throws(eResponseStatus.BulkUploadDoesNotExist)]
        [Throws(eResponseStatus.BulkUploadResultIsMissing)]
        [Throws(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk)]
        [Throws(eResponseStatus.FileExceededMaxSize)]
        [Throws(eResponseStatus.FileExtensionNotSupported)]
        [Throws(eResponseStatus.FileMimeDifferentThanExpected)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]

        public static KalturaBulkUpload AddFromBulkUpload(KalturaOTTFile fileData, KalturaBulkUploadExcelJobData jobData, KalturaBulkUploadAssetData bulkUploadAssetData)
        {
            KalturaBulkUpload bulkUpload = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                if (fileData == null || (fileData.File == null && string.IsNullOrEmpty(fileData.path)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fileData");
                }

                if (jobData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "jobData");
                }

                if (bulkUploadAssetData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkUploadAssetData");
                }

                jobData.Validate(fileData);
                bulkUploadAssetData.Validate(groupId);

                var assetType = bulkUploadAssetData.GetBulkUploadObjectType();

                bulkUpload =
                    ClientsManager.CatalogClient().AddBulkUpload(groupId, userId, assetType, jobData, bulkUploadAssetData, fileData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return bulkUpload;
        }
    }
}