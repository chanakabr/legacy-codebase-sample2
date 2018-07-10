using ApiObjects.Response;
using System;
using System.Linq;
using System.Web.Http;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Users;
using WebAPI.Utils;
using WebAPI.ClientManagers.Client;
using WebAPI.Models.Upload;
using System.Net.Http;
using WebAPI.Models.General;

namespace WebAPI.Controllers
{
    [Service("uploadToken")]
    public class UploadTokenController : ApiController
    {
        /// <summary>
        /// Adds new upload token to upload a file
        /// </summary>
        /// <param name="uploadToken">Upload token details</param>
        [Action("add")]
        [ApiAuthorize]
        public KalturaUploadToken Add(KalturaUploadToken uploadToken = null)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                return UploadManager.AddUploadToken(uploadToken, groupId);
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
        public KalturaUploadToken Upload(string uploadTokenId, KalturaOTTFile fileData)
        {
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                return UploadManager.UploadUploadToken(uploadTokenId, fileData.path, groupId);
            }
            catch (ClientException ex)
            {
                throw new ApiException(ex);
            }
        }

    }
}