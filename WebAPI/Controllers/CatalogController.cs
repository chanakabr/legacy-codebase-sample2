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
using WebAPI.Filters;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Controllers;
using System.ComponentModel;
using System.Web.Http.ModelBinding;


namespace WebAPI.Controllers
{
    [RoutePrefix("catalog")]
    public class CatalogController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.        
        /// </summary>
        /// <param name="request">The search asset request parameter</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Bad search request = 4002, Missing index = 4003, SyntaxError = 4004, InvalidSearchField = 4005</remarks>
        [Route("search"), HttpGet]
        public KalturaAssetInfoWrapper Search(string partner_id, [FromUri] KalturaSearchAssetsRequest request, [FromUri] string language = null)
        {
            return _Search(partner_id, request);
        }

        [Route("search"), HttpPost]
        [ApiExplorerSettings(IgnoreApi = true)]
        public KalturaAssetInfoWrapper _Search(string partner_id, KalturaSearchAssetsRequest request, string language = null)
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

        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("autocomplete"), HttpPost]
        public KalturaSlimAssetInfoWrapper _Autocomplete(string partner_id, KalturaAutocompleteRequest request, string language = null)
        {
            return Autocomplete(partner_id, request.query, request.with, request.filter_types, request.order_by, request.size, language);
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
        /// <param name="size">Maximum number of assets to return.  Possible range 1 ≤ size ≥ 10. If omitted or not in range – default to 5</param>
        /// <param name="language">Language Code</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008, Bad search request = 4002, Missing index = 4003</remarks>
        [Route("autocomplete"), HttpGet]
        public KalturaSlimAssetInfoWrapper Autocomplete(string partner_id, string query,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaWith>))] List<KalturaWith> with = null,
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
        /// Returns related media by media identifier<br />        
        /// </summary>        
        /// <param name="media_id">Media identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="media_types">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size">Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("media/{media_id}/related"), HttpGet]
        public KalturaAssetInfoWrapper GetRelatedMedia(string partner_id, int media_id,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<int>))] List<int> media_types = null,
            int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaWith>))] List<KalturaWith> with = null,
            string language = null, string user_id = null, int household_id = 0)
        {
            KalturaAssetInfoWrapper response = null;

            int groupId = int.Parse(partner_id);

            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be 0");
            }

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (page_size == null || page_size > 10 || page_size < 1)
            {
                page_size = 5;
            }

            try
            {
                response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, user_id, household_id, string.Empty, language, page_index, page_size, media_id, media_types, with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns all channel media        
        /// </summary>
        /// <param name="channel_id">Channel identifier</param>
        /// <param name="partner_id">Partner identifier</param>
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use channel default ordering.</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size">Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("channels/{channel_id}/media"), HttpGet]
        public KalturaAssetInfoWrapper GetChannelMedia(string partner_id, int channel_id, KalturaOrder? order_by = null, 
            int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaWith>))] List<KalturaWith> with = null,
            string language = null, string user_id = null, int household_id = 0)
        {
            KalturaAssetInfoWrapper response = null;

            int groupId = int.Parse(partner_id);

            if (channel_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "channel_id cannot be 0");
            }

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (page_size == null || page_size > 10 || page_size < 1)
            {
                page_size = 5;
            }

            try
            {
                response = ClientsManager.CatalogClient().GetChannelMedia(groupId, user_id, household_id, string.Empty, language, page_index, page_size, channel_id, order_by.Value, with);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns media by media identifiers        
        /// </summary>
        /// <param name="media_ids">Media identifiers separated by ',' </param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size">Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>     
        [Route("media/{media_ids}"), HttpGet]
        public KalturaAssetInfoWrapper GetMediaByIds(string partner_id, string media_ids, int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaWith>))] List<KalturaWith> with = null,
            string language = null, string user_id = null, int household_id = 0)
        {
            KalturaAssetInfoWrapper response = null;

            int groupId = int.Parse(partner_id);

            if (string.IsNullOrEmpty(media_ids))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_ids cannot be empty");
            }

            List<int> mediaIds;
            try
            {
                mediaIds = media_ids.Split(',').Select(m => int.Parse(m)).ToList();
            }
            catch
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "each media id must be int");
            }

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (page_size == null || page_size > 10 || page_size < 1)
            {
                page_size = 5;
            }

            try
            {
                response = ClientsManager.CatalogClient().GetMediaByIds(groupId, user_id, household_id, string.Empty, language, page_index, page_size, mediaIds, with);

                // if no response - return not found status 
                if (response == null || response.Assets == null || response.Assets.Count == 0)
                {
                    throw new NotFoundException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns channel info        
        /// </summary>
        /// <param name="channel_id">Channel Identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("channels/{channel_id}"), HttpGet]
        public KalturaChannel GetChannel(string partner_id, int channel_id, string language = null, string user_id = null, int household_id = 0)
        {
            KalturaChannel response = null;

            int groupId = int.Parse(partner_id);

            if (channel_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "channel_id cannot be 0");
            }

            try
            {
                response = ClientsManager.CatalogClient().GetChannelInfo(groupId, user_id, household_id, language, channel_id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns category by category identifier        
        /// </summary>
        /// <param name="category_id">Category Identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="language">Language Code</param>
        /// <param name="user_id">User Identifier</param>
        /// <param name="household_id">Household Identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("categories/{category_id}"), HttpGet]
        public KalturaCategory GetCategory(string partner_id, int category_id, string language = null, string user_id = null, int household_id = 0)
        {
            KalturaCategory response = null;

            int groupId = int.Parse(partner_id);

            if (category_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "category_id cannot be 0");
            }

            try
            {
                response = ClientsManager.CatalogClient().GetCategory(groupId, user_id, household_id, language, category_id);

                // if no response - return not found status 
                if (response == null || response.Id == 0)
                {
                    throw new NotFoundException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}