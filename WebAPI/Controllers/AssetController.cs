using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Filters;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/asset/action")]
    [OldStandardAction("listOldStandard", "list")]
    [OldStandardAction("getOldStandard", "get")]
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
        [Route("listOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.NotFound)]
        public KalturaAssetInfoListResponse ListOldStandard(KalturaAssetInfoFilter filter, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

            string udid = KSUtils.ExtractKSPayload().UDID;

            if (pager == null)
                pager  = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            if (filter.IDs == null || filter.IDs.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaAssetInfoFilter.IDs");
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
                                throw new BadRequestException(BadRequestException.MEDIA_IDS_MUST_BE_NUMERIC);
                            }


                            response = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                pager.getPageIndex(), pager.PageSize, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
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
                                throw new BadRequestException(BadRequestException.EPG_INTERNAL_IDS_MUST_BE_NUMERIC);
                            }

                            response = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               pager.getPageIndex(), pager.PageSize, ids, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                        }
                        break;
                    case KalturaCatalogReferenceBy.EPG_EXTERNAL:
                        {
                            response = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                  pager.getPageIndex(), pager.PageSize, filter.IDs.Select(id => id.value).ToList(), with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (response == null || response.Objects == null || response.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }
                        }
                        break;
                    case KalturaCatalogReferenceBy.channel:
                        {
                            int channelID;
                            if (!int.TryParse(filter.IDs.First().value, out channelID))
                            {
                                throw new BadRequestException(BadRequestException.INVALID_ACTION_PARAMETER, "filter.ids");
                            }

                            var withList = with.Select(x => x.type).ToList();
                            response = ClientsManager.CatalogClient().GetChannelAssets(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                            pager.getPageIndex(), pager.PageSize, withList, channelID, order_by, string.Empty);
                        }
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
            {
                filter = new KalturaSearchAssetFilter();
            }
            else
            {
                filter.Validate();
            }
           
            try
            {              
                // external channel 
                if (filter is KalturaChannelExternalFilter)
                {
                    KalturaChannelExternalFilter channelExternalFilter = (KalturaChannelExternalFilter)filter;                   
                    string deviceType = System.Web.HttpContext.Current.Request.UserAgent;
                    response = ClientsManager.CatalogClient().GetExternalChannelAssets(groupId, channelExternalFilter.IdEqual.ToString(), userID, domainId, udid,
                        language, pager.getPageIndex(), pager.PageSize, filter.OrderBy, deviceType, channelExternalFilter.UtcOffsetEqual.ToString(), channelExternalFilter.FreeText);
                }
                //SearchAssets - Unified search across – VOD: Movies, TV Series/episodes, EPG content.
                else if (filter is KalturaSearchAssetFilter)
                {
                    KalturaSearchAssetFilter regularAssetFilter = (KalturaSearchAssetFilter)filter;
                    response = ClientsManager.CatalogClient().SearchAssets(groupId, userID, domainId, udid, language, pager.getPageIndex(), pager.PageSize, regularAssetFilter.KSql,
                        regularAssetFilter.OrderBy, regularAssetFilter.getTypeIn(), regularAssetFilter.getEpgChannelIdIn());
                }
                //Return list of media assets that are related to a provided asset ID (of type VOD). 
                //Returned assets can be within multi VOD asset types or be of same type as the provided asset. 
                //Response is ordered by relevancy. On-demand, per asset enrichment is supported. Maximum number of returned assets – 20, using paging
                else if (filter is KalturaRelatedFilter)
                {
                    KalturaRelatedFilter relatedFilter = (KalturaRelatedFilter)filter;
                    response = ClientsManager.CatalogClient().GetRelatedMedia(groupId, userID, domainId, udid,
                    language, pager.getPageIndex(), pager.PageSize, relatedFilter.getMediaId(), relatedFilter.KSql, relatedFilter.getTypeIn(), relatedFilter.OrderBy);
                }
                //Return list of assets that are related to a provided asset ID. Returned assets can be within multi asset types or be of same type as the provided asset. 
                //Support on-demand, per asset enrichment. Related assets are provided from the external source (e.g. external recommendation engine). 
                //Maximum number of returned assets – 20, using paging  
                else if (filter is KalturaRelatedExternalFilter)
                {
                    KalturaRelatedExternalFilter relatedExternalFilter = (KalturaRelatedExternalFilter)filter;
                    response = ClientsManager.CatalogClient().GetRelatedMediaExternal(groupId, userID, domainId, udid,
                        language, pager.getPageIndex(), pager.PageSize, relatedExternalFilter.IdEqual, relatedExternalFilter.getTypeIn(), relatedExternalFilter.UtcOffsetEqual,
                        relatedExternalFilter.FreeText);
                }
                // Search for assets via external service (e.g. external recommendation engine). 
                //Search can return multi asset types. Support on-demand, per asset enrichment. Maximum number of returned assets – 100, using paging
                else if (filter is KalturaSearchExternalFilter)
                {
                    KalturaSearchExternalFilter searchExternalFilter = (KalturaSearchExternalFilter)filter;
                    if (pager == null)
                        pager = new KalturaFilterPager() { PageIndex = 0, PageSize = 5 };

                    List<int> typeIn = searchExternalFilter.getTypeIn();
                    if (typeIn.Contains(0))
                    {
                        response = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, domainId, udid, language, pager.getPageIndex(),
                                                                                       pager.PageSize, searchExternalFilter.convertQueryToList(), searchExternalFilter.OrderBy);
                    }
                    else
                    {
                        response = ClientsManager.CatalogClient().GetSearchMediaExternal(groupId, userID, domainId, udid, language, pager.getPageIndex(), pager.PageSize,
                                                                                        searchExternalFilter.Query, searchExternalFilter.getTypeIn(), searchExternalFilter.UtcOffsetEqual);
                    }
                }
                // Returns assets that belong to a channel
                else if (filter is KalturaChannelFilter)
                {
                    KalturaChannelFilter channelFilter = (KalturaChannelFilter)filter;
                    if (pager == null)
                        pager = new KalturaFilterPager();

                    response = ClientsManager.CatalogClient().GetChannelAssets(groupId, userID, domainId, udid, language, pager.getPageIndex(),
                        pager.PageSize, channelFilter.IdEqual, channelFilter.OrderBy, channelFilter.KSql, channelFilter.GetShouldUseChannelDefault());
                }
                else if (filter is KalturaBundleFilter)
                {
                    KalturaBundleFilter bundleFilter = (KalturaBundleFilter)filter;
                    response = ClientsManager.CatalogClient().GetBundleAssets(groupId, userID, domainId, udid, language,
                       pager.getPageIndex(), pager.PageSize, bundleFilter.IdEqual, bundleFilter.OrderBy, bundleFilter.getTypeIn(), bundleFilter.BundleTypeEqual);
                }
                // returns assets that are scheduled to be recorded
                else if (filter is KalturaScheduledRecordingProgramFilter)
                {
                    KalturaScheduledRecordingProgramFilter scheduledRecordingFilter = (KalturaScheduledRecordingProgramFilter)filter;
                    response = ClientsManager.CatalogClient().GetScheduledRecordingAssets(groupId, userID, domainId, udid, language, scheduledRecordingFilter.ConvertChannelsIn(), pager.getPageIndex(), 
                                    pager.getPageSize(), scheduledRecordingFilter.StartDateGreaterThanOrNull, scheduledRecordingFilter.EndDateLessThanOrNull, scheduledRecordingFilter.OrderBy, scheduledRecordingFilter.RecordingTypeEqual);
                }
                else
                {
                    throw new InternalServerErrorException();
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
        /// <param name="assetReferenceType">Asset type</param>
        /// <remarks></remarks>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(WebAPI.Managers.Models.StatusCode.NotFound)]
        public KalturaAsset Get(string id, KalturaAssetReferenceType assetReferenceType)
        {
            KalturaAsset response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            try
            {
                string userID = KS.GetFromRequest().UserId;
                string udid = KSUtils.ExtractKSPayload().UDID;
                string language = Utils.Utils.GetLanguageFromRequest();

                switch (assetReferenceType)
                {
                    case KalturaAssetReferenceType.media:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }
                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                0, 1, new List<int>() { mediaId }, KalturaAssetOrderBy.START_DATE_DESC);

                            // if no response - return not found status 
                            if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_internal:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               0, 1, new List<int> { epgId }, KalturaAssetOrderBy.START_DATE_DESC);

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_external:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                              0, 1, new List<string> { id }, KalturaAssetOrderBy.START_DATE_DESC);

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
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
        [Route("getOldStandard"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [Throws(WebAPI.Managers.Models.StatusCode.NotFound)]
        public KalturaAssetInfo GetOldStandard(string id, KalturaAssetReferenceType type, List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfo response = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(id))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
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
                    case KalturaAssetReferenceType.media:
                        {
                            int mediaId;
                            if (!int.TryParse(id, out mediaId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }
                            var mediaRes = ClientsManager.CatalogClient().GetMediaByIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                                0, 1, new List<int>() { mediaId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (mediaRes == null || mediaRes.Objects == null || mediaRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = mediaRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_internal:
                        {
                            int epgId;
                            if (!int.TryParse(id, out epgId))
                            {
                                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "id");
                            }

                            var epgRes = ClientsManager.CatalogClient().GetEPGByInternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                               0, 1, new List<int> { epgId }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
                            }

                            response = epgRes.Objects.First();
                        }
                        break;
                    case KalturaAssetReferenceType.epg_external:
                        {
                            var epgRes = ClientsManager.CatalogClient().GetEPGByExternalIds(groupId, userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid, language,
                              0, 1, new List<string> { id }, with.Select(x => x.type).ToList());

                            // if no response - return not found status 
                            if (epgRes == null || epgRes.Objects == null || epgRes.Objects.Count == 0)
                            {
                                throw new NotFoundException(NotFoundException.OBJECT_NOT_FOUND, "Asset");
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
        /// (maximum length of entire filter is 2048 characters)]]></param>
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
        [SchemeArgument("filter", MaxLength = 2048)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        public KalturaAssetInfoListResponse Search(KalturaOrder? order_by, List<KalturaIntegerValue> filter_types = null, string filter = null,
            List<KalturaCatalogWithHolder> with = null, KalturaFilterPager pager = null, string request_id = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userID = KS.GetFromRequest().UserId;
            int domainId = (int)HouseholdUtils.GetHouseholdIDByKS(groupId);
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

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
        [Obsolete]
        [Throws(eResponseStatus.IndexMissing)]
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
        [SchemeArgument("id", MinInteger = 1)]
        public KalturaAssetInfoListResponse Related(int media_id, string filter = null, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_types = null,
            List<KalturaCatalogWithHolder> with = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;

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
        [Obsolete]
        [SchemeArgument("asset_id", MinInteger = 1)]
        public KalturaAssetInfoListResponse RelatedExternal(int asset_id, KalturaFilterPager pager = null, List<KalturaIntegerValue> filter_type_ids = null, int utc_offset = 0,
            List<KalturaCatalogWithHolder> with = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string language = Utils.Utils.GetLanguageFromRequest();

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
        [Obsolete]
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
        /// (maximum length of entire filter is 2048 characters)]]></param>
        /// <remarks>Possible status codes: 
        /// BadSearchRequest = 4002, IndexMissing = 4003, SyntaxError = 4004, InvalidSearchField = 4005, Channel does not exist = 4018
        /// </remarks>
        [Route("channel"), HttpPost]
        [ApiAuthorize]
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        [Throws(eResponseStatus.ObjectNotExist)]
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
        [Obsolete]
        [SchemeArgument("id", MinInteger = 1)]
        [SchemeArgument("utc_offset", MinFloat = -12, MaxFloat = 12)]
        [Throws(eResponseStatus.ExternalChannelHasNoRecommendationEngine)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.AdapterUrlRequired)]
        [Throws(eResponseStatus.BadSearchRequest)]
        [Throws(eResponseStatus.IndexMissing)]
        [Throws(eResponseStatus.SyntaxError)]
        [Throws(eResponseStatus.InvalidSearchField)]
        [Throws(eResponseStatus.RecommendationEngineNotExist)]
        [Throws(eResponseStatus.ExternalChannelNotExist)]
        public KalturaAssetInfoListResponse ExternalChannel(int id, List<KalturaCatalogWithHolder> with = null, KalturaOrder? order_by = null,
            KalturaFilterPager pager = null, float? utc_offset = null, string free_param = null)
        {
            KalturaAssetInfoListResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (pager == null)
                pager = new KalturaFilterPager();

            if (with == null)
                with = new List<KalturaCatalogWithHolder>();

            try
            {
                string userID = KS.GetFromRequest().UserId;

                var convertedWith = with.Select(x => x.type).ToList();

                string deviceType = System.Web.HttpContext.Current.Request.UserAgent;
                string str_utc_offset = utc_offset.HasValue ? utc_offset.Value.ToString() : null;
                response = ClientsManager.CatalogClient().GetExternalChannelAssets(groupId, id.ToString(), userID, (int)HouseholdUtils.GetHouseholdIDByKS(groupId), udid,
                    language, pager.getPageIndex(), pager.PageSize, order_by, convertedWith, deviceType, str_utc_offset, free_param);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// This action delivers all data relevant for player
        /// </summary>
        [Route("getPlaybackContext"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.RecordingNotFound)]
        [Throws(eResponseStatus.ProgramDoesntExist)]
        [Throws(eResponseStatus.DeviceNotInDomain)]
        [Throws(eResponseStatus.RecordingStatusNotValid)]
        public KalturaPlaybackContext GetPlaybackContext(string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams)
        {
            KalturaPlaybackContext response = null;
           
            KS ks = KS.GetFromRequest();
            string userId = ks.UserId;

            try
            {
                response = ClientsManager.ConditionalAccessClient().GetPlaybackContext(ks.GroupId, userId, KSUtils.ExtractKSPayload().UDID, assetId, assetType, contextDataParams);
                // build manifest url
                string baseUrl = string.Format("{0}://{1}{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, HttpContext.Current.Request.ApplicationPath.TrimEnd('/'));
                foreach (var source in response.Sources)
                {
                    StringBuilder url = new StringBuilder(string.Format("{0}/api_v3/service/assetFile/action/playManifest/partnerId/{1}/assetId/{2}/assetType/{3}/assetFileId/{4}/contextType/{5}",
                        baseUrl, ks.GroupId, assetId, assetType, source.Id, contextDataParams.Context));

                    if (!string.IsNullOrEmpty(userId) && userId != "0")
                    {
                        url.AppendFormat("/ks/{0}", ks.ToString());
                    }
                    source.Url = url.ToString();
                    source.Protocols = !string.IsNullOrEmpty(source.Url) ? (source.Url.ToLower().StartsWith("https") ? "https" : "http") : string.Empty;
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
