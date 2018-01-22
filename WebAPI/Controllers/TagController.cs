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
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/tag/action")]
    public class TagController : ApiController
    {
        /// <summary>
        /// Get the list of tags for the partner
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Page size and index</param>
        /// <returns></returns>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaTagListResponse List(KalturaTagFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaTagListResponse response = null;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaTagFilter();
            }

            if (string.IsNullOrEmpty(filter.LanguageEqual))
            {
                filter.LanguageEqual = Utils.Utils.GetDefaultLanguage();
            }

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                filter.Validate();

                // call client      
                if (!string.IsNullOrEmpty(filter.TagEqual))
                {
                    //search using tag
                    response = ClientsManager.CatalogClient().SearchTags(groupId, true, filter.TagEqual, filter.TypeEqual, filter.LanguageEqual, pager.getPageIndex(), pager.getPageSize());
                }
                else
                {
                    //search using TagStartsWith
                    response = ClientsManager.CatalogClient().SearchTags(groupId, false, filter.TagStartsWith, filter.TypeEqual, filter.LanguageEqual, pager.getPageIndex(), pager.getPageSize());
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Add a new tag
        /// </summary>
        /// <param name="tag">Tag Object</param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Route("add"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotFound)]        
        public KalturaTag Add(KalturaTag tag)
        {
            KalturaTag response = null;

            try
            {
                int groupId = KS.GetFromRequest().GroupId;
                long userId = Utils.Utils.GetUserIdFromKs();

                if (tag.Tag != null)
                {
                    if ((tag.Tag.Values == null || tag.Tag.Values.Count == 0))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "tag");
                    }
                    else
                    {
                        tag.Tag.Validate("multilingualTag");
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "multilingualTag");
                }

                // call client
                response = ClientsManager.CatalogClient().AddTag(groupId, tag, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return response;
        }

        /// <summary>
        /// Update an existing tag
        /// </summary>
        /// <param name="id">Tag Identifier</param>
        /// <param name="tag">Tag Object</param>
        /// <returns></returns>
        [Route("update"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.TopicNotFound)]
        [Throws(eResponseStatus.TagDoesNotExist)]
        public KalturaTag Update(long id, KalturaTag tag)
        {
            KalturaTag response = null;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            if (tag.Tag != null)
            {
                if ((tag.Tag.Values == null || tag.Tag.Values.Count == 0))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "tag");
                }
                else
                {
                    tag.Tag.Validate("multilingualTag");
                }
            }
            else
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "multilingualTag");
            }

            try
            {
                response = ClientsManager.CatalogClient().UpdateTag(groupId, id, tag, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Delete an existing tag
        /// </summary>
        /// <param name="id">Tag Identifier</param>
        /// <returns></returns>
        [Route("delete"), HttpPost]
        [ApiAuthorize]
        [Throws(eResponseStatus.TagDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        public bool Delete(long id)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                result = ClientsManager.CatalogClient().DeleteTag(groupId, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

    }
}