using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/image/action")]
    public class ImageController : ApiController
    {
        /// <summary>
        /// Get the list of images by different filtering 
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaImageListResponse List(KalturaImageFilter filter = null)
        {
            KalturaImageListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaImageFilter();
            }

            try
            {
                filter.Validate();

                // call client      
                if (!string.IsNullOrEmpty(filter.IdIn))
                {
                    //search using IDs
                    response = ClientsManager.CatalogClient().GetImagesByIds(groupId, filter.GetIdIn(), filter.IsDefaultEqual);
                }
                else
                {
                    //search using object ID and type
                    response = ClientsManager.CatalogClient().GetImagesByObject(groupId, filter.ImageObjectIdEqual.Value, filter.ImageObjectTypeEqual.Value, filter.IsDefaultEqual);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new image 
        /// </summary>
        /// <param name="image">Image</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.AssetDoesNotExist)]
        [Throws(eResponseStatus.ImageTypeDoesNotExist)]
        [Throws(eResponseStatus.ImageTypeAlreadyInUse)]
        public KalturaImage Add(KalturaImage image)
        {
            KalturaImage response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (!image.ImageObjectType.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "image.imageObjectType");
            }

            if (image.ImageTypeId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "image.imageTypeId");
            }

            if (image.ImageObjectId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "image.imageObjectId");
            }

            try
            {
                response = ClientsManager.CatalogClient().AddImage(groupId, userId, image);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }
        
        /// <summary>
        /// Delete an existing image 
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.ImageDoesNotExist)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteImage(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }     

        /// <summary>
        /// Sets the content of an existing image 
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <param name="content">Content of the image to set</param>
        /// <returns></returns>
        [Route("setContent"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.ImageDoesNotExist)]
        [Throws(eResponseStatus.InvalidRatioForImage)]
        public bool SetContent(long id, KalturaContentResource content)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                if (content is KalturaUrlResource)
                {
                    KalturaUrlResource urlContent = (KalturaUrlResource)content;
                    result = ClientsManager.CatalogClient().SetContent(groupId, userId, id, urlContent.Url);
                }
                else
                {
                    throw new BadRequestException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}