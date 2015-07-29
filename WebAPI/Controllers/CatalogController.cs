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

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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

        /// <summary>
        /// Returns media by media identifiers        
        /// </summary>
        /// <param name="media_ids">Media identifiers separated by ',' </param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size"><![CDATA[Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>     
        [Route("media/{media_ids}"), HttpGet]
        public KalturaAssetInfoWrapper GetMediaByIds(string partner_id, string media_ids, int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
            string language = null, string user_id = null, int household_id = 0)
        {
            KalturaAssetInfoWrapper response = null;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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
    }
}