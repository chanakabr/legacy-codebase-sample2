using ApiObjects.Response;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.ModelsValidators;
using WebAPI.Utils;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    [Service("mediaFile")]
    public class MediaFileController : IKalturaController
    {
        /// <summary>
        /// Add a new media file
        /// </summary>
        /// <param name="mediaFile">Media file object</param>        
        /// <returns></returns>
        /// <remarks>Possible error codes: InvalidArgument = 50026, ArgumentCannotBeEmpty = 50027, ArgumentMaxLengthCrossed = 500045, ArgumentsDuplicate = 500066, MaxArguments = 500088,
        /// AssetDoesNotExist = 4039, MediaFileTypeDoesNotExist = 4052, MediaFileExternalIdMustBeUnique = 4056, MediaFileAltExternalIdMustBeUnique = 4057,
        /// ExternaldAndAltExternalIdMustBeUnique = 4058, CdnAdapterProfileDoesNotExist = 4062, DefaultCdnAdapterProfileNotConfigurd, MediaFileWithThisTypeAlreadyExistForAsset = 4065,
        /// DynamicDataKeyDoesNotExist = 4120.</remarks>
        [Action("add")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.CdnAdapterProfileDoesNotExist)]
        [Throws(eResponseStatus.DefaultCdnAdapterProfileNotConfigurd)]
        [Throws(eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset)]
        [Throws(eResponseStatus.DynamicDataKeyDoesNotExist)]
        static public KalturaMediaFile Add(KalturaMediaFile mediaFile)
        {
            MediaFileValidator.Instance.ValidateToAdd(mediaFile, nameof(mediaFile));

            KalturaMediaFile response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().AddMediaFile(groupId, mediaFile, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing media file
        /// </summary>
        /// <param name="id">Media file identifier</param>        
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteMediaFile(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// update an existing media file
        /// </summary>
        /// <param name="id">Media file identifier</param>        
        /// <param name="mediaFile">Media file object</param>
        /// <returns></returns>
        /// <remarks>Possible error codes: InvalidArgument = 50026, ArgumentCannotBeEmpty = 50027, ArgumentMaxLengthCrossed = 500045, ArgumentsDuplicate = 500066, MaxArguments = 500088,
        /// MediaFileDoesNotExist = 4053, MediaFileNotBelongToAsset = 4054, MediaFileExternalIdMustBeUnique = 4056, MediaFileAltExternalIdMustBeUnique = 4057,
        /// ExternaldAndAltExternalIdMustBeUnique = 4058, MediaFileWithThisTypeAlreadyExistForAsset = 4065, DynamicDataKeyDoesNotExist = 4120.</remarks>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        [Throws(eResponseStatus.MediaFileNotBelongToAsset)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        [Throws(eResponseStatus.MediaFileWithThisTypeAlreadyExistForAsset)]
        [Throws(eResponseStatus.CdnAdapterProfileDoesNotExist)]
        [Throws(eResponseStatus.DefaultCdnAdapterProfileNotConfigurd)]
        [Throws(eResponseStatus.DynamicDataKeyDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaMediaFile Update(long id, KalturaMediaFile mediaFile)
        {
            MediaFileValidator.Instance.ValidateToUpdate(mediaFile, nameof(mediaFile));

            KalturaMediaFile response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().UpdateMediaFile(groupId, id, mediaFile, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns a list of media-file
        /// </summary>
        /// <param name="filter">Filter</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaMediaFileListResponse List(KalturaMediaFileFilter filter = null)
        {
            KalturaMediaFileListResponse response = null;

            if (filter == null)
            {
                filter = new KalturaMediaFileFilter();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                filter.Validate();

                // call client      
                response = ClientsManager.CatalogClient().GetMediaFiles(groupId, filter.IdEqual, filter.AssetIdEqual);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
    }
}