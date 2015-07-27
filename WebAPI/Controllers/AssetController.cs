using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("asset")]
    public class AssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.        
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Bad search request = 4002, Missing index = 4003, SyntaxError = 4004, InvalidSearchField = 4005</remarks>
        [Route("search"), HttpPost]
        public KalturaAssetInfoWrapper Search(string partner_id, KalturaSearchAssetsRequest request, string language = null)
        {
            KalturaAssetInfoWrapper response = null;

            int groupId = int.Parse(partner_id);

            // parameters validation
            if (!string.IsNullOrEmpty(request.filter) && request.filter.Length > 1024)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "too long filter");
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
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, language,
                request.page_index, request.page_size, request.filter, request.order_by, request.filter_types, request.with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Cross asset types search optimized for autocomplete search use. Search is within the title only, “starts with”, consider white spaces. Maximum number of returned assets – 10, no paging.
        /// </summary>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="query">Search string to look for within the assets’ title only. Search is starts with. White spaces are not ignored</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array</param>
        /// <param name="filter_types">List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system). 
        /// If omitted – all types should be included. </param>
        /// <param name="order_by"> Required sort option to apply for the identified assets. If omitted – will use newest.</param>
        /// <param name="size"><![CDATA[Maximum number of assets to return.  Possible range 1 ≤ size ≥ 10. If omitted or not in range – default to 5]]></param>
        /// <param name="language">Language Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Bad search request = 4002, Missing index = 4003</remarks>
        [Route("autocomplete"), HttpPost]
        public KalturaSlimAssetInfoWrapper Autocomplete(string partner_id, string query,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<int>))] List<int> filter_types = null,
            KalturaOrder? order_by = null, int? size = null, string language = null)
        {
            KalturaSlimAssetInfoWrapper response = null;

            int groupId = int.Parse(partner_id);

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (size == null || size > 10 || size < 1)
            {
                size = 5;
            }

            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, string.Empty, string.Empty, language, size, query, order_by, filter_types, with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Get recently watched media for user, ordered by recently watched first.    
        /// </summary>
        /// <param name="partner_id" >Partner identifier</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="filter_types">List of asset types to search within. The list is a string separated be comma.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="filter_status">Which type of recently watched media to include in the result – those that finished watching, those that are in progress or both.
        /// If omitted or specified filter = all – return all types.
        /// Allowed values: progress – return medias that are in-progress, done – return medias that finished watching.</param>
        /// <param name="days">How many days back to return the watched media. If omitted, default to 7 days</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size"><![CDATA[Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008
        /// </remarks>
        [Route("{user_id}/views"), HttpGet]
        public KalturaWatchHistoryAssetWrapper WatchHistory(string partner_id, string user_id, string filter_types = null, KalturaWatchStatus? filter_status = null,
            int days = 0, int page_index = 0, int? page_size = null, [FromUri] List<KalturaCatalogWith> with = null, string language = null)
        {
            KalturaWatchHistoryAssetWrapper response = null;

            int groupId = int.Parse(partner_id);

            // page size - 5 <= size <= 50
            if (page_size == null || page_size == 0)
            {
                page_size = 25;
            }
            else if (page_size > 50)
            {
                page_size = 50;
            }
            else if (page_size < 5)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "page_size range can be between 5 and 50");
            }

            List<int> filterTypes = null;
            if (!string.IsNullOrEmpty(filter_types))
            {
                try
                {
                    filterTypes = filter_types.Split(',').Select(x => int.Parse(x)).ToList();
                }
                catch
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "invalid filter types");
                }
            }

            // days - default value 7
            if (days == 0)
                days = 7;
            try
            {
                // call client
                response = ClientsManager.CatalogClient().WatchHistory(groupId, user_id, language, page_index, page_size, filter_status, days, filterTypes, with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }        
    }
}
