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
        /// Returns media or EPG assets. Filters by media identifiers or by channel identifier or by EPG internal or external identifier or external channel identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request. Possible additional object types: KalturaExternalChannelFilter</param>
        /// <param name="order_by">Ordering the channel</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="udid">Unique device identifier</param>
        /// <param name="language">Language code</param>
        /// <remarks>Possible status codes: 
        /// External Channel reference type: ExternalChannelHasNoRecommendationEngine = 4014, AdapterAppFailure = 6012, AdapterUrlRequired = 5013,
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, 
        /// RecommendationEngineNotExist = 4007, ExternalChannelNotExist = 4011</remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse List(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, string language = null, string udid = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager  = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter == null)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter cannot be null");
            }

            if (filter.IDs == null || filter.IDs.Count == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "filter ids cannot be empty");
            }
            
            try
            {
                string userID = KS.GetFromRequest().UserId;
                List<int> ids = null;

                switch (filter.ReferenceType)
                {
                    case KalturaCatalogReferenceBy.media:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is media");
                            }

                            response = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                                0, 0, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                                throw new NotFoundException();
                        }
                        break;
                    case KalturaCatalogReferenceBy.channel:
                        {
                            int channelID;
                            if (!int.TryParse(filter.IDs.First().value, out channelID))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is channel");
                            }

                            response = ClientsManager.CatalogClient().GetChannelMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                                pager.PageIndex, pager.PageSize, channelID, order_by, with.Select(x => x.type).ToList(),
                                filter.FilterTags == null ? null : filter.FilterTags.Select(x => new KeyValue() { m_sKey = x.Key, m_sValue = x.Value.value }).ToList(),
                                filter.cutWith);
                        }
                        break;
                    case KalturaCatalogReferenceBy.epg_internal:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is epg_internal");
                            }

                            response = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                               0, 0, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                        }
                        break;
                    case KalturaCatalogReferenceBy.epg_external:
                        {
                            response = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                                  0, 1, filter.IDs.Select(id => id.value).ToList(), with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }
                        }
                        break;
                    case KalturaCatalogReferenceBy.external_channel:
                        {
                            if (filter.IDs.Count != 1)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "Must have only 1 ID when type is external channel");
                            }

                            string externalChannelId = filter.IDs.First().value;

                            var convertedWith = with.Select(x => x.type).ToList();

                            KalturaExternalChannelFilter convertedFilter = filter as KalturaExternalChannelFilter;

                            string utcOffset = convertedFilter.UtcOffset;

                            double utcOffsetDouble;

                            if (!double.TryParse(utcOffset, out utcOffsetDouble))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                            }
                            else if (utcOffsetDouble > 12 || utcOffsetDouble < -12)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                            }

                            string deviceType = System.Web.HttpContext.Current.Request.UserAgent;

                            response = ClientsManager.CatalogClient().GetExternalChannelAssets(groupId, externalChannelId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                                language, pager.PageIndex, pager.PageSize, order_by, convertedWith, deviceType, convertedFilter.UtcOffset);

                            break;
                        }
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
        /// Returns media or EPG asset by media / EPG internal or external identifier
        /// </summary>
        /// <param name="id">Asset identifier</param>                
        /// <param name="type">Asset type</param>                
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfo Get(string id, KalturaAssetReferenceType type, List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            KalturaAssetInfo response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                switch (type)
                {
                    case KalturaAssetReferenceType.media:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is media");
                            }
                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                                0, 1, new List<int>() { mediaId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_internal:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is epg_internal");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                               0, 1, new List<int> { epgId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_external:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), string.Empty, language,
                              0, 1, new List<string> { id }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    default:
                        break;
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
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 1024 characters)]]></param>
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
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);

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
                response = ClientsManager.CatalogClient().SearchAssets(groupId, userID, domainId, string.Empty, language,
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
        /// <param name="query">Search string to look for within the assets’ title only. Search is starts with. White spaces are not ignored. Limited to 20 characters</param>
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

        /// <summary>
        /// Returns related media from external recommendation engine by media identifier<br />        
        /// </summary>        
        /// <param name="media_id">Media identifier</param>
        /// <param name="media_types">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Paging filter</param>
        /// <param name="language">Language code</param>        
        /// <remarks></remarks>
        [Route("relatedExternal"), HttpPost]
        [ApiAuthorize(true)]
        public KalturaAssetInfoListResponse relatedExternal(int media_id, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utcOffet = 0,
            List<KalturaCatalogWithHolder> with = null, string language = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (media_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "media_id cannot be 0");
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_type_ids == null)
                filter_type_ids = new List<KalturaIntegerValue>();

            if (pager == null)
                pager = new KalturaFilterPager() { PageIndex = 0, PageSize = 5 };

            string udid = KSUtils.ExtractKSPayload(KS.GetFromRequest()).UDID;

            try
            {
                string userID = KS.GetFromRequest().UserId;

                response = ClientsManager.CatalogClient().GetRelatedMediaExternal(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.PageIndex, pager.PageSize, media_id, filter_type_ids.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
