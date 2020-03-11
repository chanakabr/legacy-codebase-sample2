using System;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Utils;
using WebAPI.Models.Upload;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [Service("uploadToken")]
    public class UploadTokenController : IKalturaController
    {
        /// <summary>
        /// Adds new upload token to upload a file
        /// </summary>
        /// <param name="uploadToken">Upload token details</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaUploadToken Add(KalturaUploadToken uploadToken = null)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                return UploadTokenManager.AddUploadToken(groupId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

        /// <summary>
        /// Upload a file using the upload token id
        /// </summary>
        /// <param name="uploadTokenId">Identifier of existing upload-token</param>
        /// <param name="fileData">File to upload</param>
        [Action("upload")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        static public KalturaUploadToken Upload(string uploadTokenId, KalturaOTTFile fileData)
        {
            KalturaUploadToken uploadToken = null;
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                uploadToken = UploadTokenManager.UploadUploadToken(uploadTokenId, fileData.path, groupId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return uploadToken;
        }
    }
}