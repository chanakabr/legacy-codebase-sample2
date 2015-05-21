using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.Clients.Exceptions;
using WebAPI.Clients.Utils;
using WebAPI.Filters;
using WebAPI.Models;
using log4net;
using System.Reflection;

namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [Route("search"), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public AssetInfoWrapper GetSearch(string group_id, [FromUri] SearchAssets search_assets)
        {
            return PostSearch(group_id, search_assets);
        }

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
        /// </summary>
        /// <param name="search_assets">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("search"), HttpPost]
        public AssetInfoWrapper PostSearch(string group_id, SearchAssets search_assets)
        {
            AssetInfoWrapper response = null;
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.StatusCode.BadRequest, "group_id must be int");
            }

            try
            {
                response = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, 0,
                search_assets.page_index, search_assets.page_size, search_assets.filter, search_assets.order_by, search_assets.filter_types, search_assets.with);
            }
            catch (ClientException ex)
            {
                // Catalog possible error codes: BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
                if (ex.Code == (int)WebAPI.Models.StatusCode.BadRequest || (ex.Code >= 4002 && ex.Code <= 4005))
                {
                    throw new BadRequestException(ex.Code, ex.Message);
                }

                throw new InternalServerErrorException(ex.Code, ex.Message);
            }

            return response;
        }

        /// <summary>
        /// Cross asset types search optimized for autocomplete search use. Search is within the title only, “starts with”, consider white spaces. Maximum number of returned assets – 10, no paging.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("autocomplete"), HttpPost]
        public SlimAssetInfoWrapper PostAutocomplete(string group_id, Autocomplete request)
        {
            SlimAssetInfoWrapper response = null;
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.StatusCode.BadRequest, "group_id must be int");
            }

            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, string.Empty, string.Empty, 0, request.size, request.query, request.order_by, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                // Catalog possible error codes: BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
                if (ex.Code == (int)WebAPI.Models.StatusCode.BadRequest || (ex.Code >= 4002 && ex.Code <= 4005))
                {
                    throw new BadRequestException(ex.Code, ex.Message);
                }

                throw new InternalServerErrorException(ex.Code, ex.Message);
            }

            return response;
        }

        [Route("autocomplete"), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public SlimAssetInfoWrapper GetAutocomplete(string group_id, [FromUri] Autocomplete request)
        {
            return PostAutocomplete(group_id, request);
        }

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="lang">Language Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("watch_history"), HttpPost]
        public WatchHistoryAssetWrapper PostWatchHistory(string group_id, string user_id, string lang, WatchHistory request)
        {
            WatchHistoryAssetWrapper response = null;
            try
            {
                // parameters validation
                int groupId;
                if (!int.TryParse(group_id, out groupId))
                {
                    throw new BadRequestException((int)WebAPI.Models.StatusCode.BadRequest, "group_id must be an integer");
                }

                // page size - 5 <= size <= 50
                if (request.page_size == null || request.page_size == 0)
                {
                    request.page_size = 25;
                }
                else if (request.page_size > 50)
                {
                    request.page_size = 50;
                }
                else if (request.page_size < 5)
                {
                    throw new ClientException((int)WebAPI.Models.StatusCode.BadRequest, "page_size range can be between 5 and 50");
                }

                // days - default value 7
                if (request.days == 0)
                    request.days = 7;

                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, user_id, lang, request.page_index, request.page_size,
                                                                       request.filter_status, request.days, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                if (ex.Code == (int)WebAPI.Models.StatusCode.BadRequest)
                {
                    throw new BadRequestException(ex.Code, ex.Message);
                }

                throw new InternalServerErrorException(ex.Code, ex.Message);
            }

            return response;
        }

        //[Route("autocomplete"), HttpGet]
        //[ApiExplorerSettings(IgnoreApi = true)]
        //public WatchHistoryAssetWrapper GetWatchHistory(string group_id, [FromUri] Autocomplete request)
        //{
        //    return PostAutocomplete(group_id, request);
        //}
    }
}