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
    [RoutePrefix("_service/imageType/action")]
    public class ImageTypeController : ApiController
    {
        /// <summary>
        /// Get the list of image types for the partner
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaImageTypeListResponse List(KalturaImageTypeFilter filter = null)
        {
            KalturaImageTypeListResponse response = null;
            int groupId = KS.GetFromRequest().GroupId;

            if (filter == null)
            {
                filter = new KalturaImageTypeFilter();
            }

            try
            {
                filter.Validate();

                // call client      
                if (!string.IsNullOrEmpty(filter.IdIn))
                {
                    //search using Ids
                    response = ClientsManager.CatalogClient().GetImageTypes(groupId, true, filter.GetIdIn());
                }
                else
                {
                    //search using rationIds
                    response = ClientsManager.CatalogClient().GetImageTypes(groupId, false, filter.GetRatioIdIn());
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new image type
        /// </summary>
        /// <param name="imageType">Image type object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        public KalturaImageType Add(KalturaImageType imageType)
        {
            KalturaImageType response = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().AddImageType(groupId, userId, imageType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update an existing image type
        /// </summary>
        /// <param name="id">Image type ID</param>
        /// <param name="imageType">Image type object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaImageType Update(long id, KalturaImageType imageType)
        {
            KalturaImageType response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                response = ClientsManager.CatalogClient().UpdateImageType(groupId, userId, id, imageType);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing image type
        /// </summary>
        /// <param name="id">Image type ID</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteImageType(groupId, userId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}