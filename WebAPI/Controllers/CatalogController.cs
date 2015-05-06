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

namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        [Route("search"), HttpGet]
        //[ApiExplorerSettings(IgnoreApi = true)]
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
    }
}