using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using WebAPI.Catalog;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Schema;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/asset/action")]
    [OldStandard("listOldStandard", "list")]
    [OldStandard("getOldStandard", "get")]
    public class AssetController : ApiController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns media or EPG assets. Filters by media identifiers or by EPG internal or external identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="order_by">Ordering the assets</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Route("listOldStandart"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaAssetInfoListResponse ListOldStandart(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;

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
                string language = Utils.Utils.GetLanguageFromRequest();
                List<int> ids = null;

                switch (filter.ReferenceType)
                {
                    case KalturaCatalogReferenceBy.MEDIA:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is media");
                            }


                            response = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                0, 0, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                                throw new NotFoundException();
                        }
                        break;
                    case KalturaCatalogReferenceBy.EPG_INTERNAL:
                        {
                            try
                            {
                                ids = filter.IDs.Select(x => int.Parse(x.value)).ToList();
                            }
                            catch (Exception)
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "ids must be numeric when type is epg_internal");
                            }

                            response = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               0, 0, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                        }
                        break;
                    case KalturaCatalogReferenceBy.EPG_EXTERNAL:
                        {
                            response = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                  0, 1, filter.IDs.Select(id => id.value).ToList(), with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }
                        }
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
        /// Returns media or EPG assets. Filters by media identifiers or by EPG internal or external identifier.
        /// </summary>
        /// <param name="filter">Filtering the assets request</param>
        /// <param name="pager">Paging the request</param>
        /// <remarks></remarks>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetListResponse List(KalturaAssetFilter filter = null,  KalturaFilterPager pager = null)
        {
            KalturaAssetListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            // parameters validation
            if (pager == null)
                pager = new KalturaFilterPager();

            if (filter == null)
                filter = new KalturaAssetFilter();

            if (filter.TypesIn == null)
                filter.TypesIn = new List<KalturaIntegerValue>();

            if (!string.IsNullOrEmpty(filter.KSql) && filter.KSql.Length > 1024)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "too long filter");
            }

            try
            {
                // no related media id - search
                if (string.IsNullOrEmpty(filter.RelatedMediaIdEqual))
                {
                    response = ClientsManager.CatalogClient().SearchAssets(groupId, userID, domainId, udid, language, pager.getPageIndex(), pager.PageSize, filter.KSql, filter.OrderBy, filter.TypesIn.Select(x => x.value).ToList());
                }
                // related
                else
                {
                    int mediaId = 0;
                    if (!int.TryParse(filter.RelatedMediaIdEqual, out mediaId))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "related media id must be numeric");
                    }

                    response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.getPageIndex(), pager.PageSize, mediaId, filter.KSql, filter.TypesIn.Select(x => x.value).ToList(), filter.OrderBy);
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
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemaValidationType.ACTION_ARGUMENTS)]
        public KalturaAsset Get(string id, KalturaAssetReferenceType assetReferenceType)
        {
            KalturaAsset response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id cannot be empty");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                switch (assetReferenceType)
                {
                    case KalturaAssetReferenceType.MEDIA:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is media");
                            }
                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                0, 1, new List<int>() { mediaId }, KalturaAssetOrderBy.NEWEST);

                            // if no response - return not found status 
                            if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.EPG_INTERNAL:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is epg_internal");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               0, 1, new List<int> { epgId }, KalturaAssetOrderBy.NEWEST);

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.EPG_EXTERNAL:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                              0, 1, new List<string> { id }, KalturaAssetOrderBy.NEWEST);

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
        /// Returns media or EPG asset by media / EPG internal or external identifier
        /// </summary>
        /// <param name="id">Asset identifier</param>                
        /// <param name="type">Asset type</param>                
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Route("getOldStandart"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaAssetInfo GetOldStandart(string id, KalturaAssetReferenceType type, List<KalturaCatalogWithHolder> with = null)
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
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                switch (type)
                {
                    case KalturaAssetReferenceType.MEDIA:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is media");
                            }
                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                0, 1, new List<int>() { mediaId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.EPG_INTERNAL:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be numeric when type is epg_internal");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               0, 1, new List<int> { epgId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException();
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.EPG_EXTERNAL:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
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
        /// geo_block - only valid value is "true": When enabled, only assets that are not restriced to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// Comparison operators: for numerical fields =, >, >=, <, <=. For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 1024 characters)]]></param>
        /// <param name="order_by">Required sort option to apply for the identified assets. If omitted – will use relevancy.
        /// Possible values: relevancy, a_to_z, z_to_a, views, ratings, votes, newest.</param>
        /// <param name="with"> Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Page size and index</param>
        /// <param name="request_id">Current request identifier (used for paging)</param>
        /// <remarks>Possible status codes: Bad search request = 4002, Missing index = 4003, SyntaxError = 4004, InvalidSearchField = 4005</remarks>
        [Route("search"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaAssetInfoListResponse Search(KalturaOrder? order_by, List<KalturaIntegerValue> filter_types = null, string filter = null,
            List<KalturaCatalogWithHolder> with = null, KalturaFilterPager pager = null, string request_id = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

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
                response = ClientsManager.CatalogClient().SearchAssets(groupId, userID, domainId, udid, language,
                pager.getPageIndex(), pager.PageSize, filter, order_by, filter_types.Select(x => x.value).ToList(), 
                request_id,
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
        /// <remarks>Possible status codes: Missing index = 4003</remarks>
        [Route("autocomplete"), HttpPost]
        [ApiAuthorize]
        public KalturaSlimAssetInfoWrapper Autocomplete(string query, List<KalturaCatalogWithHolder> with = null, List<KalturaIntegerValue> filter_types = null,
            KalturaOrder? order_by = null, int? size = null)
        {
            KalturaSlimAssetInfoWrapper response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            string udid = KSUtils.ExtractKSPayload().UDID;

            // Size rules - according to spec.  10>=size>=1 is valid. default is 5.
            if (size == null || size > 10 || size < 1)
            {
                size = 5;
            }

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            string language = Utils.Utils.GetLanguageFromRequest();
            try
            {
                response = ClientsManager.CatalogClient().Autocomplete(groupId, userID, udid, language, size, query, order_by, filter_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return list of media assets that are related to a provided asset ID (of type VOD). Returned assets can be within multi VOD asset types or be of same type as the provided asset. Response is ordered by relevancy. On-demand, per asset enrichment is supported. Maximum number of returned assets – 20, using paging <br />        
        /// </summary>        
        /// <param name="media_id">The ID of the asset for which to return related assets</param>
        /// <param name="filter_types">List of type of related assets to return. Possible values: 0 - for EPG ; any media type ID (according to media type IDs defined dynamically in the system). If omitted – return assets of same asset type as the provided asset type. </param>        
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 50. If omitted – 5 is used. Value greater than 50 will set to 50</param>
        /// <param name="filter">Valid KSQL expression. If provided – the filter is applied on the result set and further reduce it</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <remarks></remarks>
        [Route("related"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        public KalturaAssetInfoListResponse Related(int media_id, string filter = null, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_types = null,
            List<KalturaCatalogWithHolder> with = null)
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

            if (filter_types == null)
                filter_types = new List<KalturaIntegerValue>();

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.getPageIndex(), pager.PageSize, media_id, filter, filter_types.Select(x => x.value).ToList(), with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Return list of assets that are related to a provided asset ID. Returned assets can be within multi asset types or be of same type as the provided asset. Support on-demand, per asset enrichment. Related assets are provided from the external source (e.g. external recommendation engine). Maximum number of returned assets – 20, using paging <br />        
        /// </summary>        
        /// <param name="asset_id">The ID of the asset for which to return related assets</param>
        /// <param name="filter_type_ids">The type of related assets to return. Possible values: ALL – include all asset types ; any media type ID (according to media type IDs defined dynamically in the system). If ommited = ALL.</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 20. If omitted – 5 is used. Value greater than 20 will set to 20</param>
        /// <param name="utc_offset">Client’s offset from UTC. Format: +/-HH:MM. Example (client located at NY - EST): “-05:00”. If provided – may be used to further fine tune the returned collection</param>        
        /// <param name="free_param">Suplimentry data that the client can provide the external recommnedation engine</param>        
        /// <remarks></remarks>
        [Route("relatedExternal"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetInfoListResponse RelatedExternal(int asset_id, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utc_offset = 0,
            List<KalturaCatalogWithHolder> with = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (asset_id == 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "asset_id cannot be 0");
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
                    language, pager.getPageIndex(), pager.PageSize, asset_id, filter_type_ids.Select(x => x.value).ToList(), utc_offset, with.Select(x => x.type).ToList(), free_param);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Search for assets via external service (e.g. external recommendation engine). Search can return multi asset types. Support on-demand, per asset enrichment. Maximum number of returned assets – 100, using paging <br />        
        /// </summary>        
        /// <param name="query">Search string </param>
        /// <param name="filter_type_ids">Related media types list - possible values:
        /// any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.</param>        
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="pager">Paging filter - Page number to return. If omitted returns first page. Number of assets to return per page. Possible range 5 ≤ size ≥ 20. If omitted – 10 is used. Value greater than 20 will set to 20.</param>
        /// <param name="utc_offset">Client’s offset from UTC. Format: +/-HH:MM. Example (client located at NY - EST): “-05:00”. If provided – may be used to further fine tune the returned collection</param>  
        /// <remarks></remarks>
        [Route("searchExternal"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetInfoListResponse searchExternal(string query, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utc_offset = 0,
            List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
                        
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
                string language = Utils.Utils.GetLanguageFromRequest();

                response = ClientsManager.CatalogClient().GetSearchMediaExternal(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.getPageIndex(), pager.PageSize, query, filter_type_ids.Select(x => x.value).ToList(), utc_offset, with.Select(x => x.type).ToList());
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns assets that belong to a channel
        /// </summary>
        /// <param name="id">Channel identifier</param>
        /// <param name="order_by">Ordering the channel</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="filter_query"><![CDATA[Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date.
        /// Comparison operators: for numerical fields =, >, >=, <, <=. For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 1024 characters)]]></param>
        /// <remarks>Possible status codes: 
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, 
        /// </remarks>
        [Route("channel"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetInfoListResponse Channel(int id, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, string filter_query = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be positive");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                var withList = with.Select(x => x.type).ToList();
                response = ClientsManager.CatalogClient().GetChannelAssets(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                    pager.getPageIndex(), pager.PageSize, withList, id, order_by, filter_query);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Returns assets as defined by an external channel (3rd party recommendations)
        /// </summary>
        /// <param name="id">External channel's identifier</param>
        /// <param name="order_by">Ordering the assets</param>
        /// <param name="pager">Paging the request</param>
        /// <param name="with">Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.</param>
        /// <param name="utc_offset">UTC offset for request's enrichment</param>
        /// <param name="free_param">Suplimentry data that the client can provide the external recommnedation engine</param>
        /// <remarks>Possible status codes: 
        /// External Channel reference type: ExternalChannelHasNoRecommendationEngine = 4014, AdapterAppFailure = 6012, AdapterUrlRequired = 5013,
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, 
        /// RecommendationEngineNotExist = 4007, ExternalChannelNotExist = 4011</remarks>
        [Route("externalChannel"), HttpPost]
        [ApiAuthorize]
        public KalturaAssetInfoListResponse ExternalChannel(int id, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, string utc_offset = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (id <= 0)
            {
                throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "id must be positive");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;

                var convertedWith = with.Select(x => x.type).ToList();

                if (!string.IsNullOrEmpty(utc_offset))
                {
                    double utcOffsetDouble;

                    if (!double.TryParse(utc_offset, out utcOffsetDouble))
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                    }
                    else if (utcOffsetDouble > 12 || utcOffsetDouble < -12)
                    {
                        throw new BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "UTC Offset must be a valid number between -12 and 12");
                    }
                }

                string deviceType = System.Web.HttpContext.Current.Request.UserAgent;

                response = ClientsManager.CatalogClient().GetExternalChannelAssets(groupId, id.ToString(), userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.getPageIndex(), pager.PageSize, order_by, convertedWith, deviceType, utc_offset, free_param);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }
    }
}
