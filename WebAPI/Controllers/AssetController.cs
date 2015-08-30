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
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/asset/action")]
    public class AssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns media or EPG assets. Filters by media identifiers or by channel identifier or by EPG channel identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="order_by">Ordering the channel</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse List(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, string language = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (filter.ReferenceType)
                {
                    case KalturaCatalogReferenceBy.media:

                        response = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                            0, 1, filter.IDs.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());

                        // if no response - return not found status 
                        if (response == null || response.Objects == null || response.Objects.Count == 0)
                            throw new NotFoundException();

                        break;
                    case KalturaCatalogReferenceBy.channel:

                        int channelID = filter.IDs.First().value;
                        if (channelID == 0)
                            throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "channel_id cannot be 0");

                        response = ClientsManager.CatalogClient().GetChannelMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                            pager.PageIndex, pager.PageSize, channelID, order_by, with.Select(x => x.type).ToList(), 
                            filter.FilterTags.Select(x=> new KeyValue() { m_sKey = x.Key, m_sValue = x.Value.value }).ToList(), filter.cutWith);

                        break;
                    default:
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Not implemented");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns media or EPG asset by media / EPG identifier
        /// </summary>
        /// <param name="id">Asset identifier</param>                
        /// <param name="type">Asset type</param>                
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfo Get(int id, KalturaAssetType type, List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            KalturaAssetInfo response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Illegal asset ID");
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                if (type == KalturaAssetType.media)
                {
                    var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                        0, 1, new int[] { id }.ToList(), with.Select(x => x.type).ToList());

                    // if no response - return not found status 
                    if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                    {
                        throw new NotFoundException();
                    }

                    response = mediaRes.Objects.First();
                }
                else if (type == KalturaAssetType.epg)
                {
                    var epgRes = ClientsManager.CatalogClient().GetEPGByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                      0, 1, new int[] { id }.ToList(), with.Select(x => x.type).ToList());

                    // if no response - return not found status 
                    if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                    {
                        throw new NotFoundException();
                    }

                    response = epgRes.Objects.First();
                }
                else if (type == KalturaAssetType.recording)
                {
                    throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.NotImplemented, "Not implemented");
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Unified search across – VOD: Movies, TV Series/episodes, EPG content.        
        /// </summary>
        /// <param name="filter_types">List of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>
        /// <param name="filter"> <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date.
        /// Comparison operators: for numerical fields =, >, >=, <, <=. For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or.
        /// (maximum length of 1024 characters)]]></param>
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.</param>
        /// <param name="with"> Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language Code</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks>Possible status codes: Bad search request = 4002, Missing index = 4003, SyntaxError = 4004, InvalidSearchField = 4005</remarks>
        [Route("search"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse Search(KalturaOrder? order_by, List<KalturaIntegerValue> filter_types = null, string filter = null,
            List<KalturaCatalogWithHolder> with = null, string language = null, KalturaFilterPager pager = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // parameters validation
            if (!string.IsNullOrEmpty(filter) && filter.Length > 1024)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "too long filter");
            }

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            try
            {
                // call client
                response = ClientsManager.CatalogClient().SearchAssets(groupId, string.Empty, string.Empty, language,
                pager.PageIndex, pager.PageSize, filter, order_by, filter_types.Select(x => x.value).ToList(),
                with.Select(x => x.type).ToList());
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
        /// <param name="query">Search string to look for within the assets’ title only. Search is starts with. White spaces are not ignored</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array</param>
        /// <param name="filter_types">List of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system). 
        /// If omitted – all types should be included. </param>
        /// <param name="order_by"> Required sort option to apply for the identified assets. If omitted – will use newest.</param>
        /// <param name="size"><![CDATA[Maximum number of assets to return.  Possible range 1 ≤ size ≥ 10. If omitted or not in range – default to 5]]></param>
        /// <param name="language">Language Code</param>
        /// <remarks>Possible status codes: Missing index = 4003</remarks>
        [Route("autocomplete"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaSlimAssetInfoWrapper Autocomplete(string query, List<KalturaCatalogWithHolder> with = null, List<KalturaIntegerValue> filter_types = null,
            KalturaOrder? order_by = null, int? size = null, string language = null)
        {
            KalturaSlimAssetInfoWrapper response = null;

            int groupId = KS.GetFromRequest().GroupId;

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (size == null || size > 10 || size < 1)
            {
                size = 5;
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, string.Empty, string.Empty, language, size, query, order_by,
                    filter_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
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
        /// <param name="media_types">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>        
        /// <param name="pager">Paging filter</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("related"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse Related(int media_id, KalturaFilterPager pager = null, List<KalturaIntegerValue> media_types = null,
            List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be 0");
            }

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (media_types == null)
                media_types = new List<KalturaIntegerValue>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty,
                    language, pager.PageIndex, pager.PageSize, media_id, media_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
