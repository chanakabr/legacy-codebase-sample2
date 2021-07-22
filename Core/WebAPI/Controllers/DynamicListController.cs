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
    [AddAction(ClientThrows = new [] { eResponseStatus.ExceededMaxCapacity })]
    [UpdateAction(ClientThrows = new [] { eResponseStatus.DynamicListDoesNotExist })]
    [DeleteAction(ClientThrows = new [] { eResponseStatus.DynamicListDoesNotExist })]
    [ListAction(IsFilterOptional = false, IsPagerOptional = true)]
    public class DynamicListController : KalturaCrudController<KalturaDynamicList, KalturaDynamicListListResponse, DynamicList, long, KalturaDynamicListFilter>
    {
        /// <summary>
        /// Add new bulk upload batch job Conversion profile id can be specified in the API.
        /// </summary>
        /// <param name="fileData">fileData</param>
        /// <param name="jobData">jobData</param>
        /// <param name="bulkUploadData">bulkUploadData</param>
        /// <returns></returns>
        [Action("addFromBulkUpload")]
        [ApiAuthorize]
        [Throws(eResponseStatus.FileDoesNotExists)]
        [Throws(eResponseStatus.FileAlreadyExists)]
        [Throws(eResponseStatus.ErrorSavingFile)]
        [Throws(eResponseStatus.FileIdNotInCorrectLength)]
        [Throws(eResponseStatus.InvalidFileType)]
        [Throws(eResponseStatus.EnqueueFailed)]
        [Throws(eResponseStatus.BulkUploadDoesNotExist)]
        [Throws(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk)]
        [Throws(eResponseStatus.FileExceededMaxSize)]
        [Throws(eResponseStatus.FileExtensionNotSupported)]
        [Throws(eResponseStatus.FileMimeDifferentThanExpected)]
        [Throws(eResponseStatus.DynamicListDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]

        public static KalturaBulkUpload AddFromBulkUpload(KalturaOTTFile fileData, KalturaBulkUploadExcelJobData jobData, KalturaBulkUploadDynamicListData bulkUploadData)
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

                if (bulkUploadData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkUploadData");
                }

                jobData.Validate(fileData);
                bulkUploadData.Validate(groupId);

                var dynamicListType = bulkUploadData.GetBulkUploadObjectType();

                bulkUpload =
                    ClientsManager.CatalogClient().AddBulkUpload(groupId, userId, dynamicListType, jobData, bulkUploadData, fileData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return bulkUpload;
        }
    }
}