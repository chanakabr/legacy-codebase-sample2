using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("related_media")]
    public class RelatedMediaController : ApiController
    {
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
        public KalturaAssetInfoWrapper Get(string partner_id, int media_id,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<int>))] List<int> media_types = null,
            int page_index = 0, int? page_size = null,
            [ModelBinder(typeof(WebAPI.Utils.SerializationUtils.ConvertCommaDelimitedList<KalturaCatalogWith>))] List<KalturaCatalogWith> with = null,
            string language = null, string user_id = null, int household_id = 0)
        {
            KalturaAssetInfoWrapper response = null;

            // validate group ID
            int groupId = 0;
            if (!int.TryParse(partner_id, out groupId) || groupId < 1)
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal partner ID");

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
    }
}