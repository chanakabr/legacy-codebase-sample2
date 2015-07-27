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
        /// Returns related media by media identifier<br />        
        /// </summary>        
        /// <param name="media_id">Media identifier</param>
        /// <param name="partner_id">Partner Identifier</param>
        /// <param name="media_types">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="page_index">Page number to return. If omitted will return first page.</param>
        /// <param name="page_size"><![CDATA[Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
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
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
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
        /// <param name="page_size"><![CDATA[Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted - will be set to 25. If a value > 50 provided – will set to 50]]></param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>
        /// <param name="user_id">User identifier</param>
        /// <param name="household_id">Household identifier</param>
        /// <remarks>Possible status codes: Bad credentials = 500000, Internal connection = 500001, Timeout = 500002, Bad request = 500003, Forbidden = 500004, Unauthorized = 500005, Configuration error = 500006, Not found = 500007, Partner is invalid = 500008</remarks>
        [Route("channels/{channel_id}/media"), HttpGet]
        public KalturaAssetInfoWrapper GetChannelMedia(string partner_id, int channel_id, KalturaOrder? order_by = null, 
            int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
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

        
    }
}