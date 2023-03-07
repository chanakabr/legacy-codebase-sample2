using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using ApiObjects.Response;
using WebAPI.Validation;

namespace WebAPI.Controllers
{
    [Service("mediaFileType")]
    public class MediaFileTypeController : IKalturaController
    {
        /// <summary>
        /// Returns a list of media-file types
        /// </summary>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaMediaFileTypeListResponse List()
        {
            KalturaMediaFileTypeListResponse response = new KalturaMediaFileTypeListResponse();

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            try
            {
                response = ClientsManager.CatalogClient().GetMediaFileTypes(groupId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }

            return response;
        }

        /// <summary>
        /// Add new media-file type
        /// </summary>
        /// <param name="mediaFileType">Media-file type</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.MediaFileTypeNameAlreadyInUse)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        public static KalturaMediaFileType Add(KalturaMediaFileType mediaFileType)
        {
            MediaFileTypeValidator.Instance.ValidateToAdd(mediaFileType, nameof(mediaFileType));

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                return ClientsManager.CatalogClient().AddMediaFileType(groupId, mediaFileType, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

        /// <summary>
        /// Update existing media-file type
        /// </summary>
        /// <param name="id">Media-file type identifier</param>
        /// <param name="mediaFileType">Media-file type</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.MediaFileTypeNameAlreadyInUse)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        public static KalturaMediaFileType Update(int id, KalturaMediaFileType mediaFileType)
        {
            MediaFileTypeValidator.Instance.ValidateToUpdate(mediaFileType, nameof(mediaFileType));

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                return ClientsManager.CatalogClient().UpdateMediaFileType(groupId, id, mediaFileType, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

        /// <summary>
        /// Delete media-file type by id
        /// </summary>
        /// <param name="id">Media-file type identifier</param>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        static public bool Delete(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                return ClientsManager.CatalogClient().DeleteMediaFileType(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }
    }
}