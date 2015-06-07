using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using System.Web.Http.Description;
using WebAPI.Exceptions;
using WebAPI.Models;
using System.Reflection;
using WebAPI.Models.Catalog;
using WebAPI.Utils;
using WebAPI.ClientManagers.Client;
using System.Net.Http;
using KLogMonitor;


namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [Route("search"), HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public AssetInfoWrapper GetSearch(string group_id, [FromUri] SearchAssets search_assets, string language = null)
        {
            return PostSearch(group_id, search_assets);
        }

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("search"), HttpPost]
        public AssetInfoWrapper PostSearch(string group_id, SearchAssets request, string language = null)
        {
            AssetInfoWrapper response = null;

            // parameters validation
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be an integer");
            }

            if (!string.IsNullOrEmpty(request.filter) && request.filter.Length > 1024)
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "too long filter");
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
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, language,
                request.page_index, request.page_size, request.filter, request.order_by, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                // Catalog possible error codes: BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005
                ErrorUtils.HandleClientException(ex, new List<int>() { 4002, 4003, 4004, 4005 });
            }

            return response;
        }

        [ApiExplorerSettings(IgnoreApi = true)]       
        [Route("autocomplete"), HttpPost]
        public SlimAssetInfoWrapper PostAutocomplete(string group_id, Autocomplete request, string language = null)
        {
            SlimAssetInfoWrapper response = null;
            int groupId;
            if (!int.TryParse(group_id, out groupId))
            {
                throw new BadRequestException((int)WebAPI.Models.General.StatusCode.BadRequest, "group_id must be int");
            }

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (request.size == null || request.size > 10 || request.size < 1)
            {
                request.size = 5;
            }

            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, string.Empty, string.Empty, language, request.size, request.query, request.order_by, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                // Catalog possible error codes: BadSearchRequest = 4002, IndexMissing = 4003
                ErrorUtils.HandleClientException(ex, new List<int>() { 4002, 4003 });
            }

            return response;
        }

        /// <summary>
        /// Cross asset types search optimized for autocomplete search use. Search is within the title only, “starts with”, consider white spaces. Maximum number of returned assets – 10, no paging.<br />
        /// Possible status codes: BadCredentials = 500000, InternalConnectionIssue = 500001, Timeout = 500002, BadRequest = 500003, BadSearchRequest = 4002, IndexMissing = 4003
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="group_id">Group Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks></remarks>
        /// <response code="200">OK</response>
        /// <response code="400">Bad request</response>
        /// <response code="500">Internal Server Error</response>
        [Route("autocomplete"), HttpGet]
        public SlimAssetInfoWrapper GetAutocomplete(string group_id, [FromUri] Autocomplete request, string language = null)
        {
            return PostAutocomplete(group_id, request);
        }

        
    }
}