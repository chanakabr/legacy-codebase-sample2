using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/mediaFile/action")]
    public class MediaFileController : ApiController
    {
        /// <summary>
        /// Add a new media file
        /// </summary>
        /// <param name="mediaFile">Media file object</param>        
        /// <returns></returns>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [ValidationException(SchemeValidationType.ACTION_RETURN_TYPE)]
        [Throws(eResponseStatus.MediaFileTypeDoesNotExist)]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.MediaFileExternalIdMustBeUnique)]
        [Throws(eResponseStatus.MediaFileAltExternalIdMustBeUnique)]
        [Throws(eResponseStatus.ExternaldAndAltExternalIdMustBeUnique)]
        [Throws(eResponseStatus.StreamingSupplierDoesNotExist)]
        [Throws(eResponseStatus.DefaultStreamingSupplierNotConfigurd)]
        public KalturaMediaFile Add(KalturaMediaFile mediaFile)
        {
            KalturaMediaFile response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (mediaFile.AssetId <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "assetId");
            }

            if (!mediaFile.TypeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "type");
            }

            if (string.IsNullOrEmpty(mediaFile.ExternalId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalId");
            }

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
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        public bool Delete(long id)
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
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.MediaFileDoesNotExist)]
        [Throws(eResponseStatus.MediaFileNotBelongToAsset)]
        [Throws(eResponseStatus.MediaFileExternalIdMustBeUnique)]
        [Throws(eResponseStatus.MediaFileAltExternalIdMustBeUnique)]
        [Throws(eResponseStatus.ExternaldAndAltExternalIdMustBeUnique)]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaMediaFile Update(long id, KalturaMediaFile mediaFile)
        {
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
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaMediaFileListResponse List(KalturaMediaFileFilter filter = null)
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